﻿using PCG3.TestFramework;
using System;

namespace PCG3.TestUnitTests {

  public class TestUnitTests2 {

    /// <summary>
    /// Adds two numbers and compares the actual result with the expected one.
    /// Test passes.
    /// </summary>
    [Test]
    public void AddTest2() {
      int expected = 3;
      int actual   = 1 + 2;
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Adds two numbers and compares the actual result with the expected one.
    /// Test fails.
    /// </summary>
    [Test]
    public void AddFailingTest2() {
      int expected = 4;
      int actual   = 1 + 2;
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// The expected exception DivideByZeroException is thrown.
    /// Test passes.
    /// </summary>
    [Test]
    [ExpectedException(typeof(DivideByZeroException))]
    public void DivideByZeroTest2() {
      int zero   = 1 - 1;
      int result = 3 / zero;
    }

    /// <summary>
    /// A DivideByZeroException is thrown instead of a IndexOutOfRangeException.
    /// Test fails.
    /// </summary>
    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void AnotherExceptionThrownTest2() {
      int zero = 1 - 1;
      int result = 3 / zero; // throws a DivideByZeroException
    }

    /// <summary>
    /// A DivideByZeroException is expected, but no exception is thrown.
    /// Test fails.
    /// </summary>
    [Test]
    [ExpectedException(typeof(DivideByZeroException))]
    public void NoExceptionThrownTest2() {
      int result = 10 / 5;
    }

    /// <summary>
    /// This method is not intended to be executed by a test runner.
    /// </summary>
    public void ThisTestIsNotExecutedTest2() {
      // nothing to do here
    }
  }
}
