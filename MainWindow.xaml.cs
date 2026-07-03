using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace WebhookGUI
{
    public partial class MainWindow : Window
    {
        private int _wipedCount = 0;
        private int _sentCount = 0;
        private bool _isSpamming = false;

        public MainWindow()
        {
            InitializeComponent();
            AddLog("SYSTEM READY...");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlInput.Text;
            if (string.IsNullOrWhiteSpace(url))
            {
                AddLog("ERROR: INVALID TARGET URL");
                return;
            }

            StatusTxt.Text = "DELETING";
            StatusTxt.Foreground = Brushes.Red;
            AddLog("INITIATING PURGE SEQUENCE...");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.DeleteAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        _wipedCount++;
                        WipedTxt.Text = _wipedCount.ToString();
                        AddLog("SUCCESS: TARGET SECTOR PURIFIED.");
                    }
                    else
                    {
                        AddLog($"FAILURE: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"CRITICAL: {ex.Message}");
            }

            StatusTxt.Text = "ONLINE";
            StatusTxt.Foreground = Brushes.Magenta;
        }

        private async void Spam_Click(object sender, RoutedEventArgs e)
        {
            if (_isSpamming)
            {
                _isSpamming = false;
                SpamBtn.Content = "INITIATE OVERLOAD (SPAM)";
                SpamBtn.BorderBrush = Brushes.Lime;
                AddLog("OVERLOAD DISCONTINUED.");
                return;
            }

            string url = UrlInput.Text;
            string msg = MsgInput.Text;
            if (string.IsNullOrWhiteSpace(url))
            {
                AddLog("ERROR: TARGET URL REQUIRED");
                return;
            }

            _isSpamming = true;
            SpamBtn.Content = "STOP OVERLOAD (SPAM)";
            SpamBtn.BorderBrush = Brushes.Red;
            StatusTxt.Text = "OVERLOADING";
            StatusTxt.Foreground = Brushes.Lime;
            AddLog("OVERLOAD SEQUENCE ENGAGED...");

            while (_isSpamming)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var content = new StringContent("{\"content\":\"" + msg + "\"}", Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(url, content);
                        if (response.IsSuccessStatusCode)
                        {
                            _sentCount++;
                            SentTxt.Text = _sentCount.ToString();
                        }
                        else if ((int)response.StatusCode == 429)
                        {
                            AddLog("RATE LIMIT: RETRYING...");
                            await Task.Delay(2000);
                        }
                        else
                        {
                            AddLog($"DROP: {response.StatusCode}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddLog($"CRITICAL: {ex.Message}");
                    _isSpamming = false;
                }

                await Task.Delay(50);
            }

            StatusTxt.Text = "ONLINE";
            StatusTxt.Foreground = Brushes.Magenta;
        }

        private void AddLog(string text)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            LogBox.Items.Insert(0, $"[{time}] {text}");
            if (LogBox.Items.Count > 50) LogBox.Items.RemoveAt(50);
        }
    }
}
