﻿using System;
using System.IO;
using NUnit.Framework;

namespace Xania.AspNet.TagHelpers.Tests
{
    public class TagHelperTests
    {
        // Html tags
        [TestCase("< a >< / a>", "<a></a>")]
        [TestCase("<a attrs />", "<a attrs=\"\" />")]
        [TestCase("<a       attrs />", "<a attrs=\"\" />")]
        [TestCase("<a    attrs=asdfas a=b />", "<a attrs=\"asdfas\" a=\"b\" />")]
        [TestCase("<a a=\"b\" />", "<a a=\"b\" />")]
        [TestCase("<a><b >x< /b></a>", "<a><b>x</b></a>")]
        [TestCase("<a a=\"b\" />", "<a a=\"b\" />")]
        [TestCase("<c P1=\"a &euro;\" />", "<div><h1>a &euro;<span></span></h1></div>")]
        [TestCase("<c P1=\"a &euro;\" >x</c>", "<div><h1>a &euro;<span>X</span></h1></div>")]
        [TestCase("<br/>", "<br />")]
        [TestCase("<br>", "<br>")]

        // Comment and tag declaration
        [TestCase("<!>", "<!>")]
        [TestCase("<! ... >>", "<! ... >>")]
        [TestCase("<!-- comment -->", "<!-- comment -->")]
        [TestCase("<!-- > < c > -->", "<!-- > < c > -->")]

        // Tag with namespace
        [TestCase("<div></div>", "<div></div>")]
        [TestCase("<xn:div></xn:div>", "<div>xn</div>")]

        public void TransformTest(String input, string expected)
        {
            // arrange
            var writer = new StringWriter();
            var tagHelperProvider = new TagHelperContainer();
            tagHelperProvider.Register<TagC>("c");
            tagHelperProvider.Register<TagWithNs>("xn:div");
            var mng = new HtmlProcessor(writer, tagHelperProvider);
            // act
            foreach(var ch in input)
                mng.Write(ch);
            // assert
            Assert.AreEqual(expected, writer.GetStringBuilder().ToString());
        }

        [TestCase("<foo controller=\"Home\" action=\"Index\" target=\"_blank\">Home</foo>", "<a href=\"/home/index\" target=\"_blank\">Home</a>")]
        public void ActionLinkTest(String input, string expected)
        {
            // arrange
            var writer = new StringWriter();
            var tagHelperProvider = new TagHelperContainer();
            tagHelperProvider.Register<TestTagHelper>("foo");

            var mng = new HtmlProcessor(writer, tagHelperProvider);
            var bytes = writer.Encoding.GetBytes(input);
            // act
            mng.Write(bytes, 0, bytes.Length);
            // assert
            Assert.AreEqual(expected, writer.GetStringBuilder().ToString());
        }

        [Test]
        public void BindPropertiesAreSet()
        {
            // arrange
            var tagHelperProvider = new TagHelperContainer();
            tagHelperProvider.Register<TestTagHelper>("foo");

            // act
            var tagHelper = (TestTagHelper) tagHelperProvider
                .GetTagHelper("foo", new[] {new TagAttribute("controller", "Home"), new TagAttribute("action", "Index")});

            // assert
            Assert.AreEqual("Home", tagHelper.Controller);
            Assert.AreEqual("Index", tagHelper.Action);
        }
    }
}
