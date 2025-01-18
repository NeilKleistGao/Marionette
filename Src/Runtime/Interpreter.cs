using Marionette.Parser;
using Marionette.Utils;

namespace Marionette.Runtime {
  public class Environment {
    private Dictionary<string, Value> env = new Dictionary<string, Value>();

    private Environment() {
    }

    public static Environment CreateEmpty() {
      return new Environment();
    }
  }

  public class Interpreter {

    private Environment env = Environment.CreateEmpty();

    private ResultList<Value, Environment> AllocateList() {
      throw new NotImplementedException();
    }

    private Literal<Value, Environment> AllocateLiteral(string text) {
      if (text.StartsWith('"')) {
        // TODO: string
      }
      else if (text == "true") {
        // TODO: bool
      }
      else if (text == "false") {
        // TODO: bool
      }
      else {
        int i; double d;
        if (int.TryParse(text, out i)) {
          return new EvalIntLit(i);
        }
        else if (double.TryParse(text, out d)) {
          return new EvalNumberLit(d);
        }
      }

      throw new Exception("Cannot parse literal " + text);
    }

    private Symbol<Value, Environment> AllocateSymbol(string name) {
      throw new NotImplementedException();
    }

    private EOF<Value, Environment> AllocateEOF() {
      return new EvalEOF();
    }

    public Result Interpret(string code) {
      var parser = new Parser<Value, Environment>(code, AllocateList, AllocateLiteral, AllocateSymbol, AllocateEOF);
      var errors = new List<Diagnosis>();
      Value result = new UnitValue();

      while (parser.MoveNext()) {
        var node = parser.Current;
        if (node is ParseError<Value, Environment> err) {
          errors.Add(err.ToDiagnosis());
        }
        else {
          result = node.Evaluate(env);
        }
      }

      if (errors.IsEmpty()) {
        return new Result(result);
      }
      else {
        return new Result(errors);
      }
    }
  }
} // namespace Marionette.Runtime
