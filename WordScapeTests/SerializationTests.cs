using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using WordScape;
namespace WordScapeTests
{
    // https://sergeyteplyakov.github.io/Blog/csharp/2024/03/21/Mythical_MissingMethodException.html

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
                await Task.Delay(2000);
                wordScapeWindow.Close();
            });
        }
        [TestMethod]
        public async Task TestSerializeWordGenerator()
        {
            LogMessage($"serialization TestSerializeWordGenerator");
            await RunInSTAExecutionContextAsync(async () =>
            {
                await Task.Yield();
                var opts = new WordGenerationParms()
                {
                    LenTargetWord = 7,
                    MinSubWordLength = 3
                };

                var wordGen = new WordGenerator(opts);
                var serOptions = new JsonSerializerOptions
                {
                    //IncludeFields = true,
                    IgnoreReadOnlyFields = false,
                };
                var json = JsonSerializer.Serialize(wordGen, serOptions);
                LogMessage("json={0}", json);
                var wordGenDeserialized = JsonSerializer.Deserialize<WordGenerator>(json, serOptions);
                // got a new random from deserialization
                Assert.AreEqual(wordGen._MinSubWordLen, wordGenDeserialized._MinSubWordLen);
                var wcont = wordGenDeserialized.GenerateWord();
                LogMessage($"NumLookups = {wcont.cntLookups} #SubWords = {wcont.subwords.Count} {wcont.InitialWord}");
            });
        }

        [TestMethod]
        public async Task TestSerializeGenGrid()
        {
            LogMessage($"serialization TestSerializeGenGrid");
            await RunInSTAExecutionContextAsync(async () =>
            {
                await Task.Yield();
                var opts = new WordGenerationParms()
                {
                    LenTargetWord = 7,
                    MinSubWordLength = 3
                };
                var wordGen = new WordGenerator(opts);
                var wcont = wordGen.GenerateWord();
                var genGrid = new GenGrid(15, 15, wcont, new Random(1));
                genGrid.Generate();
                var serOptions = new JsonSerializerOptions
                {
                    IncludeFields = true,
                    IgnoreReadOnlyFields = false,
                    //PreferredObjectCreationHandling = JsonObjectCreationHandling.Replace
                };
                serOptions.Converters.Add(new TwoDimensionalCharArrayJsonConverter());
                var json = JsonSerializer.Serialize(genGrid, serOptions);
                LogMessage("json={0}", json);
                var genGridDeserialized = JsonSerializer.Deserialize<GenGrid>(json, serOptions);
                Assert.AreEqual(genGrid._MaxX, genGridDeserialized._MaxX);
                Assert.AreEqual(genGrid._MaxY, genGridDeserialized._MaxY);
                Assert.AreEqual(genGrid.nLtrsPlaced, genGridDeserialized.nLtrsPlaced);
                Assert.AreEqual(genGrid._ltrsPlaced[0].ltr, genGridDeserialized._ltrsPlaced[0].ltr);
            });
        }
        [TestMethod]
        public async Task TestSerializeGuessedWordList()
        {
            LogMessage($"serialization TestSerializeGuessedWordList");
            await RunInSTAExecutionContextAsync(async () =>
            {
                await Task.Yield();
                var lst = new ObservableCollection<MyTextBlockWithOnlineLookup>();
                lst.Add(new MyTextBlockWithOnlineLookup()
                {
                    Text = "hello"
                });
                lst.Add(new MyTextBlockWithOnlineLookup()
                {
                    Text = "there"
                });
                var serOptions = new JsonSerializerOptions
                {
                    IncludeFields = true,
                    IgnoreReadOnlyFields = false,
                    //PreferredObjectCreationHandling = JsonObjectCreationHandling.Replace
                };
                serOptions.Converters.Add(new ObservableCollectionUiElementConverter());
                var json = JsonSerializer.Serialize(lst, serOptions);
                LogMessage("json={0}", json);
                var lstDeserialized = JsonSerializer.Deserialize<ObservableCollection<MyTextBlockWithOnlineLookup>>(json, serOptions);

            });
        }


        [TestMethod]
        public async Task TestSerializeSimple()
        {
            LogMessage($"serialization TestSerializeSimple");
            await RunInSTAExecutionContextAsync(async () =>
            {
                await Task.Yield();
                var person1 = new Person("fred", 42);
                person1.children = new List<Person>
                {
                    new Person("child1", 1),
                    new Person("child2", 2),
                };
                var serOptions = new JsonSerializerOptions
                {
                    //IncludeFields = true,
                    //IgnoreReadOnlyFields = true,
                };
                var json = JsonSerializer.Serialize(person1, serOptions);
                LogMessage("json={0}", json);
                var personDeserialized = JsonSerializer.Deserialize<Person>(json, serOptions);
                Assert.AreEqual(person1.name, personDeserialized.name);
                // get the age from reflection
                var age1 = personDeserialized.GetType().GetField("age", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(person1);
                var age2 = personDeserialized.GetType().GetField("age", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(personDeserialized);
                Assert.AreEqual(age1, age2);
            });
        }


        public class PrivateFieldConverter<T> : JsonConverter<T>
        {
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException("Deserialization is not implemented in this example.");
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                foreach (var field in typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    var fieldValue = field.GetValue(value);
                    writer.WritePropertyName(field.Name);
                    JsonSerializer.Serialize(writer, fieldValue, options);
                }

                writer.WriteEndObject();
            }
        }
        public class Person
        {
            public Person() { } // for deserialization
            public Person(string name, int age)
            {
                this.name = name;
                this.age = age;
            }
            [JsonInclude]
            public string name;
            [JsonInclude]
            int age;
            [JsonInclude]
            internal List<Person> children;
            [JsonIgnore]
            public int[] ints = new int[] { 1, 2, 3 };


        }
    }
}
