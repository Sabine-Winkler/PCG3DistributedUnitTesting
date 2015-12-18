using System;

namespace PCG3.TestFramework {

  /// <summary>
  /// Exception which indicates that an assert statement failed.
  /// </summary>
  public class AssertFailedException : Exception {

    public object Expected { get; set; }
    public object Actual   { get; set; }
    
    public const string MESSAGE_TEMPLATE = "Expected: <{0}>, Actual: <{1}>.";

    public AssertFailedException(object expected, object actual)
      : base(String.Format(MESSAGE_TEMPLATE,
                           expected.ToString(), actual.ToString())) {

      this.Expected = expected;
      this.Actual   = actual;
    }
  }
}
