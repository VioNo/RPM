using RPM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest3
    {
        [TestMethod]
        public void AuthTestSuccess()
        {
            var page = new MainPage();
            Assert.IsTrue(page.Auth("test", "test11"));
            Assert.IsTrue(page.Auth("123", "123qwe"));
            Assert.IsTrue(page.Auth("1234", "1234qwe"));
            Assert.IsTrue(page.Auth("123455", "12345qwe"));
        }
    }
}
