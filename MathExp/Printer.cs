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
        public string PrettyPrint(string s, IEnumerable<Token> tokens) {
            var builder = new StringBuilder();

            var fence = "";
            foreach (var t in tokens) {
                builder.Append(fence);
                builder.Append(s, t.Start, t.Length);

                switch (t.Type) {
                    case TokenType.CharVar:
                        fence = "";
                        break;
                    default:
                        fence = " ";
                        break;
                }
            }

            return builder.ToString();
        }

        public static string Prettify(string s) => new Printer().PrettyPrint(s, new Lexer().Lex(s));
    }
}
