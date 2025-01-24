using System.Text;

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

    public static string Duplicate(this string value, int times) {
      var builder = new StringBuilder();
      for (int i = 0; i < times; ++i) {
        builder.Append(value);
      }

      return builder.ToString();
    }
  }

  public static class CharExtensions {
    public static bool IsDelimiter(this char value) {
      return char.IsWhiteSpace(value) || char.IsControl(value) || value == '(' || value == ')';
    }

    public static bool IsDigitComponent(this char value) {
      return char.IsDigit(value) || value == '.'  || value == '+' || value == '-' || char.ToLower(value) == 'e' ||
        char.ToLower(value) == 'x' || char.ToLower(value) == 'b' || char.ToLower(value) == 'a' || char.ToLower(value) == 'b' ||
        char.ToLower(value) == 'c' || char.ToLower(value) == 'd' || char.ToLower(value) == 'f';
    }
  }

  public static class ListExtensions {
    public static bool IsEmpty<T>(this List<T> value) {
      return value == null || value.Count == 0;
    }

    public static List<S> Map<T, S>(this List<T> value, Func<T, S> func) {
      var res = new List<S>();
      foreach (var item in value) {
        res.Add(func(item));
      }

      return res;
    }
  }

  public static class ArrayExtensions {
    public static List<T> TakeWhile<T>(this T[] value, Func<T, bool> predicate, int from = 0) {
      var res = new List<T>();
      for (int i = from; i < value.Length; ++i) {
        if (predicate(value[i])) {
          res.Add(value[i]);
        }
        else { break; }
      }

      return res;
    }
  }
} // namespace Marionette.Utils
