using System;
using System.Collections.Generic;

namespace PCG3.TestFramework {

  /// <summary>
  /// Results of a list of test methods executed by the test runner.
  /// </summary>
  [Serializable]
  public class TestResults : List<TestResult> { }
}
