using System.Diagnostics;
using System.Text;

namespace tests;

enum TestResult {
  Success,
  Fail,
  Timeout
}

struct TestMode {
  public bool DebugAll {
    get {
      return DebugParse && DebugCodegen;
    }
    set {
      DebugParse = DebugCodegen = value;
    }
  }
  public bool DebugParse {
    get; set;
  }
  public bool DebugCodegen {
    get; set;
  }
  public bool PrintAll {
    get {
      return PrintParse && PrintCodegen;
    }
    set {
      PrintParse = PrintCodegen = value;
    }
  }
  public bool PrintParse {
    get; set;
  }
  public bool PrintCodegen {
    get; set;
  }
  public bool NoExecution {
    get; set;
  }
  public bool ExpectError {
    get; set;
  }
  public bool Fixme {
    get; set;
  }
  public bool Todo {
    get; set;
  }

  public TestMode() {
    DebugAll = false;
    PrintAll = false;
    NoExecution = false;
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
    shell.StandardInput.WriteLine("git status --porcelain " + String.Join("/", System.Environment.CurrentDirectory, DiffTests.testPath));
    shell.StandardInput.Close();
    string res = shell.StandardOutput.ReadToEnd();
    shell.WaitForExit();
    shell.Close();

    Array.ForEach(res.Split('\n'), line => {
      if (line.Length < 3) { return; }

      string s = line.TrimStart()[..2];
      string path = line[(line.LastIndexOf("/") + 1)..];
      if (path.EndsWith(DiffTests.testExtension)) {
        bool flag = false;
        switch (s) {
          case "??":
          case "M ":
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

  public readonly static string testPath = "../../../mario";
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
    var result = TestResult.Success;
    var outputBuilder = new StringBuilder();
    var mode = new TestMode(); 
    DateTime begin = DateTime.Now;

    for (int i = 0; i < lines.Length; ++i) {
      outputBuilder.Append(lines[i]); // TODO: test

      if (i != lines.Length - 1) {
        outputBuilder.Append('\n');
      }
    }

    DateTime end = DateTime.Now;
    int time = (end - begin).Milliseconds;
    var color = ConsoleColor.Green;

    if (result == TestResult.Success && time > timeLimit) {
      color = ConsoleColor.Gray;
    }
    else if (result == TestResult.Fail) {
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
