using System;
using System.Collections.Generic;
using System.Linq;

namespace pack_csharp.Util
{
  internal static class ListExtensions
  {
    public static List<string> AddCliFlag(this List<string> list, string flagLabel, string flagValue)
    {
      Ensure.NotNull(list, nameof(list));
      Ensure.NotNullOrEmpty(flagLabel, nameof(flagLabel));

      if (!string.IsNullOrEmpty(flagValue))
        list.Add($"{flagLabel} \"{flagValue}\"");

      return list;
    }

    public static List<string> AddCliFlag(this List<string> list, string flagLabel, IDictionary<string, string> values)
    {
      Ensure.NotNull(list, nameof(list));
      Ensure.NotNullOrEmpty(flagLabel, nameof(flagLabel));

      if (values == default || !values.Any()) return list;

      foreach (var (key, value) in values)
        list.Add($"{flagLabel} '{key}{(string.IsNullOrEmpty(value) ? "" : "=" + value)}'");

      return list;
    }

    public static List<string> AddCliFlag(this List<string> list, string flagLabel, SortedSet<string> values)
    {
      Ensure.NotNull(list, nameof(list));
      Ensure.NotNullOrEmpty(flagLabel, nameof(flagLabel));

      if (values == default || !values.Any()) return list;

      for (var i = 0; i < values.Count; i++)
      {
        if (values.Last() is null || string.IsNullOrWhiteSpace(values.Last()))
          throw new Exception($"Can not add blank values for flag {flagLabel}");

        var val = values.Last();
        list.Add($"{flagLabel} \"{val.Replace("\"", "-")}\"");
        values.Remove(val);
      }

      return list;
    }
  }
}