﻿using System.Collections.Generic;
using System.Globalization;
using System.Security.Principal;
using System.Web.Mvc;

namespace Xania.AspNet.Simulator
{
    public static class ActionRequestExtensions
    {

        public static TActionRequest Get<TActionRequest>(this TActionRequest actionRequest)
            where TActionRequest : IControllerAction
        {
            actionRequest.HttpMethod = "GET";
            return actionRequest;
        }

        public static TActionRequest Post<TActionRequest>(this TActionRequest actionRequest)
            where TActionRequest : IControllerAction
        {
            actionRequest.HttpMethod = "POST";
            return actionRequest;
        }

        public static TActionRequest Authenticate<TActionRequest>(this TActionRequest actionRequest, string userName,
            string[] roles, string identityType = "Simulator")
            where TActionRequest : IControllerAction
        {
            actionRequest.User = new GenericPrincipal(new GenericIdentity(userName, identityType),
                roles ?? new string[] {});
            return actionRequest;
        }

        public static TActionRequest RequestData<TActionRequest>(this TActionRequest actionRequest, object values)
            where TActionRequest : ControllerAction
        {
            {
                actionRequest.MvcApplication.ValueProvider = new DictionaryValueProvider<object>(values.ToDictionary(),
                    CultureInfo.CurrentCulture);
                return actionRequest;
            }
        }

        public static TActionRequest RequestData<TActionRequest>(this TActionRequest actionRequest, IDictionary<string, object> values)
            where TActionRequest : ControllerAction
        {
            {
                actionRequest.MvcApplication.ValueProvider = new DictionaryValueProvider<object>(values,
                    CultureInfo.CurrentCulture);
                return actionRequest;
            }
        }

        public static TActionRequest IsChildAction<TActionRequest>(this TActionRequest actionRequest, bool isChildAction = true)
            where TActionRequest : ControllerAction
        {
            {
                actionRequest.IsChildAction = isChildAction;
                return actionRequest;
            }
        }
    }
}