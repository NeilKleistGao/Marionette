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
        return new EvalStringLit(text[1..^1]);
      }
      else if (text == "#t") {
        return new EvalBoolLit(true);
      }
      else if (text == "#f") {
        return new EvalBoolLit(false);
      }
      else if (text.Contains(".") || text.Contains("e") || text.Contains("E")) {
        return new EvalNumberLit(Convert.ToDouble(text));
      }
      else {
        text = text.ToLower();
        int flag = 1;
        if (text.StartsWith("-")) {
          flag = -1;
          text = text[1..];
        }
        else if (text.StartsWith("+")) {
          text = text[1..];
        }

        if (text.StartsWith("0x")) {
          return new EvalIntLit(flag * Convert.ToInt32(text[2..], 16));
        }
        else if (text.StartsWith("0b")) {
          return new EvalIntLit(flag * Convert.ToInt32(text[2..], 2));
        }
        else if (text.StartsWith("0")) {
          return new EvalIntLit(flag * Convert.ToInt32(text[1..], 8));
        }
        else {
          return new EvalIntLit(flag * Convert.ToInt32(text));
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
