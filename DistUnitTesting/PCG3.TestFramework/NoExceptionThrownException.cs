
using System;
namespace PCG3.TestFramework {

  /// <summary>
  /// Exception which indicates that no exception was thrown, although an exception was expected.
  /// </summary>
  public class NoExceptionThrownException : Exception {

    public Type ExpectedExceptionType { get; set; }

    public const string MESSAGE_TEMPLATE_NO_EXCEPTION_THROWN
      = "Expected an exception of type {0}, but no exception was thrown.";

    public NoExceptionThrownException(Type expectedExceptionType)
      : base(String.Format(MESSAGE_TEMPLATE_NO_EXCEPTION_THROWN,
                           expectedExceptionType)) {

      this.ExpectedExceptionType = expectedExceptionType;
    }
  }
}
