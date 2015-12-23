using Microsoft.Ccr.Core;
using PCG3.TestFramework;
using System;
using System.Collections.Generic;
using XcoAppSpaces.Core;

namespace PCG3.Middleware {

  #region AssemblyWorker

  /// <summary>
  /// Worker which deploys an assembly to a server.
  /// </summary>
  public class AssemblyWorker : Port<AssemblyRequest> {
  }

  [Serializable]
  public class AssemblyRequest {
    public string ServerAddress { get; set; }
    public string ClientAddress { get; set; }
    public string AssemblyPath { get; set; }
    public byte[] AssemblyByteStream { get; set; }
    public Port<AssemblyResponse> ResponsePort { get; set; }
  }

  [Serializable]
  public class AssemblyResponse {
    public bool Worked { get; set; }
    public string ErrorMsg { get; set; }
  }
  #endregion


  #region TestWorker

  /// <summary>
  /// Worker which distributes tests to servers and executes them by
  ///  1. allocating cores
  ///  2. running tests
  ///  3. freeing a core
  /// </summary>
  public class TestWorker : PortSet<RunTestsRequest,
                                    AllocCoresRequest, Subscribe<AllocCoresRequest>, Unsubscribe<AllocCoresRequest>,
                                    FreeCoreRequest,   Subscribe<FreeCoreRequest>,   Unsubscribe<FreeCoreRequest>> {
    public int ServerId { get; set; }
    public String ServerAddress { get; set; }
    public String LocalName { get; set; }
  }

  [Serializable]
  public class AllocCoresRequest {
    public string ClientAddress { get; set; }
    public int ServerId { get; set; }
    public string ServerAddress { get; set; }

    // number of tests which are currently available to be executed
    public int TestCount { get; set; }

    public Port<AllocCoresResponse> ResponsePort { get; set; }
  }

  [Serializable]
  public class AllocCoresResponse {

    // number of cores which were allocated by a server
    public int AllocCores { get; set; }
  }

  [Serializable]
  public class FreeCoreRequest {
    public string ClientAddress { get; set; }
    public int ServerId { get; set; }
    public string ServerAddress { get; set; }
    public Port<FreeCoreResponse> ResponsePort { get; set; }
  }

  [Serializable]
  public class FreeCoreResponse {
  }

  [Serializable]
  public class RunTestsRequest {
    public string ClientAddress { get; set; }
    public int ServerId { get; set; }
    public string ServerAddress { get; set; }
    public List<Test> Tests { get; set; }
    public Port<RunTestsResponse> ResponsePort { get; set; }
  }

  [Serializable]
  public class RunTestsResponse {
    public Test Result { get; set; }
  }
  #endregion
}