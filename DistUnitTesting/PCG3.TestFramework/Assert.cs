namespace PCG3.TestFramework {

  /// <summary>
  /// Provides methods to check, if certain conditions in a test method are met.
  /// If a condition is not met, the methods throw an AssertFailedException.
  /// </summary>
  public static class Assert {

    public static void AreEqual(object expected, object actual) {

      if (!expected.Equals(actual)) {
        throw new AssertFailedException(expected, actual);
      }
    }
  }
}
