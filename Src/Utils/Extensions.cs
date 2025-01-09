namespace Marionette.Utils {
  public static class StringExtensions {
    public static bool IsEmpty(this string value) {
      return value == null || value.Length == 0;
    }

    public static int CountChar(this string value, char c) {
      int res = 0;
      foreach (char vc in value) {
        if (vc == c) { ++res; }
      }

      return res;
    }

    public static bool StartsWithChar(this string value, Func<char, bool> predicate) {
      if (value == null || value.IsEmpty()) { return false ;}
      else { return predicate(value.ElementAt(0)); }
    }

    public static string TakeWhile(this string value, Func<char, bool> predicate) {
      string res = "";
      foreach (char c in value) {
        if (predicate(c)) { res += c;}
        else { break; }
      }

      return res;
    }

    public static bool StartsWithDigit(this string value) {
      return value.StartsWithChar(char.IsDigit) ||
        value.StartsWithChar(c => c == '+' || c == '-') && value.Length > 1 && char.IsDigit(value.ElementAt(1));
    }
  }

  public static class CharExtensions {
    public static bool IsDelimiter(this char value) {
      return char.IsWhiteSpace(value) || char.IsControl(value) || value == '(' || value == ')';
    }

    public static bool IsDigitComponent(this char value) {
      return char.IsDigit(value) || value == '.' || char.ToLower(value) == 'e' || value == '_' ||
        char.ToLower(value) == 'x' || char.ToLower(value) == 'b' || char.ToLower(value) == 'p';
    }
  }
} // namespace Marionette.Utils
