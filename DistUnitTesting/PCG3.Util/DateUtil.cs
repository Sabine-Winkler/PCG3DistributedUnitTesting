using System;

namespace PCG3.Util {

  /// <summary>
  /// Provides date and time utility methods.
  /// </summary>
  public static class DateUtil {

    private const string DATE_TIME_TEMPLATE = "[{0}]";
    private const string DATE_TIME_FORMAT = "yyyyMMdd-HH:mm:ss.fff";

    /// <summary>
    /// Returns the current date and time as a formatted string.
    /// </summary>
    /// <returns>current date and time as formatted string</returns>
    public static string GetCurrentDateTime() {
      return string.Format(DATE_TIME_TEMPLATE, DateTime.Now.ToString(DATE_TIME_FORMAT));
    }
  }
}