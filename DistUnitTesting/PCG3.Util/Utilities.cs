using System;

namespace PCG3.Util {

  public static class Utilities {

    /// <summary>
    /// Checks if the array of attributes contains an attribute of type T.
    /// </summary>
    /// <param name="attributes">attributes of a method</param>
    /// <returns>attribute of type T or null, if not found</returns>
    public static T FindAttribute<T>(object[] attributes) where T : Attribute {

      foreach (Attribute attribute in attributes) {
        if (attribute is T) {
          return (T)attribute;
        }
      }

      return null;
    }
  }
}
