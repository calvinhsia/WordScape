using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WordScape
{
    public class LetterWheel : Canvas
    {
        private MainWindow mainWindow;
        private WordContainer wordCont;
        private GenGrid gridgen;

        private List<LetterWheelLetter> _lstLetters = new List<LetterWheelLetter>();
        public LetterWheel()
        {

        }
        public void LetterWheelInit(MainWindow mainWindow, WordContainer wordCont, GenGrid gridgen)
        {
            this.mainWindow = mainWindow;
            this.wordCont = wordCont;
            this.gridgen = gridgen;
            this.CreateCircle();
        }

        private void CreateCircle()
        {
            this.Children.Clear();
            this.Background = Brushes.AliceBlue;
            var circRadius = 150;
            var circ = new Ellipse()
            {
                Width = 2 * circRadius,
                Height = 2 * circRadius,
                Fill = Brushes.White,
                StrokeThickness = 3,
                Stroke = Brushes.Black
            };
            var ptcircpos = new Point(50, 10);
            Canvas.SetLeft(circ, ptcircpos.X);
            Canvas.SetTop(circ, ptcircpos.Y);
            var ptCircCtr = new Point(ptcircpos.X + circ.Width / 2, ptcircpos.Y + circ.Height / 2);
            this.Children.Add(circ);

            var numLtrs = wordCont.InitialWord.Length;
            int ndx = 0;
            var radsPerLetter = (2 * Math.PI / numLtrs);
            foreach (var ltr in wordCont.InitialWord.ToUpper())
            {
                var lett = new LetterWheelLetter(ltr);
                _lstLetters.Add(lett);

                var x = ptCircCtr.X + .7 * circRadius * Math.Cos(radsPerLetter * ndx) - lett.Width / 2;
                var y = ptCircCtr.Y - .7 * circRadius * Math.Sin(radsPerLetter * ndx) - lett.Height / 2;
                var letpt = new Point(x, y);
                Canvas.SetLeft(lett, letpt.X);
                Canvas.SetTop(lett, letpt.Y);
                this.Children.Add(lett);
                if (ndx % 2 == 0)
                {
                    //                    lett.Select();
                }
                ndx++;
            }

            var polyLine = new Polyline()
            {
                Stroke = Brushes.Red,
                StrokeThickness = 10
            };
            this.Children.Add(polyLine);
            var mouseIsDown = false;
            Func<MouseEventArgs, LetterWheelLetter> ltrFromArgs = (args) =>
            {
                var pt = args.GetPosition(this);
                LetterWheelLetter ltrUnderMouse = null;
                var x = this.InputHitTest(pt);
                if (x != null)
                {
                    if (x is TextBlock)
                    {
                        ltrUnderMouse = VisualTreeHelper.GetParent(x as TextBlock) as LetterWheelLetter;
                    }
                    else if (x is LetterWheelLetter)
                    {
                        ltrUnderMouse = x as LetterWheelLetter;
                    }
                }
                return ltrUnderMouse;
            };
            this.MouseDown += (o, e) =>
             {
                 polyLine.Points.Clear();
                 var ltrUnderMouse = ltrFromArgs(e);
                 if (ltrUnderMouse != null)
                 {
                     ltrUnderMouse.Select();
                     //var tb = (ltrUnderMouse.Child as TextBlock);
                     //var pp = tb.TranslatePoint(new System.Windows.Point(0, 0), this);
                     //Debug.WriteLine($"ltrw = {ltrUnderMouse.ltr} {pp} ");
                     mouseIsDown = true;
                     var pt = ltrUnderMouse.TranslatePoint(new Point(0, 0), this);
                     pt.X += ltrUnderMouse.Width / 2;
                     pt.Y += ltrUnderMouse.Height / 2;
                     polyLine.Points.Add(pt);
                 }
             };
            this.MouseMove += (o, e) =>
              {
                  var ltrUnderMouse = ltrFromArgs(e);
                  if (mouseIsDown)
                  {
                      if (ltrUnderMouse != null)
                      {
                          ltrUnderMouse.Select();
                          var pt = ltrUnderMouse.TranslatePoint(new Point(0, 0), this);
                          pt.X += ltrUnderMouse.Width / 2;
                          pt.Y += ltrUnderMouse.Height / 2;
                          polyLine.Points.Add(pt);
                      }
                      else
                      {
                          if (polyLine.Points.Count > 1)
                          {
                              polyLine.Points.RemoveAt(polyLine.Points.Count - 1);
                          }
                          polyLine.Points.Add(e.GetPosition(this));
                      }
                  }
              };
            this.MouseUp += (o, e) =>
             {
                 mouseIsDown = false;
                 polyLine.Points.Clear();
                 foreach (var ltr in _lstLetters)
                 {
                     ltr.UnSelect();
                 }
             };
        }
    }
    public class LetterWheelLetter : Border
    {
        internal char ltr;
        public bool IsSelected;

        /*
<Border Grid.Row="1" CornerRadius="45" Height="50" Width="50">
<TextBlock Text="D" FontSize="40" Foreground="Black" HorizontalAlignment="Center" VerticalAlignment="Center"/>
</Border>

<Border Grid.Row="1" BorderThickness="1" BorderBrush="Black" Background="Green" CornerRadius="45" Height="50" Width="50">
<TextBlock Text="D" FontSize="40" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center"/>
</Border>
*/
        public LetterWheelLetter(char ltr)
        {
            this.ltr = ltr;
            //            this.BorderThickness = new System.Windows.Thickness(0);
            //            this.BorderBrush = Brushes.Black;
            this.CornerRadius = new System.Windows.CornerRadius(45);
            this.Height = 60;
            this.Width = 60;
            this.Background = Brushes.White;
            this.Child = new TextBlock()
            {
                Text = ltr.ToString(),
                FontSize = 50,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
        }

        public void Select()
        {
            if (!IsSelected)
            {
                var txtb = this.Child as TextBlock;
                txtb.Foreground = Brushes.White;
                this.Background = Brushes.Green;
                IsSelected = true;
            }
        }
        public void UnSelect()
        {
            if (IsSelected)
            {
                var txtb = this.Child as TextBlock;
                txtb.Foreground = Brushes.Black;
                this.Background = Brushes.Green;
                this.Background = Brushes.White;
                IsSelected = false;
            }
        }
    }
}