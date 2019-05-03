using Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.RegularExpressions;

namespace EngineTests
{
    [TestClass]
    public class CalculatorTests {
        static TestContext context;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext) => context = testContext;

        [TestMethod]
        public void Number() {
            Assert.AreEqual("1", Evaluate("1"));
            Assert.AreEqual("123", Evaluate("123"));
            Assert.AreEqual(@"1
3", Evaluate(@"1; 3"));
        }

        [TestMethod]
        public void Solve() {
            Assert.AreEqual(@"2", Evaluate(@"1+1=2; 1+1"));
            Assert.AreEqual(@"2", Evaluate(@"0+0=0; 2=1+1; 1+1"));
            Assert.AreEqual(@"5", Evaluate(@"2+2=5; 2+2"));
        }

        static Regex splitLines = new Regex("; *", RegexOptions.Compiled);
        static string Evaluate(string input) {
            input = splitLines.Replace(input, Environment.NewLine);
            var eval = new Evalutation(input);
            Assert.IsFalse(eval.Error);
            return eval.Result.Trim();
        }

        // TODO test TryEvaluate("X") and "1X"
        // TODO test with \n and with \r\n
    }
}
