using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordScape;

namespace WordScapeTests
{
    [TestClass]
    public class WordTests : BaseTestClass
    {
        [TestMethod]
        public void TestGenGrid()
        {
            var wordGen = new WordGenerator(new Random(1));
            for (int i = 0; i < 100; i++)
            {
                var wcont = wordGen.GenerateWord(Targetlen: 7, numMaxSubWords: 1500);
                LogMessage($"NumLookups = {wcont.cntLookups} #SubWords = {wcont.subwords.Count} {wcont.InitialWord}");
                foreach (var sword in wcont.subwords)
                {
                    LogMessage($"   {sword}");
                }
                var genGrid = new GenGrid(10, 10, wcont, wordGen._rand);
                var gr = Environment.NewLine + genGrid.ShowGrid();
                LogMessage($"{gr}");
            }
        }

        [TestMethod]
        public void TestGetWord()
        {
            LogMessage("Starting");
            var wordGen = new WordGenerator(new Random(1));

            for (int i = 0; i < 100; i++)
            {
                var wcont = wordGen.GenerateWord(Targetlen: 7, numMaxSubWords: 1500);
                LogMessage($"NumLookups = {wcont.cntLookups} #SubWords = {wcont.subwords.Count} {wcont.InitialWord}");
                foreach (var subword in wcont.subwords)
                {
                    LogMessage($"   {subword}");
                }
            }
        }


    }


}
