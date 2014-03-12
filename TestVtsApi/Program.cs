using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace TestVtsApi
{
    internal class Program
    {
        private const string Url = "http://win.vtsystem.ru/api/";

        private const string UserAgent =
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.101 Safari/537.36";

        private const string UrlEncoded = "application/x-www-form-urlencoded";

        private static void Main(string[] args)
        {
            TestTarifs();
            TestPrice();
            Console.ReadKey();
        }

        private static void TestTarifs()
        {
            Console.Write("Testing tarifs: ");
            var points = GetPoints();
            var data = GetTestData();
            Console.WriteLine(TestVtsApi(data, points));
        }

        private static void TestPrice()
        {
            Console.Write("Testing price: ");
            var points = GetPoints();
            var data = GetTestData();
            data.Add("class_id","2");
            Console.WriteLine(TestVtsApi(data, points));
        }

        private static Dictionary<string, string> GetTestData()
        {
            var dic = new Dictionary<string, string>
            {
                {"key", "6378f98dbacf0e50e7db69ba35ea14e0"},
                {"phone", "+79119438660"},
                {"time", "2014-07-25"}
            };

            return dic;
        }

        private static List<Dictionary<string, string>> GetPoints()
        {
            var point = new Dictionary<string, string>
            {
                {"street", "Шереметьевка, Псковская"},
                {"lat", "59.9608"},
                {"lon", "30.3067"}
            };
            var point2 = new Dictionary<string, string>
            {
                {"street", "Кафе Друзья(Пушкарская 42)"},
                {"lat", "59.9723"},
                {"lon", "31.0301"}
            };
            var point3 = new Dictionary<string, string>
            {
                {"street", "Генерала Симоняка"},
                {"lat", "59.8308"},
                {"lon", "30.2023"}
            };
            return new List<Dictionary<string, string>> { point, point2, point3 };
        }

        private static string TestVtsApi(Dictionary<string, string> data,List<Dictionary<string, string>> points)
        {
            var request = (HttpWebRequest)WebRequest.Create(Url);

            request.Method = "POST";
            request.AllowAutoRedirect = false;
            request.UserAgent = UserAgent;
           
            if (data != null)
            {
                request.ContentType = UrlEncoded;
                var body = data.ToUrlEncoded()+points.ToUrlEncoded();
                request.ContentLength = body.Length;

                using (var sm = request.GetRequestStream())
                {
                    using (var sw = new StreamWriter(sm))
                    {
                        sw.Write(body);
                    }
                }
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("Response status is {0}", response.StatusCode));
                    }

                    if (response.ContentLength == 0)
                    {
                        throw new Exception("Response doesn't contain content");
                    }

                    using (var rs = response.GetResponseStream())
                    {
                        using (var sr = new StreamReader(rs))
                        {
                            var content = sr.ReadToEnd();
                            return content;
                        }
                    }
                }
            }
            return null;
        }
    }

    public static class Extentions
    {
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
