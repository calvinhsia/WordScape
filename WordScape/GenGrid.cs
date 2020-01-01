﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordScape
{
    class LtrPlaced
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
    public class GenGrid
    {
        const char Blank = '_';
        readonly WordContainer _wordContainer;
        readonly public Random _random;
        readonly int _MaxY;
        readonly int _MaxX;
        public char[,] _chars;
        int nWordsPlaced;
        readonly List<LtrPlaced> _ltrsPlaced = new List<LtrPlaced>();
        public GenGrid(int maxX, int maxY, WordContainer wordCont, Random rand)
        {
            this._random = rand;
            this._wordContainer = wordCont;
            this._MaxY = maxY;
            this._MaxX = maxX;
            _chars = new char[_MaxY, _MaxX];
            for (int y = 0; y < _MaxY; y++)
            {
                for (int x = 0; x < _MaxX; x++)
                {
                    _chars[x, y] = Blank;
                }
            }
            PlaceWords();
        }

        private void PlaceWords()
        {
            foreach (var subword in _wordContainer.subwords)
            {
                if (nWordsPlaced++ == 0)
                {
                    int x, y, incY = 0, incX = 0;
                    if (_random.NextDouble() < .5) // horiz
                    {
                        y = _random.Next(_MaxY);
                        x = _random.Next(_MaxX - subword.Length);
                        incX = 1;
                    }
                    else
                    { // up/down
                        x = _random.Next(_MaxX);
                        y = _random.Next(_MaxY - subword.Length);
                        incY = 1;
                    }
                    PlaceOneWord(subword, x, y, incX, incY);
                }
                else
                {// not 1st word: find random common letter and see if it can be placed
                    ShuffleLettersPlaced();
                    foreach (var ltrPlaced in _ltrsPlaced)
                    {
                        if (TryPlaceWord(subword, ltrPlaced))
                        {
                            break;
                        }
                    }
                }
                if (nWordsPlaced == 2)
                {
                    return;
                }
            }
        }

        private void PlaceOneWord(string subword, int x, int y, int incX, int incY)
        {
            foreach (var ltr in subword)
            {
                _chars[x += incX, y += incY] = ltr;
                _ltrsPlaced.Add(new LtrPlaced() { nX = x, nY = y, ltr = ltr, IsHoriz = incX > 0 });
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
                {
                    if (ltrPlaced.nX - at >= 0)
                    {
                        if (ltrPlaced.nX - at + subword.Length < _MaxX)
                        {
                            x0 = ltrPlaced.nX - at;
                            y0 = ltrPlaced.nY;
                            incx = 1;
                        }
                    }
                }
                else
                {
                    if (ltrPlaced.nY - at >= 0)
                    {
                        if (ltrPlaced.nY - at + subword.Length < _MaxY)
                        {
                            x0 = ltrPlaced.nX;
                            y0 = ltrPlaced.nY - at;
                            incy = 1;
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
                        ndxc++;
                    }
                    if (doesfit)
                    {
//                        PlaceOneWord(subword, x0, y0, incx, incy);
                        ndxc = 0;
                        foreach (var chr in subword)
                        {
                            _chars[x0 + incx * ndxc, y0 + incy * ndxc] = chr;
                            ndxc++;
                        }
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
            var grid = string.Empty;
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
