using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        [TestMethod]
        public async Task TestSerializeSimple()
        {
            LogMessage($"serialization TestSerializeSimple");
            await RunInSTAExecutionContextAsync(async () =>
            {
                await Task.Yield();
                var testSerialization = new TestDataForSerialization() { name = "fred", age = 42 };
                var serOptions = new JsonSerializerOptions
                {
                    IncludeFields = true,
                    IgnoreReadOnlyFields = true,
                };
                var json = JsonSerializer.Serialize(testSerialization, serOptions);
                LogMessage("json={0}", json);
                var testSerialization2 = JsonSerializer.Deserialize<TestDataForSerialization>(json, serOptions);
                Assert.AreEqual(testSerialization.name, testSerialization2.name);
                Assert.AreEqual(testSerialization.age, testSerialization2.age);
            });
        }

        public class TestDataForSerialization
        {
            //[JsonInclude]
            public string name;
            public int age { get; set; }
        }
    }
}
