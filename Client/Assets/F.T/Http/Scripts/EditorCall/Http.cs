using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FTServer.Http.Editor
{
    public class Http
    {
        public static string RequestGET(string url)
        {
            HackCertificate();

            var request = WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            var response = request.GetResponse();
            return (new StreamReader(response.GetResponseStream())).ReadToEnd();
        }

        public static string ResquestPATCH(string url, string json)
        {
            HackCertificate();

            var request = WebRequest.Create(url);
            request.Method = "PATCH";
            request.ContentType = "application/json";
            var buffer = Encoding.UTF8.GetBytes(json);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            var response = request.GetResponse();
            return (new StreamReader(response.GetResponseStream())).ReadToEnd();
            
        }

        private static void HackCertificate()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
        }
    }
}