using System;
using System.Linq;
using System.Text.RegularExpressions;
using Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngineTests
{
  [TestClass]
  public class ExpressionTests
  {
    static readonly Regex splitLines = new Regex("; *", RegexOptions.Compiled);
    static Expression Parse(string input) => ExprExtractor.GetExpression(splitLines.Replace(input, Environment.NewLine)).Single();

    static readonly Expression[] kinds = new[] {
            Parse("a"),
            Parse("0"),
            Parse("a0"),
            Parse("0a"),
            Parse("a+a"),
            Parse("a+b"),
            Parse("a*a"),
            Parse("a:a"),
            Parse("0=0"),
            Parse("0=0+1"),
            Parse("0=0; |1=b"),
        };

    [TestMethod]
    public void HashCode() {
      for (var i = 0; i < kinds.Length; ++i) {
        for (var j = 0; j < kinds.Length; j++) {
          if (i == j) {
            Assert.AreEqual(kinds[i].GetHashCode(), kinds[j].GetHashCode(), $"{i}=={j}");
          } else {
            Assert.AreNotEqual(kinds[i].GetHashCode(), kinds[j].GetHashCode(), $"{i}!={j}");
          }
        }
      }
    }

    [TestMethod]
    public void Equality() {
      for (var i = 0; i < kinds.Length; ++i) {
        for (var j = 0; j < kinds.Length; j++) {
          if (i == j) {
            Assert.AreEqual(kinds[i], kinds[j], $"{i}=={j}");
          } else {
            Assert.AreNotEqual(kinds[i], kinds[j], $"{i}!={j}");
          }
        }
      }

      Assert.AreEqual(Parse("bab"), Parse("aba"));
      Assert.AreEqual(Parse("0 * x = x + y"), Parse("0 * a = a + x"));
    }

    [TestMethod]
    public void ArityThree() {
      var x1z = new[] {
        new Character('x'),
        new Character('1'),
        new Character('z')
      };
      Assert.AreEqual("x1z", new Func(":", x1z).ToString());
      Assert.AreEqual("+(x, 1, z)", new Func("+", x1z).ToString());
    }
  }
}
