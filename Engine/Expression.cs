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

    public abstract class Expression
    {
        public Expression(ExpressionType id) {
            Id = id;
        }

        public ExpressionType Id { get; protected set; }

        public abstract IEnumerable<Expression> Children { get; }

        public override string ToString() => $"{Enum.GetName(typeof(ExpressionType), Id)} -- should implement ToString for {GetType().Name}";
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
}
