using System;

namespace pack_csharp.Util
{
  internal static class Ensure
  {
    public static T NotNull<T>(T obj, string paramName) where T : class
    {
      if (obj is null) throw new ArgumentNullException(paramName);
      return obj;
    }

    public static T NotNull<T>(T obj) where T : class
    {
      return NotNull(obj, nameof(obj));
    }

    public static string NotNullOrEmpty(string obj, string paramName)
    {
      if (string.IsNullOrEmpty(obj)) throw new ArgumentException("Value cannot be null or an empty string.", paramName);
      return obj;
    }

    public static string NotNullOrEmpty(string obj)
    {
      if (string.IsNullOrEmpty(obj)) throw new ArgumentException("Value cannot be null or an empty string.", nameof(obj));
      return obj;
    }

    public static string NotNullOrEmpty<TException>(string obj) where TException : Exception, new()
    {
      if (string.IsNullOrEmpty(obj)) throw Activator.CreateInstance(typeof(TException), $"Value of {nameof(obj)} cannot be null or an empty string.") as TException;
      return obj;
    }

    public static T NotNull<T, TException>(T obj) where T : class where TException : Exception, new()
    {
      if (obj is null) throw Activator.CreateInstance(typeof(TException), $"Value of {nameof(obj)} cannot be null.") as TException;
      return obj;
    }
  }
}