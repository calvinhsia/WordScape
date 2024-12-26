using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace WordScapeTests
{
    [TestClass]
    public class SerializationTests : BaseTestClass
    {
        [TestMethod]
        public async Task TestSerialization()
        {
            LogMessage($"serialization test");
            await RunInSTAExecutionContextAsync(async () =>
            {
                await Task.Yield();
                var wordScapeWindow = new WordScape.WordScapeWindow();
                wordScapeWindow._WordScapeOptions._Random = new Random(1);
                wordScapeWindow.Show();
                await Task.Delay(15000);
                wordScapeWindow.Close();
            });

        }
    }
}
