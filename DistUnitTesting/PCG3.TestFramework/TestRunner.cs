using PCG3.Util;
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

    public Test RunTest(Test test) {
      object testClass = test.Type.Assembly.CreateInstance(test.Type.FullName);

      object[] attributes = test.MethodInfo.GetCustomAttributes(false);
      ExpectedExceptionAttribute expectedExceptionAttribute
            = Utilities.FindAttribute<ExpectedExceptionAttribute>(attributes);
      TestAttribute testAttribute
            = Utilities.FindAttribute<TestAttribute>(attributes);

      if (testAttribute != null) {
        return RunTest(test, testClass, expectedExceptionAttribute);
      }
      return null;
    }

    public Test RunTest(Test test, object testClass,
                        ExpectedExceptionAttribute expectedExceptionAttribute) {

      Stopwatch watch = new Stopwatch();

      try {
        watch.Start();
        test.MethodInfo.Invoke(testClass, Type.EmptyTypes);
        watch.Stop();

        if (expectedExceptionAttribute != null) {
          // an exception was expected, but not thrown
          // -> test failed
          test.Exception = new NoExceptionThrownException(expectedExceptionAttribute.Type);
          test.Failed = true;
          test.Status = TestStatus.FAILED;
        } else {
          // everything is ok
          // -> test passed
          test.Failed = false;
          test.Status = TestStatus.PASSED;
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
            test.Failed = false;
            test.Status = TestStatus.PASSED;
          } else {

            // another exception than the expected one was thrown
            // -> test failed
            test.Exception = new ExpectedExceptionNotThrownException(
              expectedExceptionType, resultException);
            test.Failed = true;
            test.Status = TestStatus.FAILED;
          }
        } else {

          // an unexpected exception or AssertionFailedException was thrown
          // -> test failed
          test.Exception = resultException;
          test.Failed = true;
          test.Status = TestStatus.FAILED;
        }

      } finally {
        test.ElapsedTime = watch.Elapsed;
      }
      return test;
    }

    /// <summary>
    /// Executes methods marked with [Test] contained in the given, already loaded assembly.
    /// </summary>
    /// <param name="assembly">loaded assembly</param>
    /// <returns>list of test results; one result per test method</returns>
    public List<Test> RunTests(Assembly assembly) {

      List<Test> tests = new List<Test>();

      foreach (Type type in assembly.GetTypes()) {

        object testClass = type.Assembly.CreateInstance(type.FullName);

        foreach (MethodInfo method in type.GetMethods()) {

          object[] attributes = method.GetCustomAttributes(false);
          ExpectedExceptionAttribute expectedExceptionAttribute
            = Utilities.FindAttribute<ExpectedExceptionAttribute>(attributes);
          TestAttribute testAttribute
            = Utilities.FindAttribute<TestAttribute>(attributes);

          if (testAttribute != null) {
            Test test = new Test();

            test.MethodInfo = method;
            test.Type = type;
            test.Status = TestStatus.WAITING;

            RunTest(test, testClass, expectedExceptionAttribute);
            tests.Add(test);
          }
        }
      }

      return tests;

    }

    public static void Main(string[] args) {

      TestRunner runner = new TestRunner();
      string assemblyName = @"C:\SABINE\Master\3_WS_15_16\PCG3\Teil2-Scheller\UE-Dist-Unittesting\PCG3DistributedUnitTesting\DistUnitTesting\PCG3.TestUnitTests\bin\Debug\PCG3.TestUnitTests.dll";
      Assembly assembly = Assembly.LoadFrom(assemblyName);
      List<Test> tests = runner.RunTests(assembly);
      Console.WriteLine(tests);
    }
  }
}
