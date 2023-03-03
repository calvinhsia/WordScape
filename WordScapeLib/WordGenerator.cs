using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordScape
{
    public class WordGenerationParms
    {
        private Random _random;
        public Random _Random { get { if (_random == null) { _random = new Random(1); } return _random; } set { _random = value; } }
        public int LenTargetWord { get; set; } = 7;
        public int MinSubWordLength { get; set; } = 5;
        public int MaxX { get; set; } = 15;
        public int MaxY { get; set; } = 15;
        public int MaxSubWords { get; set; } = 1500;
        public override string ToString() => $"LenTargetWord = {LenTargetWord}  MinSubWordLength={MinSubWordLength}  {MaxX},{MaxY}";
    }
    public class WordScapePuzzle
    {
        public int LenTargetWord = 7; // at the time of generation. User could have changed it, invalidating this one
        public int MinSubWordLength = 5; // at the time of generation. User could have changed it, invalidating this one
        public WordGenerator wordGenerator;
        public WordContainer wordContainer;
        public GenGrid genGrid;
        public static Task<WordScapePuzzle> CreateNextPuzzleTask(WordGenerationParms wordGenerationParms)
        {
            return Task.Run(() =>
            {
                WordScapePuzzle puzzleNext = null;
                    puzzleNext = new WordScapePuzzle()
                    {
                        LenTargetWord = wordGenerationParms.LenTargetWord,
                        MinSubWordLength = wordGenerationParms.MinSubWordLength
                    };
                    try
                    {
                        puzzleNext.wordGenerator = new WordGenerator(wordGenerationParms);
                        puzzleNext.wordContainer = puzzleNext.wordGenerator.GenerateWord();
                        puzzleNext.genGrid = new GenGrid(wordGenerationParms.MaxX, wordGenerationParms.MaxY, puzzleNext.wordContainer, wordGenerationParms._Random);
                        puzzleNext.genGrid.Generate();
                    }
                    catch (Exception)
                    {
                        // old version of dict threw nullref sometimes at end of alphabet
                    }
                Debug.WriteLine($"");
                return puzzleNext;
            });
        }

    }

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
        public readonly DictionaryLib.DictionaryLib _dictionaryLibSmall;
        public readonly DictionaryLib.DictionaryLib _dictionaryLibLarge;
        public int _MinSubWordLen =>_wordGenerationParms.MinSubWordLength;
        public int _TargetLen=>_wordGenerationParms.LenTargetWord;
        private readonly WordGenerationParms _wordGenerationParms;
        private int _numMaxSubWords=>_wordGenerationParms.MaxSubWords;
        public WordGenerator(WordGenerationParms wordGenerationParms)
        {
            _wordGenerationParms = wordGenerationParms;
            _dictionaryLibSmall = new DictionaryLib.DictionaryLib(DictionaryLib.DictionaryType.Small, _wordGenerationParms._Random);
            _dictionaryLibLarge = new DictionaryLib.DictionaryLib(DictionaryLib.DictionaryType.Large, _wordGenerationParms._Random);
        }
        // avoid havin
        public bool IsWordInLargeDictionary(string word) // forwarder so xamarin doesn't need ref to dict
        {
            return _dictionaryLibLarge.IsWord(word);
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
        public static bool IgnorePluralGerundPastTenseWords<T>(string subword, Dictionary<string, T> _dictWords)
        {
            if (!subword.EndsWith("S"))
            {
                if (_dictWords.ContainsKey(subword + "S"))
                {
                    return true;
                }
            }
            if (!subword.EndsWith("D")) // past tense: if "removed", don't add "remove"
            {
                if (_dictWords.ContainsKey(subword + "D"))
                {
                    return true;
                }
            }
            else
            {
                if (_dictWords.ContainsKey(subword.Substring(0, subword.Length - 2) + "R"))
                {
                    return true;
                }
            }
            if (subword.EndsWith("R")) // "remover", "removed": allow only one
            {
                if (_dictWords.ContainsKey(subword.Substring(0, subword.Length - 2) + "D"))
                {
                    return true;
                }
            }
            if (_dictWords.ContainsKey(subword + "LY")) // discretely: dont put discrete
            {
                return true;
            }
            if (_dictWords.ContainsKey(subword + "ED")) // disobeyed : don't put disobey
            {
                return true;
            }
            if (_dictWords.ContainsKey(subword + "ER")) // disobeyer : don't put disobey
            {
                return true;
            }
            if (_dictWords.ContainsKey(subword + "ING")) // disobeying : don't put disobey
            {
                return true;
            }
            return false;
        }
    }
}
