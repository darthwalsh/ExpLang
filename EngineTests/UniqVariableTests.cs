using Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EngineTests
{
  [TestClass]
  public class UniqVariableTests
  {
#pragma warning disable IDE0022 // Use expression body for methods

    [TestMethod]
    public void NextTests() {
      Assert.AreEqual('a', Next("1", 'a'));
      Assert.AreEqual('A', Next("1", 'A'));
      Assert.AreEqual('b', Next("a", 'a'));
      Assert.AreEqual('a', Next("A", 'a'));
      Assert.AreEqual('a', Next("b", 'a'));
      Assert.AreEqual('d', Next("abc", 'a'));    
    }

    [TestMethod]
    public void State() {
      var uniq = new UniqVariable(ExprExtractor.GetExpression("c").Single());
      Assert.AreEqual('b', uniq.Next('b'));
      Assert.AreEqual('a', uniq.Next('b'));
      Assert.AreEqual('d', uniq.Next('b'));
    }

    [TestMethod]
    public void Errors() {
      var abcxyz = "abcdefghijklmnopqrstuvwxyz";

      Assert.ThrowsException<InvalidOperationException>(() => Next(abcxyz.ToUpper(), 'A'));
      Assert.ThrowsException<InvalidOperationException>(() => Next(abcxyz, 'a'));

      var bcxyz = abcxyz.Substring(1);
      var uniq = new UniqVariable(ExprExtractor.GetExpression(bcxyz + abcxyz.ToUpper()).Single());
      Assert.AreEqual('a', uniq.Next('a'));
      Assert.ThrowsException<InvalidOperationException>(() => uniq.Next('a'));
      
      uniq = new UniqVariable(ExprExtractor.GetExpression(bcxyz).Single());
      Assert.AreEqual('a', uniq.Next('b'));
      Assert.ThrowsException<InvalidOperationException>(() => uniq.Next('a'));
    }

#pragma warning restore IDE0022 // Use expression body for methods

    static char Next(string vars, char next) {
      var e = ExprExtractor.GetExpression(vars).Single();
      var uniq = new UniqVariable(e);
      return uniq.Next(next);
    }
  }
}
