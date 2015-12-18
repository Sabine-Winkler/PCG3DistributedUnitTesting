using System;

namespace PCG3.TestFramework {
  
  /// <summary>
  /// Exception which is thrown when another exception than the expected one was thrown.
  /// </summary>
  public class ExpectedExceptionNotThrownException : Exception {

    public Type ExpectedExceptionType { get; set; }
    public Exception ActualException  { get; set; }

    public const string MESSAGE_TEMPLATE_ANOTHER_EXCEPTION_THROWN
      = "Expected: <{0}>, Actual: <{1}>. Exception message: <{2}>";
    
    public ExpectedExceptionNotThrownException(
      Type expectedExceptionType, Exception actualException)
      : base(String.Format(MESSAGE_TEMPLATE_ANOTHER_EXCEPTION_THROWN,
                           expectedExceptionType, actualException.GetType().Name,
                           actualException.Message)) {

      this.ExpectedExceptionType = expectedExceptionType;
      this.ActualException = actualException;
    }

    
  }
}
