using System.IO;
using Engine;
using Engine.Generated;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerCederberg.Grammatica.Runtime;

namespace EngineTests
{
  [TestClass]
  public class ParserTests
  {
    static TestContext context;

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext) => context = testContext;

    [TestMethod]
    public void Sanity() {
      var node = Parse("1");
      context.WriteLine(node.Printed());

      node = Find(node, ExpConstants.DIGIT);
      var child = (Token)node;

      Assert.AreEqual(ExpConstants.DIGIT, (ExpConstants)child.Id, "Child Id");
      Assert.AreEqual("1", child.Image, "Child Value");
    }

    [TestMethod]
    public void Cons() {
      var cons = Parse("11");
      Find(cons, ExpConstants.DIGIT);

      cons = Find(Parse("1:111=a"), ExpConstants.CONS);
      Find(cons[2], ExpConstants.DIGIT);
    }

    [TestMethod]
    public void Add() {
      var sum = Parse("1+1");
      Find(sum, ExpConstants.DIGIT);

      Assert.ThrowsException<ParserLogException>(() => Parse("1+1+1"));
    }

    [TestMethod]
    public void Multiply() {
      var or = Parse("1*1");
      Find(or, ExpConstants.OP);
    }

    [TestMethod]
    public void Fact() {
      var facts = Parse(@"a=1+1
                |b=2

                c=3");
      facts = Find(facts, ExpConstants.FACTS);
      Find(facts[1], ExpConstants.WHERES);

      Parse(@"a=1+1
                b=2
                |2=3f
                |2=3g
                c=3");

      Parse(@"a=1+1
                b=2
                1+1
                2=1");
    }

    static Node Parse(string s) => new ExpParser(new StringReader(s.TrailingNewline())).Parse();

    static Node Find(Node n, ExpConstants id) {
      Assert.IsTrue(TryFind(n, id, out var found));
      return found;
    }

    static bool TryFind(Node n, ExpConstants id, out Node found) {
      if ((ExpConstants)n.Id == id) {
        found = n;
        return true;
      }
      for (var i = 0; i < n.GetChildCount(); ++i) {
        if (TryFind(n[i], id, out found)) {
          return true;
        }
      }
      found = default;
      return false;
    }
  }

  public static class Extensions
  {
    public static string Printed(this Node node) {
      using (var writer = new StringWriter()) {
        node.PrintTo(writer);
        return writer.ToString();
      }
    }
  }
}
