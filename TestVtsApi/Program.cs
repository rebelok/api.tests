using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace TestVtsApi
{
    internal class Program
    {
        private const string Url = "http://vtsystem.ru/api/";
        private const string Password = "818828";
        private const string ClientPhone = "+79119438660";
        private const string UserAgent =
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.101 Safari/537.36";

        private const string SecretKey = "aa40c527003225813a4d59866bc682ad";

        private const string UrlEncoded = "application/x-www-form-urlencoded";

        enum TestScenario
        {
            GetPassword,
            CheckPassword,
            RecoverPassword,
            NormalFlow
        }
        private static void Main(string[] args)
        {
            var testScenario = TestScenario.NormalFlow;
            switch (testScenario)
            {
                case TestScenario.GetPassword:
                    Console.WriteLine(TestGetPassword());
                    break;
                case TestScenario.CheckPassword:
                    Console.WriteLine(TestCheckPassword());
                    break;
                case TestScenario.RecoverPassword:
                    Console.WriteLine(TestRecoverPassword());
                    break;
                case TestScenario.NormalFlow:
                    Console.WriteLine(TestGetServices());
                    Console.WriteLine(TestTarifs());
                    var order = TestPrice();
                    Console.WriteLine(order);
                    Console.WriteLine(TestOrderInfo(order, true));
                    Console.WriteLine(TestOrderInfo(order, false));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }



            Console.ReadKey();
        }
        private static Dictionary<string, string> GetTestData()
        {
            var data = new Dictionary<string, string>
            {
                {"key", SecretKey},
                {"client_phone", ClientPhone},
                {"time", DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")}
            };

            return data;
        }
        private static string TestGetPassword()
        {
            Console.Write("Testing GetPassword: ");
            var data = GetTestData();
            data.Add("register", "1");
            data.Remove("time");
            return (TestVtsApi("POST", data.ToUrlEncoded()));
        }

        private static string TestRecoverPassword()
        {
            Console.Write("Testing RecoverPassword: ");
            var data = GetTestData();
            data.Add("recover", "1");
            return (TestVtsApi("POST", data.ToUrlEncoded()));
        }

        private static string TestCheckPassword()
        {
            Console.Write("Testing CheckPassword: ");
            var data = GetTestData();
            data.Add("password", GetMD5(Password));
            return (TestVtsApi("POST", data.ToUrlEncoded()));
        }

        private static string TestGetServices()
        {
            Console.Write("Testing GetServices: ");
            var data = GetTestData();
            data.Add("get_services", "1");
            return (TestVtsApi("POST", data.ToUrlEncoded()));
        }

        private static string TestOrderInfo(string orderId, bool fullInfo)
        {
            Console.Write("Testing OrderInfo {0}: ", fullInfo ? "full" : string.Empty);
            var data = GetTestData();
            data.Add("order_id", orderId);
            data.Add("type", fullInfo ? "1" : "0");
            return (TestVtsApi("POST", data.ToUrlEncoded()));
        }

        private static string TestTarifs()
        {
            Console.Write("Testing tarifs: ");
            var points = GetPoints();
            var data = GetTestData();
            return (TestVtsApi("POST", GetData(data, points)));
        }

        private static string TestPrice()
        {
            Console.Write("Testing price: ");
            var points = GetPoints();
            var data = GetTestData();
            data.Add("class_id", "2");
            return (TestVtsApi("POST", GetData(data, points)));
        }



        private static string GetMD5(string input)
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

        private static string GetData(Dictionary<string, string> data, List<Dictionary<string, string>> points)
        {
            return data.ToUrlEncoded() + points.ToUrlEncoded();
        }

        private static string TestVtsApi(string method, string data)
        {
            var request = (HttpWebRequest)WebRequest.Create(Url);

            request.Method = method;
            request.AllowAutoRedirect = false;
            request.UserAgent = UserAgent;


            request.ContentType = UrlEncoded;
            if (data != null)
            {
                request.ContentLength = data.Length;

                using (var sm = request.GetRequestStream())
                {
                    using (var sw = new StreamWriter(sm))
                    {
                        sw.Write(data);
                    }
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
                    using (var sr = new StreamReader(rs, Encoding.UTF8))
                    {
                        var content = sr.ReadToEnd();
                        return content;
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
