using RPM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest4
    {
        [TestMethod]
        public void AuthTestNegative()
        {
            var page = new MainPage();
            Assert.IsFalse(page.Auth("user1", "12345qwe"));
            Assert.IsFalse(page.Auth("user1", "test11"));
            Assert.IsFalse(page.Auth("", "123йwe"));
            Assert.IsFalse(page.Auth("", ""));
            Assert.IsFalse(page.Auth(" ", " "));
        }
    }
}

