using System;
using System.IO;
using MathExp.Generated;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerCederberg.Grammatica.Runtime;

namespace MathExpTests
{
    [TestClass]
    public class ParserTests {
        static TestContext TestContext;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext) {
            TestContext = testContext;
        }

        [TestMethod]
        public void Sanity() {
            var node = Parse("1");
            TestContext.WriteLine(node.Printed());

            node = Find(node, ExpConstants.DIGIT);
            var child = (Token)node;

            Assert.AreEqual(ExpConstants.DIGIT, (ExpConstants)child.Id, "Child Id");
            Assert.AreEqual("1", child.Image, "Child Value");
        }

        [TestMethod]
        public void Cons() {
            var cons = Parse("11");
            Find(cons, ExpConstants.DIGIT);

            cons = Find(Parse("1:111"), ExpConstants.CONS);
            Find(cons.GetChildAt(2), ExpConstants.DIGIT);
        }

        [TestMethod]
        public void Add() {
            var sum = Parse("1+1");
            Assert.IsTrue(TryFind(sum, ExpConstants.DIGIT, out _));

            Assert.ThrowsException<ParserLogException>(() => Parse("1+1+1"));
        }

        static Node Parse(string s) => new ExpParser(new StringReader(s)).Parse();

        static Node Find(Node n, ExpConstants id) {
            Assert.IsTrue(TryFind(n, id, out var found));
            return found;
        }

        static bool TryFind(Node n, ExpConstants id, out Node found) {
            if (n.Id == (int)id) {
                found = n;
                return true;
            }
            for (var i = 0; i < n.GetChildCount(); ++i) {
                if (TryFind(n.GetChildAt(i), id, out found)) {
                    return true;
                }
            }
            found = default;
            return false;
        }
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
