using Marionette.Parser;
using Marionette.Utils;

namespace Marionette.Runtime {
  public class BinaryOperator : IEvaluatable<Value, Environment> {
    protected string name;
    protected const string LHS_NAME = "lhs";
    protected const string RHS_NAME = "rhs";

    private Value Add(Value lhs, Value rhs) {
      if (lhs is LiteralValue<int> i1 && rhs is LiteralValue<int> i2) {
        return new LiteralValue<int>(i1.Value + i2.Value);
      }
      else if (lhs is LiteralValue<int> i && rhs is LiteralValue<double> d) {
        return new LiteralValue<double>(i.Value + d.Value);
      }
      else if (lhs is LiteralValue<double> d2 && rhs is LiteralValue<int> i3) {
        return new LiteralValue<double>(d2.Value + i3.Value);
      }
      else if (lhs is LiteralValue<double> d3 && rhs is LiteralValue<double> d4) {
        return new LiteralValue<double>(d3.Value + d4.Value);
      }
      else if (lhs is LiteralValue<string> s1 && rhs is LiteralValue<string> s2) {
        return new LiteralValue<string>(s1.Value + s2.Value);
      }

      throw new RuntimeException(string.Format("cannot add {0} and {1}.", lhs.Show(), rhs.Show()));
    }

    private Value Sub(Value lhs, Value rhs) {
      if (lhs is LiteralValue<int> i1 && rhs is LiteralValue<int> i2) {
        return new LiteralValue<int>(i1.Value - i2.Value);
      }
      else if (lhs is LiteralValue<int> i && rhs is LiteralValue<double> d) {
        return new LiteralValue<double>(i.Value - d.Value);
      }
      else if (lhs is LiteralValue<double> d2 && rhs is LiteralValue<int> i3) {
        return new LiteralValue<double>(d2.Value - i3.Value);
      }
      else if (lhs is LiteralValue<double> d3 && rhs is LiteralValue<double> d4) {
        return new LiteralValue<double>(d3.Value - d4.Value);
      }

      throw new RuntimeException(string.Format("cannot subtract {1} from {0}.", lhs.Show(), rhs.Show()));
    }

    private Value Mult(Value lhs, Value rhs) {
      if (lhs is LiteralValue<int> i1 && rhs is LiteralValue<int> i2) {
        return new LiteralValue<int>(i1.Value * i2.Value);
      }
      else if (lhs is LiteralValue<int> i && rhs is LiteralValue<double> d) {
        return new LiteralValue<double>(i.Value * d.Value);
      }
      else if (lhs is LiteralValue<double> d2 && rhs is LiteralValue<int> i3) {
        return new LiteralValue<double>(d2.Value * i3.Value);
      }
      else if (lhs is LiteralValue<double> d3 && rhs is LiteralValue<double> d4) {
        return new LiteralValue<double>(d3.Value * d4.Value);
      }

      throw new RuntimeException(string.Format("cannot multiply {0} by {1}.", lhs.Show(), rhs.Show()));
    }

    private Value Div(Value lhs, Value rhs) {
      if (lhs is LiteralValue<int> i1 && rhs is LiteralValue<int> i2) {
        double res = (double)i1.Value / i2.Value;
        if (double.IsInteger(res)) {
          return new LiteralValue<int>((int)res);
        }
        else {
          return new LiteralValue<double>(res);
        }
      }
      else if (lhs is LiteralValue<int> i && rhs is LiteralValue<double> d) {
        return new LiteralValue<double>(i.Value / d.Value);
      }
      else if (lhs is LiteralValue<double> d2 && rhs is LiteralValue<int> i3) {
        return new LiteralValue<double>(d2.Value / i3.Value);
      }
      else if (lhs is LiteralValue<double> d3 && rhs is LiteralValue<double> d4) {
        return new LiteralValue<double>(d3.Value / d4.Value);
      }

      throw new RuntimeException(string.Format("cannot divide {0} by {1}.", lhs.Show(), rhs.Show()));
    }

    private Value Mod(Value lhs, Value rhs) {
      if (lhs is LiteralValue<int> i1 && rhs is LiteralValue<int> i2) {
        return new LiteralValue<int>(i1.Value % i2.Value);
      }

      throw new RuntimeException(string.Format("cannot compute {0} modulo {1}.", lhs.Show(), rhs.Show()));
    }

    private Value Compare(Value lhs, Value rhs, Func<int, bool> pred) {
      int? res = null;
      if (lhs is LiteralValue<int> i1 && rhs is LiteralValue<int> i2) {
        res = i1.Value.CompareTo(i2.Value);
      }
      else if (lhs is LiteralValue<int> i && rhs is LiteralValue<double> d) {
        res = d.Value.CompareTo(i.Value);
      }
      else if (lhs is LiteralValue<double> d2 && rhs is LiteralValue<int> i3) {
        res = d2.Value.CompareTo(i3.Value);
      }
      else if (lhs is LiteralValue<double> d3 && rhs is LiteralValue<double> d4) {
        res = d3.Value.CompareTo(d4.Value);
      }
      else if (lhs is LiteralValue<string> s1 && rhs is LiteralValue<string> s2) {
        res = s1.Value.CompareTo(s2.Value);
      }
      else if (lhs is LiteralValue<bool> b1 && rhs is LiteralValue<bool> b2) {
        res = b1.Value.CompareTo(b2.Value);
      }

      if (res is int r) {
        return pred(r) ? new LiteralValue<bool>(true) : new LiteralValue<bool>(false);
      }
      throw new RuntimeException(string.Format("cannot compare {1} with {0}.", lhs.Show(), rhs.Show()));
    }

    public virtual Value Evaluate(Environment env) {
      var lhs = new EvalSymbol(LHS_NAME).Evaluate(env);
      var rhs = new EvalSymbol(RHS_NAME).Evaluate(env);
      switch (name) {
        case "+":
          return Add(lhs, rhs);
        case "-":
          return Sub(lhs, rhs);
        case "*":
          return Mult(lhs, rhs);
        case "/":
          return Div(lhs, rhs);
        case "%":
          return Mod(lhs, rhs);
        case ">":
          return Compare(lhs, rhs, (x) => x > 0); 
        case ">=":
          return Compare(lhs, rhs, (x) => x >= 0);
        case "<":
          return Compare(lhs, rhs, (x) => x < 0); 
        case "<=":
          return Compare(lhs, rhs, (x) => x <= 0);
        case "=":
          return Compare(lhs, rhs, (x) => x == 0);
        // TODO: other
      }

      throw new RuntimeException(string.Format("{0} cannot be used as a builtin symbol here.", name));
    }

    protected BinaryOperator(string name) {
      this.name = name;
    }

    public static void CreateOperator(Environment env, string name) {
      env.Add(name, new Closure([LHS_NAME, RHS_NAME], env, new BinaryOperator(name)));
    }
  }

  public class ShortCircuitOperator : BinaryOperator {
    private ShortCircuitOperator(string name) : base(name) {}

    public override Value Evaluate(Environment env) {
      var lhs = new EvalSymbol(LHS_NAME).Evaluate(env);
      var rhs = new EvalSymbol(RHS_NAME);
      var getBool = (Value? v) => {
        if (v is LiteralValue<bool> b) {
          return b.Value;
        }
        else {
          throw new RuntimeException(string.Format("{0} is not a boolean expression.", v?.Show()));
        }
      };
      
      switch (name) {
        case "and":
          return new LiteralValue<bool>(getBool(lhs) ? getBool(rhs.Evaluate(env)) : false);
        case "or":
          return new LiteralValue<bool>(getBool(lhs) ? true : getBool(rhs.Evaluate(env)));
      }

      throw new RuntimeException(string.Format("{0} cannot be used as a builtin symbol here.", name));
    }

    public static new void CreateOperator(Environment env, string name) {
      env.Add(name, new LazyClosure([LHS_NAME, RHS_NAME], env, new ShortCircuitOperator(name)));
    }
  }

  public class UnaryOperator : IEvaluatable<Value, Environment> {
    private string name;
    private const string OPRAND_NAME = "value";

    private UnaryOperator(string name) {
      this.name = name;
    }

    public Value Evaluate(Environment env) {
      var value = new EvalSymbol(OPRAND_NAME).Evaluate(env);
      switch (name) {
        case "neg":
          if (value is LiteralValue<int> i) {
            return new LiteralValue<int>(-i.Value);
          }
          else if (value is LiteralValue<double> f) {
            return new LiteralValue<double>(-f.Value);
          }
          else {
            throw new RuntimeException(string.Format("{0} is not a numeric expression.", value?.Show()));
          }
        case "not":
          if (value is LiteralValue<bool> b) { // TODO: refactor
            return new LiteralValue<bool>(!b.Value);
          }
          else {
            throw new RuntimeException(string.Format("{0} is not a boolean expression.", value?.Show()));
          }
        case "display":
          Console.WriteLine(value.ToString());
          return new UnitValue();
      }

      throw new RuntimeException(string.Format("{0} cannot be used as a builtin symbol here.", name));
    }

    public static void CreateOperator(Environment env, string name) {
      env.Add(name, new Closure([OPRAND_NAME], env, new UnaryOperator(name)));
    }
  }
} // namespace Marionette.Runtime
