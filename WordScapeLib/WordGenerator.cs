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
        public override string ToString()
        {
            return $"{InitialWord} #Subw={subwords.Count}";
        }
    }
    public class WordGenerator
    {
        public readonly Random _rand;
        public readonly DictionaryLib.DictionaryLib _dictionaryLibSmall;
        public readonly DictionaryLib.DictionaryLib _dictionaryLibLarge;
        public int _MinSubWordLen = 5;
        public int _TargetLen;
        private readonly int _numMaxSubWords;
        public WordGenerator(Random rand = null, int TargetLen= 10, int minSubWordLength = 3, int MaxSubWords = 1500)
        {
            _numMaxSubWords = MaxSubWords;
            _TargetLen = TargetLen;
            _MinSubWordLen = minSubWordLength;
            if (rand == null)
            {
                _rand = new Random();
            }
            else
            {
                _rand = rand;
            }
            _dictionaryLibSmall = new DictionaryLib.DictionaryLib(DictionaryLib.DictionaryType.Small, _rand);
            _dictionaryLibLarge = new DictionaryLib.DictionaryLib(DictionaryLib.DictionaryType.Large, _rand);
        }

        public WordContainer GenerateWord()
        {
            var word = string.Empty;
            while (word.Length != _TargetLen)
            {
                word = _dictionaryLibSmall.RandomWord();
            }
            return GenSubWords(word);
        }

        private WordContainer GenSubWords(string word)
        {
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
                for (int i = _MinSubWordLen; i < str.Length + 1; i++)
                {
                    var testWord = str.Substring(0, i);
                    var partial = _dictionaryLibSmall.SeekWord(testWord, out var compResult);
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
                return wc.subwords.Count != _numMaxSubWords; // continue
            });
            wc.subwords = wc.subwords.OrderByDescending(w => w.Length).Select(p => p.ToUpper()).ToList();
            wc.InitialWord = wc.InitialWord.ToUpper();
            return wc;
        }
    }
}
