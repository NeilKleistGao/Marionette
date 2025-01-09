using System;
using System.Collections;
using Marionette.Utils;

namespace Marionette.Parser {
  public class Parser<TRes, TEnv> : IEnumerator<IEvaluatable<TRes, TEnv>> {
    private readonly string code;
    private string rest;
    private Position position;

    public delegate ResultList<TRes, TEnv> ListAllocator();

    public delegate Literal<TRes, TEnv> LiteralAllocator(string text);

    public delegate Symbol<TRes, TEnv> SymbolAllocator(string name);

    public delegate EOF<TRes, TEnv> EOFAllocator();

    private ListAllocator allocateList;
    private LiteralAllocator allocateLiteral;
    private SymbolAllocator allocateSymbol;
    private EOFAllocator allocateEOF;

    private IEvaluatable<TRes, TEnv>? curResult = null;

    public Parser(string code, ListAllocator allocateList, LiteralAllocator allocateLiteral, SymbolAllocator allocateSymbol, EOFAllocator allocateEOF) {
      this.code = code;
      rest = this.code;
      this.allocateList = allocateList;
      this.allocateLiteral = allocateLiteral;
      this.allocateSymbol = allocateSymbol;
      this.allocateEOF = allocateEOF;
      position = new Position(0, 1);
    }

    private string Consume(int length) {
      if (rest.Length < length) {
        throw new ArgumentException("Invalid length of string.");
      }
      string res = rest.Substring(0, length);
      rest = rest.Substring(length);

      // * Update position
      int newLineCnt = res.CountChar('\n');
      if (newLineCnt == 0) {
        position += res.Length;
      }
      else {
        position *= newLineCnt;
        int last = res.LastIndexOf('\n');
        position &= res.Length - last + 1;
      }

      return res;
    }

    private void DropAll() {
      Consume(rest.Length);
    }

    private void Drop(int length) {
      Consume(length);
    }

    private IEvaluatable<TRes, TEnv> Parse() {
      var startPos = position;
      if (rest.StartsWith(";;")) {
        rest = Consume(2);
        int index = rest.IndexOf('\n');
        if (index < 0) {
          DropAll();
          return allocateEOF();
        }
        else {
          rest = Consume(index + 1);
          return Parse();
        }
      }
      else if (rest.StartsWith("#|")) {
        rest = Consume(2);
        int index = rest.IndexOf("|#");
        if (index < 0) {
          DropAll();
          return new ParseError<TRes, TEnv>("Unexpected EOF.", new Location(startPos, position));
        }
        else {
          rest = Consume(index + 1);
          return Parse();
        }
      }
      else if (rest.StartsWith('(')) {
        rest = Consume(1);
        var list = allocateList();
        do {
          var subterm = Parse();
          list.Append(subterm);
        } while (!rest.IsEmpty() && !rest.StartsWith(')'));

        if (rest.StartsWith(')')) {
          rest = Consume(1);
          return list;
        }
        else {
          return new ParseError<TRes, TEnv>("Unexpected EOF.", new Location(startPos, position));
        }
      }
      else if (rest.StartsWithChar(char.IsWhiteSpace)) {
        rest = Consume(1);
        return Parse();
      }
      else if (rest.StartsWithDigit()) {
        string text = rest.TakeWhile(c => c.IsDigitComponent());
        Drop(text.Length);
        return allocateLiteral(text);
      } // TODO: string
      else {
        string symbol = rest.TakeWhile(c => !c.IsDelimiter());
        Drop(symbol.Length);
        return allocateSymbol(symbol);
      }
    }

    public IEvaluatable<TRes, TEnv> Current => curResult ?? (curResult = Parse());

    object IEnumerator.Current => curResult ?? (curResult = Parse());

    public void Dispose() {}

    public bool MoveNext() {
      if (rest.IsEmpty()) {
        return false;
      }
      else {
        curResult = Parse();
        return true;
      }
    }

    public void Reset() {
      position = new Position(0, 1);
      rest = code;
    }
  }
} // namespace Marionette.Parser
