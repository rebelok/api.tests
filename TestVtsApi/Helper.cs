using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public static class Helper
    {
        public static string GetMD5(string input)
        {
            var md5Hasher = MD5.Create();
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            var sBuilder = new StringBuilder();
            foreach (var byt in data)
            {
                sBuilder.Append(byt.ToString("x2"));
            }
            return sBuilder.ToString();
        }
        public static string ToUrlEncoded(this List<Dictionary<string, string>> points)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                foreach (var kv in point)
                {
                    sb.Append("&");
                    sb.Append(WebUtility.UrlEncode(string.Format("points[{0}][{1}]", i, kv.Key)));
                    sb.Append("=");
                    sb.Append(WebUtility.UrlEncode(kv.Value));

                }
            }

            return sb.ToString();
        }
        public static string ToUrlEncoded(this Dictionary<string, string> parameters)
        {
            var sb = new StringBuilder();

            var b = true;

            foreach (var p in parameters)
            {
                if (!b)
                {
                    sb.Append("&");
                }

                b = false;

                sb.Append(p.Key);
                sb.Append("=");
                sb.Append(WebUtility.UrlEncode(p.Value));
            }

            return sb.ToString();
        }
    }
}
