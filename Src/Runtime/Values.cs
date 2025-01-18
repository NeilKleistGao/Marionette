namespace Marionette.Runtime {
  public abstract class Value {}

  public class LiteralValue<T> : Value {
    private T value;

    public LiteralValue(T v) {
      this.value = v;
    }

    public T Value {
      get => this.value;
    }
  }

  public class UnitValue: Value {}

  public class Result {
    private bool succeeded = false;
    private List<Utils.Diagnosis> diagnosis = new List<Utils.Diagnosis>();
    private Value value = new UnitValue();

    public Result(Value value) {
      this.value = value;
      this.succeeded = true;
    }

    public Result(List<Utils.Diagnosis> diagnosis) {
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

    public List<Utils.Diagnosis> Diagnosis {
      get {
        if (succeeded) { throw new Exception("No Errors found."); }
        else { return diagnosis; }
      }
    }
  }
}
