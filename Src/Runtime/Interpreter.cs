using Marionette.Parser;
using Marionette.Utils;

namespace Marionette.Runtime {
  public class Environment {
    private Dictionary<string, Value> env = new Dictionary<string, Value>();

    private Environment? parent = null;

    private Environment() {
      env.Add("+", new Closure(["lhs", "rhs"], globalEnvironment, new BinaryOperator("+")));
      env.Add("-", new Closure(["lhs", "rhs"], globalEnvironment, new BinaryOperator("-")));
      env.Add("*", new Closure(["lhs", "rhs"], globalEnvironment, new BinaryOperator("*")));
      env.Add("/", new Closure(["lhs", "rhs"], globalEnvironment, new BinaryOperator("/")));
    }

    public Environment(Environment parent) {
      this.parent = parent;
    }

    private static Environment globalEnvironment = new Environment();

    public static Environment GlobalEnvironment() {
      return globalEnvironment;
    }

    public void Add(string name, Value value) {
      env[name] = value;
    }

    public Value GetOrElse(string name, Func<string, Value> fallback) {
      if (env.ContainsKey(name)) {
        return env[name];
      }
      else if (parent is Environment p) {
        return parent.GetOrElse(name, fallback);
      }
      else {
        return fallback(name);
      }
    }
  }

  public class Interpreter {

    private Environment env = Environment.GlobalEnvironment();

    private ResultList<Value, Environment> AllocateList() {
      return new EvalList();
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
        else if (text.StartsWith("0") && text.Length > 1) {
          return new EvalIntLit(flag * Convert.ToInt32(text[1..], 8));
        }
        else {
          return new EvalIntLit(flag * Convert.ToInt32(text));
        }
      }

      throw new Exception("Cannot parse literal " + text);
    }

    private Symbol<Value, Environment> AllocateSymbol(string name) {
      return new EvalSymbol(name);
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
