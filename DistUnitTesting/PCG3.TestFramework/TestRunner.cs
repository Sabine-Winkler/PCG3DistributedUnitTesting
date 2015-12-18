using System;
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
    /// Checks if the array of attributes contains the attribute ExpectedException.
    /// </summary>
    /// <param name="attributes">attributes of a method</param>
    /// <returns>ExpectedExceptionAttribute if ExpectedException was found, otherwise null</returns>
    public ExpectedExceptionAttribute FindExpectedExceptionAttribute(object[] attributes) {
      
      ExpectedExceptionAttribute expExcAttr = null;

      foreach (Attribute attribute in attributes) {
        if (attribute is ExpectedExceptionAttribute) {
          expExcAttr = (ExpectedExceptionAttribute)attribute;
        }
      }

      return expExcAttr;
    }

    /// <summary>
    /// Executes methods marked with [Test] contained in the given, already loaded assembly.
    /// </summary>
    /// <param name="assembly">loaded assembly</param>
    /// <returns>list of test results; one result per test method</returns>
    public TestResults RunTests(Assembly assembly) {

      TestResults results = new TestResults();

      foreach (Type type in assembly.GetTypes()) {

        object testClass = type.Assembly.CreateInstance(type.FullName);

        foreach (MethodInfo method in type.GetMethods()) {

          object[] attributes = method.GetCustomAttributes(false);
          ExpectedExceptionAttribute expExcAttr
            = FindExpectedExceptionAttribute(attributes);

          foreach (Attribute attribute in attributes) {

            if (attribute is TestAttribute) {

              Stopwatch watch = new Stopwatch();
              TestResult result = new TestResult();

              try {
                watch.Start();
                method.Invoke(testClass, Type.EmptyTypes);
                watch.Stop();

                if (expExcAttr != null) {
                  // an exception was expected, but not thrown
                  // -> test failed
                  result.Exception = new NoExceptionThrownException(expExcAttr.Type);
                  result.Failed = true;
                } else {
                  // everything is ok
                  // -> test passed
                  result.Failed = false;
                }
              } catch (Exception e) {
                
                watch.Stop();

                Exception resultException;
                if (e.InnerException != null) {
                  resultException = e.InnerException;
                } else {
                  resultException = e;
                }
                
                if (expExcAttr != null) {

                  Type actualExceptionType = resultException.GetType();
                  Type expectedExceptionType = expExcAttr.Type;

                  if (actualExceptionType == expectedExceptionType) {

                    // ExceptedException was thrown
                    // -> test passed
                    result.Failed = false;
                  } else {

                    // another exception than the expected one was thrown
                    // -> test failed
                    result.Exception = new ExpectedExceptionNotThrownException(
                      expectedExceptionType, resultException);
                    result.Failed = true;
                  }
                } else {

                  // an unexpected exception or AssertionFailedException was thrown
                  // -> test failed
                  result.Exception = resultException;
                  result.Failed = true;
                }

              } finally {
                result.MethodInfo  = method;
                result.Type        = type;
                result.ElapsedTime = watch.Elapsed;

                results.Add(result);
              }
            }
          }
        }
      }

      return results;

    }

    public static void Main(string[] args) {

      TestRunner runner = new TestRunner();
      string assemblyName = @"C:\SABINE\Master\3_WS_15_16\PCG3\Teil2-Scheller\UE-Dist-Unittesting\PCG3DistributedUnitTesting\DistUnitTesting\PCG3.TestUnitTests\bin\Debug\PCG3.TestUnitTests.dll";
      Assembly assembly = Assembly.LoadFrom(assemblyName);
      TestResults results = runner.RunTests(assembly);
      Console.WriteLine(results);
    }
  }
}
