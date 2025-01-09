using System;
using System.Collections;

namespace Marionette.Parser {
  public class Parser<TRes, TEnv> : IEnumerator<IEvaluatable<TRes, TEnv>> {
    private string code;
    private Utils.Position position;
    private int pointer = -1;

    public delegate ResultList<TRes, TEnv> ListAllocator(IEvaluatable<TRes, TEnv>[] subterms);

    public delegate Literal<TRes, TEnv> LiteralAllocator(string text);

    public delegate Symbol<TRes, TEnv> SymbolAllocator(string name);

    private ListAllocator allocateList;
    private LiteralAllocator allocateLiteral;
    private SymbolAllocator allocateSymbol;

    private IEvaluatable<TRes, TEnv>? curResult = null;

    public Parser(string code, ListAllocator allocateList, LiteralAllocator allocateLiteral, SymbolAllocator allocateSymbol) {
      this.code = code;
      this.allocateList = allocateList;
      this.allocateLiteral = allocateLiteral;
      this.allocateSymbol = allocateSymbol;
      position = new Utils.Position(0, 1);
    }

    private IEvaluatable<TRes, TEnv> Parse() {
      throw new NotImplementedException();
    }

    public IEvaluatable<TRes, TEnv> Current => curResult ?? (curResult = Parse());

    object IEnumerator.Current => curResult ?? (curResult = Parse());

    public void Dispose() {}

    public bool MoveNext() {
      if (pointer >= code.Length) {
        return false;
      }
      else {
        curResult = Parse();
        return true;
      }
    }

    public void Reset() {
      position = new Utils.Position(0, 1);
      pointer = -1;
    }
  }
} // namespace Marionette.Parser
