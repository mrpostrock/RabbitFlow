using System.Text;

namespace RabbitFlow.Core.Extensions;

public static class ByteArrayExtensions
{
    extension(byte[] bytes)
    {
        public string ToReadableString()
        {
            if (bytes.Length == 0)
                return string.Empty;
            
            return Encoding.UTF8.GetString(bytes);
        }
    }
}