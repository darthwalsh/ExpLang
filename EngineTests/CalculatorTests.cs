using Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EngineTests
{
  [TestClass]
  public class CalculatorTests
  {
#pragma warning disable IDE0022 // Use expression body for methods

    [TestMethod]
    public void Number() {
      Assert.AreEqual("1", Evaluate("1"));
      Assert.AreEqual(@"1
3", Evaluate(@"1; 3"));
    }

    [Ignore] // TODO implement proper variable, not varchar
    [TestMethod]
    public void MultiDigit() {
      Assert.AreEqual("123", Evaluate("123"));
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
    public void UnknownVariable() {
      Assert.IsTrue(GetEvaluation(@"1x").Error);
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

    [TestMethod]
    public void DuplicateVariable() {
      Assert.IsTrue(GetEvaluation(@"1+1=a; |a=1; |a=2; 1+1").Error);
      Assert.AreEqual(@"3", Evaluate(@"1+1=a; |b=3; |a=b; |a=3; 1+1"));
    }

    [TestMethod]
    public void BackwardsLookup() {
      Assert.AreEqual(@"1", Evaluate(@"1+1=2; 8+8=a; |1+a=2; 8+8"));
      Assert.AreEqual(@"5", Evaluate(@"1 + 1 = 2
2 + 1 = 3
3 + 1 = 4
4 + 1 = 5

a + b = c
| a + 1 = x
| y + 1 = b
| x + y = c

2+3"));
    }

    [TestMethod]
    public void Op() {
      Assert.AreEqual(@"3", Evaluate(@"2+1=3; 2*1=2; 2+1"));
      Assert.AreEqual(@"2", Evaluate(@"2+1=3; 2*1=2; 2*1"));
    }

    [TestMethod]
    public void RecursionDoesntStackOverflow() {
      Assert.IsTrue(GetEvaluation(@"1+1=a; |a=1+1; 1+1").Error);
    }

    [TestMethod]
    public void DifferentEval() {
      Assert.IsTrue(GetEvaluation(@"0/0=0; 0/0=1; 0/0").Error);
      Assert.AreEqual(@"2", Evaluate(@"1+1=2; 1+1=2; 1+1"));

      Assert.IsTrue(GetEvaluation(@"1*0=0; 2*0=0; 1^1=a; |a*0=0; 1^1").Error);
      Assert.AreEqual(@"1", Evaluate(@"1*0=0; 1*0=0; 1^1=a; |a*0=0; 1^1"));
    }

    [TestMethod]
    public void MultiUnknown() {
      Assert.AreEqual(@"1", Evaluate(@"1+2=3; 1&1=a; |a+b=3; 1&1"));
      Assert.AreEqual(@"2", Evaluate(@"1+2=3; 1&1=b; |a+b=3; 1&1"));
     
      Assert.IsTrue(GetEvaluation(@"1+2=3; 2+1=3; 1&1=b; |a+b=3; 1&1").Error);
    }

    [TestMethod]
    public void MultiUnknownOnlyOnePossible() {
      Assert.AreEqual(@"1", Evaluate(@"1+1=2; 1&1=a; |a+a=2; 1&1"));
      Assert.AreEqual(@"1", Evaluate(@"1+1=2; 0+2=2; 1&1=a; |a+a=2; 1&1"));
      Assert.AreEqual(@"1", Evaluate(@"0+2=2; 1+1=2; 1&1=a; |a+a=2; 1&1"));
    }

    [TestMethod]
    public void MultipathAddtion() {
      Assert.AreEqual(@"3", Evaluate(@"1 + 1 = 2
2 + 1 = 3
1 + 2 = 3

a + b = c
| a + 1 = x
| y + 1 = b
| x + y = c

1+2"));
    }

    [TestMethod]
    public void TestExplained() {
      Assert.AreEqual(@"3
    Solving 1 + 2...
    Applied rule a + b = c Implies a = 1, b = 2, c = 3
    | a + 1 = x Implies x = 2
        Solving 1 + 1 = |
        Applied rule 1 + 1 = 2
    | y + 1 = b Implies y = 1
        Solving  + 1 = 2
        Applied rule 1 + 1 = 2
    | x + y = c Implies c = 3
        Solving 2 + 1 = ¤
        Applied rule 2 + 1 = 3
    Redundant rule 1 + 2 = 3
", EvaluateExplained(@"1 + 1 = 2
2 + 1 = 3

a + b = c
| a + 1 = x
| y + 1 = b
| x + y = c

1 + 2 = 3

1+2"));
    }

    [TestMethod]
    public void TestExplainedNegative() {
      Assert.AreEqual(@"3
    Solving 1 + 2...
    Applied rule a + b = c Implies a = 1, b = 2, c = 3
    | a + 1 = x Implies x = 2
        Solving 1 + 1 = |
        Applied rule 1 + 1 = 2
        Rules that didn't help:
            Rule a = a didn't help because 1 + 1 is Func but a is Variable
            Rule 2 + 1 = 3 didn't help because Digit 1 is not 2
            Rule a + b = c didn't help because a + 1 is Func but x is Variable
            Rule 1 + 2 = 3 didn't help because Digit 1 is not 2
    | y + 1 = b Implies y = 1
        Solving  + 1 = 2
        Applied rule 1 + 1 = 2
        Rules that didn't help:
            Rule a = a didn't help because  + 1 is Func but a is Variable
            Rule 2 + 1 = 3 didn't help because Digit 2 is not 3
            Rule a + b = c didn't help because y + 1 is Func but b is Variable
            Rule 1 + 2 = 3 didn't help because Digit 1 is not 2
    | x + y = c Implies c = 3
        Solving 2 + 1 = ¤
        Applied rule 2 + 1 = 3
        Rules that didn't help:
            Rule a = a didn't help because 2 + 1 is Func but a is Variable
            Rule 1 + 1 = 2 didn't help because Digit 2 is not 1
            Rule a + b = c didn't help because a + 1 is Func but x is Variable
            Rule 1 + 2 = 3 didn't help because Digit 2 is not 1
    Redundant rule 1 + 2 = 3
    Rules that didn't help:
        Rule a = a didn't help because 1 + 2 is Func but a is Variable
        Rule 1 + 1 = 2 didn't help because Digit 2 is not 1
        Rule 2 + 1 = 3 didn't help because Digit 1 is not 2
", EvaluateExplained(@"1 + 1 = 2
2 + 1 = 3

a + b = c
| a + 1 = x
| y + 1 = b
| x + y = c

1 + 2 = 3

1+2", true));
    }

#pragma warning restore IDE0022 // Use expression body for methods

    static string Evaluate(string input) {
      var eval = GetEvaluation(input);
      Assert.IsFalse(eval.Error);
      return string.Join(Environment.NewLine, eval.Results.Select(r => r.Line));
    }

    static string EvaluateExplained(string input, bool explainNegative = false) {
      var eval = GetEvaluation(input);
      Assert.IsFalse(eval.Error);

      var builder = new StringBuilder();
      foreach (var r in eval.Results) {
        Print(r, 0, builder, explainNegative);
      }
      return builder.ToString();
    }

    static void Print(Result result, int indent, StringBuilder builder, bool explainNegative) {
      if (!explainNegative && result.Line == "Rules that didn't help:") {
        return;
      }
      builder.AppendLine(new string(' ', indent) + result.Line);
      foreach (var c in result.Children) {
        Print(c, indent + 4, builder, explainNegative);
      }
    }

    static readonly Regex splitLines = new Regex("; *", RegexOptions.Compiled);
    static Evalutation GetEvaluation(string input) => new Evalutation(splitLines.Replace(input, Environment.NewLine));
  }
}
