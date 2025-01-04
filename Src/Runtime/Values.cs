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

  public class Result {
    private bool succeeded = false;
    private Utils.Diagnosis diagnosis = null;
    private Value value = null;

    public Result(Value value) {
      this.value = value;
      this.succeeded = true;
    }

    public Result(Utils.Diagnosis diagnosis) {
      this.diagnosis = diagnosis;
      this.succeeded = false;
    }

    public bool Succeeded {
      get => succeeded;
    }

    public Value Value {
      get {
        if (succeeded) { return value; }
        else { return null; }
      }
    }

    public Utils.Diagnosis Diagnosis {
      get {
        if (succeeded) { return null; }
        else { return diagnosis; }
      }
    }
  }
}
