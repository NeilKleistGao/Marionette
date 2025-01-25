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
        throw new RuntimeException("Empty invocation.", Loc);
      }

      if (list[0] is EvalSymbol sym && sym.IsDefine) {
        if (list.Count != 3) {
          throw new RuntimeException("Expect `(define name value).`", Loc);
        }
        if (list[1] is EvalSymbol name) {
          var value = list[2].Evaluate(env);
          env.Add(name.Name, value);
          return new UnitValue();
        }
        else if (list[1] is EvalList lst) {
          var symbols = lst.AsSymbolList();
          if (symbols.IsEmpty()) {
            var loc = Location.Empty();
            if (list[1] is LocatableData ld) {
              loc = ld.Loc;
            }
            throw new RuntimeException("Empty function declaration.", loc);
          }

          var fname = symbols[0].Name;
          symbols.RemoveAt(0);
          var bindings = symbols.Map(sym => sym.Name);
          env.Add(fname, new Closure(bindings.ToArray(), env, list[2]));
          return new UnitValue();
        }
        else {
          var loc = Location.Empty();
          if (list[1] is LocatableData ld) {
            loc = ld.Loc;
          }
          throw new RuntimeException("Expect symbol or parameter list.", loc);
        }
      }
      else {
        var fun = list[0].Evaluate(env);
        if (fun is Closure closure) {
          list.RemoveAt(0);
          return closure.Evaluate(list.Map(x => x.Evaluate(env)).ToArray(), Loc);
        }
        else {
          throw new RuntimeException(string.Format("{0} is not a function.", fun.Show()), Loc);
        }
      }
    }
  }

  public class EvalSymbol: Symbol<Value, Environment> {
    public override Value Evaluate(Environment env){
      return env.GetOrElse(name, n => throw new RuntimeException(string.Format("name not found: {0}", n), Loc));
    }

    public EvalSymbol(string name) : base(name) {}
  }

  public class EvalEOF: EOF<Value, Environment> {
    public override Value Evaluate(Environment env) {
      return new UnitValue();
    }
  }
} // namespace Marionette.Runtime
