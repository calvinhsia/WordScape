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
                await Task.Delay(1000); //allow time for window to load, 2nd puzzle to be generated
                wordScapeWindow.Show();
                var nTimes = 0;
                while (!wordScapeWindow.TimerIsEnabled)
                {
                    await Task.Delay(1000);
                    LogMessage($"waiting for timer to start");
                    if (nTimes++ > 10)
                    {
                        throw new Exception("Timer never started");
                    }
                }

                //await wordScapeWindow.taskGenNextPuzzle;
                await Task.Delay(5000);
                wordScapeWindow.Close();
            });

        }
    }
}
