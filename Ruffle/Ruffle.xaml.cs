using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class MainWindowRuffle : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public string TextScore { get; set; }
        public string TextErrorMessage { get; set; }
        public readonly Random random = new(
#if DEBUG
            1
#endif
            );
        RufflePuzzle? RufflePuzzleCurrent;


        public int LenTargetWord { get; set; } = 7;
        public int MinSubWordLength { get; set; } = 3;
        private ObservableCollection<UIElement> _LstWrdsSoFar = new();
        public ObservableCollection<UIElement> LstWrdsSoFar { get { return _LstWrdsSoFar; } set { _LstWrdsSoFar = value; RaisePropChanged(); } }
        public MainWindowRuffle()
        {
            InitializeComponent();
            DataContext = this;
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
                        if (RufflePuzzleCurrent.NumSolvedWords != RufflePuzzleCurrent.NumWordsTotal)
                        {
                            foreach (var kvpWordlength in RufflePuzzleCurrent.dictWordListsByLength) // each column of words
                            {
                                if (RufflePuzzleCurrent.dictWordListsByLength.TryGetValue(kvpWordlength.Key, out var lstAllWords))
                                {
                                    if (RufflePuzzleCurrent.dictWordsInAnswers.TryGetValue(kvpWordlength.Key, out var lstWordsfound))
                                    {
                                    }
                                    foreach (var word in lstAllWords)
                                    {
                                        if (lstWordsfound == null || !lstWordsfound.Contains(word))
                                        {
                                            if (RufflePuzzleCurrent.dictTiles.TryGetValue(kvpWordlength.Key, out var lstlstTiles))
                                            {
                                                foreach (var lstTile in lstlstTiles)  // find an empty spot
                                                {
                                                    if (lstTile[0].ruffleTileState.HasFlag(RuffleTileState.Hidden))
                                                    {
                                                        var ndx = 0;
                                                        foreach (var ltr in word)
                                                        {
                                                            lstTile[ndx++].ChangeState(RuffleTileState.ShownAsAnswer | RuffleTileState.Revealed, ltr);
                                                        }
                                                        RufflePuzzleCurrent.NumSolvedWords++;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            RufflePuzzleCurrent = await RufflePuzzle.CreateRufflePuzzleAsync(LenTargetWord, MinSubWordLength, this);
                        }
                        break;
                    case "_Ruffle":
                        {
                            ResetEnteredLetters();
                            var newlist = spLettersAvailable.Children.Cast<RuffleTile>().OrderBy(p => random.NextDouble()).ToList();
                            spLettersAvailable.Children.Clear();
                            newlist.ForEach(t => spLettersAvailable.Children.Add(t));
                        }
                        break;
                    case "_Submit":
                        break;
                }
                Focus();
            }
            catch (Exception)
            {
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter || e.Key == Key.Back || e.Key == Key.Escape) // or backspace or escape
                {
                    e.Handled = true;
                    var wordUserInput = "";
                    foreach (var tile in spLettersEntered.Children.Cast<RuffleTile>())
                    {
                        if (tile.ruffleTileState.HasFlag(RuffleTileState.Revealed))
                        {
                            wordUserInput += tile._letter;
                        }
                        else
                        {
                            break;
                        }
                    }
                    var doReset = false;
                    if (e.Key == Key.Enter)
                    {
                        var wasAdded = false;
                        if (wordUserInput.Length >= MinSubWordLength && RufflePuzzleCurrent?.dictWordListsByLength != null)
                        {
                            if (RufflePuzzleCurrent.dictWordListsByLength.TryGetValue(wordUserInput.Length, out var list) && list.Contains(wordUserInput))
                            {
                                if (!RufflePuzzleCurrent.dictWordsInAnswers.TryGetValue(wordUserInput.Length, out var lstWordsInAnswers))
                                {
                                    lstWordsInAnswers = new SortedSet<string>();
                                    RufflePuzzleCurrent.dictWordsInAnswers[wordUserInput.Length] = lstWordsInAnswers;
                                }
                                if (!lstWordsInAnswers.Contains(wordUserInput))
                                {
                                    lstWordsInAnswers.Add(wordUserInput);
                                    RufflePuzzleCurrent.RefreshWordsInAnswers(wordUserInput.Length);
                                    RufflePuzzleCurrent.NumSolvedWords++;
                                    TextScore = $"{RufflePuzzleCurrent.NumSolvedWords}/{RufflePuzzleCurrent.NumWordsTotal}";
                                    RaisePropChanged(nameof(TextScore));
                                    if (RufflePuzzleCurrent.NumSolvedWords == RufflePuzzleCurrent.NumWordsTotal)
                                    {

                                    }
                                }
                                if (!string.IsNullOrEmpty(TextErrorMessage))
                                {
                                    TextErrorMessage = string.Empty;
                                    RaisePropChanged(nameof(TextErrorMessage));
                                }
                                wasAdded = true;
                            }
                            else
                            {
                                TextErrorMessage = $"{wordUserInput} not found";
                                RaisePropChanged(nameof(TextErrorMessage));
                            }
                            var alreadyadded = LstWrdsSoFar.Cast<TextBlock>().Where(t => t.Text == wordUserInput).Any();
                            if (!alreadyadded)
                            {
                                LstWrdsSoFar.Add(new TextBlock() { Text = wordUserInput, Foreground = (wasAdded ? Brushes.Black : Brushes.LightPink) });
                                LstWrdsSoFar = new ObservableCollection<UIElement>(LstWrdsSoFar.Cast<TextBlock>().OrderBy(t => t.Text));
                            }
                            //var lstWrdsTriedSofar = new SortedSet<string>(LstWrdsSoFar.Cast<TextBlock>().Select(t => t.Text));
                            //lstWrdsTriedSofar.Add(wordUserInput);
                            //LstWrdsSoFar.Clear();
                            //foreach(var wrd in lstWrdsTriedSofar)
                            //{
                            //    LstWrdsSoFar.Add(new TextBlock() { Text = wordUserInput, Foreground = (wasAdded ? Brushes.Black : Brushes.LightBlue) });
                            //}
                        }
                        doReset = true;
                    }
                    else if (e.Key == Key.Escape)
                    {
                        doReset = true;
                    }
                    else
                    { // backspace
                        if (wordUserInput.Length > 0)
                        {
                            var tile = spLettersEntered.Children.Cast<RuffleTile>().ToList()[wordUserInput.Length - 1];
                            tile.ChangeState(RuffleTileState.Hidden);
                            foreach (var tileAvaileble in spLettersAvailable.Children.Cast<RuffleTile>())
                            {
                                if (tileAvaileble.ruffleTileState.HasFlag(RuffleTileState.Hidden) && tileAvaileble._letter == wordUserInput[^1])
                                {
                                    tileAvaileble.ChangeState(RuffleTileState.Revealed);
                                    break;
                                }
                            }
                        }
                    }
                    if (doReset)
                    {
                        ResetEnteredLetters();
                    }
                }
                else if (e.Key >= Key.A && e.Key <= Key.Z)
                {
                    var key = e.Key.ToString();
                    foreach (var tile in spLettersAvailable.Children.Cast<RuffleTile>())
                    {
                        if (tile._letter == key[0] && !tile.ruffleTileState.HasFlag(RuffleTileState.Hidden))
                        {
                            tile.ChangeState(RuffleTileState.Hidden, ltr: null);
                            foreach (var tileEntered in spLettersEntered.Children.Cast<RuffleTile>())
                            {
                                if (tileEntered.ruffleTileState.HasFlag(RuffleTileState.Hidden))
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

        private void ResetEnteredLetters()
        {
            foreach (var tile in spLettersEntered.Children.Cast<RuffleTile>())
            {
                tile.ChangeState(RuffleTileState.Hidden);
            }
            foreach (var tile in spLettersAvailable.Children.Cast<RuffleTile>())
            {
                tile.ChangeState(RuffleTileState.Revealed);
            }
        }
    }
    public class RufflePuzzle
    {
        WordScapePuzzle? _WordScapePuzzle; // user wordscape lib for word gen
        int maxWordListLength = 14; // max # of e.g. 4 letter words
        private MainWindowRuffle mainWindowRuffle;
        int LtrWidthSmall = 25;
        int LtrHeighSmall = 25;
        int LtrWidthLarge = 20;
        public int NumSolvedWords = 0;
        public int NumWordsTotal = 0;

        internal Dictionary<int, List<string>> dictWordListsByLength = new(); // WordLen => list<words>
        internal Dictionary<int, List<List<RuffleTile>>> dictTiles = new(); // WordLen=>List<Tile>
        internal Dictionary<int, SortedSet<string>> dictWordsInAnswers = new(); // Wordlen=>List<words>

        public static async Task<RufflePuzzle> CreateRufflePuzzleAsync(int lenTargetWord, int minSubWordLength, MainWindowRuffle mainWindowRuffle)
        {
            var puz = new RufflePuzzle(mainWindowRuffle)
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
            mainWindowRuffle.LstWrdsSoFar.Clear();
            dictWordListsByLength = _WordScapePuzzle.wordContainer.subwords.GroupBy(w => w.Length).ToDictionary(kvp => kvp.Key, kvp => kvp.ToList());
            FillRuffleGrid();
            var ltrs = _WordScapePuzzle.wordContainer.InitialWord;
            mainWindowRuffle.spLettersAvailable.Children.Clear();
            mainWindowRuffle.spLettersEntered.Children.Clear();
            foreach (var ltr in ltrs.OrderBy(p => mainWindowRuffle.random.NextDouble())) // shuffle
            {
                var tile = new RuffleTile(mainWindowRuffle, ltr, ruffleTileState: RuffleTileState.Large);
                mainWindowRuffle.spLettersAvailable.Children.Add(tile);
                tile = new RuffleTile(mainWindowRuffle, ltr, ruffleTileState: RuffleTileState.Large | RuffleTileState.Hidden);
                mainWindowRuffle.spLettersEntered.Children.Add(tile);
            }
        }

        private RufflePuzzle(MainWindowRuffle mainWindowRuffle)
        {
            this.mainWindowRuffle = mainWindowRuffle;
        }
        public void FillRuffleGrid()
        {
            { // we want to remove a singular if the plural is already placed. Bias toward keeping longer word
                var dictWords = new Dictionary<string, object?>();
                foreach (var kvp in dictWordListsByLength.OrderByDescending(kvp => kvp.Key)) // for each word list longest set to shortest
                {
                    var lstIgnoredWords = new List<string>();
                    foreach (var word in kvp.Value) // each word of fixed length
                    {
                        if (!WordGenerator.IgnorePluralGerundPastTenseWords(word, dictWords))
                        {
                            dictWords[word] = null; // doesn't matter
                        }
                        else
                        {
                            lstIgnoredWords.Add(word);
                        }
                    }
                    foreach (var word in lstIgnoredWords)
                    {
                        dictWordListsByLength[kvp.Key].Remove(word);
                    }
                }
            }
            foreach (var kvp in dictWordListsByLength) // now shorten any long lists
            {
                while (kvp.Value.Count >= maxWordListLength)
                {
                    kvp.Value.RemoveAt(mainWindowRuffle.random.Next(kvp.Value.Count));
                }
            }
            if (_WordScapePuzzle == null)
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
                                var tile = new RuffleTile(mainWindowRuffle, word[i], rowNdx, RuffleTileState.Hidden);
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
                            NumWordsTotal++;
                        }
                    }
                    column++;
                }
            }
        }

        internal void RefreshWordsInAnswers(int length)
        {
            if (dictTiles.TryGetValue(length, out var lstlstTiles))
            {
                if (dictWordsInAnswers.TryGetValue(length, out var lstlstWords))
                {
                    var row = 0;
                    foreach (var word in lstlstWords)
                    {
                        for (var ndxltr = 0; ndxltr < word.Length; ndxltr++)
                        {
                            lstlstTiles[row][ndxltr].ChangeState(RuffleTileState.Revealed, ltr: word[ndxltr]);
                        }
                        row++;
                    }
                }
            }
        }
    }

    // A tile can be large or small: Large for the Letters Available List and the list of entered letters
    [Flags]
    public enum RuffleTileState
    {
        Hidden = 0x1,
        Revealed = 0x2, // if revealed, show the letter content, else hide it
        ShownAsAnswer = 0x4, // revealed as word that user didn't get
        Small = 0x8,
        Empty = 0x10,
        Large = 0x20,
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
            if (!ruffleTileState.HasFlag(RuffleTileState.Empty) && !ruffleTileState.HasFlag(RuffleTileState.Hidden))
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
            if (ruffleTileState.HasFlag(RuffleTileState.Hidden))
            {
                _TxtBlk.Text = string.Empty;
            }
            if (ruffleTileState.HasFlag(RuffleTileState.Revealed))
            {
                _TxtBlk.Text = _letter.ToString();
            }
            if (ruffleTileState.HasFlag(RuffleTileState.ShownAsAnswer))
            {
                Background = Brushes.LightBlue;
            }
        }
        public override string ToString() => $"{(_letter == '\0' ? " " : _letter)} {ruffleTileState.ToString()}";
    }
}
