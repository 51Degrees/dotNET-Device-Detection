#if !NET40
namespace System.Runtime.Serialization
{
    /// <summary>
    /// Required for .NET Standard build to work.
    /// </summary>
    public class SerializableAttribute : Attribute
    {
    }
}
#endif