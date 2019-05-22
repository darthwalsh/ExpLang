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

        string GetValue(Expression e, Environment env) {
            if (TryGetLiteralDigits(e, env, out var digits)) {
                return digits;
            }

            foreach (var fact in facts) {
                if (TrySolve(e, fact, true, env, out var result)) {
                    return result;
                }
                if (TrySolve(e, fact, false, env, out result)) {
                    return result;
                }
            }

            Error = true;
            return $"Error! Can't evaluate '{e}'";
        }

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

        bool TrySolve(Expression e, Fact fact, bool left, Environment env, out string result) {
            var pattern = left ? fact.Equality.Left : fact.Equality.Right;
            var matcher = new Matcher(pattern, e, fact.Wheres, env);
            if (matcher.Matches) {
                result = GetValue(left ? fact.Equality.Right : fact.Equality.Left, matcher.Env);
                return true;
            }

            result = default;
            return false;
        }

        public void Evaluate() {
            foreach (var e in expressions) {
                output.AppendLine(GetValue(e, new Environment()));
            }
        }

        class Matcher
        {
            public Matcher(Expression x, Expression y, IEnumerable<Func> wheres, Environment env) {
                Env = new Environment(env);
                Solve(x, y);

                var toResolve = wheres.ToList();

                while (toResolve.Any()) {
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
                        Matches = false;
                        return;
                    }

                    var where = toResolve[minI];
                    toResolve.RemoveAt(minI);
                    Solve(where.Left, where.Right);
                }
            }

            public bool Matches { get; private set; } = true;
            public Environment Env { get; private set; }

            void Solve(Expression x, Expression y) {
                if (x is Character xc && y is Character yc) {
                    if (x.Id == ExpressionType.Variable && TryGetLiteralDigits(y, Env, out var digits) && digits.Length == 1) {
                        Env[xc.Value] = digits;
                        return;
                    }
                    if (y.Id == ExpressionType.Variable && TryGetLiteralDigits(x, Env, out digits) && digits.Length == 1) {
                        Env[yc.Value] = digits;
                        return;
                    }
                }

                if (x.Children.Count() != y.Children.Count() || x.Id != y.Id) {
                    Matches = false;
                    return;
                }

                foreach (var (xx, yy) in x.Children.Zip(y.Children, (xx, yy) => (xx, yy))) {
                    Solve(xx, yy);
                }

                switch (x.Id) {
                    case ExpressionType.Digit:
                        if (((Character)x).Value != ((Character)y).Value) {
                            Matches = false;
                            return;
                        }
                        break;
                    case ExpressionType.Variable:
                        Matches = false;
                        return;
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
