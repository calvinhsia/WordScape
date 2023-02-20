using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WordScape;

namespace Ruffle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowRuffle : Window
    {
        //        private readonly DictionaryLib.DictionaryLib dictionaryLib;
        public readonly Random random = new(
#if DEBUG
            1
#endif
            );
        RufflePuzzle? RufflePuzzleCurrent;


        public int LenTargetWord { get; set; } = 7;
        public int MinSubWordLength { get; set; } = 3;
        public MainWindowRuffle()
        {
            InitializeComponent();
            Loaded += MainWindowRuffle_Loaded;
        }
        private async void MainWindowRuffle_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                RufflePuzzleCurrent = await RufflePuzzle.CreateRufflePuzzleAsync(LenTargetWord, MinSubWordLength, this);
            }
            catch (Exception)
            {
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btnText = (sender as Button)?.Content as string;
                switch (btnText)
                {
                    case "_End":
                        RufflePuzzleCurrent = await RufflePuzzle.CreateRufflePuzzleAsync(LenTargetWord, MinSubWordLength, this);
                        break;
                    case "_Submit":
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    var wordSofar = "";
                    foreach (var tile in spLettersEntered.Children.Cast<RuffleTile>())
                    {
                        if (tile.ruffleTileState.HasFlag(RuffleTileState.Revealed))
                        {
                            wordSofar += tile._letter;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (wordSofar.Length >= MinSubWordLength)
                    {
                        if (RufflePuzzleCurrent.dictWordListsByLength.TryGetValue(wordSofar.Length, out var list) && list.Contains(wordSofar))
                        {
                            if (RufflePuzzleCurrent.dictTiles.TryGetValue(wordSofar.Length, out var lstlst))
                            {
                                foreach (var lstTiles in lstlst) // for each set of tiles for each word
                                {
                                    if (lstTiles[0].ruffleTileState.HasFlag(RuffleTileState.UnRevealed)) // found a row that has 1st entry unused?
                                    {
                                        var ltrndx = 0;
                                        foreach (var tile in lstTiles) // then set the word to the word user typed
                                        {
                                            tile.ChangeState(RuffleTileState.Revealed, ltr: wordSofar[ltrndx++]);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    // now reset
                    foreach (var tile in spLettersEntered.Children.Cast<RuffleTile>())
                    {
                        tile.ChangeState(RuffleTileState.UnRevealed);
                    }
                    foreach (var tile in spLettersAvailable.Children.Cast<RuffleTile>())
                    {
                        tile.ChangeState(RuffleTileState.Revealed);
                    }
                }
                else if (e.Key >= Key.A && e.Key <= Key.Z)
                {
                    var key = e.Key.ToString();
                    foreach (var tile in spLettersAvailable.Children.Cast<RuffleTile>())
                    {
                        if (tile._letter == key[0] && !tile.ruffleTileState.HasFlag(RuffleTileState.UnRevealed))
                        {
                            tile.ChangeState(RuffleTileState.UnRevealed, ltr: null);
                            foreach (var tileEntered in spLettersEntered.Children.Cast<RuffleTile>())
                            {
                                if (tileEntered.ruffleTileState.HasFlag(RuffleTileState.UnRevealed))
                                {
                                    tileEntered.ChangeState(RuffleTileState.Revealed, tile._letter);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
            }
        }
    }

    public class RufflePuzzle
    {
        WordScapePuzzle? _WordScapePuzzle;
        int maxWordListLength = 34; // max # of e.g. 4 letter words
        private MainWindowRuffle mainWindowRuffle;
        int LtrWidthSmall = 25;
        int LtrHeighSmall = 25;
        int LtrWidthLarge = 20;

        internal Dictionary<int, List<string>>? dictWordListsByLength; // WordLen => list<words>
        internal Dictionary<int, List<List<RuffleTile>>> dictTiles = new(); // WordLen=>List<Tile>
                                                                            //        internal RuffleTile[,] ruffleTiles; // 2d array of 
        public static async Task<RufflePuzzle> CreateRufflePuzzleAsync(int lenTargetWord, int minSubWordLength, MainWindowRuffle mainWindowRuffle)
        {
            var puz = new RufflePuzzle
            {
                _WordScapePuzzle = await WordScapePuzzle.CreateNextPuzzleTask(new WordGenerationParms()
                {
                    LenTargetWord = lenTargetWord,
                    MinSubWordLength = minSubWordLength,
                    _Random = mainWindowRuffle.random
                })
            };
            await puz.InitializeAsync(mainWindowRuffle);
            return puz;
        }

        private async Task InitializeAsync(MainWindowRuffle mainWindowRuffle)
        {
            await Task.Yield();
            if (_WordScapePuzzle == null) throw new NullReferenceException();
            this.mainWindowRuffle = mainWindowRuffle;
            dictWordListsByLength = _WordScapePuzzle.wordContainer.subwords.GroupBy(w => w.Length).ToDictionary(kvp => kvp.Key, kvp => kvp.ToList());
            FillRuffleGrid();
            var ltrs = _WordScapePuzzle.wordContainer.InitialWord;
            mainWindowRuffle.spLettersAvailable.Children.Clear();
            mainWindowRuffle.spLettersEntered.Children.Clear();
            foreach (var ltr in ltrs.OrderBy(p => mainWindowRuffle.random.NextDouble())) // shuffle
            {
                var tile = new RuffleTile(mainWindowRuffle, ltr, ruffleTileState: RuffleTileState.Large);
                mainWindowRuffle.spLettersAvailable.Children.Add(tile);
                tile = new RuffleTile(mainWindowRuffle, ltr, ruffleTileState: RuffleTileState.Large | RuffleTileState.UnRevealed);
                mainWindowRuffle.spLettersEntered.Children.Add(tile);
            }
        }

        private RufflePuzzle()
        {

        }
        public void FillRuffleGrid()
        {
            if (dictWordListsByLength != null)
            {
                foreach (var kvp in dictWordListsByLength)
                {
                    while (kvp.Value.Count >= maxWordListLength)
                    {
                        kvp.Value.RemoveAt(mainWindowRuffle.random.Next(kvp.Value.Count));
                    }
                }
            }
            //var lstWords = new List<List<string>>(); 
            if (_WordScapePuzzle == null || dictWordListsByLength == null)
            {
                throw new NullReferenceException();
            }
            var column = 0;
            mainWindowRuffle.cvs.Children.Clear();
            for (var wordlength = _WordScapePuzzle.MinSubWordLength; wordlength <= _WordScapePuzzle.LenTargetWord; wordlength++)
            {
                dictTiles[wordlength] = new List<List<RuffleTile>>();
                if (dictWordListsByLength.TryGetValue(wordlength, out var curwordlist))
                {
                    if (curwordlist != null)
                    {
                        var rowNdx = 0;
                        foreach (var word in curwordlist) // for each word of same length
                        {
                            var lstRuffleTile = new List<RuffleTile>();
                            for (var i = 0; i < wordlength; i++) // for each letter in word
                            {
                                var tile = new RuffleTile(mainWindowRuffle, word[i], rowNdx, RuffleTileState.UnRevealed);
                                lstRuffleTile.Add(tile);
                                var x = column * (wordlength) * LtrWidthSmall + i * LtrWidthSmall;
                                var y = rowNdx * LtrHeighSmall;
                                Canvas.SetTop(tile, y);
                                Canvas.SetLeft(tile, x);
                                tile.ToolTip = $"{x} {y}";
                                mainWindowRuffle.cvs.Children.Add(tile);
                            }
                            rowNdx++;
                            dictTiles[wordlength].Add(lstRuffleTile);
                        }
                    }
                    column++;
                }
            }
        }
    }

    // A tile can be large or small: Large for the Letters Available List and the list of entered letters
    [Flags]
    public enum RuffleTileState
    {
        UnRevealed = 0x1,
        Revealed = 0x2, // if revealed, show the letter content, else hide it
        Small = 0x4,
        Empty = 0x8,
        Large = 0x10,
    }
    public class RuffleTile : DockPanel
    {
        internal char _letter;
        internal RuffleTileState ruffleTileState;
        internal TextBlock _TxtBlk;
        public RuffleTile(MainWindowRuffle mainWindowRuffle, char letter, int Rowndx = -1, RuffleTileState ruffleTileState = RuffleTileState.Small)
        {
            Margin = new Thickness(1, 1, 1, 1);
            Width = 20;
            Height = 20;
            this.ruffleTileState = ruffleTileState;
            if (ruffleTileState.HasFlag(RuffleTileState.Large))
            {
                Width *= 2;
                Height *= 2;
            }
            if (!ruffleTileState.HasFlag(RuffleTileState.Empty) && !ruffleTileState.HasFlag(RuffleTileState.UnRevealed))
            {
                _letter = letter;
            }
            Background = Brushes.White;
            _TxtBlk = new TextBlock()
            {
                FontSize = 20,
                Foreground = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                //                Visibility = Visibility.Hidden,
                Text = _letter.ToString()
            };
            this.Children.Add(_TxtBlk);
        }

        internal void ChangeState(RuffleTileState newState, char? ltr = null)
        {
            var oldState = ruffleTileState;
            this.ruffleTileState = newState;
            if (ltr != null)
            {
                _letter = ltr.Value;
            }
            if (ruffleTileState.HasFlag(RuffleTileState.UnRevealed))
            {
                _TxtBlk.Text = string.Empty;
            }
            if (ruffleTileState.HasFlag(RuffleTileState.Revealed))
            {
                _TxtBlk.Text = _letter.ToString();
            }

        }
        public override string ToString() => $"{(_letter == '\0' ? " " : _letter)} {ruffleTileState.ToString()}";
    }
}
