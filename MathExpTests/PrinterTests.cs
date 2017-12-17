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
            Verify(" 12 + $a ", "12 + $a");
        }

        [TestMethod]
        public void Ops() {
            Verify("+-*/", "+ - * /");
            Verify("1+2+", "1 + 2 +");
        }

        [TestMethod]
        public void Variable() {
            Verify("$a0", "$a0");
            Verify("$a0$a1", "$a0 $a1");
        }

        [TestMethod]
        public void CharVar() {
            Verify("_a0", "_a0");
            Verify("1+_a0+2", "1 + _a0 + 2");
        }

        static void Verify(string input, string pretty) {
            var actual = Printer.Prettify(input);
            Assert.AreEqual(pretty, actual);
        }
    }
}
