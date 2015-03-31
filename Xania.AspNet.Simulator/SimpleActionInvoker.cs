using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace Xania.AspNet.Simulator
{
    internal class SimpleActionInvoker : ControllerActionInvoker
    {
        private readonly ControllerContext _controllerContext;
        private readonly ActionDescriptor _actionDescriptor;
        private readonly FilterInfo _filterInfo;

        public SimpleActionInvoker(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IEnumerable<Filter> filters)
        {
            _controllerContext = controllerContext;
            _actionDescriptor = actionDescriptor;
            _filterInfo = new FilterInfo(filters);
        }

        public virtual ActionResult AuthorizeAction()
        {
            var authorizationContext = InvokeAuthorizationFilters(_controllerContext,
                _filterInfo.AuthorizationFilters, _actionDescriptor);

            var authorizationResult = authorizationContext.Result;
            if (authorizationResult != null)
            {
                InvokeAuthenticationFiltersChallenge(_controllerContext, _filterInfo.AuthenticationFilters, _actionDescriptor, authorizationResult);
            }

            return authorizationResult;
        }

        public virtual ActionResult InvokeAction()
        {
            return AuthorizeAction() ?? InvokeActionMethodWithFilters();
        }

        private ActionResult InvokeActionMethodWithFilters()
        {
            var controller = _controllerContext.Controller as Controller;
            if (controller != null)
            {
                controller.ViewEngineCollection = new ViewEngineCollection();
            }

            var parameters = GetParameterValues(_controllerContext, _actionDescriptor);
            var actionExecutedContext = InvokeActionMethodWithFilters(_controllerContext,
                _filterInfo.ActionFilters, _actionDescriptor, parameters);

            if (actionExecutedContext == null)
                throw new Exception("InvokeActionMethodWithFilters returned null");

            return actionExecutedContext.Result;
        }

        protected override object GetParameterValue(ControllerContext controllerContext, ParameterDescriptor parameterDescriptor)
        {
            var parameterName = parameterDescriptor.ParameterName;
            var value = base.GetParameterValue(controllerContext, parameterDescriptor);

            if (value == null)
                return null;

            var validationResults = ValidateModel(parameterDescriptor.ParameterType, value, controllerContext);

            var modelState = controllerContext.Controller.ViewData.ModelState;
            Func<ModelValidationResult, bool> isValidField = res => modelState.IsValidField(String.Format("{0}.{1}", parameterName, res.MemberName));

            foreach (var validationResult in validationResults.Where(isValidField).ToArray())
            {
                var subPropertyName = String.Format("{0}.{1}", parameterDescriptor.ParameterName, validationResult.MemberName);
                modelState.AddModelError(subPropertyName, validationResult.Message);
            }

            return value;
        }

        //protected virtual void ValidateArgument(string parameterName, Type parameterType, object parameterValue, ControllerContext controllerContext)
        //{
        //    var validationResults = ValidateModel(parameterType, parameterValue, controllerContext);

        //    var modelState = controllerContext.Controller.ViewData.ModelState;
        //    Func<ModelValidationResult, bool> isValidField = res => modelState.IsValidField(String.Format("{0}.{1}", parameterName, res.MemberName));

        //    foreach (var validationResult in validationResults.Where(isValidField).ToArray())
        //    {
        //        var subPropertyName = String.Format("{0}.{1}", parameterName, validationResult.MemberName);
        //        modelState.AddModelError(subPropertyName, validationResult.Message);
        //    }

        //}
        protected virtual IEnumerable<ModelValidationResult> ValidateModel(Type modelType, object modelValue, ControllerContext controllerContext)
        {
            var modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => modelValue, modelType);
            return ModelValidator.GetModelValidator(modelMetadata, controllerContext).Validate(null);
        }
    }
}