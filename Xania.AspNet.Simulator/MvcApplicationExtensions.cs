﻿using System;
using System.IO;
using System.Web.Mvc;
using Xania.AspNet.Core;

namespace Xania.AspNet.Simulator
{
    internal static class MvcApplicationExtensions
    {
        public static DirectControllerAction Action(this IMvcApplication mvcApplication, string controllerName, string actionName)
        {
            var controller = mvcApplication.CreateController(controllerName);

            var controllerType = controller.GetType();
            var controllerDescriptor = new ReflectedControllerDescriptor(controllerType);
            var actionDescriptor = controllerDescriptor.FindAction(controller.ControllerContext, actionName);

            return new DirectControllerAction(controller, actionDescriptor)
            {
                Output = new StringWriter()
            };
        }
    }

    internal class NotNullAttribute : Attribute
    {
    }
}