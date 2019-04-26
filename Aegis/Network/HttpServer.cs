using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using Aegis.Threading;



namespace Aegis.Network
{
    public enum HttpMethodType
    {
        Get,
        Post
    }





    public class HttpServer
    {
        private Thread _thread;
        public HttpListener HttpListener { get; private set; } = new HttpListener();
        private Dictionary<string, HttpRequestHandler> _routes = new Dictionary<string, HttpRequestHandler>();
        private RWLock _lock = new RWLock();


        public delegate bool PreprocessRequestDelegator(HttpRequestData request);
        public PreprocessRequestDelegator PreprocessRequestHandler { get; set; }

        public delegate void InvalidRouteDelegator(HttpRequestData request);
        public InvalidRouteDelegator InvalidRouteHandler { get; set; }
        public bool DispatchWithWorkerThread { get; set; } = false;





        public HttpServer()
        {
        }


        /// <summary>
        /// 사용할 URL 및 포트정보를 지정합니다.
        /// http:// 혹은 https:// 로 시작되어야 하며, /로 끝나는 URI로 지정해야 합니다.
        /// (ex. http://*:8080/)
        /// Start를 호출하기 전에 먼저 호출되어야 합니다.
        /// </summary>
        /// <param name="prefix"></param>
        public void AddPrefix(string prefix)
        {
            HttpListener.Prefixes.Add(prefix);
        }


        public void Start()
        {
            if (_thread != null)
                throw new AegisException(AegisResult.AlreadyInitialized);


            HttpListener.Start();

            _thread = new Thread(Run);
            _thread.Start();
        }


        public void Join()
        {
            if (_thread == null)
                throw new AegisException(AegisResult.NotInitialized);

            _thread.Join();
        }


        public void Stop()
        {
            if (_thread == null)
                return;

            HttpListener.Stop();
            _thread = null;
        }


        public void Route(string path, HttpRequestHandler handler)
        {
            if (path.Length == 0 || path[0] != '/')
                throw new AegisException(AegisResult.InvalidArgument, "The path must be a string that starts with '/'.");

            if (path.Length > 1 && path[path.Length - 1] == '/')
                path = path.Remove(path.Length - 1);


            using (_lock.WriterLock)
            {
                if (_routes.ContainsKey(path.ToLower()) == true)
                    throw new AegisException(AegisResult.InvalidArgument, "'{0}' is already exists route path.", path);

                _routes.Add(path.ToLower(), handler);
            }
        }


        private void Run()
        {
            while (HttpListener.IsListening)
            {
                try
                {
                    var context = HttpListener.GetContext();
                    ProcessContext(context);
                }
                catch (HttpListenerException e) when ((uint)e.HResult == 0x80004005)
                {
                    //  스레드 종료 또는 응용 프로그램 요청 때문에 I/O 작업이 취소되었습니다
                }
                catch (Exception e)
                {
                    Logger.Err(LogMask.Aegis, e.ToString());
                }
            }
        }


        private void ProcessContext(HttpListenerContext context)
        {
            string path, rawUrl = context.Request.RawUrl;
            if (rawUrl == "")
                return;


            string[] splitUrl = rawUrl.Split('?');
            string rawMessage;
            HttpRequestData request = null;


            //  Path 가져오기
            path = splitUrl[0].ToLower();
            if (path.Length == 0)
                return;

            if (path.Length > 1 && path[path.Length - 1] == '/')
                path = path.Remove(path.Length - 1);


            //  Query / Message Body 가져오기
            if (context.Request.HttpMethod == "GET")
            {
                if (splitUrl.Length > 1)
                {
                    rawMessage = splitUrl[1];
                    request = new HttpRequestData(HttpMethodType.Get, rawUrl, path, context, rawMessage);
                }
                else
                    request = new HttpRequestData(HttpMethodType.Get, rawUrl, path, context, "");
            }
            if (context.Request.HttpMethod == "POST")
            {
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    rawMessage = reader.ReadToEnd();
                    request = new HttpRequestData(HttpMethodType.Post, rawUrl, path, context, rawMessage);
                }
            }
            if (request == null)
                return;


            //  Routing
            using (_lock.ReaderLock)
            {
                int callCount = 0;
                foreach (KeyValuePair<string, HttpRequestHandler> route in _routes.ToList())
                {
                    if (//  정확한 path를 지정했거나
                        route.Key == path ||

                        //  *를 사용한 하위 path 전체를 지정한 경우
                        (route.Key.Length <= path.Length &&
                         route.Key[route.Key.Length - 1] == '*' &&
                         path.Substring(0, route.Key.Length - 1) == route.Key.Substring(0, route.Key.Length - 1))
                       )
                    {
                        ++callCount;
                        PostToSpinWorker(() =>
                        {
                            if (PreprocessRequestHandler == null || PreprocessRequestHandler?.Invoke(request) == true)
                                route.Value.Process(request);
                        });
                    }
                }


                if (callCount == 0)
                {
                    PostToSpinWorker(() =>
                    {
                        PreprocessRequestHandler?.Invoke(request);
                        InvalidRouteHandler?.Invoke(request);
                    });
                }
            }
        }


        private void PostToSpinWorker(Action action)
        {
            if (DispatchWithWorkerThread == true)
                SpinWorker.Work(action);
            else
                SpinWorker.Dispatch(action);
        }
    }
}
