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
        readonly List<string> lines;

        private Calculator(string input) {
            lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
        }

        public override Node ExitEqualOrExpr(Production node) {
            var isExpression = (ExpConstants)node[1].Id == ExpConstants.NEWLINE;
            if (isExpression) {
                output.AppendLine(GetValue(node[0]));
            } else {
                facts.Add(node);
            }

            return base.ExitEqualOrExpr(node);
        }

        string GetValue(Node node) {
            if (node.GetStartLine() != node.GetEndLine()) {
                throw new NotSupportedException("input ranges multiple lines");
            }

            if ((ExpConstants)node.Id == ExpConstants.ADD && node.TryGetSingleChild(ExpConstants.CONS, out var cons)) {
                StringBuilder value = new StringBuilder();
                while (true) {
                    if (cons.TryGetFirstChild(ExpConstants.CHAR, out var @char) && 
                        @char.TryGetSingleChild(ExpConstants.DIGIT, out var digit) &&
                        digit is Token token) {
                        value.Append(token.Image);
                    } else {
                        value = null;
                    }

                    if (value == null || cons.GetChildCount() == 1) {
                        break;
                    }
                    cons = cons[1];
                }

                if (value != null) {
                    return value.ToString();
                }
            }

            //TODO support cons of numbers

            // Grammatica lines and columns are indexed from 1, inclusive on ends
            var line = lines[node.GetStartLine() - 1];
            var text = line.Substring(node.GetStartColumn() - 1, node.GetEndColumn() + 1 - node.GetStartColumn());
            return $"Error! Can't evaluate '{text}'";
        }

        public static string Evaluate(string input) {
            var node = new ExpParser(new StringReader(input.TrailingNewline())).Parse();

            var calc = new Calculator(input);
            calc.Analyze(node);

            return calc.output.ToString();
        }
    }

    public static class Extensions
    {
        public static bool TryGetFirstChild(this Node node, ExpConstants id, out Node child) {
            if (node.GetChildCount() >= 1 && (ExpConstants)node[0].Id == id) {
                child = node[0];
                return true;
            }
            child = default;
            return false;
        }

        public static bool TryGetSingleChild(this Node node, ExpConstants id, out Node child) => 
            node.TryGetFirstChild(id, out child) && node.GetChildCount() == 1;

        public static string TrailingNewline(this string s) =>
            s.EndsWith(Environment.NewLine) ? s : s + Environment.NewLine;
    }
}
