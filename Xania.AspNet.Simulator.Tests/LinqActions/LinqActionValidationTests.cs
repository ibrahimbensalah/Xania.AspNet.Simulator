using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentAssertions;
using NUnit.Framework;

namespace Xania.AspNet.Simulator.Tests.LinqActions
{
    public class LinqActionValidationTests
    {
        [TestCase(null, null, false, false)]
        [TestCase("pass1", "pass1", true, true)]
        [TestCase("pass1", "pass2", true, false)]
        public void ModelStateTest(string newPassword, string confirmPassword, bool newPasswordValid, bool confirmPasswordValid)
        {
            // arrange
            // var model = new ChangePasswordModel { NewPassword = newPassword, ConfirmPassword = confirmPassword };
            var controllerAction = new AccountController()
                .Action("ChangePassword")
                .RequestData(new Dictionary<string, object>()
                {
                    {"newpassword", newPassword},
                    {"confirmPassword", confirmPassword}
                });

            // act
            var modelState = controllerAction.ValidateRequest();

            // assert
            Assert.AreEqual(newPasswordValid, modelState.IsValidField("model.NewPassword"));
            Assert.AreEqual(confirmPasswordValid, modelState.IsValidField("model.ConfirmPassword"));
        }

        [Test]
        public void PrivitiveParameterTest()
        {
            // arrange
            var controllerAction = new AccountController().Action(e => e.DeleteUser(1));
            // act
            var result = (ContentResult)controllerAction.GetActionResult();
            //assert
            Assert.AreEqual("Deleting User 1", result.Content);
        }

        [Test]
        public void ShouldNotValidateModelWhenGetActionResult()
        {
            // arrange
            var invalidModel = new ChangePasswordModel {  };
            var controllerAction = new AccountController().Action(c => c.ChangePassword(invalidModel));
            var context = controllerAction.GetExecutionContext();

            // act
            controllerAction.GetActionResult(context);
            //assert
            context.ModelState.IsValid.Should().BeTrue();
        }

        [Test]
        public void ExpressionParameterTest()
        {
            // arrange
            var controllerAction = new AccountController()
                .Action(e => e.DeleteUser(0))
                .RequestData(new Dictionary<string, object>
                {
                    {"userId", 1 }
                });
            // act
            var result = (ContentResult)controllerAction.GetActionResult();
            //assert
            Assert.AreEqual("Deleting User 1", result.Content);
        }

        private class AccountController : Controller
        {
            public ActionResult ChangePassword(ChangePasswordModel model)
            {
                if (model == null) throw new ArgumentNullException("model");

                return null;
            }

            public string DeleteUser(int? userId)
            {
                return String.Format("Deleting User {0}", userId);
            }
        }

        private class ChangePasswordModel
        {
            [Required]
            public String NewPassword { get; set; }

            [Required, Compare("NewPassword")]
            public String ConfirmPassword { get; set; }
        }
    }
}