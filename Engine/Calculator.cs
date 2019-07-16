using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Environment = System.Collections.Generic.Dictionary<char, string>;

namespace Engine
{
    public class Calculator
    {
        readonly List<Fact> facts = new List<Fact>();
        readonly List<Expression> expressions = new List<Expression>();
        readonly StringBuilder output = new StringBuilder();
        readonly UniqVariable uniq = new UniqVariable();

        readonly Dictionary<Expression, bool> recursionCheck = new Dictionary<Expression, bool>();

        const int max_recursion = 10;
        int matcherRecursiveCalls;

        internal Calculator(IEnumerable<Expression> expressions) {
            foreach (var e in expressions) {
                if (e.Id == ExpressionType.Fact) {
                    facts.Add((Fact)e);
                } else {
                    this.expressions.Add(e);
                }
            }
        }

        internal string Output => output.ToString();
        internal bool Error { get; private set; }

        static bool TryGetLiteralDigits(Expression e, Environment env, out string digits) {
            var builder = new StringBuilder();
            if (TryGetLiteralDigits(e, env, builder)) {
                digits = builder.ToString();
                return true;
            }
            digits = default;
            return false;
        }

        static bool TryGetLiteralDigits(Expression e, Environment env, StringBuilder digits) {
            if (e is Character c) {
                if (c.Id == ExpressionType.Digit) {
                    digits.Append(c.Value);
                    return true; 
                } else if (c.Id == ExpressionType.Variable && env.TryGetValue(c.Value, out var value)) {
                    digits.Append(value);
                    return true;
                }
            }

            if (e is Func cons && cons.Name == ":") {
                return TryGetLiteralDigits(cons.Left, env, digits) && 
                    TryGetLiteralDigits(cons.Right, env, digits);
            }

            return false;
        }

        public void Evaluate() {
            foreach (var e in expressions) {
                // e = X, solve for X
                var next = uniq.Next;
                var temp = new Func("=", e, new Character(next, ExpressionType.Variable));
                if ((MatchesSelf(temp, out var env) || TryResolveFact(temp, out env)) && env.TryGetValue(next, out var value)) {
                    output.AppendLine(value);
                } else {
                    Error = true;
                    output.AppendLine($"Error! Can't evaluate '{e}'");
                }
            }
        }

        bool MatchesSelf(Func test, out Environment env) {
            var matcher = new Matcher(test.Left, test.Right, new Func[] { }, this); // TODO don't need wheres?
            if (matcher.Matches) {
                env = matcher.Env;
                return true;
            }
            env = default;
            return false;
        }

        bool TryResolveFact(Func test, out Environment env) {
            if (recursionCheck.TryGetValue(test, out var completed) && !completed) {
                // if completed, could try to cache and return env?
                env = default;
                return false;
            }
            recursionCheck[test] = false;

            // Equality is implicitly reflexive, even though other ops aren't
            var result = TryResolveFactImpl(test, out env) || TryResolveFactImpl(new Func(test.Name, test.Right, test.Left), out env);

            recursionCheck[test] = true;
            return result;
        }

        bool TryResolveFactImpl(Func test, out Environment env) {
            foreach (var fact in facts) {
                var matcher = new Matcher(test, fact.Equality, fact.Wheres, this);
                if (matcher.Matches) {
                    env = matcher.Env;
                    return true;
                }
            }
            env = default;
            return false;
        }

        // TODO test for rewrite needed?
        Expression RewriteVariables(Expression e, out Dictionary<char, Character> backMap) {
            var rewriter = new VariableRewriter {
                Uniq = uniq
            };
            var expression = rewriter.Visit(e);
            backMap = rewriter.Rewrites.ToDictionary(kvp => kvp.Value, kvp => new Character(kvp.Key, ExpressionType.Variable));
            return expression;
        }

        // TODO test for inline needed?
        class VariableInliner : ExpressionVisitor
        {
            public Environment Env { get; set; }

            protected override Character VisitVariable(Character e) =>
                Env.TryGetValue(e.Value, out var value) ? new Character(value.Single(), ExpressionType.Digit) : e;
        }

        class Matcher
        {
            readonly Calculator calc;
            bool? matches = true; // null for possibly, depending on vars

            public Matcher(Expression x, Expression y, IEnumerable<Func> wheres, Calculator calc) {
                if (calc.matcherRecursiveCalls++ > max_recursion) {
                    throw new StackOverflowException("Recursion too deep"); // doesn't actually crash the .NET VM
                }

                try {
                    this.calc = calc;

                    // Do a pretest to try to assign as many variables as possible first, then check Solve() again later
                    // This nicely prevents some recurive StackOverflow, but it might still be easy to trip it when
                    // Matcher recursively calls TryResolveFact which calls Matcher...
                    // TODO the solution is probably some sort of cache that lets you mark an evaluation as in-progress
                    Solve(x, y);
                    if (matches.HasValue && !matches.Value) {
                        return;
                    }
                    matches = true;

                    var toResolve = wheres.ToList();

                    while (toResolve.Any()) {
                        HandleWhere(toResolve);
                    }

                    Solve(x, y);
                } finally {
                    calc.matcherRecursiveCalls--;
                }
            }

            public Environment Env { get; private set; } = new Environment();
            public bool Matches => matches.HasValue && matches.Value;

            void HandleWhere(List<Func> toResolve) {
                var min = int.MaxValue;
                var minI = -1;
                for (var i = 0; i < toResolve.Count; ++i) {
                    var unknowns = Unknowns(toResolve[i]);
                    if (unknowns < min) {
                        min = unknowns;
                        minI = i;
                    }
                }

                if (min > 1) {
                    if (matches != false) {
                        matches = null;
                    }
                    return;
                }

                var where = toResolve[minI];
                toResolve.RemoveAt(minI);

                var inlined = new VariableInliner {
                    Env = Env
                }.Visit(where);
                var replaced = calc.RewriteVariables(inlined, out var backMap);
                if (calc.TryResolveFact((Func)replaced, out var resolvedEnv)) {
                    foreach (var kvp in resolvedEnv) {
                        // Only try to set variable we've introduce into the fact
                        // TODO add some tests around this
                        if (backMap.TryGetValue(kvp.Key, out var actualVar)) {
                            SetOrAdd(actualVar.Value, kvp.Value);
                        }
                    }
                    return;
                }

                Solve(where.Left, where.Right);
            }

            void Solve(Expression x, Expression y) {
                if (x is Character xc && y is Character yc) {
                    if (x.Id == ExpressionType.Variable && TryGetLiteralDigits(y, Env, out var digits) && digits.Length == 1) {
                        SetOrAdd(xc.Value, digits);
                        return;
                    }
                    if (y.Id == ExpressionType.Variable && TryGetLiteralDigits(x, Env, out digits) && digits.Length == 1) {
                        SetOrAdd(yc.Value, digits);
                        return;
                    }
                }

                if (x.Children.Count() != y.Children.Count() || x.Id != y.Id) {
                    matches = false;
                    return;
                }

                if (x.Id == ExpressionType.Func && ((Func)x).Name != ((Func)y).Name) {
                    matches = false;
                    return;
                }

                foreach (var (xx, yy) in x.Children.Zip(y.Children, (xx, yy) => (xx, yy))) {
                    Solve(xx, yy);
                }

                switch (x.Id) {
                    case ExpressionType.Digit:
                        if (((Character)x).Value != ((Character)y).Value) {
                            matches = false;
                            return;
                        }
                        break;
                    case ExpressionType.Variable:
                        if (matches != false) {
                            matches = null;
                        }
                        return;
                }
            }

            void SetOrAdd(char name, string digits) {
                if (Env.TryGetValue(name, out var value)) {
                    if (value != digits) {
                        matches = false;
                    }
                } else {
                    Env.Add(name, digits);
                }
            }

            int Unknowns(Expression e) {
                var current = e.Id == ExpressionType.Variable && !Env.ContainsKey(((Character)e).Value);
                return (current ? 1 : 0) + e.Children.Sum(Unknowns);
            }
        }
    }

    public class Evalutation
    {
        public string Result { get; set; }
        public bool Error { get; set; }

        public Evalutation(string input) {
            var expression = ExprExtractor.GetExpression(input);

            var calc = new Calculator(expression);
            calc.Evaluate();

            Result = calc.Output;
            Error = calc.Error;
        }
    }

    public static class Extensions
    {
        public static string TrailingNewline(this string s) =>
            s.EndsWith(System.Environment.NewLine) ? s : s + System.Environment.NewLine;
    }
}
