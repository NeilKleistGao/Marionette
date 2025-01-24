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

    private void Consume(int length) {
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
    }

    private void ConsumeAll() {
      Consume(rest.Length);
    }

    private IEvaluatable<TRes, TEnv> Parse() {
      var startPos = position;
      if (rest.IsEmpty()) {
        return allocateEOF();
      }
      else if (rest.StartsWith(";;")) {
        Consume(2);
        int index = rest.IndexOf('\n');
        if (index < 0) {
          ConsumeAll();
          return allocateEOF();
        }
        else {
          Consume(index + 1);
          return Parse();
        }
      }
      else if (rest.StartsWith("#|")) {
        Consume(2);
        int index = rest.IndexOf("|#");
        if (index < 0) {
          ConsumeAll();
          return new ParseError<TRes, TEnv>("Unexpected EOF.", new Location(startPos, position));
        }
        else {
          Consume(index + 2);
          return Parse();
        }
      }
      else if (rest.StartsWith('(')) {
        Consume(1);
        var list = allocateList();
        do {
          var subterm = Parse();
          list.Append(subterm);
        } while (!rest.IsEmpty() && !rest.StartsWith(')'));

        if (rest.StartsWith(')')) {
          Consume(1);
          return list;
        }
        else {
          return new ParseError<TRes, TEnv>("Unexpected EOF.", new Location(startPos, position));
        }
      }
      else if (rest.StartsWithChar(char.IsWhiteSpace)) {
        Consume(1);
        return Parse();
      }
      else if (rest.StartsWithDigit()) {
        string text = rest.TakeWhile(c => c.IsDigitComponent());
        Consume(text.Length);
        try {
          return allocateLiteral(text);
        }
        catch (Exception) {
          return new ParseError<TRes, TEnv>(string.Format("{0} is not a valid number literal.", text), new Location(startPos, position));
        }
      }
      else if (rest.StartsWith("#t")) {
        Consume(2);
        return allocateLiteral("#t");
      }
      else if (rest.StartsWith("#f")) {
        Consume(2);
        return allocateLiteral("#f");
      }
      else if (rest.StartsWith('"')) {
        Consume(1);
        string s = rest.TakeWhile(c => c != '\n' && c != '"');
        Consume(s.Length);
        if (!rest.StartsWith('"')) {
          return new ParseError<TRes, TEnv>("Unfinished string.", new Location(startPos, position));
        }
        Consume(1);
        return allocateLiteral(string.Format("\"{0}\"", s));
      }
      else {
        string symbol = rest.TakeWhile(c => !c.IsDelimiter());
        Consume(symbol.Length);
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
