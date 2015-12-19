using System;
using System.Reflection;

namespace PCG3.TestFramework {

  /// <summary>
  /// Result of a test method executed by the test runner.
  /// </summary>
  [Serializable]
  public class TestResult {

    public MethodInfo MethodInfo  { get; set; }
    public Exception  Exception   { get; set; }
    public TimeSpan   ElapsedTime { get; set; }
    public bool       Failed      { get; set; }
    public string     Status      { get; set; }
    public Type       Type        { get; set; }

    public override string ToString() {
      return MethodInfo.ToString();
    }
  }
}
