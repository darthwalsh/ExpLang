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
        public void Expression() {
            CollectionAssert.AreEqual(
                new[] {
                    new Token { Type = TokenType.Number, Start = 1, Length = 2 },
                    new Token { Type = TokenType.Op, Start = 4, Length = 1 },
                    new Token { Type = TokenType.Variable, Start = 6, Length = 1 },
                }, new Lexer().Lex(" 12 + A ").ToList());

            CollectionAssert.AreEqual(
                new[] {
                    new Token { Type = TokenType.Number, Start = 0, Length = 2 },
                    new Token { Type = TokenType.Op, Start = 2, Length = 1 },
                    new Token { Type = TokenType.Variable, Start = 3, Length = 1 },
                }, new Lexer().Lex("12+A").ToList());

            CollectionAssert.AreEqual(
                new[] {
                    new Token { Type = TokenType.Char, Start = 0, Length = 1 },
                    new Token { Type = TokenType.Equals, Start = 2, Length = 1 },
                    new Token { Type = TokenType.Char, Start = 4, Length = 1 },
                    new Token { Type = TokenType.Newline, Start = 5, Length = 2 },
                    new Token { Type = TokenType.Where, Start = 7, Length = 1 },
                    new Token { Type = TokenType.Char, Start = 9, Length = 1 },
                    new Token { Type = TokenType.Newline, Start = 10, Length = 4 },
                }, new Lexer().Lex(@"a = b
| c

").ToList());
        }
        [TestMethod]
        public void Ops() {
            CollectionAssert.AreEqual(
                new[] {
                    new Token { Type = TokenType.Op, Start = 0, Length = 1 },
                    new Token { Type = TokenType.Op, Start = 1, Length = 1 },
                    new Token { Type = TokenType.Op, Start = 2, Length = 1 },
                    new Token { Type = TokenType.Op, Start = 3, Length = 1 },
                }, new Lexer().Lex("+-*/").ToList());
        }

        [TestMethod]
        public void Variable() {
            CollectionAssert.AreEqual(
                new[] {
                    new Token { Type = TokenType.Variable, Start = 0, Length = 1 },
                    new Token { Type = TokenType.Number, Start = 1, Length = 1 },
                }, new Lexer().Lex("A0").ToList());
        }

        [TestMethod]
        public void CharVar() {
            CollectionAssert.AreEqual(
                new[] {
                    new Token { Type = TokenType.Char, Start = 0, Length = 1 },
                    new Token { Type = TokenType.Number, Start = 1, Length = 1 },
                }, new Lexer().Lex("a0").ToList());
        }

        [TestMethod]
        public void Unknown() {
            Assert.ThrowsException<Exception>(
                () => new Lexer().Lex("\0").ToList(),
                "Unknown token starting at: \0");

            Assert.ThrowsException<Exception>(
                () => new Lexer().Lex("\0 0123456789").ToList(),
                "Unknown token starting at: \0 012345678");
        }
    }
}
