using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        string GetValue(Expression e) {
            var digits = new StringBuilder();
            if (TryGetLiteralDigits(e, digits)) {
                return digits.ToString();
            }

            foreach (var fact in facts) {
                if (TrySolve(e, fact, true, out var result)) {
                    return result;
                }
                if (TrySolve(e, fact, false, out result)) {
                    return result;
                }
            }

            Error = true;
            return $"Error! Can't evaluate '{e}'";
        }

        static bool TryGetLiteralDigits(Expression e, StringBuilder digits) {
            if (e is Character c && c.Id == ExpressionType.Digit) {
                digits.Append(c.Value);
                return true;
            }

            if (e is Func cons && cons.Name == ":") {
                return TryGetLiteralDigits(e.Children.First(), digits) && TryGetLiteralDigits(e.Children.Skip(1).Single(), digits);
            }

            return false;
        }

        bool TrySolve(Expression e, Fact fact, bool left, out string result) {
            var pattern = fact.Equality.Children.Skip(left ? 0 : 1).First();

            var wheres = fact.Children.Skip(1).ToList();

            var matcher = new Matcher(pattern, e);
            if (matcher.Matches && wheres.Count == 0) { // TODO don't fail on the where clause
                result = GetValue(fact.Equality.Children.Skip(left ? 1 : 0).First());
                return true;
            }

            result = default;
            return false;
        }

        public void Evaluate() {
            foreach (var e in expressions) {
                output.AppendLine(GetValue(e));
            }
        }

        class Matcher
        {
            public Matcher(Expression x, Expression y) {
                Solve(x, y);
            }

            public bool Matches { get; private set; } = true;

            void Solve(Expression x, Expression y) {
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
                        throw new NotSupportedException("Variable"); //TODO variable matching with dictionary
                }
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
            s.EndsWith(Environment.NewLine) ? s : s + Environment.NewLine;
    }
}
