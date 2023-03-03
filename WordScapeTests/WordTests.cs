using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using DictionaryLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordScape;

namespace WordScapeTests
{
    [TestClass]
    public class WordTests : BaseTestClass
    {
        [TestMethod]
        public void TestGridResizeSmaller()
        {
            var opts = new WordGenerationParms()
            {
                LenTargetWord = 8,
                MinSubWordLength = 5
            };
            var wordGen = new WordGenerator(opts);
            for (int i = 0; i < 100; i++)
            {
                var wcont = wordGen.GenerateWord();
                LogMessage($"NumLookups = {wcont.cntLookups} #SubWords = {wcont.subwords.Count} {wcont.InitialWord}");
                foreach (var sword in wcont.subwords)
                {
                    //                    LogMessage($"   {sword}");
                }
                var genGrid = new GenGrid(12, 12, wcont, opts._Random);
                genGrid.PlaceWords();
                LogMessage($"  ({genGrid._MaxX},{genGrid._MaxY})  ({genGrid._tmpminX},{genGrid._tmpminY}) ({genGrid._tmpmaxX},{genGrid._tmpmaxY}) {genGrid.ShowGrid()}");

                genGrid.ResizeGridArraySmaller();
                LogMessage($"  ({genGrid._MaxX},{genGrid._MaxY})  ({genGrid._tmpminX},{genGrid._tmpminY}) ({genGrid._tmpmaxX},{genGrid._tmpmaxY}) {genGrid.ShowGrid()}");
            }
        }
        [TestMethod]
        public void TestGenGrid()
        {
            var opts = new WordGenerationParms()
            {
                LenTargetWord = 7,
                MinSubWordLength = 3
            };

            var wordGen = new WordGenerator(opts);
            for (int i = 0; i < 100; i++)
            {
                var wcont = wordGen.GenerateWord();
                LogMessage($"NumLookups = {wcont.cntLookups} #SubWords = {wcont.subwords.Count} {wcont.InitialWord}");
                foreach (var sword in wcont.subwords)
                {
                    LogMessage($"   {sword}");
                }
                var genGrid = new GenGrid(12, 12, wcont, opts._Random);
                LogMessage($"{genGrid.ShowGrid()}");
                LogMessage($"Grid Ltrs= {genGrid.nLtrsPlaced} Wrds= {genGrid.NumWordsPlaced}");
            }
        }

        [TestMethod]
        public void TestGotDictionary()
        {
            LogMessage($"test {nameof(TestGotDictionary)}");
            var asm = typeof(DictionaryLib.DictionaryLib).Assembly;
            //            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var names = asm.GetManifestResourceNames(); // "Dictionary.Properties.Resources.resources"

            var resInfo = asm.GetManifestResourceInfo(names[0]);

            var resdata = asm.GetManifestResourceStream(names[0]);
            var resman = new System.Resources.ResourceManager("DictionaryLib.Properties.Resources", System.Reflection.Assembly.LoadFrom(asm.Location));

            /// Note: getting resource throws filenotfound exception for DictionaryLib.resources event though it's embedded: it gets caught and swallowed
            var dict1 = (byte[])resman.GetObject("dict1"); //large
            LogMessage($"test {nameof(TestGotDictionary)} Large = {dict1.Length}");

            var dict2 = (byte[])resman.GetObject("dict2");
            var dictSmall = new DictionaryLib.DictionaryLib(DictionaryType.Small);
            var dictLarge = new DictionaryLib.DictionaryLib(DictionaryType.Large);
            LogMessage($"test {nameof(TestGotDictionary)} done");

        }
        [TestMethod]
        public void TestGetWord()
        {

            LogMessage("Starting");
            var opts = new WordGenerationParms()
            {
                LenTargetWord = 7,
                MinSubWordLength = 3
            };
            var wordGen = new WordGenerator(opts);

            for (int i = 0; i < 100; i++)
            {
                var wcont = wordGen.GenerateWord();
                LogMessage($"NumLookups = {wcont.cntLookups} #SubWords = {wcont.subwords.Count} {wcont.InitialWord}");
                foreach (var subword in wcont.subwords)
                {
                    LogMessage($"   {subword}");
                }
            }
        }


    }


}
