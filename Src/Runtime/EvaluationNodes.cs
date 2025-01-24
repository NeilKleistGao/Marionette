using Marionette.Parser;

namespace Marionette.Runtime {
  public class EvalIntLit: IntLiteral<Value, Environment> {
    public override Value Evaluate(Environment env) {
      return new LiteralValue<int>(Value);
    }

    public EvalIntLit(int i) {
      Value = i;
    }
  }

  public class EvalNumberLit: NumberLiteral<Value, Environment> {
    public override Value Evaluate(Environment env) {
      return new LiteralValue<double>(Value);
    }

    public EvalNumberLit(double d) {
      Value = d;
    } 
  }

  public class EvalBoolLit: BoolLiteral<Value, Environment> {
    public override Value Evaluate(Environment env) {
      return new LiteralValue<bool>(Value);
    }

    public EvalBoolLit(bool b) {
      Value = b;
    } 
  }

  public class EvalStringLit: StringLiteral<Value, Environment> {
    public override Value Evaluate(Environment env) {
      return new LiteralValue<string>(Value??"");
    }

    public EvalStringLit(string s) {
      Value = s;
    } 
  }

  public class EvalEOF: EOF<Value, Environment> {
    public override Value Evaluate(Environment env) {
      return new UnitValue();
    }
  }
} // namespace Marionette.Runtime
