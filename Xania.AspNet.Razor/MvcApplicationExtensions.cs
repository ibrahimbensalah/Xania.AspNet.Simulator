﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.WebPages;
using Xania.AspNet.Core;

namespace Xania.AspNet.Razor
{
    public static class MvcApplicationExtensions
    {
        public static IWebViewPage Create(this IMvcApplication mvcApplication, string virtualPath)
        {
            using (var reader = mvcApplication.OpenText(virtualPath, false))
            {
                return new WebViewPageFactory().Create(virtualPath, reader);
            }
        }

        public static IWebViewPage Create(this IMvcApplication mvcApplication, ViewContext viewContext, string virtualPath, TextReader reader)
        {
            var webPage = new WebViewPageFactory().Create(virtualPath, reader);
            webPage.Initialize(viewContext, virtualPath, mvcApplication);

            return webPage;
        }

        public static IMvcApplication EnableRazor(this IMvcApplication mvcApplication)
        {
            DisplayModeProvider.Instance.Modes.Clear();
            DisplayModeProvider.Instance.Modes.Add(new DisplayModeSimulator());

            mvcApplication.ViewEngines.Add(new RazorViewEngineSimulator(mvcApplication));

            BundleTable.MapPathMethod = mvcApplication.GetPhysicalPath;

            return mvcApplication;
        }
    }
}