﻿using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Xania.AspNet.Simulator
{
    public class HttpServerSimulator : IDisposable
    {
        private readonly HttpListener _listener;

        private static readonly Stopwatch Stopwatch = new Stopwatch();

        static HttpServerSimulator()
        {
            Stopwatch.Start();
        }

        public HttpServerSimulator(params string[] prefixes)
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
            }

            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            _listener = new HttpListener();

            foreach (var prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }
            _listener.Start();
        }

        public Task<HttpContextBase> GetContextAsync()
        {
            return
                _listener.GetContextAsync()
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                            return null;

                        task.Result.Response.AppendHeader("Server", "Xania");
                        return (HttpContextBase) new HttpListenerContextSimulator(task.Result);
                    });
        }

        public void Dispose()
        {
            _listener.Stop();
        }

        public async void Use(Action<HttpContextBase> handler)
        {
            bool running = true;

            while (running)
            {
                running = await GetContextAsync().ContinueWith(task =>
                {
                    var context = task.Result;
                    if (context == null)
                        return false;

                    try
                    {
                        PrintElapsedMilliseconds("handler started");
                        handler(context);
                        PrintElapsedMilliseconds("handler complete");

                        Stopwatch.Reset();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());

                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        context.Response.StatusDescription = "Internal Server Error";
                        context.Response.Write(ex.Message);
                        context.Response.Write("\n");
                        context.Response.Write(ex.StackTrace);
                    }
                    finally
                    {
                        context.Response.Close();
                    }
                    return true;
                });
            }
        }

        public static void PrintElapsedMilliseconds(string category)
        {
            Console.WriteLine("{0,-10} {1}", Stopwatch.ElapsedMilliseconds, category);
        }
    }
}
