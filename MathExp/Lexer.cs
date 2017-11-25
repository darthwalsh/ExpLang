using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathExp
{
    public enum TokenType
    {
        number,
        op,
        variable,
    }

    public struct Token : IEquatable<Token>
    {
        public TokenType type;
        public int start;
        public int end;

        public override bool Equals(object obj) => obj is Token && Equals((Token)obj);
        public override int GetHashCode() => type.GetHashCode() ^ (start.GetHashCode() * 7) ^ (end.GetHashCode() * 11);
        public bool Equals(Token other) => type == other.type && start == other.start && end == other.end;
    }

    public class Lexer
    {
        public IEnumerable<Token> Lex(string s) {
            throw null;
        }
    }
}
