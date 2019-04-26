using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;



namespace Aegis.Network
{
    public sealed class HttpRequestData
    {
        public readonly HttpMethodType MethodType;
        public readonly string RawUrl, Path, MessageBody;
        public readonly Encoding ContentEncoding;
        public readonly HttpListenerContext Context;

        public Dictionary<string, string> Arguments { get; } = new Dictionary<string, string>();
        public string this[string key] { get { return Arguments[key]; } }





        internal HttpRequestData(HttpMethodType methodType, string url, string path, HttpListenerContext context, string messageBody)
        {
            MethodType = methodType;
            RawUrl = url;
            Path = path;
            Context = context;
            MessageBody = messageBody;
            ContentEncoding = context.Request.ContentEncoding;
        }


        public void ParseArguments()
        {
            Arguments.Clear();

            foreach (string arg in MessageBody.Split('&'))
            {
                if (arg.Length == 0)
                    continue;


                string[] keyValue = arg.Split('=');
                if (keyValue.Length != 2)
                    throw new AegisException(AegisResult.InvalidArgument);


                Arguments[keyValue[0]] = keyValue[1];
            }
        }


        public void AppendResponseHeader(string name, string value)
        {
            Context.Response.AppendHeader(name, value);
        }


        public void Response(string result)
        {
            //  결과 전송
            try
            {
                var httpResponse = Context.Response;

                //  httpResponse는 한 번만 사용할 수 있다.
                if (httpResponse.ContentLength64 != 0)
                    return;


                byte[] buffer = Encoding.UTF8.GetBytes(result);
                httpResponse.ContentLength64 = buffer.Length;
                httpResponse.OutputStream.Write(buffer, 0, buffer.Length);
                httpResponse.OutputStream.Close();
            }
            catch (HttpListenerException e) when ((uint)e.ErrorCode == 1229)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine("------------------------------");
                Console.WriteLine(e.ToString());
            }
        }


        public void Response(string result, string contentType)
        {
            //  결과 전송
            try
            {
                var httpResponse = Context.Response;

                //  httpResponse는 한 번만 사용할 수 있다.
                if (httpResponse.ContentLength64 != 0)
                    return;


                byte[] buffer = Encoding.UTF8.GetBytes(result);
                httpResponse.ContentType = contentType;
                httpResponse.ContentLength64 = buffer.Length;
                httpResponse.OutputStream.Write(buffer, 0, buffer.Length);
                httpResponse.OutputStream.Close();
            }
            catch (HttpListenerException e) when ((uint)e.ErrorCode == 1229)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine("------------------------------");
                Console.WriteLine(e.ToString());
            }
        }
    }
}
