using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MathExp
{
    public enum TokenType
    {
        Skipped = -1,
        Unknown = 0,

        Number,
        Op,
        Variable,
    }

    public struct Token : IEquatable<Token>
    {
        public TokenType Type;
        public int Start;
        public int Length;

        public override bool Equals(object obj) => obj is Token && Equals((Token)obj);
        public override int GetHashCode() => Type.GetHashCode() ^ (Start.GetHashCode() * 7) ^ (Length.GetHashCode() * 11);
        public bool Equals(Token other) => Type == other.Type && Start == other.Start && Length == other.Length;
    }

    public class Lexer
    {
        static IEnumerable<(Regex regex, TokenType type)> grammar = new[] {
            (@"\G\s+", TokenType.Skipped),
            (@"\G\d+", TokenType.Number),
            (@"\G(\+|-|\*|/)", TokenType.Op),
            (@"\G\$\w+", TokenType.Variable),
        }.Select(g => (new Regex(g.Item1, RegexOptions.Compiled), g.Item2)).ToList();

        public IEnumerable<Token> Lex(string s) {
            for (var i = 0; i < s.Length;) {
                var (match, type) = grammar.Select(g => (match: g.regex.Match(s, i), g.type)).FirstOrDefault(maybe => maybe.match.Success);
                if (match == null) {
                    throw new Exception($"Unknown token starting at: {s.Substring(i, Math.Min(10, s.Length - i))}");
                }

                if (type != TokenType.Skipped) {
                    yield return new Token { Type = type, Start = match.Index, Length = match.Length };
                }

                i = match.Index + match.Length;
            }
        }
    }
}
