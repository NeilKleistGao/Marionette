using System;

namespace Marionette.Utils {
  public class Debug {
    public static bool Enabled = false;

    public delegate void Println(string text);

    public static Println println = DefaultPrint;

    public static void Log(string text) {
      if (Enabled) {
        println(text);
      }
    }

    public static void LogFormat(string format, params object?[] args) {
      Log(string.Format(format, args));
    }

    public static void DefaultPrint(string s) {
      Console.WriteLine(s);
    }

    public static void PrintStack() {
      var t = new System.Diagnostics.StackTrace();
      Log(t.ToString());
    }
  }
} // namespace Marionette.Utils
