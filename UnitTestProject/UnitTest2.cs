using RPM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void AuthTest()
        {
            var page = new MainPage();
            Assert.IsTrue(page.Auth("test", "test11"));
            Assert.IsFalse(page.Auth("user1", "user123"));
            Assert.IsFalse(page.Auth("", "123йwe"));
            Assert.IsFalse(page.Auth(" ", " "));
        }
    }
}
