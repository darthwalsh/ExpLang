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

        [TestMethod]
        public void SimpleVariable() {
            Assert.AreEqual(@"3", Evaluate(@"1+1=a; |a=3; 1+1"));
        }

        [TestMethod]
        public void MultiVariable() {
            Assert.AreEqual(@"3", Evaluate(@"1+1=a; |a=b; |b=3; 1+1"));
            Assert.AreEqual(@"3", Evaluate(@"1+1=a; |b=3; |a=b; 1+1"));
        }

        [TestMethod]
        public void LowerVariableMatchesSingle() {
            Assert.IsTrue(GetEvaluation(@"1+1=a; |a=33; 1+1").Error);
        }

        static readonly Regex splitLines = new Regex("; *", RegexOptions.Compiled);
        static string Evaluate(string input) {
            var eval = GetEvaluation(input);
            Assert.IsFalse(eval.Error);
            return eval.Result.Trim();
        }

        static Evalutation GetEvaluation(string input) => new Evalutation(splitLines.Replace(input, Environment.NewLine));

        // TODO test TryEvaluate("X") and "1X"
        // TODO test with \n and with \r\n
    }
}
