using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Generated;
using PerCederberg.Grammatica.Runtime;

namespace Engine
{
  public class ExprExtractor : ExpAnalyzer
  {
    internal ExprExtractor() {
    }

    readonly List<Expression> expressions = new List<Expression>();

    public override Node ExitFacts(Production node) {
      var isExpression = (ExpConstants)node[0][1].Id == ExpConstants.NEWLINE;

      if (isExpression) {
        expressions.Add((Expression)node[0].Values[0]);
      } else {
        var wheres = new List<Func>();
        for (var i = 1; i < node.Count && (ExpConstants)node[i].Id == ExpConstants.WHERES; ++i) {
          wheres.Add((Func)node[i].Values[0]);
        }
        expressions.Add(new Fact((Func)node[0].Values[0], wheres));
      }

      return node;
    }

    public override Node ExitWheres(Production node) {
      node.AddValue(node[1].Values[0]);
      return node;
    }

    public override Node ExitEqualOrExpr(Production node) {
      var isExpression = (ExpConstants)node[1].Id == ExpConstants.NEWLINE;
      if (isExpression) {
        node.AddValue(node[0].Values[0]);
      } else {
        node.AddValue(new Func("=", (Expression)node[0].Values[0], (Expression)node[2].Values[0]));
      }
      return node;
    }

    public override Node ExitEqual(Production node) {
      node.AddValue(new Func("=", (Expression)node[0].Values[0], (Expression)node[2].Values[0]));
      return node;
    }

    public override Node ExitArith(Production node) {
      if (node.Count > 1) {
        node.AddValue(new Func((string)node[1].Values[0], (Expression)node[0].Values[0], (Expression)node[2].Values[0]));
      } else {
        node.AddValue(node[0].Values[0]);
      }
      return node;
    }

    public override Node ExitCons(Production node) {
      if (node.Count > 1) {
        node.AddValue(new Func(":", (Expression)node[0].Values[0], (Expression)node[node.Count - 1].Values[0]));
      } else {
        node.AddValue(node[0].Values[0]);
      }
      return node;
    }

    public override Node ExitChar(Production node) {
      node.AddValue(node[0].Values[0]);
      return node;
    }

    public override Node ExitOp(Token node) {
      node.AddValue(node.Image);
      return node;
    }

    public override Node ExitVariable(Token node) {
      node.AddValue(new Character(node.Image.Single()));
      return node;
    }

    public override Node ExitDigit(Token node) {
      node.AddValue(new Character(node.Image.Single()));
      return node;
    }

    public static IEnumerable<Expression> GetExpression(string input) {
      var node = new ExpParser(new StringReader(input.TrailingNewline())).Parse();

      var extractor = new ExprExtractor();
      extractor.Analyze(node);

      // ExitFacts called backwards to line order because of recursive definition
      extractor.expressions.Reverse();

      return extractor.expressions;
    }
  }
}
