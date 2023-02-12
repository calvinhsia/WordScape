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
        public void TestGridResizeSmaller()
        {
            var wordGen = new WordGenerator(new Random(1), minSubWordLength:5, TargetLen:8)
            {
                _MinSubWordLen = 5
            };
            for (int i = 0; i < 100; i++)
            {
                var wcont = wordGen.GenerateWord();
                LogMessage($"NumLookups = {wcont.cntLookups} #SubWords = {wcont.subwords.Count} {wcont.InitialWord}");
                foreach (var sword in wcont.subwords)
                {
//                    LogMessage($"   {sword}");
                }
                var genGrid = new GenGrid(12, 12, wcont, wordGen._rand);
                genGrid.PlaceWords();
                LogMessage($"  ({genGrid._MaxX},{genGrid._MaxY})  ({genGrid._tmpminX},{genGrid._tmpminY}) ({genGrid._tmpmaxX},{genGrid._tmpmaxY}) {genGrid.ShowGrid()}");

                genGrid.ResizeGridArraySmaller();
                LogMessage($"  ({genGrid._MaxX},{genGrid._MaxY})  ({genGrid._tmpminX},{genGrid._tmpminY}) ({genGrid._tmpmaxX},{genGrid._tmpmaxY}) {genGrid.ShowGrid()}");
            }
        }
        [TestMethod]
        public void TestGenGrid()
        {
            var wordGen = new WordGenerator(new Random(1), minSubWordLength: 3, TargetLen: 7);
            for (int i = 0; i < 100; i++)
            {
                var wcont = wordGen.GenerateWord();
                LogMessage($"NumLookups = {wcont.cntLookups} #SubWords = {wcont.subwords.Count} {wcont.InitialWord}");
                foreach (var sword in wcont.subwords)
                {
                    LogMessage($"   {sword}");
                }
                var genGrid = new GenGrid(12, 12, wcont, wordGen._rand);
                LogMessage($"{genGrid.ShowGrid()}");
                LogMessage($"Grid Ltrs= {genGrid.nLtrsPlaced} Wrds= {genGrid.NumWordsPlaced}");
            }
        }

        [TestMethod]
        public void TestGetWord()
        {
            LogMessage("Starting");
            var wordGen = new WordGenerator(new Random(1), TargetLen:7);

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
