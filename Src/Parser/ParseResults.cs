using System;
using System.Collections;
using System.Collections.Generic;

namespace Marionette.Parser {
  public interface IEvaluatable<TRes, TEnv> {
    public TRes Evaluate(TEnv env);
  }

  public abstract class ResultList<TRes, TEnv>: IEvaluatable<TRes, TEnv> {
    protected List<IEvaluatable<TRes, TEnv>> list = new List<IEvaluatable<TRes, TEnv>>();

    public void Append(IEvaluatable<TRes, TEnv> res) {
      list.Append(res);
    }

    public TRes Evaluate(TEnv env) {
      throw new NotImplementedException();
    }
  }

  public abstract class Literal<T, TRes, TEnv>: IEvaluatable<TRes, TEnv> {
    protected T Value;

    public TRes Evaluate(TEnv env) {
      throw new NotImplementedException();
    }
  }

  public abstract class Symbol<TRes, TEnv>: IEvaluatable<TRes, TEnv> {
    private string name;

    public TRes Evaluate(TEnv env) {
      throw new NotImplementedException();
    }
  }
} // namespace Marionette.Parser