using System;
using System.Runtime.Serialization;
using System.Text;

namespace Marionette.Utils {
  public enum ErrorType {
    ParseError,
    RuntimeError
  }

  public record Diagnosis(ErrorType type, string message, Location location) {
    private static string BuildErrorLocation(string source, string prefix, Location location, int globalLine) {
      var resBuilder = new StringBuilder();
      var lines = source.Split("\n");
      int maxLineLen = (location.end.row + globalLine + 1).ToString().Length;
      for (int i = location.start.row; i <= location.end.row; ++i) {
        string s = (i + globalLine + 1).ToString();
        string lineNum = string.Format("{0}{1} |", s, " ".Duplicate(maxLineLen - s.Length));
        resBuilder.AppendFormat("{0} {1} {2}\n", prefix, lineNum, lines[i]);
        var indicatorBuilder = new StringBuilder();
        for (int j = 0; j < lines[i].Length; ++j) {
          var pos = new Position(i, j + 1);
          indicatorBuilder.Append((location & pos) ? "^" : " ");
        }
        resBuilder.AppendFormat("{0} {1} {2}{3}",
          prefix, " ".Duplicate(lineNum.Length), indicatorBuilder.ToString(), (i == location.end.row) ? "" : "\n");
      }

      return resBuilder.ToString();
    }

    public string Show(string source, string prefix, int globalLine) {
      var head = (type == ErrorType.ParseError) ? "[Parse Error]" : "[Runtime Error]";
      return string.Format("{0}: {1}\n{2}", head, message, BuildErrorLocation(source, prefix, location, globalLine));
    }
  }

  [Serializable]
  public class RuntimeException : Exception {
    private Location location = Location.Empty();
    
    public RuntimeException(string msg, Location? location) : base(msg) {
      this.location = location ?? Location.Empty();
    }

    public RuntimeException(string msg) : base(msg) {}

    public RuntimeException() : base("") {}

    public RuntimeException(string msg, Exception innerException) : base(msg, innerException) {}

    public Location Loc { get => location; }
  }
} // namespace Marionette.Utils
