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
            Assert.IsTrue(page.Registr("user1", "password1", "email121@mail.ru","+79878767665"));
            Assert.IsTrue(page.Registr("user2", "password2", "email2@mail.ru", "+79878767665"));
            Assert.IsTrue(page.Registr("user3", "password3", "email3@mail.ru", "+79878767665"));
            Assert.IsTrue(page.Registr("user4", "password4", "email4@mail.ru", "+79878767665"));
            Assert.IsFalse(page.Registr("user1", "password5", "email5@mail.ru", "+79878767665"));
            Assert.IsFalse(page.Registr("user6", "password2", "email6@mail.ru", "+79878767665"));

        }
    }
}
