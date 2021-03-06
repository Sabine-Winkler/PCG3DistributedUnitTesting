﻿using PCG3.Middleware;
using PCG3.TestFramework;
using PCG3.Util;
using System;
using System.Reflection;
using System.Threading.Tasks;
using XcoAppSpaces.Core;

namespace PCG3.Server {


  /// <summary>
  /// Implementation of an AssemblyWorker.
  /// </summary>
  public class ServerAssemblyWorker : AssemblyWorker {

    #region message templates
    private const string TEMPLATE_LOAD_ASSEMBLY
      = "[Server/AssWo] [Me|{0}] [C|{1}] Assembly '{2}' got load on server.";
    #endregion

    /// <summary>
    /// Loads an assembly.
    /// </summary>
    /// <param name="req">assembly loading request containing the assembly as byte stream</param>
    [XcoConcurrent]
    public void Process(AssemblyRequest req) {

      AssemblyResponse resp = new AssemblyResponse();

      try {
        Assembly.Load(req.AssemblyByteStream);
        resp.Worked = true;
        string message = string.Format(TEMPLATE_LOAD_ASSEMBLY,
                                       req.ServerAddress, req.ClientAddress,
                                       req.AssemblyPath);
        Logger.Log(message);
      } catch (Exception e) {
        resp.Worked = false;
        resp.ErrorMsg = e.ToString();
      } finally {
        req.ResponsePort.Post(resp);
      }
    }
  }


  /// <summary>
  /// Implementation of a TestWorker.
  /// </summary>
  public class ServerTestWorker : TestWorker {

    #region message templates
    private const string TEMPLATE_TEST_RESULT
      = "[S/TstWo] [Me|{0}] [C|{1}] [Result]\n{2}\n";
    private const string TEMPLATE_AVAILABLE_TESTS
      = "[S/TstWo] [Me|{0}] [C|{1}] {2} tests available.";
    private const string TEMPLATE_FREE_CORE_REQUEST
      = "[S/TstWo] [Me|{0}] [C|{1}] Free core request.";
    private const string TEMPLATE_RUN_TESTS
      = "[S/TstWo] [Me|{0}] [C|{1}] Run tests.";
    private const string TEMPLATE_RUN_TEST
      = "[S/TstWo] [Me|{0}] [C|{1}] Run test ...";
    #endregion

    // XcoPublisher automatically manages incoming subscriptions
    private readonly XcoPublisher<AllocCoresRequest> allocCoresPublisher
      = new XcoPublisher<AllocCoresRequest>();
    private readonly XcoPublisher<FreeCoreRequest> freeCorePublisher
      = new XcoPublisher<FreeCoreRequest>();

    /// <summary>
    /// Publishes the allocating cores request to subscribed clients.
    /// </summary>
    /// <param name="req">allocating cores request</param>
    [XcoConcurrent]
    public void Process(AllocCoresRequest req) {
      Logger.Log(string.Format(TEMPLATE_AVAILABLE_TESTS,
                               req.ServerAddress, req.ClientAddress, req.TestCount));
      allocCoresPublisher.Publish(req);
    }

    /// <summary>
    /// Publishes the free core request to subscribed clients.
    /// </summary>
    /// <param name="req">free core request</param>
    [XcoConcurrent]
    public void Process(FreeCoreRequest req) {
      Logger.Log(string.Format(TEMPLATE_FREE_CORE_REQUEST,
                               req.ServerAddress, req.ClientAddress));
      freeCorePublisher.Publish(req);
    }


    /// <summary>
    /// Runs multiple tests in parallel and sends a response after a test finished.
    /// </summary>
    /// <param name="req">run tests request containing a list of tests to be executed</param>
    [XcoConcurrent]
    public void Process(RunTestsRequest req) {

      Logger.Log(string.Format(TEMPLATE_RUN_TESTS,
                               req.ServerAddress, req.ClientAddress));
      TestRunner testRunner = new TestRunner();

      foreach (Test test in req.Tests) {
        
        Task.Run(() => {
          RunTestsResponse resp = new RunTestsResponse();
          test.ServerAddress = req.ServerAddress;
          Logger.Log(string.Format(TEMPLATE_RUN_TEST,
                                   req.ServerAddress, req.ClientAddress));
          resp.Result = testRunner.RunTest(test);
          Logger.Log(string.Format(TEMPLATE_TEST_RESULT,
                                   req.ServerAddress, req.ClientAddress, resp.Result));
          req.ResponsePort.Post(resp);
        });
      }
    }
  }
}