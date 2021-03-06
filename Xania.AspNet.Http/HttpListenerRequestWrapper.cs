using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Routing;

namespace Xania.AspNet.Http
{
    internal class HttpListenerRequestWrapper: HttpRequestBase
    {
        private readonly HttpListenerRequest _request;
        private readonly Func<IPrincipal> _principalFunc;
        private readonly string _physicalApplicationPath;
        private readonly HttpCookieCollection _cookies;
        private readonly RequestContext _requestContext;

        private NameValueCollection _params;
        private NameValueCollection _serverVariables;
        private NameValueCollection _form;

        public HttpListenerRequestWrapper(HttpListenerRequest request, RequestContext requestContext, Func<IPrincipal> principalFunc)
        {
            _request = request;
            _principalFunc = principalFunc;
            _physicalApplicationPath = null;
            _cookies = new HttpCookieCollection();
            _requestContext = requestContext;
            foreach (Cookie cookie in _request.Cookies)
            {
                Debug.Assert(cookie != null, "cookie != null");
                _cookies.Add(new HttpCookie(cookie.Name, cookie.Value)
                {
                    Path = cookie.Path,
                    Domain = cookie.Domain
                });
            }
        }

        public override NameValueCollection Headers
        {
            get { return _request.Headers; }
        }

        public override string ContentType
        {
            get { return _request.ContentType ?? string.Empty; }
            set { throw new NotSupportedException(); }
        }

        public override Stream InputStream
        {
            get { return _request.InputStream; }
        }

        public override NameValueCollection Form
        {
            get
            {
                if (_form == null)
                {
                    using (var reader = new StreamReader(_request.InputStream))
                    {
                        _form = HttpUtility.ParseQueryString(reader.ReadToEnd());
                    }
                }
                return _form;
            }
        }

        public override NameValueCollection QueryString
        {
            get { return _request.QueryString; }
        }

        public override NameValueCollection Params
        {
            get
            {
                if (_params == null)
                {
                    _params = new NameValueCollection(_request.QueryString);
                }
                return _params;
            }
        }

        public override NameValueCollection ServerVariables
        {
            get
            {
                if (_serverVariables == null)
                {
                    _serverVariables = new NameValueCollection();
                }
                return _serverVariables;
            }
        }

        public override HttpCookieCollection Cookies
        {
            get { return _cookies; }
        }

        public override Uri Url
        {
            get { return _request.Url; }
        }

        public override string AppRelativeCurrentExecutionFilePath
        {
            get
            {
                var lastIndex = _request.RawUrl.IndexOf('?');
                if (lastIndex > 0)
                    return "~" + _request.RawUrl.Substring(0, lastIndex);
                return "~" + _request.RawUrl;
            }
        }

        public override string PhysicalApplicationPath
        {
            get { return _physicalApplicationPath; }
        }

        public override string FilePath
        {
            get { return _request.Url.AbsolutePath; }
        }

        public override string ApplicationPath
        {
            get
            {
                return "/";
            }
        }

        public override string PathInfo
        {
            get
            {
                return null;
            }
        }

        public override bool IsLocal
        {
            get { return true; }
        }

        public override bool IsAuthenticated
        {
            get
            {
                var user = _principalFunc();
                return (user != null && user.Identity != null && user.Identity.IsAuthenticated);
            }
        }

        public override string HttpMethod
        {
            get { return _request.HttpMethod; }
        }

        public override string RawUrl
        {
            get { return _request.RawUrl; }
        }

        public override string MapPath(string virtualPath)
        {
            return @"C:\asdflaksdf\asdfa\asdfasdf.cshtml";
        }

        public override string UserAgent
        {
            get { return _request.UserAgent; }
        }

        public override HttpBrowserCapabilitiesBase Browser
        {
            get { return new HttpBrowserCapabilitiesSimulator(); }
        }

        public override RequestContext RequestContext { get { return _requestContext; } }
    }

    internal class HttpBrowserCapabilitiesSimulator : HttpBrowserCapabilitiesBase
    {
        public override bool IsMobileDevice
        {
            get { return false; }
        }
    }
}