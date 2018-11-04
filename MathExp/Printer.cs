﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MathExp
{
    public class Printer
    {
        string GetSeparator(string s, RangeToken last, RangeToken next) {
            var lastT = last.Type;
            var nextT = next.Type;
            if (lastT == TokenType.Char || nextT == TokenType.Char) {
                var other = lastT == TokenType.Char ? nextT : lastT;
                switch (other) {
                    case TokenType.Char:
                    case TokenType.Variable:
                    case TokenType.Number:
                        return "";
                }
            }
            if (lastT == TokenType.Newline || nextT == TokenType.Newline) {
                return "";
            }
            if (lastT == TokenType.Parens && s[last.Start] == '(') {
                return "";
            }
            if (nextT == TokenType.Parens && s[next.Start] == ')') {
                return "";
            }
            return " ";
        }

        public string PrettyPrint(string s, IEnumerable<RangeToken> tokens) {
            var builder = new StringBuilder();

            var last = new RangeToken { Type = TokenType.Newline };
            foreach (var t in tokens) {
                builder.Append(GetSeparator(s, last, t));
                builder.Append(s, t.Start, t.Length);
                last = t;
            }

            return builder.ToString();
        }

        public static string Prettify(string s) => new Printer().PrettyPrint(s, new Lexer().Lex(s));
    }
}
