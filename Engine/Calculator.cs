using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Engine.Generated;
using PerCederberg.Grammatica.Runtime;

namespace Engine
{
    public class Calculator : ExpAnalyzer
    {
        readonly StringBuilder output = new StringBuilder();
        readonly List<Node> facts = new List<Node>();
        string input;

        Lazy<List<string>> lines;

        private Calculator() {
            lines = new Lazy<List<string>>(GetLines);
        }

        public override Node ExitEqualOrExpr(Production node) {
            var isExpression = node[1].Id == (int)ExpConstants.NEWLINE;
            if (isExpression) {
                output.AppendLine(GetValue(node.GetChildAt(0)));
            } else {
                facts.Add(node);
            }

            return base.ExitEqualOrExpr(node);
        }

        string GetValue(Node node) {
            if (node.GetStartLine() != node.GetEndLine()) {
                throw new NotSupportedException("input ranges multiple lines");
            }

            //TODO support cons of numbers

            // Grammatica lines and columns are indexed from 1, inclusive on ends
            var line = lines.Value[node.GetStartLine() - 1];
            var text = line.Substring(node.GetStartColumn() - 1, node.GetEndColumn() + 1 - node.GetStartColumn());
            return $"Error! Can't evaluate '{text}'";
        }

        List<string> GetLines() => input.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

        public static string Evaluate(string input) {
            var node = new ExpParser(new StringReader(input.TrailingNewline())).Parse();

            var calc = new Calculator {
                input = input
            };
            calc.Analyze(node);

            return calc.output.ToString();
        }
    }

    public static class Extensions
    {
        public static string TrailingNewline(this string s) =>
            s.EndsWith(Environment.NewLine) ? s : s + Environment.NewLine;
    }
}
