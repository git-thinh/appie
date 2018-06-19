using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace appel
{
    class api_request_url
    {
        public static string getHtml(string url)
        {
            string s = string.Empty;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls;

            //using (WebClient w = new WebClient())
            //{ 
            //    w.Encoding = Encoding.UTF7;
            //    s = w.DownloadString(url);
            //}

            //HttpWebRequest w = (HttpWebRequest)WebRequest.Create(url);
            //w.Method = "GET";
            //w.KeepAlive = false;
            //WebResponse rs = w.GetResponse();
            //StreamReader sr = new StreamReader(rs.GetResponseStream(), System.Text.Encoding.UTF8);
            //s = sr.ReadToEnd();
            //sr.Close();
            //rs.Close();

            //            var uri = new Uri(url);
            //            string req =
            //@"GET " + uri.PathAndQuery + @" HTTP/1.1
            //User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36
            //Host: " + uri.Host + @"
            //Accept: */*
            //Accept-Encoding: gzip, deflate
            //Connection: Keep-Alive 
            //";
            //            var requestBytes = Encoding.UTF8.GetBytes(req);
            //            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //            socket.Connect(uri.Host, 80);
            //            if (socket.Connected)
            //            {
            //                socket.Send(requestBytes);
            //                var responseBytes = new byte[socket.ReceiveBufferSize];
            //                socket.Receive(responseBytes);
            //                s = Encoding.UTF8.GetString(responseBytes);
            //            }
            //            s = HttpUtility.HtmlDecode(s);
            //result = CleanHTMLFromScript(result);

            //HttpWebRequest w = (HttpWebRequest)WebRequest.Create(new Uri(url));
            //w.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36";
            //w.BeginGetResponse(asyncResult =>
            //{
            //    HttpWebResponse rs = (HttpWebResponse)w.EndGetResponse(asyncResult); //add a break point here 
            //    StreamReader sr = new StreamReader(rs.GetResponseStream(), System.Text.Encoding.UTF8);
            //    s = sr.ReadToEnd();
            //    sr.Close();
            //    rs.Close();
            //    s = HttpUtility.HtmlDecode(s);
            //}, w);
















            //////            string para_url = (string)m.Input;
            //////            var uri = new Uri(para_url);
            //////            string host = uri.Host;
            //////            int port = uri.Scheme == "https" ? 443 : 80;
            //////            string req =
            //////@"GET " + uri.PathAndQuery + @" HTTP/1.1
            //////User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36
            //////Host: " + uri.Host + (uri.Scheme == "https" ? ":443" : string.Empty) + @"
            //////Accept: */*
            //////Accept-Encoding: gzip, deflate
            //////Connection: Keep-Alive 
            //////";

            //////            //////System.Net.ServicePointManager.CertificatePolicy = new MyPolicy();
            //////            //////ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

            //////            //////System.Net.ServicePointManager.DefaultConnectionLimit = 1000;
            //////            //////// active SSL 1.1, 1.2, 1.3 for WebClient request HTTPS
            //////            ////ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            //////            //////ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | (SecurityProtocolType)3072 | (SecurityProtocolType)0x00000C00 | SecurityProtocolType.Tls;


            //////            ////HttpWebRequest req = (HttpWebRequest)WebRequest.Create(para_url);
            //////            //////if (req.IgnoreCertificateErrors)
            //////            ////    //ServicePointManager.CertificatePolicy = delegate { return true; }

            //////            ////Stream receiveStream = req.GetResponse().GetResponseStream();

            //////            ////// Pipes the stream to a higher level stream reader with the required encoding format. 
            //////            ////StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            //////            ////htm = readStream.ReadToEnd();

            //////            ////readStream.Close();


            //////            //para_url = "https://www.google.com.vn/search?q=define+forget";

            //////            //////System.Net.ServicePointManager.CertificatePolicy = new MyPolicy();

            //////            //////// machineName is the host running the server application.
            //////            //////TcpClient client = new TcpClient(host, port);

            //////            //////// Create an SSL stream that will close the client's stream.
            //////            //////SslStream sslStream = new SslStream(
            //////            //////    client.GetStream(),
            //////            //////    false,
            //////            //////    new RemoteCertificateValidationCallback(ValidateServerCertificate),
            //////            //////    null
            //////            //////    );

            //////            //////// The server name must match the name on the server certificate.
            //////            //////try
            //////            //////{
            //////            //////    sslStream.AuthenticateAsClient(host);
            //////            //////}
            //////            //////catch (AuthenticationException e)
            //////            //////{
            //////            //////}


            //////            //// Connect socket
            //////            //TcpClient client = new TcpClient(host, 443);
            //////            //NetworkStream stream = client.GetStream();

            //////            //// Wrap in SSL stream
            //////            //SslStream sslStream = new SslStream(stream);
            //////            //sslStream.AuthenticateAsClient(host);

            //////            //Build request
            //////            var sb = new StringBuilder();

            //////            sb.AppendLine("GET / HTTP/1.1");
            //////            sb.AppendLine(string.Format("Host: {0}", host));
            //////            sb.AppendLine("Connection: keep-alive");
            //////            sb.AppendLine("User-Agent: CSharp");
            //////            sb.AppendLine("Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            //////            sb.AppendLine("Accept-Encoding: gzip,deflate,sdch");
            //////            sb.AppendLine("Accept-Language: en-US,en;q=0.8");
            //////            sb.AppendLine("Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.3");
            //////            sb.AppendLine();

            //////            //req = sb.ToString();
            //////            //sslStream.Write(Encoding.UTF8.GetBytes(req));
            //////            //sslStream.Flush();
            //////            //string m1 = ReadMessage(sslStream);

            //////            string m3 = SendWebRequest(host, sb.ToString(), true);

            //////            ////req = String.Format("GET {0} HTTP/1.1\r\nHost: {1}:443\r\n\r\n", uri.PathAndQuery, host);
            //////            ////// Encode a test message into a byte array.
            //////            ////// Signal the end of the message using the "<EOF>".
            //////            //////byte[] messsage = Encoding.UTF8.GetBytes(String.Format("CONNECT {0}:443  HTTP/1.1\r\nHost: {0}\r\n\r\n", host));
            //////            //////byte[] messsage = Encoding.UTF8.GetBytes(req);
            //////            //////byte[] messsage = Encoding.UTF8.GetBytes(String.Format("GET https://{0}/  HTTP/1.1\r\nHost: {0}\r\n\r\n", host));
            //////            ////sslStream.Write(Encoding.UTF8.GetBytes(req));
            //////            ////sslStream.Flush();

            //////            ////// Read message from the server.
            //////            ////string m2 = ReadMessage(sslStream);

            //////            ////sslStream.Close();
            //////            ////// Close the client connection.
            //////            ////client.Close();
            //////            ;


            //////            ////string host = uri.Host; // "encrypted.google.com";
            //////            ////string proxy = "127.0.0.1";//host;
            //////            ////int proxyPort =  18888;//443;

            //////            ////byte[] buffer = new byte[2048 * 500];
            //////            ////int bytes; 
            //////            ////// Connect socket
            //////            ////TcpClient client = new TcpClient(proxy, proxyPort);
            //////            ////NetworkStream stream = client.GetStream();

            //////            ////// Establish Tcp tunnel
            //////            ////byte[] tunnelRequest = Encoding.UTF8.GetBytes(String.Format("CONNECT {0}:443  HTTP/1.1\r\nHost: {0}\r\n\r\n", host));
            //////            ////stream.Write(tunnelRequest, 0, tunnelRequest.Length);
            //////            ////stream.Flush();

            //////            ////// Read response to CONNECT request
            //////            ////// There should be loop that reads multiple packets
            //////            ////bytes = stream.Read(buffer, 0, buffer.Length);
            //////            ////htm = Encoding.UTF8.GetString(buffer, 0, bytes);

            //////            ////// Wrap in SSL stream
            //////            ////SslStream sslStream = new SslStream(stream);
            //////            ////sslStream.AuthenticateAsClient(host);

            //////            ////// Send request
            //////            ////byte[] request = Encoding.UTF8.GetBytes(String.Format("GET https://{0}/  HTTP/1.1\r\nHost: {0}\r\n\r\n", host));
            //////            ////sslStream.Write(request, 0, request.Length);
            //////            ////sslStream.Flush();

            //////            ////// Read response
            //////            ////do
            //////            ////{
            //////            ////    bytes = sslStream.Read(buffer, 0, buffer.Length);
            //////            ////    htm = Encoding.UTF8.GetString(buffer, 0, bytes);
            //////            ////} while (bytes != 0);

            //////            ////client.Close();




            //////            return m;


            //////            //byte[] requestBytes = Encoding.UTF8.GetBytes(req);
            //////            //byte[] responseBytes;
            //////            //var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //////            //socket.Connect(uri.Host, uri.Scheme == "https" ? 443 : 80);
            //////            //if (socket.Connected)
            //////            //{
            //////            //    socket.Send(requestBytes);
            //////            //    responseBytes = new byte[socket.ReceiveBufferSize];
            //////            //    socket.Receive(responseBytes);
            //////            //    htm = Encoding.UTF8.GetString(responseBytes);
            //////            //} 























            return s;
        }

        private static string SendWebRequest(string url, string request, bool isHttps)
        {
            using (var tc = new TcpClient())
            {
                tc.Connect(url, isHttps ? 443 : 80);
                using (var ns = tc.GetStream())
                {
                    if (isHttps)
                    {
                        // Secure HTTP
                        using (SslStream ssl = new SslStream(
                            ns,
                            false,
                            new RemoteCertificateValidationCallback(ValidateServerCertificate),
                            null
                            ))
                        //using (var ssl = new SslStream(ns, false, _ValidateServerCertificate, null))
                        {
                            ssl.AuthenticateAsClient(url, null, SslProtocols.Tls, false);
                            using (var sw = new System.IO.StreamWriter(ssl))
                            {
                                using (var sr = new System.IO.StreamReader(ssl))
                                {
                                    sw.Write(request);
                                    sw.Flush();
                                    string m = sr.ReadToEnd();
                                    return m;
                                }
                            }
                        }
                    }
                    // Normal HTTP
                    using (var sw = new System.IO.StreamWriter(ns))
                    {
                        using (var sr = new System.IO.StreamReader(ns))
                        {
                            sw.Write(request);
                            sw.Flush();
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }

        // The following method is invoked by the RemoteCertificateValidationDelegate. 
        private static bool _ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Accept all certs
        }

        private static Hashtable certificateErrors = new Hashtable();

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        static string ReadMessage(SslStream sslStream)
        {
            //// Read the  message sent by the server.
            //// The end of the message is signaled using the
            //// "<EOF>" marker.
            //byte[] buffer = new byte[2048];
            //StringBuilder messageData = new StringBuilder();
            //int bytes = -1;
            //do
            //{
            //    bytes = sslStream.Read(buffer, 0, buffer.Length);

            //    // Use Decoder class to convert from bytes to UTF8
            //    // in case a character spans two buffers.
            //    Decoder decoder = Encoding.UTF8.GetDecoder();
            //    char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
            //    decoder.GetChars(buffer, 0, bytes, chars, 0);
            //    messageData.Append(chars);
            //    // Check for EOF.
            //    if (messageData.ToString().IndexOf("<EOF>") != -1)
            //    {
            //        break;
            //    }
            //} while (bytes != 0);
            //return messageData.ToString();


            // Read the  message sent by the server.
            // The end of the message is signaled using the
            // "<EOF>" marker.
            byte[] buffer = new byte[2048 * 1024];
            string data = string.Empty;
            int bytes = -1;
            do
            {
                bytes = sslStream.Read(buffer, 0, buffer.Length);
                if (bytes > 0)
                {
                    data = Encoding.UTF8.GetString(buffer);
                    break;
                }
            } while (bytes != 0);

            return data.Trim();
        }

        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }

    public class MyPolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint srvPoint,
          X509Certificate certificate, WebRequest request,
          int certificateProblem)
        {
            //Return True to force the certificate to be accepted.
            return true;
        }
    }
}
