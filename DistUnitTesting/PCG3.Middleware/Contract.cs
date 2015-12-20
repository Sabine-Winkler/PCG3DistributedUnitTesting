using Microsoft.Ccr.Core;
using PCG3.TestFramework;
using System;
using System.Collections.Generic;

namespace PCG3.Middleware {

  public class AssemblyWorker : Port<AssemblyRequest> {
  }

  [Serializable]
  public class AssemblyRequest{
    public byte [] Bytes { get; set; }
    public Port<AssemblyResponse> ResponsePort { get; set; }
  }

  [Serializable]
  public class AssemblyResponse {
    public bool Worked { get; set; }
    public string ErrorMsg { get; set; }
  }


  public class TestWorker : Port<TestRequestTest> {

  
    public TestWorker() {
      this.Cores = 1;
      this.coresInUse = 0;
    }
    public int Cores { get; set; }
    protected int coresInUse;
    
    public int AvailableCores() {
      return Cores - coresInUse;
    }
    
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
  /*
  [Serializable]
  public class TestRequestAvailableCoreFromServer {
    public Port<TestResponseAvailableCore> ResponsePort { get; set; }
  }

  
  [Serializable]
  public class TestResponseAvailableCore {
    public bool availableCore { get; private set; }
    public Port<TestRequestTest> ResponsePort { get; set; }
  }
  */


}