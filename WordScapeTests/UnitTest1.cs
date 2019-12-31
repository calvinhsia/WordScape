using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WordScapeTests
{
    [TestClass]
    public class UnitTest1: BaseTestClass
    {
        [TestMethod]
        public void TestGetWord()
        {
            LogMessage("Starting");
            var r = new Random(1);
            var dict = new DictionaryLib.DictionaryLib(DictionaryLib.DictionaryType.Small, r);
            for (int i = 0; i < 10; i++)
            {
                LogMessage($"{dict.RandomWord()}");
            }
        }
    }
}
