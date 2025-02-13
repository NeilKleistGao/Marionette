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

  class Block: IEvaluatable<Value, Environment> {
    private List<IEvaluatable<Value, Environment>> statements = new List<IEvaluatable<Value,Environment>>();
    private IEvaluatable<Value, Environment> result;

    public Block(List<IEvaluatable<Value, Environment>> statements, IEvaluatable<Value, Environment> result) {
      this.statements = statements;
      this.result = result;
    }

    public Value Evaluate(Environment env) {
      var nest = new Environment(env);
      foreach (var s in statements) {
        s.Evaluate(nest);
      }

      return result.Evaluate(nest);
    }
  }

  public class EvalList: ResultList<Value, Environment> {
    public EvalList() {}

    private Value EvaluateDefine(Environment env) {
      if (list.Count < 3) {
        throw new RuntimeException("Expect `(define name (stmt)* value).`", Loc);
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

        var statements = new List<IEvaluatable<Value, Environment>>();
        for (int i = 2; i < list.Count - 1; ++i) {
          statements.Add(list[i]);
        }

        var fname = symbols[0].Name;
        symbols.RemoveAt(0);
        var bindings = symbols.Map(sym => sym.Name);
        env.Add(fname, new Closure(bindings.ToArray(), env, new Block(statements, list[list.Count - 1])));
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

    private Value EvaluateIf(Environment env) {
      if (list.Count != 4) {
        throw new RuntimeException("Expect `(if condition res alt).`", Loc);
      }
        
      var cond = list[1].Evaluate(env);
      if (cond is LiteralValue<bool> boolCond) {
        if (boolCond.Value) {
          return list[2].Evaluate(env);
        }
        else {
          return list[3].Evaluate(env);
        }
      }
      else {
        throw new RuntimeException("Expect boolean condition.`", Loc);
      }
    }

    private Value EvaluateCond(Environment env) {
      if (list.Count < 2) {
        throw new RuntimeException("Expect `(cond (condition res)*).`", Loc);
      }

      for (int i = 1; i < list.Count; ++i) {
        if (list[i] is EvalList checkList && checkList.list.Count == 2) {
          if (checkList.list[0] is EvalSymbol syme && syme.IsElse) {
            return checkList.list[1].Evaluate(env);
          }
          else {
            var cond = checkList.list[0].Evaluate(env);
            if (cond is LiteralValue<bool> boolCond) {
              if (boolCond.Value) {
                return checkList.list[1].Evaluate(env);
              }
            }
            else {
              throw new RuntimeException("Expect boolean condition.`", checkList.Loc);
            }
          }
        }
        else if (list[i] is LocatableData d) {
          throw new RuntimeException("Expect `(condition res).`", d.Loc);
        }
        // Impossible
      }

      throw new RuntimeException("Unexhausted cond expression.`", Loc);
    }

    public override Value Evaluate(Environment env) {
      if (list.IsEmpty()) {
        throw new RuntimeException("Empty invocation.", Loc);
      }

      Debug.LogFormat("Evalueate {0}", ToString());
      if (list[0] is EvalSymbol sym && sym.IsDefine) {
        return EvaluateDefine(env);
      }
      else if (list[0] is EvalSymbol symi && symi.IsIf) {
        return EvaluateIf(env);
      }
      else if (list[0] is EvalSymbol symc && symc.IsCond) {
        return EvaluateCond(env);
      }
      if (list[0] is EvalSymbol syme && syme.IsElse) {
        throw new RuntimeException("Unexpected else expression.", Loc);
      }
      else {
        var fun = list[0].Evaluate(env);
        if (fun is LazyClosure lazyClosure) {
          var values = new List<Value>();
          for (int i = 1; i < list.Count; ++i) {
            values.Add(new LazyValue(list[i], env));
          }
          return lazyClosure.Evaluate(values.ToArray(), Loc);
        }
        else if (fun is Closure closure) {
          var values = new List<Value>();
          for (int i = 1; i < list.Count; ++i) {
            values.Add(list[i].Evaluate(env));
          }
          return closure.Evaluate(values.ToArray(), Loc);
        }
        else {
          throw new RuntimeException(string.Format("{0} is not a function.", fun.Show()), Loc);
        }
      }
    }
  }

  public class EvalSymbol: Symbol<Value, Environment> {
    public override Value Evaluate(Environment env){
      var res = env.GetOrElse(name, n => throw new RuntimeException(string.Format("name not found: {0}", n), Loc));
      if (res is LazyValue lazyValue) {
        return lazyValue.EvaluateAndCache();
      }

      return res;
    }

    public EvalSymbol(string name) : base(name) {}
  }

  public class EvalEOF: EOF<Value, Environment> {
    public override Value Evaluate(Environment env) {
      return new UnitValue();
    }
  }
} // namespace Marionette.Runtime
