using System;

namespace PCG3.TestFramework {

  /// <summary>
  /// Data structure for test method executed by the test runner.
  /// </summary>
  [Serializable]
  public class Test {

    public string   MethodName  { get; set; }
    public string   Message     { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public bool     Failed      { get; set; }
    public string   Status      { get; set; }
    public Type     Type        { get; set; }


    public override string ToString() {
       
       return String.Format("MethodName={0}, Message={1}, Failed={2}, Status={3}, Type={4}",
                             MethodName, Message, Failed, Status, Type.FullName);
    }
  }
}
