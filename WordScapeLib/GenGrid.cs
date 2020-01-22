using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("WordScapeTests")]

namespace WordScape
{
    public enum WordStatus
    {
        IsAlreadyInGrid,
        IsShownInGridForFirstTime,
        IsNotInGrid
    }
    public enum FoundWordType
    {
        SubWordNotAWord,
        SubWordInGrid,
        SubWordNotInGrid,
        SubWordInLargeDictionary
    };
    public class FoundWord
    {
        public FoundWordType foundStringType;
        public string word;
    }
    public class LtrPlaced
    {
        public int nX;
        public int nY;
        public char ltr;
        public bool IsHoriz; // orientation of the 1st word placed in this square
        public override string ToString()
        {
            return $"({nX,2},{nY,2}) {ltr} IsHorz={IsHoriz}";
        }
    }
    /// <summary>
    /// Given a set or words, places them in an array of char
    /// </summary>
    public class GenGrid
    {
        public const char Blank = '_';
        readonly WordContainer _wordContainer;
        readonly public Random _random;
        /// <summary>
        /// initially, the max size of desired grid. Once grid filled in and recentered, 
        /// these are recalculated to be potentially smaller
        /// </summary>
        public int _MaxY;
        public int _MaxX;
        public char[,] _chars;
        public int NumWordsPlaced => _dictPlacedWords.Count;
        //        public List<string> _lstWordsPlaced = new List<string>();
        public Dictionary<string, LtrPlaced> _dictPlacedWords = new Dictionary<string, LtrPlaced>(); // subword to 1st letter
        public int nLtrsPlaced;
        public readonly List<LtrPlaced> _ltrsPlaced = new List<LtrPlaced>();

        internal int _tmpminX;
        internal int _tmpmaxX;
        internal int _tmpminY;
        internal int _tmpmaxY;
        public GenGrid(int maxX, int maxY, WordContainer wordCont, Random rand)
        {
            this._random = rand;
            this._wordContainer = wordCont;
            this._MaxY = maxY;
            this._MaxX = maxX;
            _tmpminX = maxX;
            _tmpminY = maxY;
            _tmpmaxX = 0;
            _tmpmaxY = 0;
            _chars = new char[_MaxX, _MaxY];
            for (int y = 0; y < _MaxY; y++)
            {
                for (int x = 0; x < _MaxX; x++)
                {
                    _chars[x, y] = Blank;
                }
            }
        }
        public void Generate()
        {
            PlaceWords();
            ResizeGridArraySmaller();
        }

        internal void ResizeGridArraySmaller()
        {
            if (NumWordsPlaced > 1)
            {
                _MaxX = _tmpmaxX - _tmpminX + 1;
                _MaxY = _tmpmaxY - _tmpminY + 1;
                foreach (var ltr in _ltrsPlaced)
                {
                    ltr.nX -= _tmpminX;
                    ltr.nY -= _tmpminY;
                }
                char[,] newCharArr = new char[_MaxX, _MaxY];
                for (int y = 0; y < _MaxY; y++)
                {
                    for (int x = 0; x < _MaxX; x++)
                    {
                        newCharArr[x, y] = _chars[x + _tmpminX, y + _tmpminY];
                    }
                }
                _chars = newCharArr;
            }
        }

        internal void PlaceWords()
        {
            foreach (var subword in _wordContainer.subwords)
            {
                if (NumWordsPlaced == 0)
                {
                    int x, y, incY = 0, incX = 0;
                    if (_random.NextDouble() < .5) // horiz. Try to make 1st word centrally located
                    {
                        y = _MaxY / 4 + _random.Next(_MaxY / 2);
                        x = _random.Next(_MaxX - subword.Length);
                        incX = 1;
                    }
                    else
                    { // up/down
                        x = _MaxX / 4 + _random.Next(_MaxX / 2);
                        y = _random.Next(_MaxY - subword.Length);
                        incY = 1;
                    }
                    PlaceOneWord(subword, x, y, incX, incY);
                }
                else
                {// not 1st word: find random common letter and see if it can be placed. don't do singular if plural already placed
                    if (!subword.EndsWith("S"))
                    {
                        if (_dictPlacedWords.ContainsKey(subword + "S"))
                        {
                            continue;
                        }
                    }
                    if (!subword.EndsWith("D")) // past tense: if "removed", don't add "remove"
                    {
                        if (_dictPlacedWords.ContainsKey(subword + "D"))
                        {
                            continue;
                        }
                    }
                    if (subword.EndsWith("R")) // "remover", "removed": allow only one
                    {
                        if (_dictPlacedWords.ContainsKey(subword.Substring(0, subword.Length - 2) + "D"))
                        {
                            continue;
                        }
                    }
                    if (subword.EndsWith("D")) // "remover", "removed": allow only one
                    {
                        if (_dictPlacedWords.ContainsKey(subword.Substring(0, subword.Length - 2) + "R"))
                        {
                            continue;
                        }
                    }
                    if (_dictPlacedWords.ContainsKey(subword + "ED")) // disobeyed : don't put disobey
                    {
                        continue;
                    }
                    if (_dictPlacedWords.ContainsKey(subword + "ER")) // disobeyer : don't put disobey
                    {
                        continue;
                    }
                    ShuffleLettersPlaced();
                    foreach (var ltrPlaced in _ltrsPlaced)
                    {
                        if (TryPlaceWord(subword, ltrPlaced))
                        {
                            break;
                        }
                    }
                }
                if (NumWordsPlaced == 6)
                {
                    //                    return;
                }
            }
        }

        private void PlaceOneWord(string subword, int x, int y, int incX, int incY)
        {
            var isFirstLetter = true;
            if (x < _tmpminX)
            {
                _tmpminX = x;
            }
            if (y < _tmpminY)
            {
                _tmpminY = y;
            }
            foreach (var ltr in subword)
            {
                var ltrPlaced = new LtrPlaced() { nX = x, nY = y, ltr = ltr, IsHoriz = incX > 0 };
                if (isFirstLetter)
                {
                    _dictPlacedWords[subword] = ltrPlaced;
                    isFirstLetter = false;
                }
                _ltrsPlaced.Add(ltrPlaced);
                _chars[x, y] = ltr;
                x += incX;
                y += incY;
                nLtrsPlaced++;
            }
            if (x > _tmpmaxX)
            {
                _tmpmaxX = x - incX;
            }
            if (y > _tmpmaxY)
            {
                _tmpmaxY = y - incY;
            }
        }

        private bool TryPlaceWord(string subword, LtrPlaced ltrPlaced)
        {
            var didPlaceWord = false;
            var theChar = ltrPlaced.ltr;
            // if the cur ltr is part of a horiz word, then we'll try to go vert and vv
            var DoHoriz = !ltrPlaced.IsHoriz;
            var ndxAt = 0;
            while (true)
            {
                var at = subword.IndexOf(theChar, ndxAt);
                if (at < 0)
                {
                    break;
                }
                int x0 = -1, y0 = -1, incx = 0, incy = 0;
                if (DoHoriz)
                { // if it fits on grid
                    if (ltrPlaced.nX - at >= 0)
                    {
                        if (ltrPlaced.nX - at + subword.Length <= _MaxX)
                        {
                            // if the prior and post squares are empty if they exist
                            if (ltrPlaced.nX - at == 0 || _chars[ltrPlaced.nX - at - 1, ltrPlaced.nY] == Blank)
                            {
                                if (ltrPlaced.nX - at + subword.Length == _MaxX || _chars[ltrPlaced.nX - at + subword.Length, ltrPlaced.nY] == Blank)
                                {
                                    x0 = ltrPlaced.nX - at;
                                    y0 = ltrPlaced.nY;
                                    incx = 1;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (ltrPlaced.nY - at >= 0)
                    {
                        if (ltrPlaced.nY - at + subword.Length <= _MaxY)
                        {
                            // if the prior and post squares are empty if they exist
                            if (ltrPlaced.nY - at == 0 || _chars[ltrPlaced.nX, ltrPlaced.nY - at - 1] == Blank)
                            {
                                if (ltrPlaced.nY - at + subword.Length == _MaxY || _chars[ltrPlaced.nX, ltrPlaced.nY - at + subword.Length] == Blank)
                                {
                                    x0 = ltrPlaced.nX;
                                    y0 = ltrPlaced.nY - at;
                                    incy = 1;
                                }
                            }
                        }
                    }
                }
                if (x0 >= 0)
                {
                    var doesfit = true;
                    int ndxc = 0;
                    foreach (var chr in subword)
                    {
                        var val = _chars[x0 + incx * ndxc, y0 + incy * ndxc];
                        if (val != Blank && val != chr) // reject?
                        {
                            doesfit = false;
                            break;
                        }
                        // if blank and the adjacent ones are not empty, we need to reject (brit xword)
                        if (val == Blank)
                        {
                            if (DoHoriz) // incx>0
                            {
                                if (y0 - 1 >= 0 && _chars[x0 + incx * ndxc, y0 - 1] != Blank)
                                {
                                    doesfit = false;
                                    break;
                                }
                                if (y0 + 1 < _MaxY && _chars[x0 + incx * ndxc, y0 + 1] != Blank)
                                {
                                    doesfit = false;
                                    break;
                                }
                            }
                            else
                            { // incy>0
                                if (x0 - 1 >= 0 && _chars[x0 - 1, y0 + incy * ndxc] != Blank)
                                {
                                    doesfit = false;
                                    break;
                                }
                                if (x0 + 1 < _MaxX && _chars[x0 + 1, y0 + incy * ndxc] != Blank)
                                {
                                    doesfit = false;
                                    break;
                                }

                            }
                        }
                        ndxc++;
                    }
                    if (doesfit)
                    {
                        PlaceOneWord(subword, x0, y0, incx, incy);
                        //ndxc = 0;
                        //foreach (var chr in subword)
                        //{
                        //    _chars[x0 + incx * ndxc, y0 + incy * ndxc] = chr;
                        //    ndxc++;
                        //}
                        didPlaceWord = true;

                    }
                }
                ndxAt = at + 1;
            }

            return didPlaceWord;
        }


        private void ShuffleLettersPlaced()
        {
            for (int i = 0; i < _ltrsPlaced.Count; i++)
            {
                var tmp = _ltrsPlaced[i];
                var r = _random.Next(_ltrsPlaced.Count);
                _ltrsPlaced[i] = _ltrsPlaced[r];
                _ltrsPlaced[r] = tmp;
            }
        }

        public string ShowGrid()
        {
            var grid = Environment.NewLine;
            for (int y = 0; y < _MaxY; y++)
            {
                for (int x = 0; x < _MaxX; x++)
                {
                    grid += _chars[x, y].ToString();
                }
                grid += Environment.NewLine;
            }
            return grid;
        }
    }
}
