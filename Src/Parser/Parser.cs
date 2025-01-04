namespace Marionette.Parser {
  public class Parser {
    private string code;
    private Utils.Position position;

    public Parser(string code) {
      this.code = code;
      this.position = new Utils.Position(0, 1);
    }
  }
} // namespace Marionette.Parser
