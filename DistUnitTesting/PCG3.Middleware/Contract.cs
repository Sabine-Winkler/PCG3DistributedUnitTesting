using Microsoft.Ccr.Core;
using PCG3.TestFramework;
using System;
using System.Collections.Generic;

namespace PCG3.Middleware {

  #region AssemblyWorker
  public class AssemblyWorker : Port<AssemblyRequest> {
  }

  [Serializable]
  public class AssemblyRequest{
    public byte[] Bytes { get; set; }
    public Port<AssemblyResponse> ResponsePort { get; set; }
  }

  [Serializable]
  public class AssemblyResponse {
    public bool Worked { get; set; }
    public string ErrorMsg { get; set; }
  }
  #endregion



  #region TestWorker
  public class TestWorker : Port<TestRequestTest> {
  }

  [Serializable]
  public class TestRequestTest {
    public List<Test> Tests { get; set; }
    public Port<TestResponseResult> ResponsePort { get; set; }
  }

  [Serializable]
  public class TestResponseResult {
    public List<Test> Results { get; set; }
  }
  #endregion
}