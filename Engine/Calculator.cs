﻿using System;
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
        readonly List<Result> results = new List<Result>();
        readonly UniqVariable uniq = new UniqVariable();

        // MAYBE Have a Dictionary<Expression, Env> that also works as a cache, so evaluations aren't repeated
        readonly Dictionary<Expression, bool> recursionCheck = new Dictionary<Expression, bool>();

        // MAYBE Could probably remove the counting check because of the Dictionary, but need to think about that
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

        internal IEnumerable<Result> Results => results;
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
                if ((MatchesSelf(temp, out var env, out var result) || TryResolveFact(temp, out env, out result)) && env.TryGetValue(next, out var value)) {
                    if (result.Children.Count > 0) {
                        // MAYBE this logic could be avoided if top-level expressions didn't do the next weirdness
                        var solving = result.Children[0].Line;
                        result.Children[0].Line = solving.Substring(0, solving.Length - 4) + "...";
                    }
                    result.Line = value;
                    results.Add(result);
                } else {
                    Error = true;
                    results.Add(new Result($"Error! Can't evaluate '{e}'"));
                }
            }
        }

        bool MatchesSelf(Func test, out Environment env, out Result result) {
            var matcher = new Matcher(test.Left, test.Right, new Func[] { }, this); // TODO don't need wheres?
            if (matcher.Matches) {
                env = matcher.Env;
                result = new Result("");
                return true;
            }
            env = default;
            result = default;
            return false;
        }

        bool TryResolveFact(Func test, out Environment env, out Result result) {
            if (recursionCheck.TryGetValue(test, out var completed) && !completed) {
                // if completed, could try to cache and return env?
                env = default;
                result = default;
                return false;
            }
            recursionCheck[test] = false;

            // Equality is implicitly reflexive, even though other ops aren't
            // MAYBE there could be an implicit rule A = B |B = A that allow for reflecting and solving?
            var success = TryResolveFactImpl(test, out env, out result) || TryResolveFactImpl(new Func(test.Name, test.Right, test.Left), out env, out result);

            recursionCheck[test] = true;
            return success;
        }

        // TODO TODO TODO add some tests
        /// When solving i.e. 1 + 2 = x
        /// env = { x: 3 }
        /// result = $ToBeFilledInLater
        ///   Applied rule a + b = c Implies a = 1, b = 2, c = 3
        ///     | a + 1 = x Implies x = 2
        ///       Solving 1 + 1 = x
        ///       ...
        ///     | y + 1 = b Implies y = 1
        ///       ...
        ///     ...
        bool TryResolveFactImpl(Func test, out Environment env, out Result result) {
            foreach (var fact in facts) {
                var matcher = new Matcher(test, fact.Equality, fact.Wheres, this);
                if (matcher.Matches) {
                    env = matcher.Env;

                    result = new Result("");
                    result.Children.Add(new Result($"Solving {test}"));

                    var factVars = VariableFinder.Find(fact.Equality);
                    var implies = string.Join(", ", factVars.Select(v => $"{v} = {matcher.Env[v]}"));
                    var line = $"Applied rule {fact.Equality}";
                    if (factVars.Any()) {
                        line += $" Implies {implies}";
                    }
                    result.Children.Add(new Result(line));

                    var defined = new HashSet<char>(matcher.InitialEnv.Keys);
                    foreach (var (where, r) in matcher.Solution) {
                        var whereVars = VariableFinder.Find(where);
                        var defs = string.Join(", ", whereVars.Where(defined.Add).Select(v => $"{v} = {matcher.Env[v]}"));

                        r.Line = $"| {where} {(defs.Any() ? "Implies " + defs : "")}";
                        result.Children.Add(r);
                    }
                    return true; // TODO shouldn't something like a + a = 4 require looping all rules to show a isn't unique?
                }
            }
            env = default;
            result = default;
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

        class VariableFinder : ExpressionVisitor
        {
            readonly List<char> vars = new List<char>();
            readonly HashSet<char> seen = new HashSet<char>();

            public static ICollection<char> Find(Expression e) {
                var finder = new VariableFinder();
                finder.Visit(e);
                return finder.vars;
            }

            protected override Character VisitVariable(Character e) {
                if (seen.Add(e.Value)) {
                    vars.Add(e.Value);
                }
                return base.VisitVariable(e);
            }
        }

        class Matcher
        {
            readonly Calculator calc;
            bool? matches = true; // null for possibly, depending on vars
            public readonly List<(Expression, Result)> Solution = new List<(Expression, Result)>();

            public Matcher(Expression x, Expression y, IEnumerable<Func> wheres, Calculator calc) {
                if (calc.matcherRecursiveCalls++ > max_recursion) {
                    throw new StackOverflowException("Recursion too deep"); // doesn't actually crash the .NET VM
                }

                try {
                    this.calc = calc;

                    // Do a pretest to try to assign as many variables as possible first, then check Solve() again later
                    Solve(x, y);
                    if (matches.HasValue && !matches.Value) {
                        return;
                    }
                    InitialEnv = new Environment(Env);
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

            public Environment InitialEnv { get; private set; }
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

                // TODO TODO TODO BUG!
                /*
                   This program goes into an infinite loop....
                    7&7 = a
                    | a + a = 3

                    7&7
                */
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
                var replaced = calc.RewriteVariables(inlined, out var backMap); // MAYBE is this needed? It's really ugly in the explanation
                if (calc.TryResolveFact((Func)replaced, out var resolvedEnv, out var result)) {
                    Solution.Add((where, result));

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

    class Result
    {
        public List<Result> Children { get; set; } = new List<Result>();
        public string Line { get; set; }
        public Result(string line) {
            Line = line;
        }

        public override string ToString() => Line + (Children.Any() ? "..." : "");
    }

    public class Evalutation
    {
        public string Result { get; set; }
        public bool Error { get; set; }

        public Evalutation(string input, bool includeChildren) {
            var expression = ExprExtractor.GetExpression(input);

            var calc = new Calculator(expression);
            calc.Evaluate();

            var builder = new StringBuilder();

            // TODO this should move to test code, and GUI should display clickable tree
            foreach (var r in calc.Results) {
                if (includeChildren) {
                    Print(r, 0, builder);
                } else {
                    builder.AppendLine(r.Line);
                }
            }
            Result = builder.ToString();
            Error = calc.Error;
        }

        void Print(Result result, int indent, StringBuilder builder) {
            builder.AppendLine(new string(' ', indent) + result.Line);
            foreach (var c in result.Children) {
                Print(c, indent + 4, builder);
            }
        }
    }

    public static class Extensions
    {
        public static string TrailingNewline(this string s) =>
            s.EndsWith(System.Environment.NewLine) ? s : s + System.Environment.NewLine;
    }
}
