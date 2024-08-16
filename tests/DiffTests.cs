using System.Diagnostics;

namespace tests;

public enum GitStatus {
  Modified,
  Renamed,
  Added,
  Other
}

enum TestResult {
  Success,
  Fail,
  Timeout
}

public class GitDiffData: IDisposable {
  private Dictionary<string, GitStatus> fileStatus = new Dictionary<string, GitStatus>();

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
      path = path[..(path.Length - 1)];
      if (path.EndsWith(DiffTests.testExtension)) {
        var status = GitStatus.Other;
        switch (s) {
          case "A ":
          case "??":
            status = GitStatus.Added;
            break;
          case "M ":
            status = GitStatus.Modified;
            break;
          case "R ":
            status = GitStatus.Renamed;
            break;
        }
        fileStatus[path] = status;
      }
    });
  }

  public void Dispose(){}

  public bool NeedToTest(string filename) {
    GitStatus s;
    bool flag = fileStatus.TryGetValue(filename, out s);
    return fileStatus.Count == 0 ? true : !flag && s != GitStatus.Other;
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
    var result = TestResult.Success;
    DateTime begin = DateTime.Now;
    // TODO: test
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
    Console.WriteLine();
    reader.Close();
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
