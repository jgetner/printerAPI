using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;

namespace Server
{
    public class ServerConfig
    {
        public string protocol = "http";
        public string host = "localhost";
        public int port = 80;
    }

    public class ServerRouteMethod
    {
        public static string GET = "get";
        public static string POST = "post";
        public static string PUT = "put";
        public static string DELETE = "delete";
        public static string OPTIONS = "options";
    }

    public class ServerRoute
    {
        public string method;
        public string route;
        public Func<ServerRequest, ServerResponse, ServerResponse> action;
    }

    public class ServerRoutes : List<ServerRoute> { }

    public class ServerListener
    {
        public ServerConfig config;
        public ServerRoutes routes;

        protected HttpListener listener;

        public ServerListener(ServerConfig config, ServerRoutes routes)
        {
            this.config = config;
            this.routes = routes;
        }

        public async void Start()
        {
            if (!HttpListener.IsSupported)
            {
                throw new Exception(
                    "HttpListener is not supported.  Please upgrade to a version supporting HttpListener.");
            }

            else if(this.routes.Count == 0)
            {
                throw new Exception("No server routes found. please add server routes to properly handle request.");
            }

            else
            {
                try
                {

                    this.listener = new HttpListener();

                    string prefix = 
                        this.config.protocol + 
                        "://" + 
                        this.config.host + 
                        ":" + 
                        this.config.port + "/";

                    this.listener.Prefixes.Add(prefix);

                    this.listener.Start();

                    Console.WriteLine("Listening on port: " + this.config.port);

                    HttpListenerContext context = await listener.GetContextAsync();
                    listener.BeginGetContext(new AsyncCallback(OnRequestReceive), listener);
                   

                }

                catch(Exception e)
                {
                    throw e;
                }
            }
        }

        private void OnRequestReceive(IAsyncResult result)
        {
            try
            {
                HttpListener _listener = (HttpListener)result.AsyncState;
                HttpListenerContext context = listener.EndGetContext(result);

                ServerRequest serverRequest = new ServerRequest(context.Request);
                ServerResponse serverResponse = new ServerResponse(context.Response);
                Console.WriteLine(serverRequest.request.Url.AbsolutePath);

                foreach (ServerRoute route in this.routes)
                {
                    Console.WriteLine("route:" + route.route);
                    if (route.route == "/" && serverRequest.request.Url.AbsolutePath == "/")
                    {
                        Console.WriteLine("MATCHED HOME URL");
                        route.action(serverRequest, serverResponse);
                        _listener.BeginGetContext(new AsyncCallback(OnRequestReceive), listener);
                        return;
                    }

                    else if (route.route != "/" && route.route != null)
                    {
                        Match matched = Regex.Match(
                            serverRequest.request.Url.AbsolutePath,
                            route.route,
                            RegexOptions.IgnoreCase);

                        //did we match the route
                        if (matched.Success)
                        {
                            Console.WriteLine("MATCHED PATTERN");
                            //did we match the request method
                            if (route.method.ToLower() == serverRequest.request.HttpMethod.ToLower())
                            {
                                route.action(serverRequest, serverResponse);
                                _listener.BeginGetContext(new AsyncCallback(OnRequestReceive), listener);
                                return;

                            }
                        }
                    }
                }

                Console.WriteLine("NOT FOUND");
                serverResponse.Status(404, "Not Found");
                serverResponse.response.Close();
                _listener.BeginGetContext(new AsyncCallback(OnRequestReceive), listener);
            }

            catch(Exception e)
            {
                throw e;
            }
        }

        public void Stop()
        {
            try
            {
                this.listener.Stop();
            }
            
            catch(Exception e)
            {
                throw e;
            }
        }
    }

    public class ServerRequest
    {
        public HttpListenerRequest request;

        public ServerRequest(HttpListenerRequest request)
        {
            this.request = request;
        }
    }

    public class ServerResponse
    {
        public HttpListenerResponse response;

        public ServerResponse(HttpListenerResponse response)
        {
            this.response = response;
        }

        public void Status(int code,  string message = "")
        {
            try
            {
                this.Respond(code, "text/html; charset=utf-8", message);
            }

            catch(Exception e)
            {
                throw e;
            }
        }

        public void Json(string json)
        {
            try
            {
                this.Respond(200, "application/json charset=utf-8", json);
            }

            catch(Exception e)
            {
                throw e;
            }
           
        }

        protected async void Respond(int code = 200, string contentType = "text/html; charset=utf-8", string contents = "")
        {
            try
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(contents);
                this.response.StatusCode = code;
                this.response.ContentType = contentType;
                this.response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                await output.WriteAsync(buffer, 0, buffer.Length);
                this.response.Close();
            }

            catch(Exception e)
            {
                throw e;
            }
           
        }
    }
}
