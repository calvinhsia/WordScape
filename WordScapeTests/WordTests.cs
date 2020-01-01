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

            for (int i = 0; i < 1000; i++)
            {
                var wcont = wordGen.GenerateWord(Targetlen: 7, numMaxSubWords: 1500);
                LogMessage($"NumLookups = {wcont.cntLookups} #SubWords = {wcont.subwords.Count} {wcont.InitialWord}");
                foreach (var subword in wcont.subwords)
                {
                    LogMessage($"   {subword}");
                }
            }
        }
        [TestMethod]
        public void TestPermute()
        {
            LogMessage("Starting");
            var word = "per";
            DictionaryLib.DictionaryLib.PermuteString(word, LeftToRight: true, (str) =>
             {
                 LogMessage($"{str}");
                 return true;
             });

        }

        public class GenGrid
        {
            readonly WordContainer _wordContainer;
            readonly public Random _random;
            int nWordsPlaced;
            int _nRows;
            int _nCols;
            public char[,] _chars;
            public GenGrid(int rows, int cols, WordContainer wordCont, Random rand)
            {
                this._random = rand;
                this._wordContainer = wordCont;
                this._nRows = rows;
                this._nCols = cols;
                _chars = new char[rows, cols];
                for (int i = 0; i < _nRows; i++)
                {
                    for (int j = 0; j < _nCols; j++)
                    {
                        _chars[i, j] = '_';
                    }
                }
                foreach (var subword in wordCont.subwords)
                {
                    if (nWordsPlaced++ == 0)
                    {
                        if (_random.NextDouble() < .5) // leftToRight
                        {
                            var row = _random.Next(_nRows);
                            var col = _random.Next(_nCols - subword.Length);
                            foreach (var ltr in subword)
                            {
                                _chars[row, col++] = ltr;
                            }
                        }
                        else
                        { // up/down
                            var col = _random.Next(_nCols);
                            var row = _random.Next(_nRows - subword.Length);
                            foreach (var ltr in subword)
                            {
                                _chars[row++, col] = ltr;
                            }

                        }
                    }
                }
            }
            public string ShowGrid()
            {
                var grid = string.Empty;
                for (int i = 0; i < _nRows; i++)
                {
                    for (int j = 0; j < _nCols; j++)
                    {
                        grid += _chars[i, j].ToString();
                    }
                    grid += Environment.NewLine;
                }
                return grid;
            }
        }

    }


}
