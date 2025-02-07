using Marionette.Parser;
using Marionette.Utils;

namespace Marionette.Runtime {
  public abstract class Value {
    public abstract string Show();
  }

  public class LiteralValue<T> : Value {
    private T value;

    public LiteralValue(T v) {
      this.value = v;
    }

    public T Value {
      get => this.value;
    }

    public override string Show() {
      if (value is string s) {
        return string.Format("\"{0}\"", s);
      }
      else {
        return value?.ToString() ?? "";
      }
    }
  }

  public class Closure : Value {
    private string[] bindings;
    private Environment environment;
    private IEvaluatable<Value, Environment> body;

    public override string Show() {
      return "[Function]";
    }

    public Closure(string[] bindings, Environment environment, IEvaluatable<Value, Environment> body) {
      this.bindings = bindings;
      this.environment = environment;
      this.body = body;
    }

    public Value Evaluate(Value[] values, Location? loc) {
      if (bindings.Length != values.Length) {
        throw new RuntimeException(
          string.Format("Expect {0} argument{1}, got {2}", bindings.Length, (bindings.Length > 1) ? "s" : "", values.Length),
          loc
        );
      }

      var nestCxt = new Environment(environment);
      foreach (var p in bindings.Zip(values)) {
        nestCxt.Add(p.First, p.Second);
      }

      try {
        Debug.LogFormat("Evalueate closure body {0}", body.ToString());
        return body.Evaluate(nestCxt);
      }
      catch (RuntimeException ex) {
        throw new RuntimeException(ex.Message, loc);
      }
    }
  }

  public class UnitValue : Value {
    public override string Show() {
      return "";
    }
  }

  public class Result {
    private bool succeeded = false;
    private List<Diagnosis> diagnosis = new List<Diagnosis>();
    private Value value = new UnitValue();

    public Result(Value value) {
      this.value = value;
      this.succeeded = true;
    }

    public Result(List<Diagnosis> diagnosis) {
      this.diagnosis = diagnosis;
      this.succeeded = false;
    }

    public bool Succeeded {
      get => succeeded;
    }

    public Value Value {
      get {
        if (succeeded) { return value; }
        else { throw new Exception("Value is invalid."); }
      }
    }

    public List<Diagnosis> Diagnosis {
      get {
        if (succeeded) { throw new Exception("No Errors found."); }
        else { return diagnosis; }
      }
    }
  }
}
