﻿using System;
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

        internal Calculator(string input) {
            lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
        }

        internal string Output => output.ToString();
        internal bool Error { get; private set; }

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

            if (TryGetLiteralDigits(node, out var digits)) {
                return digits;
            }

            foreach (var fact in facts) {
                if (TrySolve(node, fact, true, out var result)) {
                    return result;
                }
                if (TrySolve(node, fact, false, out result)) {
                    return result;
                }
            }

            // Grammatica lines and columns are indexed from 1, inclusive on ends
            var line = lines[node.GetStartLine() - 1];
            var text = line.Substring(node.GetStartColumn() - 1, node.GetEndColumn() + 1 - node.GetStartColumn());

            Error = true;
            return $"Error! Can't evaluate '{text}'";
        }

        static bool TryGetLiteralDigits(Node node, out string digits) {
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
                    digits = value.ToString();
                    return true;
                }
            }
            digits = default;
            return false;
        }

        bool TrySolve(Node node, Node fact, bool left, out string result) {
            var pattern = fact[left ? 0 : 2];

            var matcher = new Matcher(pattern, node);
            if (matcher.Matches) {
                result = GetValue(fact[left ? 2 : 0]);
                return true;
            }

            // TODO don't ignore the where clause

            result = default;
            return false;
        }

        class Matcher
        {
            public Matcher(Node x, Node y) {
                Solve(x, y);
            }

            public bool Matches { get; private set; } = true;

            void Solve(Node x, Node y) {
                if (x.Count != y.Count || x.Id != y.Id) {
                    Matches = false;
                    return;
                }
                for (var i = 0; i < x.Count; ++i) {
                    Solve(x[i], y[i]);
                }
                switch ((ExpConstants)x.Id) {
                    case ExpConstants.DIGIT:
                        if (((Token)x).Image != ((Token)y).Image) {
                            Matches = false;
                            return;
                        }
                        break;
                    case ExpConstants.VARIABLE:
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
            var node = new ExpParser(new StringReader(input.TrailingNewline())).Parse();

            var calc = new Calculator(input);
            calc.Analyze(node);

            Result = calc.Output;
            Error = calc.Error;
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
