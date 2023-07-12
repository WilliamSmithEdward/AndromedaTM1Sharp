using System.Text;

namespace AndromedaTM1Sharp
{
    internal class CustomUtf8EncodingProvider : EncodingProvider
    {
        public override Encoding? GetEncoding(string name)
        {
            return (name ?? "").Equals("utf8") ? Encoding.UTF8 : null;
        }

        public override Encoding? GetEncoding(int codepage)
        {
            return null;
        }
    }
}
