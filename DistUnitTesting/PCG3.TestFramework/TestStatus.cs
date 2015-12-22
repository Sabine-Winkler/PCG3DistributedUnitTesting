namespace PCG3.TestFramework {

  /// <summary>
  /// Status of a test.
  /// </summary>
  public static class TestStatus {

    /// <summary>
    /// The test is available to be send to a server.
    /// </summary>
    public const string NONE    = "None";

    /// <summary>
    /// The test failed.
    /// </summary>
    public const string FAILED  = "Failed";

    /// <summary>
    /// The test was successful. It passed.
    /// </summary>
    public const string PASSED  = "Passed";

    /// <summary>
    /// The test was sent to a server and the client is waiting for the result now.
    /// </summary>
    public const string WAITING = "Waiting...";
  }
}
