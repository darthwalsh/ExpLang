using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Environment = System.Collections.Generic.Dictionary<char, string>;

namespace Engine
{
  public class Calculator
  {
    static readonly Fact reflexive = new Fact(new Func("=", new Character('a'), new Character('a')), new Func[0]);
    static readonly ICollection<Fact> axioms = new List<Fact> { reflexive }.AsReadOnly();
    readonly List<Fact> facts = new List<Fact>(axioms);
    readonly List<Expression> expressions = new List<Expression>();
    readonly List<Result> results = new List<Result>();

    // MAYBE Have a Dictionary<Expression, Env> that also works as a cache, so evaluations aren't repeated
    readonly Dictionary<Expression, bool> recursionCheck = new Dictionary<Expression, bool>();

    // MAYBE Could probably remove the counting check because of the Dictionary, but need to think about that
    const int max_recursion = 25;
    int matcherRecursiveCalls;

    internal Calculator(IEnumerable<Expression> expressions) {
      foreach (var e in expressions) {
        if (e.Id == ExpressionType.Fact) {
          facts.Add((Fact)e);
        } else {
          this.expressions.Add(e);
        }
      }
    }

    internal IEnumerable<Result> Results => results;
    internal bool Error { get; private set; }

    static bool TryGetLiteralDigits(Expression e, Environment env, out string digits) {
      var builder = new StringBuilder();
      if (TryGetLiteralDigits(e, env, builder)) {
        digits = builder.ToString();
        return true;
      }
      digits = default;
      return false;
    }

    static bool TryGetLiteralDigits(Expression e, Environment env, StringBuilder digits) {
      if (e is Character c) {
        if (c.Id == ExpressionType.Digit) {
          digits.Append(c.Value);
          return true;
        } else if (c.Id == ExpressionType.Variable && env.TryGetValue(c.Value, out var value)) {
          digits.Append(value);
          return true;
        }
      }

      if (e is Func cons && cons.Name == ":") {
        return TryGetLiteralDigits(cons.Left, env, digits) &&
            TryGetLiteralDigits(cons.Right, env, digits);
      }

      return false;
    }

    public void Evaluate() {
      foreach (var e in expressions) {
        // e = X, solve for X
        var next = new UniqVariable(e).Next('X');
        var temp = new Func("=", e, new Character(next));
        if (TryResolveFact(temp, out var env, out var result) && env.TryGetValue(next, out var value)) {
          // TODO decide if this logic could be avoided if top-level expressions didn't solve for X
          //  if (result.Children.Count > 0) {
          //  var solving = result.Children[0].Line;
          //  result.Children[0].Line = solving.Substring(0, solving.Length - 4) + "...";
          //}
          result.Line = value;
          results.Add(result);
        } else {
          Error = true;
          var failureMessage = new Result($"Error! Can't evaluate '{e}'");
          failureMessage.Children.Add(result);
          results.Add(failureMessage);
        }
      }
    }

    bool TryResolveFact(Func test, out Environment env, out Result result) {
      if (recursionCheck.TryGetValue(test, out var completed) && !completed) {
        // MAYBE if completed, could try to cache and return env?
        env = default;
        result = new Result("Trying to solve the exact same problem without any new progress");
        return false;
      }
      recursionCheck[test] = false;

      // Equality is implicitly symmetric, even though other ops aren't
      // MAYBE there could be an implicit rule A = B |B = A that allow for symmetry and solving?
      //   (Assuming there is a variable B that can contain function expressions)
      var success = TryResolveFactImpl(test, out env, out result);
      if (!success) {
        var testReverse = new Func(test.Name, test.Right, test.Left);
        success = TryResolveFactImpl(testReverse, out env, out var resultReverse);
        if (success) {
          result = resultReverse;
        } else {
          result.Line = $"Solving {test} got {result.Line}";
          resultReverse.Line = $"Solving {testReverse} got {resultReverse.Line}";
          
          var bothResult = new Result("Rules that didn't help:");
          bothResult.Children.Add(result);
          bothResult.Children.Add(resultReverse);
          result = bothResult;
        }
      }

      recursionCheck[test] = true;
      return success;
    }

    // TODO TODO TODO add some tests
    /// When solving i.e. 1 + 2 = x
    /// env = { x: 3 }
    /// result = $ToBeFilledInLater
    ///   Applied rule a + b = c Implies a = 1, b = 2, c = 3
    ///     | a + 1 = x Implies x = 2
    ///       Solving 1 + 1 = x
    ///       ...
    ///     | y + 1 = b Implies y = 1
    ///       ...
    ///     ...
    bool TryResolveFactImpl(Func test, out Environment env, out Result result) {
      env = default;
      result = null;
      var failures = new Result("Rules that didn't help:");
      foreach (var fact in facts) {
        var matcher = new Matcher(test, fact, this);
        if (matcher.Matches) {
          if (env != null) {
            if (!EnvironmentEqual(test, env, matcher.Env, out var diff)) {
              var diffResult = new Result($"Different results produced for variable {diff}");
              result.Line = $"Found {diff}={env[diff]}";

              var otherResult = GetResult(test, fact, matcher);
              otherResult.Line = $"Found {diff}={matcher.Env[diff]}";

              diffResult.Children.Add(result);
              diffResult.Children.Add(otherResult);
              result = diffResult;
              return false;
            } else {
              result.Children.Add(new Result($"Redundant rule {fact.Equality}"));
              continue;
            }
          }
          env = matcher.Env;
          result = GetResult(test, fact, matcher);
          // Don't return true until end of loop, to ensure there aren't different results between rules
        } else {
          failures.Children.Add(new Result($"Rule {fact.Equality} didn't help because {matcher.Reason}"));
        }
      }

      if (result == null) {
        result = failures;
      } else if (failures.Children.Any()) {
        result.Children.Add(failures);
      }

      return env != null;
    }

    static Result GetResult(Func test, Fact fact, Matcher matcher) {
      var result = new Result("");
      result.Children.Add(new Result($"Solving {test}"));

      var factVars = VariableFinder.Find(fact.Equality).Concat(VariableFinder.Find(test)).ToList();
      var implies = string.Join(", ", factVars.Select(v => $"{v} = {matcher.Env[v]}"));
      var line = $"Applied rule {fact.Equality}";
      if (factVars.Any()) {
        line += $" Implies {implies}";
      }
      result.Children.Add(new Result(line));

      var defined = new HashSet<char>(matcher.InitialEnv.Keys);
      foreach (var (where, r) in matcher.Solution) {
        var whereVars = VariableFinder.Find(where);
        var defs = string.Join(", ", whereVars.Where(defined.Add).Select(v => $"{v} = {matcher.Env[v]}"));

        r.Line = $"| {where} {(defs.Any() ? "Implies " + defs : "")}";
        result.Children.Add(r);
      }

      return result;
    }

    // Only check variables in test, ignoring variables that might have been defined in fact
    static bool EnvironmentEqual(Func test, Environment x, Environment y, out char diff) {
      diff = VariableFinder.Find(test).FirstOrDefault(c =>
        x.TryGetValue(c, out var xv) && y.TryGetValue(c, out var yv) && xv != yv);
      return diff == default;
    }

    class VariableInliner : ExpressionVisitor
    {
      public Environment Env { get; set; }

      protected override Character VisitVariable(Character e) =>
          Env.TryGetValue(e.Value, out var value) ? new Character(value.Single()) : e;
    }

    internal class VariableFinder : ExpressionVisitor
    {
      readonly List<char> vars = new List<char>();
      readonly HashSet<char> seen = new HashSet<char>();

      public static ICollection<char> Find(Expression e) {
        var finder = new VariableFinder();
        finder.Visit(e);
        return finder.vars;
      }

      protected override Character VisitVariable(Character e) {
        if (seen.Add(e.Value)) {
          vars.Add(e.Value);
        }
        return base.VisitVariable(e);
      }
    }

    class Matcher
    {
      readonly Calculator calc;
      bool? matches = true; // null for possibly, depending on vars
      string reason;
      string unReason;
      public readonly List<(Expression, Result)> Solution = new List<(Expression, Result)>();

      public Matcher(Expression test, Fact fact, Calculator calc) {
        if (calc.matcherRecursiveCalls++ > max_recursion) {
          throw new StackOverflowException("Recursion too deep"); // doesn't actually crash the .NET VM
        }

        // Fact might contain the same variables as test, so rewrite fact as needed
        // TODO include the rewrites in the Results output
        fact = (Fact)new VariableRewriter(new UniqVariable(test)).Visit(fact);

        Expression equality = fact.Equality;
        IEnumerable<Func> wheres = fact.Wheres;

        try {
          this.calc = calc;

          // Do a pretest to try to assign as many variables as possible first, then check Solve() again later
          Solve(test, equality);
          if (matches.HasValue && !matches.Value) {
            return;
          }
          InitialEnv = new Environment(Env);
          matches = true;

          var toResolve = wheres.ToList();

          while (toResolve.Any()) {
            HandleWhere(toResolve);
          }

          Solve(test, equality);
        } finally {
          calc.matcherRecursiveCalls--;
        }
      }

      public Environment InitialEnv { get; private set; }
      public Environment Env { get; private set; } = new Environment();
      public bool Matches => matches.HasValue && matches.Value;

      public string Reason => reason ?? unReason;

      void HandleWhere(List<Func> toResolve) {
        var min = int.MaxValue;
        var minI = -1;
        for (var i = 0; i < toResolve.Count; ++i) {
          var unknowns = Unknowns(toResolve[i]);
          if (unknowns < min) {
            min = unknowns;
            minI = i;
          }
        }

        var where = toResolve[minI];
        toResolve.RemoveAt(minI);

        var inlined = new VariableInliner {
          Env = Env
        }.Visit(where);
        if (calc.TryResolveFact((Func)inlined, out var resolvedEnv, out var result)) {
          Solution.Add((where, result));

          var expectedVars = VariableFinder.Find(inlined);
          foreach (var (key, value) in resolvedEnv) {
            if (expectedVars.Contains(key)) {
              SetOrAdd(key, value);
            }
          }
          return;
        }

        // TODO the error resul should get returned somehow
        Solve(where.Left, where.Right);
      }

      void Solve(Expression x, Expression y) {
        if (x is Character xc && y is Character yc) {
          if (x.Id == ExpressionType.Variable && TryGetLiteralDigits(y, Env, out var digits) && digits.Length == 1) {
            SetOrAdd(xc.Value, digits);
            return;
          }
          if (y.Id == ExpressionType.Variable && TryGetLiteralDigits(x, Env, out digits) && digits.Length == 1) {
            SetOrAdd(yc.Value, digits);
            return;
          }
        }

        if (x.Id != y.Id) {
          NoMatch($"{x} is {x.Id.GetName()} but {y} is {y.Id.GetName()}");
          return;
        }

        if (x.Children.Count() != y.Children.Count()) {
          NoMatch($"{x} has {x.Children.Count()} children but {y} has {y.Children.Count()} children");
          return;
        }

        if (x.Id == ExpressionType.Func && x is Func xFunc && y is Func yFunc && xFunc.Name != yFunc.Name) {
          NoMatch($"{x} is function {xFunc.Name} but {y} is function {yFunc.Name}");
          return;
        }

        foreach (var (xx, yy) in x.Children.Zip(y.Children, (xx, yy) => (xx, yy))) {
          Solve(xx, yy);
        }

        switch (x.Id) {
          case ExpressionType.Digit:
            if (((Character)x).Value != ((Character)y).Value) {
              NoMatch($"Digit {x} is not {y}");
              return;
            }
            break;
          case ExpressionType.Variable:
            UnMatch($"Unknown variable {x} matched with unknown variable {y}");
            return;
        }
      }

      void NoMatch(string r) {
        matches = false;
        if (reason == null) {
          reason = r;
        }
      }

      void UnMatch(string r) {
        if (matches != false) {
          matches = null;
        }
        if (unReason == null) {
          unReason = r;
        }
      }

      void SetOrAdd(char name, string digits) {
        if (Env.TryGetValue(name, out var value)) {
          if (value != digits) {
            NoMatch($"Variable {name} was already {value} and cannot be {digits}");
          }
        } else {
          Env.Add(name, digits);
        }
      }

      int Unknowns(Expression e) {
        var current = e.Id == ExpressionType.Variable && !Env.ContainsKey(((Character)e).Value);
        return (current ? 1 : 0) + e.Children.Sum(Unknowns);
      }
    }
  }

  [DebuggerDisplay("{ToString(true),nq}")]
  public class Result
  {
    // MAYBE remove setter, add varargs in ctor
    public List<Result> Children { get; set; } = new List<Result>();
    public string Line { get; set; }
    public Result(string line) {
      Line = line;
    }

    public override string ToString() => Line + (Children.Any() ? "..." : "");

    public string ToString(bool explainNegative) {      
      var builder = new StringBuilder();
      Print(this, 0, builder, explainNegative);
      return builder.ToString();
    }

    public static void Print(Result result, int indent, StringBuilder builder, bool exhaustive) {
      if (!exhaustive && result.Line == "Rules that didn't help:") {
        return;
      }
      builder.AppendLine(new string(' ', indent) + result.Line);
      foreach (var c in result.Children) {
        Print(c, indent + 4, builder, exhaustive);
      }
    }
  }

  public class Evalutation
  {
    public IEnumerable<Result> Results { get; set; }
    public bool Error { get; set; }

    public Evalutation() { }

    public Evalutation(string input) {
      var expression = ExprExtractor.GetExpression(input);

      var calc = new Calculator(expression);
      calc.Evaluate();

      Results = calc.Results;
      Error = calc.Error;
    }
  }

  public static class Extensions
  {
    public static string TrailingNewline(this string s) =>
        s.EndsWith(System.Environment.NewLine) ? s : s + System.Environment.NewLine;

    public static string GetName<T>(this T e) where T : struct => Enum.GetName(typeof(T), e);
  }

  // Useful for producing unique variables that don't conflict with the input expression.
  // Maintains the case of the output variables to match input.
  public class UniqVariable : IVariableReplacement
  {
    readonly HashSet<char> used;
    char upper = 'A';
    char lower = 'a';

    public UniqVariable(Expression e) {
      used = new HashSet<char>(Calculator.VariableFinder.Find(e));
    }

    public char Next(char replacing) {
      if (used.Add(replacing)) {
        return replacing;
      }

      if (char.IsUpper(replacing)) {
        while (used.Contains(upper)) { ++upper; }
        if (upper > 'Z') {
          throw new InvalidOperationException("Too many vars");
        }

        var res = upper;
        used.Add(upper);
        ++upper;
        return res;
      }
      while (used.Contains(lower)) { ++lower; }
      if (lower > 'z') {
        throw new InvalidOperationException("Too many vars");
      }

      var ans = lower;
      used.Add(lower);
      ++lower;
      return ans;
    }
  }
}
