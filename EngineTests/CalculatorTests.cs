using Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngineTests
{
    [TestClass]
    public class CalculatorTests {
        static TestContext context;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext) => context = testContext;

        [TestMethod]
        public void Number() {
            Assert.AreEqual("1", Evaluate("1").Trim());
            Assert.AreEqual("123", Evaluate("123").Trim());
            Assert.AreEqual(@"1
3", Evaluate(@"1
3").Trim());
        }

        static string Evaluate(string input) {
            var eval = new Evalutation(input);
            Assert.IsFalse(eval.Error);
            return eval.Result;
        }

        // TODO test TryEvaluate("X") and "1X"
    }
}
