using System;
using System.Linq;
using MathExp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MathExpTests
{
    [TestClass]
    public class PrinterTests
    {
        [TestMethod]
        public void Expression() {
            Verify("", "");
            Verify(" 12 ", "12");
            Verify(" a + A ", "a + A");
            Verify("((1+a)+3)", "((1 + a) + 3)");
            Verify(@"ab + cd = ef
| b + d = 1f
| (a + c) + 1 = e

Aa + Bb = Cc
| a + b = c
| A + B = C", @"ab + cd = ef
| b + d = 1f
| (a + c) + 1 = e

Aa + Bb = Cc
| a + b = c
| A + B = C");
        }

        [TestMethod]
        public void Ops() {
            Verify("+-*/", "+ - * /");
            Verify("1+2+", "1 + 2 +");
        }

        [TestMethod]
        public void Variable() {
            Verify("A", "A");
            Verify("A0", "A 0");
        }

        [TestMethod]
        public void CharVar() {
            Verify("a0", "a0");
            Verify("1+a0+2", "1 + a0 + 2");
        }

        static void Verify(string input, string pretty) {
            var actual = Printer.Prettify(input);
            Assert.AreEqual(pretty, actual);
        }
    }
}
