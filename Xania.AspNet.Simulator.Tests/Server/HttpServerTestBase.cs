﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xania.AspNet.Simulator.Tests.Server
{
    public class HttpServerTestBase
    {
        private int _port = 8080;

        private string _baseUrl;
        protected HttpServerSimulator Server { get; private set; }

        [SetUp]
        public virtual void StartServer()
        {
            _baseUrl = String.Format("http://localhost:{0}/", _port);
            Server = new HttpServerSimulator(_baseUrl);
        }

        [TearDown]
        public virtual void StopServer()
        {
            Server.Dispose();
            _port++;
        }

        protected string GetUrl(string path)
        {
            if (path.StartsWith("/"))
                throw new ArgumentException("path should not start with '/'");

            return _baseUrl + path;
        }

    }
}
