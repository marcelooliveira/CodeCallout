using System;
using System.Collections.Generic;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CodeCallout
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            SetupTimer();
            SetupToggleButtons();
            this.Activated += MainWindow_Activated;
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

        
    }
}
