using System;
using System.Linq;
using MathExp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MathExpTests
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void Lexes() {
            CollectionAssert.AreEqual(
                new[] {
                    new Token { type = TokenType.number, start = 1, end = 3},
                    new Token {type = TokenType.number, start = 3, end = 4 },
                }, new Lexer().Lex(" 12 +").ToList());
        }
    }
}
