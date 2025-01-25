using System;
using System.Collections;
using System.Collections.Generic;
using Marionette.Utils;

namespace Marionette.Parser {
  public interface IEvaluatable<TRes, TEnv> {
    public TRes Evaluate(TEnv env);
  }

  public abstract class ResultList<TRes, TEnv>: LocatableData, IEvaluatable<TRes, TEnv> {
    protected List<IEvaluatable<TRes, TEnv>> list = new List<IEvaluatable<TRes, TEnv>>();

    public void Append(IEvaluatable<TRes, TEnv> res) {
      list.Add(res);
    }

    public abstract TRes Evaluate(TEnv env);
  }

  public abstract class Literal<TRes, TEnv>: LocatableData, IEvaluatable<TRes, TEnv> {
    public abstract TRes Evaluate(TEnv env);
  }

  public abstract class IntLiteral<TRes, TEnv> : Literal<TRes, TEnv> {
    public int Value { get; set; }
  }

  public abstract class NumberLiteral<TRes, TEnv> : Literal<TRes, TEnv> {
    public double Value { get; set; }
  }

  public abstract class BoolLiteral<TRes, TEnv> : Literal<TRes, TEnv> {
    public bool Value { get; set; }
  }

  public abstract class StringLiteral<TRes, TEnv> : Literal<TRes, TEnv> {
    public string? Value { get; set; }
  }

  public abstract class Symbol<TRes, TEnv>: LocatableData, IEvaluatable<TRes, TEnv> {
    protected string name;

    public abstract TRes Evaluate(TEnv env);

    public Symbol(string name) {
      this.name = name;
    }
  }

  public abstract class EOF<TRes, TEnv>: LocatableData, IEvaluatable<TRes, TEnv> {
    public abstract TRes Evaluate(TEnv env);
  }

  public class ParseError<TRes, TEnv>: LocatableData, IEvaluatable<TRes, TEnv> {
    private string message;
    private Location location;

    public override Location? Loc { get => location; set => location = value ?? Location.Empty(); }

    public ParseError(string message, Location location) {
      this.message = message;
      this.location = location;
    }

    public TRes Evaluate(TEnv env) {
      throw new Exception("Parse error: " + message);
    }

    public Diagnosis ToDiagnosis() {
      return new Diagnosis(ErrorType.ParseError, message, location);
    }
  }
} // namespace Marionette.Parser
