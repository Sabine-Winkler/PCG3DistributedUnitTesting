using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PCG3.VisualStudioUnitTests {
  
  [TestClass]
  public class VisualStudioUnitTests {

    /// <summary>
    /// Adds two numbers and compares the actual result with the expected one.
    /// Test passes.
    /// </summary>
    [TestMethod]
    public void AddTest() {
      int expected = 3;
      int actual   = 1 + 2;
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Adds two numbers and compares the actual result with the expected one.
    /// Test fails.
    /// </summary>
    [TestMethod]
    public void AddFailingTest() {
      int expected = 4;
      int actual   = 1 + 2;
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// The expected exception DivideByZeroException is thrown.
    /// Test passes.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(DivideByZeroException))]
    public void DivideByZeroTest() {
      int zero   = 1 - 1;
      int result = 3 / zero;
    }

    /// <summary>
    /// A DivideByZeroException is thrown instead of a IndexOutOfRangeException.
    /// Test fails.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void AnotherExceptionThrownTest() {
      int zero = 1 - 1;
      int result = 3 / zero; // throws a DivideByZeroException
    }

    /// <summary>
    /// A DivideByZeroException is expected, but no exception is thrown.
    /// Test fails.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(DivideByZeroException))]
    public void NoExceptionThrownTest() {
      // int result = 10 / 5;
    }

    /// <summary>
    /// This method is not intended to be executed by a test runner.
    /// </summary>
    public void ThisTestIsNotExecutedTest() {
      // nothing to do here
    }
  }
}
