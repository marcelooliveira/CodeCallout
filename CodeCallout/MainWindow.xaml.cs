﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tesseract;

namespace CodeCallout
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly DispatcherTimer timer;
        System.Windows.Point downPos;
        //System.Windows.Rect rectScreenshot = new System.Windows.Rect(0, 0, 10, 10);
        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            SetupTimer();
            SetupToggleButtons();

            this.Activated += MainWindow_Activated;
            this.Loaded += MainWindow_Loaded;
            inkCanv.MouseUp += InkCanv_MouseUp;
            inkCanv.MouseMove += InkCanv_MouseMove;

            this.gridTop.MouseDown += GridTop_MouseDown;
            this.gridTop.MouseMove += GridTop_MouseMove;
            this.gridTop.MouseUp += GridTop_MouseUp;
        }

        private void GridTop_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (rectSelection.Width == 1 && 
                rectSelection.Height == 1
            )
            {
                System.Windows.Forms.SendKeys.SendWait("%{TAB}");
            }
            else
            {
                var rect = new System.Windows.Rect(
            rectSelection.Margin.Left - rectSelection.StrokeThickness * 2,
            rectSelection.Margin.Top - rectSelection.StrokeThickness * 2,
            Math.Max(1, rectSelection.Width - rectSelection.StrokeThickness * 2 - 1),
            Math.Max(1, rectSelection.Height - rectSelection.StrokeThickness * 2 - 1));

                var bitmap = CaptureScreenshot.Capture(rect);
                var text = ProcessOCR(bitmap);
                txt.Text = text;
                //rectScreenshot = new System.Windows.Rect(0, 0, 10, 10);
                rectSelection.Margin = new Thickness(0);
                rectSelection.Width = 1;
                rectSelection.Height = 1; 
            }
        }

        private void GridTop_MouseMove(object sender, MouseEventArgs e)
        {
            bool mouseIsDown = System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed;
            if (mouseIsDown)
            {
                var pos = Mouse.GetPosition(this);

                rectSelection.Width = Math.Abs(pos.X - downPos.X);
                if (pos.X - downPos.X >= 0)
                    rectSelection.Margin = 
                        new Thickness(rectSelection.Margin.Left, rectSelection.Margin.Top, 0, 0);
                else
                    rectSelection.Margin = 
                        new Thickness(pos.X, rectSelection.Margin.Top, 0, 0);

                rectSelection.Height = Math.Abs(pos.Y - downPos.Y);
                if (pos.Y - downPos.Y >= 0)
                    rectSelection.Margin =
                        new Thickness(rectSelection.Margin.Left, rectSelection.Margin.Top, 0, 0);
                else
                    rectSelection.Margin =
                        new Thickness(rectSelection.Margin.Left, pos.Y, 0, 0);
            }
            //throw new NotImplementedException();
        }

        private void GridTop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            downPos = Mouse.GetPosition(this);
            inkCanv.Strokes.Clear();
            rectSelection.Margin = new Thickness(downPos.X, downPos.Y, 0, 0);
        }

        private void InkCanv_MouseMove(object sender, MouseEventArgs e)
        {
            //rectSelection.Margin = new Thickness(rectScreenshot.X, rectScreenshot.Y, 0, 0);
            //rectSelection.Width = 50;
            //rectSelection.Height = 50;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //inkCanv.AddHandler(InkCanvas.MouseDownEvent, new MouseButtonEventHandler(InkCanvas_MouseDown), true);
        }

        private void InkCanv_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //var pos = Mouse.GetPosition(this);
            //rectScreenshot = new System.Windows.Rect(
            //    rectScreenshot.X, 
            //    rectScreenshot.Y
            //    , pos.X - rectScreenshot.X
            //    , pos.Y - rectScreenshot.Y);
            //var bitmapSource = CaptureScreenshot.Capture(rectScreenshot);
            //ProcessOCR(bitmapSource);
        }

        private static byte[] GetByteArray(BitmapSource bitmapSource)
        {
            byte[] byteArray;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = 100;
            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);
                byteArray = stream.ToArray();
                stream.Close();
            }

            return byteArray;
        }

        private static string ProcessOCR(Bitmap bitmap)
        {
            var text = "";
            var demoFilename = String.Format(@"C:\Users\marce\Desktop\OCR.png");
            bitmap.Save(demoFilename);
            //SaveBitmapSourceToFile(bitmap, demoFilename);
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                using (var pix = Pix.LoadFromFile(demoFilename))
                {
                    using (var page = engine.Process(pix))
                    {
                        text = page.GetText().Trim();
                    }
                }
            }
            return text;
        }

        public static void SaveBitmapSourceToFile(BitmapSource bitmapSource, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(fileStream);
            }
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                txt.Text = Clipboard.GetText();
            }
        }

        private void SetupToggleButtons()
        {
            string exeLocation = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
            string localFolder = System.IO.Path.GetDirectoryName(exeLocation);
            string imageFolder = System.IO.Path.Combine(localFolder, "Images");
            string[] files = Directory.GetFiles(imageFolder);
            foreach (var file in files)
            {
                var fileName = System.IO.Path.GetFileName(file);
                var toggleButton = new ToggleButton
                {
                    Content = fileName.Replace(".png", ""),
                    Margin = new Thickness(8, 0, 8, 0)
                };

                toggleButton.Click += ToggleButton_Click;
                pnlButtons.Children.Add(toggleButton);
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = (sender as ToggleButton);
            if (toggleButton != null)
            {
                int childAmount = VisualTreeHelper.GetChildrenCount(toggleButton.Parent);

                ToggleButton tb;
                for (int i = 0; i < childAmount; i++)
                {
                    tb = null;
                    tb = VisualTreeHelper.GetChild(toggleButton.Parent, i) as ToggleButton;

                    if (tb != null)
                        tb.IsChecked = false;
                }

                toggleButton.IsChecked = true;

                txt.Text = toggleButton.Content.ToString();
            }
        }

        private void SetupTimer()
        {
            timer.Interval = TimeSpan.FromMilliseconds(0);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            try
            {
                img.Source = new BitmapImage(
                    new Uri(string.Format(@"\images\{0}.png", txt.Text.ToLower()), UriKind.Relative));
            }
            catch { }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            txt.Focus();
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            timer.Stop();
            timer.Start();
        }

        private void btnPen_Click(object sender, RoutedEventArgs e)
        {
            gridTop.Visibility = 
                (btnPen.IsChecked.HasValue && (bool)btnPen.IsChecked ? Visibility.Hidden : Visibility.Visible);

            inkCanv.Strokes.Clear();
        }
    }
}
