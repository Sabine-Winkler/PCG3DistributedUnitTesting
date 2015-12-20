using System;

namespace PCG3.Util {
  
  public static class DateUtil {
  
    private const string DATE_TIME_TEMPLATE = "[{0}]";
    private const string DATE_TIME_FORMAT   = "yyyyMMdd-HH:mm:ss.fff";

    public static string GetCurrentDateTime() {
      return string.Format(DATE_TIME_TEMPLATE, DateTime.Now.ToString(DATE_TIME_FORMAT));
    }
  }
}
