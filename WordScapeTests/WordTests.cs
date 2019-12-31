using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WordScapeTests
{
    [TestClass]
    public class WordTests : BaseTestClass
    {
        [TestMethod]
        public void TestGetWord()
        {
            LogMessage("Starting");
            var wc = new WordGenerator(new Random(1));

            for (int i = 0; i < 10; i++)
            {
                var wcont = wc.GenerateWord(Targetlen: 7, numMaxSubWords: 15);
                LogMessage($"{wcont.InitialWord}");
                foreach (var subword in wcont.subwords)
                {
                    LogMessage($"   {subword}");
                }
            }
        }
    }


    public class WordContainer
    {
        public string InitialWord;
        public List<string> subwords = new List<string>();

    }
    public class WordGenerator
    {
        readonly Random _rand;
        readonly DictionaryLib.DictionaryLib _dictionaryLib;
        const int MinSubWordLen = 3;
        public WordGenerator(Random rand = null)
        {
            if (rand == null)
            {
                _rand = new Random();
            }
            else
            {
                _rand = rand;
            }
            _dictionaryLib = new DictionaryLib.DictionaryLib(DictionaryLib.DictionaryType.Small, _rand);
        }

        public WordContainer GenerateWord(int Targetlen, int numMaxSubWords)
        {
            var word = string.Empty;
            while (word.Length != Targetlen)
            {
                word = _dictionaryLib.RandomWord();
            }
            var wc = new WordContainer()
            {
                InitialWord = word
            };
            DictionaryLib.DictionaryLib.PermuteString(word, LeftToRight: true, (str) =>
               {
                   //if (!wc.subwords.Contains(str))
                   //{
                   //    wc.subwords.Add(str);
                   //}
                   //return true;
                   for (int i = MinSubWordLen; i < str.Length; i++)
                   {
                       var testWord = str.Substring(0, i);
                       var partial = _dictionaryLib.SeekWord(testWord, out var compResult);
                       if (!string.IsNullOrEmpty(partial) && compResult == 0)
                       {
                           if (!wc.subwords.Contains(testWord))
                           {
                               wc.subwords.Add(testWord);
                               return (wc.subwords.Count != numMaxSubWords);
                           }
                       }
                       else
                       {
                           if (!partial.StartsWith(testWord))
                           {
                               //break;

                           }
                       }
                   }
                   //if (_dictionaryLib.IsWord(str))
                   //{
                   //    if (!wc.subwords.Contains(str))
                   //    {
                   //        wc.subwords.Add(str);
                   //        return (wc.subwords.Count != numMaxSubWords);
                   //    }
                   //}
                   return true; // continue
               });
            return wc;
        }
    }
}
