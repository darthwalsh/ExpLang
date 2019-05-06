using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Engine
{
    public enum ExpressionType
    {
        Digit,
        Variable,
        Fact,
        Func,
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
        readonly IEnumerable<Expression> wheres;

        public Fact(Expression equality, IEnumerable<Expression> wheres)
            : base(ExpressionType.Fact) {
            Equality = equality;
            this.wheres = wheres;
        }

        public Expression Equality { get; private set; }

        public override IEnumerable<Expression> Children => new[] { Equality }.Concat(wheres);

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
}
