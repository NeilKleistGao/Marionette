using System.Diagnostics;
using System.Text;
using Marionette.Runtime;
using Marionette.Utils;

namespace tests;

struct TestMode {
  public bool ExpectError {
    get; set;
  }
  public bool Fixme {
    get; set;
  }
  public bool Todo {
    get; set;
  }

  // TODO: more mode

  public TestMode() {
    ExpectError = false;
    Fixme = false;
    Todo = false;
  }
}

public class GitDiffData: IDisposable {
  private HashSet<string> modifiedSet = new HashSet<string>();

  public GitDiffData() {
    var shell = new Process();
    shell.StartInfo.FileName = "bash"; // TODO: different system
    shell.StartInfo.UseShellExecute = false;
    shell.StartInfo.RedirectStandardInput = true;
    shell.StartInfo.RedirectStandardOutput = true;
    shell.StartInfo.RedirectStandardError = false;
    shell.StartInfo.CreateNoWindow = true;
    shell.Start();
    shell.StandardInput.WriteLine("git status --porcelain " + string.Join("/", System.Environment.CurrentDirectory, DiffTests.testPath));
    shell.StandardInput.Close();
    string res = shell.StandardOutput.ReadToEnd();
    shell.WaitForExit();
    shell.Close();

    Array.ForEach(res.Split('\n'), line => {
      if (line.Length < 3) { return; }

      string s = line[..2];
      string path = line[(line.LastIndexOf("/") + 1)..];
      if (path.EndsWith(DiffTests.testExtension)) {
        bool flag = false;
        switch (s) {
          case "??":
          case "MM":
          case " M":
          case "R ":
            flag = true;
            break;
        }
        if (flag) {
          modifiedSet.Add(path);
        }
      }
    });
  }

  public void Dispose(){}

  public bool NeedToTest(string filename) {
    return modifiedSet.Count == 0 | modifiedSet.Contains(filename);
  }
}

public class DiffTests: IClassFixture<GitDiffData> {
  private readonly static int timeLimit = 3000; // ms

  private readonly static string testOutputPrefix = ";;|| ";
  private readonly static string testOutputIndicator = ";;|| --- Result --- ||";

  public readonly static string testPath = "../../../Mario";
  public readonly static string testExtension = ".mario";

  private GitDiffData diffData;
  public DiffTests(GitDiffData data) {
    diffData = data;
  }

  [SkippableTheory]
  [MemberData(nameof(GetFileList))]
  public void ExecuteDiffTests(string filename) {
    string caseName = filename[(filename.LastIndexOf("/") + 1)..];
    Skip.IfNot(diffData.NeedToTest(caseName));

    var reader = new StreamReader(filename);
    string data = reader.ReadToEnd();
    reader.Close();
    string[] lines = data.Split("\n");
    bool succeeded = true;
    var outputBuilder = new StringBuilder();
    var interpreter = new Interpreter();
    DateTime begin = DateTime.Now;    

    for (int i = 0; i < lines.Length; ++i) {
      if (lines[i].IsEmpty()) {
        outputBuilder.Append(lines[i]);
      }
      else {
        var codeBuilder = new StringBuilder();
        var testMode = new TestMode();
        var block = lines.TakeWhile(ln => !ln.IsEmpty(), i);
        
        foreach (var line in block) {
          if (ShouldConsumeFlag(line, ref testMode)) {
            outputBuilder.AppendLine(line);
            continue;
          }
          if (line.StartsWith(testOutputIndicator)) {
            break; // drop the test outputs
          }
          outputBuilder.AppendLine(line);
          codeBuilder.AppendLine(line);
        }

        i += block.Count - 1;

        var code = codeBuilder.ToString().Trim();
        var result = interpreter.Interpret(code);
        if (result.Succeeded) {
          if (testMode.ExpectError) {
            succeeded = false;
          }
          var value = result.Value;
          if (value is not UnitValue) {
            outputBuilder.AppendLine(testOutputIndicator);
            outputBuilder.AppendLine(testOutputPrefix + "Res: " + value.Show());
          }
        }
        else {
          if (!testMode.ExpectError && !testMode.Todo && !testMode.Fixme) {
            succeeded = false;
          }
          outputBuilder.AppendLine(testOutputIndicator);
          foreach (var error in result.Diagnosis) {
            outputBuilder.AppendLine(testOutputPrefix + error.Show(code, testOutputPrefix));
          }
        }

        outputBuilder.Remove(outputBuilder.Length - 1, 1);
      }
      

      if (i != lines.Length - 1) {
        outputBuilder.Append('\n');
      }
    }

    DateTime end = DateTime.Now;
    int time = (end - begin).Milliseconds;
    var color = ConsoleColor.Green;

    if (succeeded && time > timeLimit) {
      succeeded = false;
      color = ConsoleColor.Gray;
    }
    else if (!succeeded) {
      color = ConsoleColor.Red;
    }
    ConsoleColor backup = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.WriteLine("> Case " + caseName + ": " + time.ToString() + " ms.");
    Console.ForegroundColor = backup;

    var output = outputBuilder.ToString();
    if (data != output) {
      var writer = new StreamWriter(filename, false);
      writer.Write(output);
      writer.Close();
    }

    Assert.True(succeeded);
  }

  private bool ShouldConsumeFlag(string line, ref TestMode mode) {
    bool flag = false;
    if (line.StartsWith(":e")) {
      flag = mode.ExpectError = true;
    }
    else if (line.StartsWith(":todo")) {
      flag = mode.Todo = true;
    }
    else if (line.StartsWith(":fixme")) {
      flag = mode.Fixme = true;
    }

    return flag;
  }

  public static IEnumerable<object[]> GetFileList() {
    var directory = new DirectoryInfo(testPath);
    foreach (FileInfo file in directory.GetFiles()) {
      if (file.Extension == testExtension) {
        yield return new object[] { file.FullName };
      }
    }
  }
}
