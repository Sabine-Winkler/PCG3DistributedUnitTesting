using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace PCG3.TestFramework {

  /// <summary>
  /// Executes methods marked with [Test].
  /// </summary>
  public class TestRunner {

    public TestRunner() {
      // currently nothing to do
    }

    /// <summary>
    /// Checks if the array of attributes contains an attribute of type T.
    /// </summary>
    /// <param name="attributes">attributes of a method</param>
    /// <returns>attribute of type T or null, if not found</returns>
    public T FindAttribute<T>(object[] attributes) where T : Attribute {
      
      foreach (Attribute attribute in attributes) {
        if (attribute is T) {
          return (T)attribute;
        }
      }

      return null;
    }

    public TestResult RunTest(TestResult result) {
      object testClass = result.Type.Assembly.CreateInstance(result.Type.FullName);

      object[] attributes = result.MethodInfo.GetCustomAttributes(false);
      ExpectedExceptionAttribute expectedExceptionAttribute
            = FindAttribute<ExpectedExceptionAttribute>(attributes);
      TestAttribute testAttribute
            = FindAttribute<TestAttribute>(attributes);

      if (testAttribute != null) {
        return RunTest(result, testClass, expectedExceptionAttribute);
      }
      return null;
    }

    public TestResult RunTest(TestResult result, object testClass,
                        ExpectedExceptionAttribute expectedExceptionAttribute) {

      Stopwatch watch = new Stopwatch();

      try {
        watch.Start();
        result.MethodInfo.Invoke(testClass, Type.EmptyTypes);
        watch.Stop();

        if (expectedExceptionAttribute != null) {
          // an exception was expected, but not thrown
          // -> test failed
          result.Exception = new NoExceptionThrownException(expectedExceptionAttribute.Type);
          result.Failed = true;
          result.Status = TestStatus.FAILED;
        } else {
          // everything is ok
          // -> test passed
          result.Failed = false;
          result.Status = TestStatus.PASSED;
        }
      } catch (Exception e) {

        watch.Stop();

        Exception resultException;
        if (e.InnerException != null) {
          resultException = e.InnerException;
        } else {
          resultException = e;
        }

        if (expectedExceptionAttribute != null) {

          Type actualExceptionType = resultException.GetType();
          Type expectedExceptionType = expectedExceptionAttribute.Type;

          if (actualExceptionType == expectedExceptionType) {

            // ExceptedException was thrown
            // -> test passed
            result.Failed = false;
            result.Status = TestStatus.PASSED;
          } else {

            // another exception than the expected one was thrown
            // -> test failed
            result.Exception = new ExpectedExceptionNotThrownException(
              expectedExceptionType, resultException);
            result.Failed = true;
            result.Status = TestStatus.FAILED;
          }
        } else {

          // an unexpected exception or AssertionFailedException was thrown
          // -> test failed
          result.Exception = resultException;
          result.Failed = true;
          result.Status = TestStatus.FAILED;
        }

      } finally {
        result.ElapsedTime = watch.Elapsed;
      }
      return result;
    }

    /// <summary>
    /// Executes methods marked with [Test] contained in the given, already loaded assembly.
    /// </summary>
    /// <param name="assembly">loaded assembly</param>
    /// <returns>list of test results; one result per test method</returns>
    public List<TestResult> RunTests(Assembly assembly) {

      List<TestResult> results = new List<TestResult>();

      foreach (Type type in assembly.GetTypes()) {

        object testClass = type.Assembly.CreateInstance(type.FullName);

        foreach (MethodInfo method in type.GetMethods()) {

          object[] attributes = method.GetCustomAttributes(false);
          ExpectedExceptionAttribute expectedExceptionAttribute
            = FindAttribute<ExpectedExceptionAttribute>(attributes);
          TestAttribute testAttribute
            = FindAttribute<TestAttribute>(attributes);

          if (testAttribute != null) {
            TestResult result = new TestResult();

            result.MethodInfo = method;
            result.Type = type;
            result.Status = TestStatus.WAITING;

            RunTest(result, testClass, expectedExceptionAttribute);
            results.Add(result);
          }
        }
      }

      return results;

    }

    public static void Main(string[] args) {

      TestRunner runner = new TestRunner();
      string assemblyName = @"C:\SABINE\Master\3_WS_15_16\PCG3\Teil2-Scheller\UE-Dist-Unittesting\PCG3DistributedUnitTesting\DistUnitTesting\PCG3.TestUnitTests\bin\Debug\PCG3.TestUnitTests.dll";
      Assembly assembly = Assembly.LoadFrom(assemblyName);
      List<TestResult> results = runner.RunTests(assembly);
      Console.WriteLine(results);
    }
  }
}
