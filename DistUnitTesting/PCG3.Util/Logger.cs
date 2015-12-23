using System;

namespace PCG3.Util {

  /// <summary>
  /// A simple logger writting messages to the console.
  /// </summary>
  public static class Logger {

    private const string TEMPLATE = "{0}: {1}";

    public static void Log(string message) {

      Console.WriteLine(string.Format(TEMPLATE, DateUtil.GetCurrentDateTime(), message));
    }
  }
}
