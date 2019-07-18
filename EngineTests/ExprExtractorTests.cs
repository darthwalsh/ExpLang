using System;
using Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngineTests
{
    [TestClass]
    public class ExprExtractorTests
    {
        [TestMethod]
        public void Parsing() {
            IsSame("1");
            IsSame("123");
            IsSame("1 + 2");
            IsSame("a");
            IsSame("1 = 2");
            IsSame(@"1 = 2
1 = 1");
            IsSame(@"1 = 2
| 1 = 1");
        }

        static void IsSame(string input) => Assert.AreEqual(input, string.Join(Environment.NewLine, ExprExtractor.GetExpression(input)));
    }
}
