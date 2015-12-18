using System;

namespace PCG3.TestFramework {

  /// <summary>
  /// To mark a method as test method.
  /// Test methods are executed by the test runner.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
  public class TestAttribute : Attribute { }

  /// <summary>
  /// A method marked with this attribute is expected to
  /// throw an exception of a given type.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
  public class ExpectedExceptionAttribute : Attribute {

    public Type Type { get; set; }

    public ExpectedExceptionAttribute(Type type) {
      this.Type = type; 
    }
  }
}
