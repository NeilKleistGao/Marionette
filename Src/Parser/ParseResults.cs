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

    public List<Symbol<TRes, TEnv>> AsSymbolList() {
      var res = new List<Symbol<TRes, TEnv>>();
      foreach (var item in list) {
        if (item is Symbol<TRes, TEnv> s) {
          res.Add(s);
        }
        else {
          var loc = Location.Empty(); // TODO: refactor
          if (item is LocatableData d) {
            loc = d.Loc;
          }
          throw new RuntimeException("Expect symbol here.", loc);
        }
      }

      return res;
    }

    public override string ToString(){
      return string.Format("({0}, ...)", list[0].ToString()); // TODO
    }
  }

  public abstract class Literal<TRes, TEnv>: LocatableData, IEvaluatable<TRes, TEnv> {
    public abstract TRes Evaluate(TEnv env);
  }

  public abstract class IntLiteral<TRes, TEnv> : Literal<TRes, TEnv> {
    public int Value { get; set; }

    public override string ToString(){
      return Value.ToString();
    }
  }

  public abstract class NumberLiteral<TRes, TEnv> : Literal<TRes, TEnv> {
    public double Value { get; set; }

    public override string ToString(){
      return Value.ToString();
    }
  }

  public abstract class BoolLiteral<TRes, TEnv> : Literal<TRes, TEnv> {
    public bool Value { get; set; }

    public override string ToString(){
      return Value.ToString();
    }
  }

  public abstract class StringLiteral<TRes, TEnv> : Literal<TRes, TEnv> {
    public string? Value { get; set; }

    public override string ToString(){
      return string.Format("\"{0}\"", Value?.ToString()??"");
    }
  }

  public abstract class Symbol<TRes, TEnv>: LocatableData, IEvaluatable<TRes, TEnv> {
    protected string name;

    public abstract TRes Evaluate(TEnv env);

    public Symbol(string name) {
      this.name = name;
    }

    public bool IsDefine { get => name == "define"; }

    public bool IsIf { get => name == "if"; }

    public bool IsCond { get => name == "cond"; }

    public bool IsElse { get => name == "else"; }

    public bool IsLambda { get => name == "lambda"; }

    public string Name { get => name; }

    public override string ToString(){
      return name;
    }
  }

  public abstract class EOF<TRes, TEnv>: LocatableData, IEvaluatable<TRes, TEnv> {
    public abstract TRes Evaluate(TEnv env);

    public override string ToString(){
      return "<EOF>";
    }
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

    public override string ToString(){
      return "<ERROR>";
    }
  }
} // namespace Marionette.Parser
