using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WordScape
{
    internal class LetterWheel
    {
        private MainWindow mainWindow;
        private Canvas canvasCircleControl;
        private WordContainer wordCont;
        private GenGrid gridgen;

        public LetterWheel(MainWindow mainWindow, Canvas canvasCircleControl, WordContainer wordCont, GenGrid gridgen)
        {
            this.mainWindow = mainWindow;
            this.canvasCircleControl = canvasCircleControl;
            this.wordCont = wordCont;
            this.gridgen = gridgen;
            this.CreateCircle();
        }

        private void CreateCircle()
        {
            this.canvasCircleControl.Children.Clear();
            this.canvasCircleControl.Background = Brushes.AliceBlue;
            var circ = new Ellipse()
            {
                Width = 300,
                Height = 300,
                Fill = Brushes.White,
                StrokeThickness = 3,
                Stroke = Brushes.Black
            };
            Canvas.SetLeft(circ, 50);
            Canvas.SetTop(circ, 10);
            this.canvasCircleControl.Children.Add(circ);

            var ltr1 = new TextBlock() { Text = "A", FontSize = 40 };
            Canvas.SetLeft(ltr1, 130);
            Canvas.SetTop(ltr1, 60);
            this.canvasCircleControl.Children.Add(ltr1);

            var numLtrs = wordCont.InitialWord.Length;

            foreach (var ltr in wordCont.InitialWord)
            {
                var lett = new TextBlock() { Text = ltr.ToString().ToUpper() };
            }

            var pl = new Polyline()
            {
                Stroke = Brushes.Red,
                StrokeThickness = 8
            };
            this.canvasCircleControl.Children.Add(pl);
            pl.Points.Add(new System.Windows.Point(0, 0));
            pl.Points.Add(new System.Windows.Point(77, 44));
        }
    }
}