using Marionette.Parser;

namespace Marionette.Runtime {
  public class EvalIntLit : IntLiteral<Value, Environment> {
    public override Value Evaluate(Environment env) {
      return new LiteralValue<int>(Value);
    }

    public EvalIntLit(int i) {
      this.Value = i;
    }
  }

  public class EvalNumberLit : NumberLiteral<Value, Environment> {
    public override Value Evaluate(Environment env) {
      return new LiteralValue<double>(Value);
    }

    public EvalNumberLit(double d) {
      this.Value = d;
    } 
  }
} // namespace Marionette.Runtime
