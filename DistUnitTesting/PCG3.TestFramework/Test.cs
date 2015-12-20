using System;

namespace PCG3.TestFramework {

  /// <summary>
  /// Test method executed by the test runner.
  /// </summary>
  [Serializable]
  public class Test {

    public string     MethodName { get; set; }
    public Exception  Exception   { get; set; }
    public TimeSpan   ElapsedTime { get; set; }
    public bool       Failed      { get; set; }
    public string     Status      { get; set; }
    public Type Type { get; set; }

    public override string ToString() {
       
       string exceptionText = (Exception != null) ? Exception.Message : "null";
       return String.Format("MethodName={0}, Exception={1}, Failed={2}, Status={3}, Type={4}",
                             MethodName, exceptionText, Failed, Status, Type.FullName);
    }
  }
}
