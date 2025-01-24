using System;
using System.Collections;
using System.Collections.Generic;
using Marionette.Utils;

namespace Marionette.Parser {
  public interface IEvaluatable<TRes, TEnv> {
    public TRes Evaluate(TEnv env);
  }

  public abstract class ResultList<TRes, TEnv>: IEvaluatable<TRes, TEnv> {
    protected List<IEvaluatable<TRes, TEnv>> list = new List<IEvaluatable<TRes, TEnv>>();

    public void Append(IEvaluatable<TRes, TEnv> res) {
      list.Append(res);
    }

    public abstract TRes Evaluate(TEnv env);
  }

  public abstract class Literal<TRes, TEnv>: IEvaluatable<TRes, TEnv> {
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

  public abstract class Symbol<TRes, TEnv>: IEvaluatable<TRes, TEnv> {
    private string name;

    public abstract TRes Evaluate(TEnv env);

    public Symbol(string name) {
      this.name = name;
    }
  }

  public abstract class EOF<TRes, TEnv>: IEvaluatable<TRes, TEnv> {
    public abstract TRes Evaluate(TEnv env);
  }

  public class ParseError<TRes, TEnv>: IEvaluatable<TRes, TEnv> {
    private string message;
    private Location location;

    public ParseError(string message, Location location) {
      this.message = message;
      this.location = location;
    }

    public TRes Evaluate(TEnv env) {
      throw new Exception("Parse error: " + message); // TODO: pp
    }

    public Diagnosis ToDiagnosis() {
      return new Diagnosis(ErrorType.ParseError, this.message, this.location);
    }
  }
} // namespace Marionette.Parser
