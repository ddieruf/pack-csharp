using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
  /// <summary>
  ///   A workaround when including targetframework netstandard2.1 with lang 9.0
  ///   https://github.com/dotnet/roslyn/issues/45510
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  internal class IsExternalInit
  {
  }
}