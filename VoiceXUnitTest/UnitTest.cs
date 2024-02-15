using VoiceX;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VoiceX.Views;

namespace VoiceXUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        
        [TestMethod]
        public async void TestMethod1()
        {
            RegistrationPage registration = new RegistrationPage();
            await registration.NetworkLogin("088146");
        }
    }
}
