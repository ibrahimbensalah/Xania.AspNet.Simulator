using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using Xania.AspNet.Core;
using Xania.AspNet.Simulator.Http;
using IControllerFactory = Xania.AspNet.Core.IControllerFactory;

namespace Xania.AspNet.Simulator
{
    public class MvcApplication : IMvcApplication
    {
        public MvcApplication(IContentProvider contentProvider)
            : this(new ControllerContainer(), contentProvider)
        {
        }

        public MvcApplication(IControllerFactory controllerFactory, IContentProvider contentProvider)
        {
            if (controllerFactory == null)
                throw new ArgumentNullException("controllerFactory");
            if (contentProvider == null)
                throw new ArgumentNullException("contentProvider");

            ControllerFactory = controllerFactory;
            ContentProvider = contentProvider;

            Routes = GetRoutes();
            ViewEngines = new ViewEngineCollection();
            Bundles = new BundleCollection();
            FilterProviders = new FilterProviderCollection();
            foreach (var provider in System.Web.Mvc.FilterProviders.Providers)
            {
                FilterProviders.Add(provider);
            }

            ModelMetadataProvider = ModelMetadataProviders.Current;
        }

        public ModelMetadataProvider ModelMetadataProvider { get; set; }

        public ViewEngineCollection ViewEngines { get; private set; }

        public IWebViewPageFactory WebViewPageFactory { get; set; }

        public RouteCollection Routes { get; private set; }

        public BundleCollection Bundles { get; private set; }

        public FilterProviderCollection FilterProviders { get; private set; }

        public IValueProvider ValueProvider { get; set; }

        public IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            // Use empty value provider by default to prevent use of ASP.NET MVC default value providers
            // Its not the purpose of this simulator framework to validate the ASP.NET MVC default value 
            // providers. Either a value provider is not need in case model values are predefined or a 
            // custom implementation is provided.
            var valueProviders = new ValueProviderCollection();
            if (ValueProvider != null)
                valueProviders.Add(ValueProvider);
            valueProviders.Add(new SimulatorValueProvider(controllerContext, new CultureInfo("nl-NL")));

            return valueProviders;
        }

        public IEnumerable<ModelValidationResult> ValidateModel(Type modelType, object modelValue, ControllerContext controllerContext)
        {
            var modelMetadata = ModelMetadataProvider.GetMetadataForType(() => modelValue, modelType);
            var validator = ModelValidator.GetModelValidator(modelMetadata, controllerContext);

            return validator.Validate(null);
        }

        public IEnumerable<string> Assemblies
        {
            get
            {
                var result = new Dictionary<string, string>();
                AddAssembly<Uri>(result);
                AddAssembly<Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo>(result);
                AddAssembly<EnumerableQuery>(result);
                AddAssembly<CookieProtection>(result);

                foreach (var assemblyPath in ContentProvider.GetFiles("bin\\*.dll"))
                {
                    var i = assemblyPath.LastIndexOf('\\') + 1;
                    var fullName = assemblyPath.Substring(i);

                    result.Add(fullName, assemblyPath);
                }

                var runtimeAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .GroupBy(a => a.FullName)
                    .Select(grp => grp.First())
                    .Select(a => new { Name = a.GetName().Name + ".dll", a.Location })
                    .Where(a => !result.ContainsKey(a.Name) && !string.IsNullOrWhiteSpace(a.Location))
                    .ToArray();

                foreach (var assembly in runtimeAssemblies)
                {
                    result.Add(assembly.Name, assembly.Location);
                }

                return result.Values;
            }
        }

        private static void AddAssembly<T>(Dictionary<string, string> result)
        {
            result.Add(typeof(T).Assembly.GetName().Name + ".dll", typeof(T).Assembly.Location);
        }

        public IContentProvider ContentProvider { get; private set; }

        public IControllerFactory ControllerFactory { get; private set; }

        public static RouteCollection GetRoutes()
        {
            var routes = new RouteCollection(new ActionRouterPathProvider());

            if (RouteTable.Routes.Any())
                foreach (var r in RouteTable.Routes)
                    routes.Add(r);
            else
                routes.MapRoute(
                    "Default",
                    "{controller}/{action}/{id}",
                    new { controller = "Home", action = "Index", id = UrlParameter.Optional }
                    );

            return routes;
        }

        public Stream Open(string virtualPath)
        {
            var filePath = ToFilePath(virtualPath);
            return ContentProvider.Open(filePath);
        }

        private string ToFilePath(string virtualPath)
        {
            return virtualPath.Substring(2).Replace("/", "\\");
        }

        public bool Exists(string virtualPath)
        {
            var relativePath = ToFilePath(virtualPath);
            return ContentProvider.Exists(relativePath);
        }

        public string MapPath(string virtualPath)
        {
            var relativePath = ToFilePath(virtualPath);
            return ContentProvider.GetPhysicalPath(relativePath);
        }

        public string ToAbsoluteUrl(string path)
        {
            if (path.StartsWith("~"))
                return path.Substring(1);
            return path;
        }

        public IVirtualContent GetVirtualContent(string virtualPath)
        {
            return new FileVirtualContent(ContentProvider, virtualPath);
        }

        public string MapUrl(FileInfo file)
        {
            var relativePath = ContentProvider.GetRelativePath(file.FullName);
            return String.Format("/{0}", relativePath.Replace("\\", "/"));
        }

        public IHtmlString Action(ViewContext viewContext, string actionName, object routeValues)
        {
            var controllerName = viewContext.RouteData.GetRequiredString("controller");
            var controller = ControllerFactory.CreateController(viewContext.HttpContext, controllerName);

            var partialOutput = new StringWriter();

            var action = this.Action(controller, actionName)
                .RequestData(routeValues);

            action.HttpContext = new HttpContextDecorator(viewContext.HttpContext)
            {
                Response = {Output = partialOutput}
            };

            action.Execute();

            return MvcHtmlString.Create(partialOutput.ToString());
        }

        public static MvcApplication CreateDefault()
        {
            return new MvcApplication(new ControllerContainer(), new DirectoryContentProvider(AppDomain.CurrentDomain.BaseDirectory));
        }
    }
}