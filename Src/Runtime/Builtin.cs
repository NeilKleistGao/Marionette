using Marionette.Parser;

namespace Marionette.Runtime {
  public class BinaryOperator : IEvaluatable<Value, Environment> {
    private string name;

    private Value Add(Value lhs, Value rhs) {
      if (lhs is LiteralValue<int> i1 && rhs is LiteralValue<int> i2) {
        return new LiteralValue<int>(i1.Value + i2.Value);
      }
      else if (lhs is LiteralValue<int> i && rhs is LiteralValue<double> d) {
        return new LiteralValue<double>(i.Value + d.Value);
      }
      else if (lhs is LiteralValue<double> d2 && rhs is LiteralValue<int> i3) {
        return new LiteralValue<double>(i3.Value + d2.Value);
      }
      else if (lhs is LiteralValue<double> d3 && rhs is LiteralValue<double> d4) {
        return new LiteralValue<double>(d3.Value + d4.Value);
      }
      else if (lhs is LiteralValue<string> s1 && rhs is LiteralValue<string> s2) {
        return new LiteralValue<string>(s1.Value + s2.Value);
      }

      throw new Exception(string.Format("cannot add {0} and {1}.", lhs.Show(), rhs.Show()));
    }

    public Value Evaluate(Environment env) {
      var lhs = new EvalSymbol("lhs").Evaluate(env);
      var rhs = new EvalSymbol("rhs").Evaluate(env);
      if (name == "+") {
        return Add(lhs, rhs);
      }
      // TODO: other

      throw new Exception(string.Format("{0} cannot be used as a builtin symbol here.", name));
    }

    public BinaryOperator(string name) {
      this.name = name;
    }
  }
} // namespace Marionette.Runtime
