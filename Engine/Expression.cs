using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Engine
{
  public enum ExpressionType
  {
    Fact,
    Func,
    Digit,
    Variable,
  }

  public abstract class Expression : IEquatable<Expression>
  {
    static readonly SimpleEquality simpleEquality = new SimpleEquality();

    public Expression(ExpressionType id) {
      Id = id;
    }

    public ExpressionType Id { get; protected set; }

    public abstract IEnumerable<Expression> Children { get; }

    // Variables are normalized before finding hash or equality, so a+b=b*0 is the same as x+y=y*0
    Expression NormalizeVariables() => new VariableRewriter { Uniq = new UniqVariable() }.Visit(this);
    public override bool Equals(object obj) => Equals(obj as Expression);
    public bool Equals(Expression other) {
      if (other == null) {
        return false;
      }
      return NormalizeVariables().SimpleEquals(other.NormalizeVariables());
    }

    public override int GetHashCode() => NormalizeVariables().SimpleHash();

    protected virtual int SimpleHash() {
      var hash = new HashCode();
      hash.Add(Id);
      foreach (var c in Children) {
        hash.Add(c.SimpleHash());
      }
      return hash.ToHashCode();
    }

    protected virtual bool SimpleEquals(Expression other) => Id == other.Id && Children.SequenceEqual(other.Children, simpleEquality);

    public override string ToString() => $"{Enum.GetName(typeof(ExpressionType), Id)} -- should implement ToString for {GetType().Name}";

    class SimpleEquality : IEqualityComparer<Expression>
    {
      public bool Equals(Expression x, Expression y) => x.SimpleEquals(y);
      public int GetHashCode(Expression obj) => obj.SimpleHash();
    }
  }

  public class Fact : Expression
  {
    public Fact(Func equality, IEnumerable<Func> wheres)
        : base(ExpressionType.Fact) {
      Equality = equality;
      Wheres = wheres;
    }

    public IEnumerable<Func> Wheres { get; private set; }

    public Func Equality { get; private set; }

    public override IEnumerable<Expression> Children => new[] { Equality }.Concat(Wheres);

    public override string ToString() => string.Join(Environment.NewLine + "| ", Children);
  }

  public class Func : Expression
  {
    readonly ReadOnlyCollection<Expression> args;

    public Func(string name, params Expression[] args)
        : base(ExpressionType.Func) {
      Name = name;
      this.args = new List<Expression>(args).AsReadOnly();
    }

    public string Name { get; private set; }
    public override IEnumerable<Expression> Children => args;

    public Expression Arg0 => args[0];
    public Expression Arg1 => args[1];
    public Expression Arg2 => args[2];
    public Expression Left => Arg0;
    public Expression Right => Arg1;

    protected override bool SimpleEquals(Expression other) => base.SimpleEquals(other) && Name == ((Func)other).Name;
    protected override int SimpleHash() => HashCode.Combine(Name, base.SimpleHash());

    public override string ToString() {
      switch (Name) {
        case "=":
        case "+":
        case ":":
          if (args.Count != 2) {
            throw new InvalidOperationException("Expected arity 2");
          }
          var op = Name == ":" ? "" : $" {Name} ";
          return $"{args[0]}{op}{args[1]}";
        default:
          return $"{Name}({string.Join(", ", args)})";
      }
    }
  }

  public class Character : Expression
  {
    public Character(char c, ExpressionType id)
        : base(id) {
      Value = c;
    }

    public override IEnumerable<Expression> Children => Enumerable.Empty<Expression>();

    public char Value { get; private set; }

    protected override bool SimpleEquals(Expression other) => base.SimpleEquals(other) && Value == ((Character)other).Value;
    protected override int SimpleHash() => HashCode.Combine(Value, base.SimpleHash());

    public override string ToString() => Value.ToString();
  }

  public abstract class ExpressionVisitor
  {
    public Expression Visit(Expression e) {
      switch (e.Id) {
        case ExpressionType.Fact: return Visit((Fact)e);
        case ExpressionType.Func: return Visit((Func)e);
        case ExpressionType.Digit: return VisitDigit((Character)e);
        case ExpressionType.Variable: return VisitVariable((Character)e);
        default: throw new NotSupportedException(Enum.GetName(typeof(ExpressionType), e.Id));
      }
    }

    protected virtual Fact Visit(Fact e) => new Fact(Visit(e.Equality), e.Wheres.Select(Visit));
    protected virtual Func Visit(Func e) => new Func(e.Name, e.Children.Select(Visit).ToArray());
    protected virtual Character VisitDigit(Character e) => e;
    protected virtual Character VisitVariable(Character e) => e;
  }

  class VariableRewriter : ExpressionVisitor
  {
    public UniqVariable Uniq { get; set; }

    public Dictionary<char, char> Rewrites { get; private set; } = new Dictionary<char, char>();

    protected override Character VisitVariable(Character e) {
      if (!Rewrites.TryGetValue(e.Value, out var rewrite)) {
        rewrite = Uniq.Next;
        Rewrites.Add(e.Value, rewrite);
      }
      return new Character(rewrite, ExpressionType.Variable);
    }
  }

  class UniqVariable
  {
    // If variable names could be strings, doing something like $0, $1 would be simpler...
    char c = (char)Math.Max('z', 'Z');
    public char Next => ++c;
  }
}
