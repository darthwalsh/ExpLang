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
            Assert.AreEqual("1", Calculator.Evaluate("1").Trim());
            Assert.AreEqual("123", Calculator.Evaluate("123").Trim());
            Assert.AreEqual(@"1
3", Calculator.Evaluate(@"1
3").Trim());
        }

        // TODO test TryEvaluate("X") and "1X"
    }
}
