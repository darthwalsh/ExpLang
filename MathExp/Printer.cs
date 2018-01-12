using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MathExp
{
    public class Printer
    {
        string GetSeparator(string s, Token last, Token next) {
            var lastT = last.Type;
            var nextT = next.Type;
            if (lastT == TokenType.CharVar || nextT == TokenType.CharVar) {
                var other = lastT == TokenType.CharVar ? nextT : lastT;
                switch (other) {
                    case TokenType.CharVar:
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

        public string PrettyPrint(string s, IEnumerable<Token> tokens) {
            var builder = new StringBuilder();

            var last = new Token { Type = TokenType.Newline };
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
