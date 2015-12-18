﻿using Microsoft.Ccr.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XcoAppSpaces.Core;
using PCG3.TestFramework;

namespace PCG3.Middleware {


  //general
  public class AssemblyWorker : Port<AssemblyRequest> {

  }

  public class AssemblyRequest{

    public byte [] Bytes { get; set; }
    public Port<AssemblyResponse> ResponsePort { get; set; }
  }

  public class AssemblyResponse {
    public bool Worked { get; set; }
    public string ErrorMsg { get; set; }

  }

  public class TestWorker : Port<TestRequest> {

  }

  public class TestRequest {
    public TestResult Test { get; set; }
    public Port<TestResponse> ResponsePort { get; set; }
  }

  public class TestResponse {
     public TestResult Result { get; set; }
  }
  
}
