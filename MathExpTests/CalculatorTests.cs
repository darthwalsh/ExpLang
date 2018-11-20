using MathExp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MathExpTests
{
    [TestClass]
    public class CalculatorTests {
        static TestContext context;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext) => context = testContext;

        [TestMethod]
        public void Sanity() {
            Assert.AreEqual("1", Calculator.Evaluate("1"));
        }
    }
}
