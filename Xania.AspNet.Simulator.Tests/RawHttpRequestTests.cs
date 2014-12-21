﻿using System;
using System.Web.Mvc;
using NUnit.Framework;

namespace Xania.AspNet.Simulator.Tests
{
    public class RawHttpRequestTests
    {
        [TestCase("POST /home/index HTTP/1.1\n", "POST", "/home/index", "HTTP/1.1")]
        [TestCase("GET /home/index HTTP/1.1", "GET", "/home/index", "HTTP/1.1")]
        [TestCase("OPTIONS /home/ HTTP/1.1", "OPTIONS", "/home/", "HTTP/1.1")]
        public void RequestLineTest(string requestLine, string httpMethod, string uriPath, string httpVersion)
        {
            var requestInfo = ActionRequest.Parse(requestLine);
            Assert.AreEqual(httpMethod, requestInfo.HttpMethod);
            Assert.AreEqual(uriPath, requestInfo.UriPath);
        }

        [TestCase("GET /home/index HTTP/1.1")]
        public void RouterRawRequestTest(string rawRequest)
        {
            // assert
            var router = new Router();
            router.RegisterController("home", new HomeController());
            router.RegisterDefaultRoutes();

            // act
            var controllerAction = router.ParseAction(rawRequest);

            // assert
            // TODO Assert.IsInstanceOf<HomeController>(controllerAction.Controller);
        }

        class HomeController: Controller
        {
            public ActionResult Index()
            {
                return null;
            }
        }
    }
}
