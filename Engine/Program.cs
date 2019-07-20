using System;
using System.IO;

namespace Engine
{
  class Program
  {
    static void Main(string[] args) {
      var input = @"1
1+2

1 + 2 = 3

1+2=3
|123=a";

      if (args.Length == 1) {
        input = File.ReadAllText(args[0]);
      }

      var expressions = ExprExtractor.GetExpression(input);

      Console.WriteLine(string.Join(Environment.NewLine + Environment.NewLine, expressions));
    }
  }
}
