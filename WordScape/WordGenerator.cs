using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordScape
{
    public class WordContainer
    {
        public string InitialWord;
        public List<string> subwords = new List<string>();
        public int cntLookups;
    }
    public class WordGenerator
    {
        public readonly Random _rand;
        readonly DictionaryLib.DictionaryLib _dictionaryLib;
        const int MinSubWordLen = 4;
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
            //            word = "imagery";
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
                for (int i = MinSubWordLen; i < str.Length + 1; i++)
                {
                    var testWord = str.Substring(0, i);
                    var partial = _dictionaryLib.SeekWord(testWord, out var compResult);
                    wc.cntLookups++;
                    if (!string.IsNullOrEmpty(partial) && compResult == 0)
                    {
                        if (!wc.subwords.Contains(testWord))
                        {
                            wc.subwords.Add(testWord);
                        }
                    }
                    else
                    {
                        if (!partial.StartsWith(testWord))
                        {
                            break;
                        }
                    }
                }
                return wc.subwords.Count != numMaxSubWords; // continue
            });
            wc.subwords = wc.subwords.OrderByDescending(w => w.Length).ToList();
            return wc;
        }
    }
}
