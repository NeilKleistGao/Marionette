using Marionette.Parser;
using Marionette.Utils;

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

  public class EvalList: ResultList<Value, Environment> {
    public EvalList() {}

    public override Value Evaluate(Environment env){
      if (list.IsEmpty()) {
        throw new Exception("Empty invocation.");
      }

      var fun = list[0].Evaluate(env); // TODO: 
      if (fun is Closure closure) {
        list.RemoveAt(0);
        return closure.Evaluate(list.Map(x => x.Evaluate(env)).ToArray());
      }
      else {
        throw new Exception(string.Format("{0} is not a function.", fun.Show()));
      }
    }
  }

  public class EvalSymbol: Symbol<Value, Environment> {
    public override Value Evaluate(Environment env){
      return env.GetOrElse(name, n => throw new Exception(string.Format("name not found: {0}", n)));
    }

    public EvalSymbol(string name) : base(name) {}
  }

  public class EvalEOF: EOF<Value, Environment> {
    public override Value Evaluate(Environment env) {
      return new UnitValue();
    }
  }
} // namespace Marionette.Runtime
