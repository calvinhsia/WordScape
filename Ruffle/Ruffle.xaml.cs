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


        public int LenTargetWord { get; set; } = 8;
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
                //var x = RufflePuzzleCurrent.wordContainer.InitialWord;
                RufflePuzzleCurrent.FillRuffleGrid();

            }
            catch (Exception)
            {
            }
        }
    }

    public class RufflePuzzle
    {
        WordScapePuzzle? _WordScapePuzzle;
        int maxWordListLength = 14; // max # of e.g. 4 letter words
        private MainWindowRuffle mainWindowRuffle;
        Dictionary<int, List<string>>? dictWordListsByLength; // len => list<words>
        public static async Task<RufflePuzzle> CreateRufflePuzzleAsync(int lenTargetWord, int minSubWordLength, MainWindowRuffle mainWindowRuffle)
        {
            var puz = new RufflePuzzle
            {
                _WordScapePuzzle = await WordScapePuzzle.CreateNextPuzzleTask(new WordGenerationParms()
                {
                    LenTargetWord = lenTargetWord,
                    MinSubWordLength = minSubWordLength,
                    _Random =mainWindowRuffle.random
                })
            };
            puz.mainWindowRuffle = mainWindowRuffle;
            puz.dictWordListsByLength = puz._WordScapePuzzle.wordContainer.subwords.GroupBy(w => w.Length).ToDictionary(kvp => kvp.Key, kvp => kvp.ToList());
            puz.TrimListsIfNecessary();
            return puz;
        }

        private void TrimListsIfNecessary()
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
        }

        private RufflePuzzle()
        {

        }
        public void FillRuffleGrid()
        {
            //var lstWords = new List<List<string>>(); 
            if (_WordScapePuzzle == null)
            {
                throw new NullReferenceException();
            }
            var column = 0;
            for (var wordlength = _WordScapePuzzle.MinSubWordLength; wordlength < _WordScapePuzzle.LenTargetWord; wordlength++)
            {
                if (dictWordListsByLength.TryGetValue(wordlength, out var curwordlist))
                {
                    if (curwordlist != null)
                    {
                        var rowNdx = 0;
                        foreach (var word in curwordlist)
                        {
                            for (var i = 0; i < wordlength; i++)
                            {
                                var tile = new RuffleTile(word[i], rowNdx, mainWindowRuffle);
                                var x = column * 100 + i * 20;
                                var y = rowNdx * 20;
                                Canvas.SetTop(tile, y);
                                Canvas.SetLeft(tile, x);
                                tile.ToolTip = $"{x} {y}";
                                mainWindowRuffle.cvs.Children.Add(tile);
                            }
                            rowNdx++;
                        }
                    }
                }
                column++;
            }
        }
    }

    // A tile can be large or small: Large for the Letters Available List and the list of entered letters
    [Flags]
    public enum RuffleTileState
    {
        Large = 0x100,
        Revealed = 0x2, // if revealed, show the letter content, else hide it
        Small = 0,
    }
    public class RuffleTile : Button
    {
        private readonly char _letter;

        public RuffleTile(char letter, int Rowndx, MainWindowRuffle mainWindowRuffle)
        {
            _letter = letter;
            Content = _letter.ToString();
        }

    }
}
