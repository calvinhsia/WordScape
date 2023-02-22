using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WordScape;

// xcopy /dy C:\Users\calvinh\source\repos\WordScape\Ruffle\bin\Release\net6.0-windows C:\Users\calvinh\OneDrive\Public\Ruffle
namespace Ruffle
{
    public partial class MainWindowRuffle : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public string PluralTip { get; } = "Some words are removed to reduce duplication: e.g. singular if plural is included, present tense it past tense included, '-ly','-ing'";
        public string TextScore { get; set; } = string.Empty;
        public string TextErrorMessage { get; set; } = string.Empty;
        public bool AllowPlurals { get; set; } = false;
        public int LenTargetWord { get; set; } = 7;
        public int MinSubWordLength { get; set; } = 3;
        public int MaxColumnLength { get; set; } = 15;

        public readonly Random random = new(
#if DEBUG
            1
#endif
            );
        RufflePuzzle? RufflePuzzleCurrent;


        private ObservableCollection<UIElement> _LstWrdsSoFar = new();
        public ObservableCollection<UIElement> LstWrdsSoFar { get { return _LstWrdsSoFar; } set { _LstWrdsSoFar = value; RaisePropChanged(); } }
        public MainWindowRuffle()
        {
            InitializeComponent();
            DataContext = this;
            AllowPlurals = Properties.Settings.Default.AllowPlurals;
            LenTargetWord = Properties.Settings.Default.LenTargetWord;
            MinSubWordLength = Properties.Settings.Default.MinSubwordLength;
            MaxColumnLength = Properties.Settings.Default.MaxColumnLength;
            Closed += (_, _)
                =>
            {
                Properties.Settings.Default.AllowPlurals = AllowPlurals;
                Properties.Settings.Default.LenTargetWord = LenTargetWord;
                Properties.Settings.Default.MinSubwordLength = MinSubWordLength;
                Properties.Settings.Default.MaxColumnLength = MaxColumnLength;
                Properties.Settings.Default.Save();
            };
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
                    case "_New":
                        if (RufflePuzzleCurrent != null && RufflePuzzleCurrent.NumSolvedWords != RufflePuzzleCurrent.NumWordsTotal)
                        {
                            foreach (var kvpWordlength in RufflePuzzleCurrent.dictWordListsByLength) // each column of words
                            {
                                if (RufflePuzzleCurrent.dictWordListsByLength.TryGetValue(kvpWordlength.Key, out var lstAllWordsInColumn))
                                {
                                    if (RufflePuzzleCurrent.dictWordsInAnswers.TryGetValue(kvpWordlength.Key, out var lstWordsfound))
                                    {
                                    }
                                    foreach (var word in lstAllWordsInColumn)
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
                            if (RufflePuzzleCurrent._WordScapePuzzle?.LenTargetWord != LenTargetWord
                                || RufflePuzzleCurrent._WordScapePuzzle?.MinSubWordLength != MinSubWordLength
                                || RufflePuzzleCurrent.AllowPlurals != AllowPlurals
                                || RufflePuzzleCurrent.MaxColumnLength != MaxColumnLength
                                ) // if anything changed in the UI since the puzzle was created, we need to discard/recreate
                            {
                                RufflePuzzleCurrent = await RufflePuzzle.CreateRufflePuzzleAsync(LenTargetWord, MinSubWordLength, this);
                            }
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
                        if (wordUserInput.Length >= MinSubWordLength && RufflePuzzleCurrent?.dictWordListsByLength != null)
                        {
                            var wasAdded = false;
                            var wasRemoved = false;
                            var IsInSmallDictionary = RufflePuzzleCurrent._WordScapePuzzle?.wordGenerator._dictionaryLibSmall.IsWord(wordUserInput);
                            var IsInBigDictionary = RufflePuzzleCurrent._WordScapePuzzle?.wordGenerator._dictionaryLibLarge.IsWord(wordUserInput);
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
                                if (RufflePuzzleCurrent.setWordsRemoved.Contains(wordUserInput))
                                {
                                    TextErrorMessage = $"{wordUserInput} was removed from available words";
                                    RaisePropChanged(nameof(TextErrorMessage));
                                    wasRemoved = true;
                                }
                                else
                                {
                                    TextErrorMessage = $"{wordUserInput} not found in current set. IsInBigDictionary = {IsInBigDictionary} IsInSmall = {IsInSmallDictionary}";
                                    RaisePropChanged(nameof(TextErrorMessage));
                                }
                            }
                            var alreadyadded = LstWrdsSoFar.Cast<TextBlock>().Where(t => t.Text == wordUserInput).Any();
                            if (!alreadyadded)
                            {
                                var bkcolr = Colors.Transparent;
                                var forecolorBrush = Brushes.White;
                                if (wasAdded)
                                {
                                    bkcolr = Colors.DarkCyan;
                                }
                                else
                                {
                                    if (wasRemoved)
                                    {
                                        bkcolr = Colors.LightBlue;
                                    }
                                    else
                                    {
                                        if (IsInBigDictionary != null && IsInBigDictionary.Value == true)
                                        {
                                            bkcolr = Colors.Pink;

                                        }
                                        else
                                        {
                                            bkcolr = Colors.LightPink;
                                        }
                                    }
                                }
                                //var bkcolr = wasAdded ? Colors.DarkCyan : (wasRemoved ? Colors.LightBlue : Colors.LightPink);
                                //var forecolor = wasAdded ? Brushes.White : Brushes.Black;
                                var tb = new TextBlock() { Text = wordUserInput, Background = (new SolidColorBrush(bkcolr)), Foreground = forecolorBrush };
                                LstWrdsSoFar.Add(tb);
                                LstWrdsSoFar = new ObservableCollection<UIElement>(LstWrdsSoFar.Cast<TextBlock>().OrderBy(t => t.Text));

                                var animDura = TimeSpan.FromSeconds(.2);
                                var anim = new ColorAnimation(fromValue: Colors.Transparent, toValue: bkcolr, animDura)
                                {
                                    //                                    FillBehavior = FillBehavior.HoldEnd,
                                    RepeatBehavior = new RepeatBehavior(10)
                                };
                                tb.Background = new SolidColorBrush(Colors.Transparent);
                                tb.Background.BeginAnimation(SolidColorBrush.ColorProperty, anim);
                            }
                            else
                            {
                                TextErrorMessage = $"{wordUserInput} already tried";
                                RaisePropChanged(nameof(TextErrorMessage));
                            }
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
        internal WordScapePuzzle? _WordScapePuzzle; // user wordscape lib for word gen
        internal readonly int MaxColumnLength = 16; // max # of e.g. 4 letter words
        internal bool AllowPlurals;
        private MainWindowRuffle mainWindowRuffle;
        readonly int LtrWidthSmall = 25;
        readonly int LtrHeighSmall = 25;
        public int NumSolvedWords = 0;
        public int NumWordsTotal = 0;

        internal Dictionary<int, List<string>> dictWordListsByLength = new(); // WordLen => list<words>
        internal Dictionary<int, List<List<RuffleTile>>> dictTiles = new(); // WordLen=>List<Tile>
        internal Dictionary<int, SortedSet<string>> dictWordsInAnswers = new(); // Wordlen=>List<words>
        internal SortedSet<string> setWordsRemoved = new();

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
                var tile = new RuffleTile(ltr, ruffleTileState: RuffleTileState.Large);
                mainWindowRuffle.spLettersAvailable.Children.Add(tile);
                tile = new RuffleTile(ltr, ruffleTileState: RuffleTileState.Large | RuffleTileState.Hidden);
                mainWindowRuffle.spLettersEntered.Children.Add(tile);
            }
        }

        private RufflePuzzle(MainWindowRuffle mainWindowRuffle)
        {
            MaxColumnLength = mainWindowRuffle.MaxColumnLength;
            AllowPlurals = mainWindowRuffle.AllowPlurals;
            this.mainWindowRuffle = mainWindowRuffle;
        }
        public void FillRuffleGrid()
        {
            setWordsRemoved.Clear();
            if (!AllowPlurals)
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
                        setWordsRemoved.Add(word);
                    }
                }
            }
            foreach (var kvp in dictWordListsByLength) // now shorten any long lists
            {
                while (kvp.Value.Count > MaxColumnLength)
                {
                    var ndxwordToRemove = mainWindowRuffle.random.Next(kvp.Value.Count);
                    setWordsRemoved.Add(kvp.Value[ndxwordToRemove]);
                    kvp.Value.RemoveAt(ndxwordToRemove);
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
                                var tile = new RuffleTile(word[i], RuffleTileState.Hidden);
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
        public RuffleTile(char letter, RuffleTileState ruffleTileState = RuffleTileState.Small)
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
        public override string ToString() => $"{(_letter == '\0' ? " " : _letter)} {ruffleTileState}";
    }
}
