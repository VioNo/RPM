using Microsoft.VisualStudio.TestTools.UnitTesting;
using RPM;
using System;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest5
    {
        [TestMethod]
        public void RegTest()
        {
            var page = new MainPage();
            Assert.IsTrue(page.Registr("user1", "12345qwe", "",""));
            Assert.IsTrue(page.Registr("user1", "12345qwe", "", ""));
            Assert.IsTrue(page.Registr("user1", "12345qwe", "", ""));
            Assert.IsTrue(page.Registr("user1", "12345qwe", "", ""));
            Assert.IsFalse(page.Registr("user1", "12345qwe", "", ""));
            Assert.IsFalse(page.Registr("user1", "12345qwe", "", ""));
            Assert.IsFalse(page.Registr("user1", "12345qwe", "", ""));
            Assert.IsFalse(page.Registr("user1", "12345qwe", "", ""));

        }
    }
}
