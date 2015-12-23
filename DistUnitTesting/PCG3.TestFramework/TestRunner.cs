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

    private const string TEST_PASSED = "Test passed.";
    private const string TEST_FAILED = "Test failed.";

    public TestRunner() {
      // currently nothing to do
    }


    /// <summary>
    /// Runs a test method and returns it afterwards.
    /// The class of the test method is instantiated.
    /// </summary>
    /// <param name="test">test to execute</param>
    /// <returns>updated test</returns>
    public Test RunTest(Test test) {

      object testClass = test.Type.Assembly.CreateInstance(test.Type.FullName);

      MethodInfo method = test.Type.GetMethod(test.MethodName);

      object[] attributes = method.GetCustomAttributes(false);
      ExpectedExceptionAttribute expectedExceptionAttribute
            = Utilities.FindAttribute<ExpectedExceptionAttribute>(attributes);
      TestAttribute testAttribute
            = Utilities.FindAttribute<TestAttribute>(attributes);

      if (testAttribute != null) {
        return RunTest(test, testClass, expectedExceptionAttribute);
      }
      return null;
    }


    /// <summary>
    /// Runs a test and returns it afterwards.
    /// </summary>
    /// <param name="test">test to execute</param>
    /// <param name="testClass">instantiated class of the given test method</param>
    /// <param name="expectedExceptionAttribute">[ExpectedException] attribute,
    /// or null, if the given test method does not have this attribute</param>
    /// <returns>updated test</returns>
    private Test RunTest(Test test, object testClass,
                         ExpectedExceptionAttribute expectedExceptionAttribute) {

      Stopwatch watch = new Stopwatch();

      try {
        watch.Start();
        MethodInfo method = test.Type.GetMethod(test.MethodName);
        method.Invoke(testClass, Type.EmptyTypes);
        watch.Stop();

        if (expectedExceptionAttribute != null) {
          // an exception was expected, but not thrown
          // -> test failed
          test.Message = new NoExceptionThrownException(expectedExceptionAttribute.Type).Message;
          test.Failed  = true;
          test.Status  = TestStatus.FAILED;
        } else {
          // everything is ok
          // -> test passed
          test.Message = TEST_PASSED;
          test.Failed  = false;
          test.Status  = TestStatus.PASSED;
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
            test.Message = TEST_PASSED;
            test.Failed  = false;
            test.Status  = TestStatus.PASSED;
          } else {

            // another exception than the expected one was thrown
            // -> test failed
            test.Message = new ExpectedExceptionNotThrownException(
              expectedExceptionType, resultException).Message;
            test.Failed = true;
            test.Status = TestStatus.FAILED;
          }
        } else {

          // an unexpected exception or AssertionFailedException was thrown
          // -> test failed
          test.Message = resultException.Message;
          test.Failed  = true;
          test.Status  = TestStatus.FAILED;
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
    private List<Test> RunTests(Assembly assembly) {

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

            test.MethodName = method.Name;
            test.Type = type;
            test.Status = TestStatus.IN_PROGRESS;

            RunTest(test, testClass, expectedExceptionAttribute);
            tests.Add(test);
          }
        }
      }

      return tests;
    }


    /// <summary>
    /// Main method to test the TestRunner.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args) {

      TestRunner runner = new TestRunner();

      // TODO: add a path here, if you want to test the TestRunner
      string assemblyName = "";
      
      Assembly assembly = Assembly.LoadFrom(assemblyName);
      List<Test> tests = runner.RunTests(assembly);
      
      Console.WriteLine(tests);
    }
  }
}
