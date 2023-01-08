using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using AngleSharp;
using System.ComponentModel;
using System.Collections.Specialized;
using Microsoft.UI.Xaml.Input;
using Uno.Foundation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DCSSTV.Pages
{
    /// <summary>
    /// SilentObservableCollection is a ObservableCollection with some extensions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SilentObservableCollection<T> : ObservableCollection<T>
    {
        public new void Clear()
        {
            CheckReentrancy();

            Items.Clear();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        }
        /// <summary>
        /// Adds a range of items to the observable collection.
        /// Instead of iterating through all elements and adding them
        /// one by one (which causes OnPropertyChanged events), all
        /// the items gets added instantly without firing events.
        /// After adding all elements, the OnPropertyChanged event will be fired.
        /// </summary>
        /// <param name="enumerable"></param>
        public void AddRange(IEnumerable<T> enumerable)
        {
            CheckReentrancy();

            int startIndex = Count;

            foreach (var item in enumerable)
                Items.Add(item);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T>(enumerable), startIndex));
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        }
    }
    public sealed partial class TtyrecDownloadSelectionDialog : ContentDialog
    {
        private DispatcherTimer _timer;
        IList<string> ttyrecList;
        SilentObservableCollection<string> TtyrecListFiltered;
        readonly Dictionary<string, string> hostsites = new() {

            { "CAO | crawl.akrasiac.org", "http://crawl.akrasiac.org/rawdata/" },
            { "CBR | crawl.berotato.org", "http://crawl.berotato.org/crawl/ttyrec/" },
            { "CDO | crawl.develz.org", "http://crawl.develz.org/ttyrecs/"},
            { "CKO | crawl.kelbi.org", "https://crawl.kelbi.org/crawl/ttyrec/"},
            { "CPO | crawl.project357.org", "https://crawl.project357.org/ttyrec/"},
            { "CUE | underhound.eu", "https://underhound.eu/crawl/ttyrec/" },
            { "CWZ | webzook.net", "https://webzook.net/soup/ttyrecs/"},
            { "CXC | crawl.xtahua.com", "https://crawl.xtahua.com/crawl/ttyrec/"},
            { "LLD | lazy-life.ddo.jp", "http://lazy-life.ddo.jp/mirror/ttyrecs/"}};

        public string CORS_PROXY;
        public string DownloadedWebsite = "";

        public Action<string> PassTtyrecUrl;

        public TtyrecDownloadSelectionDialog(Action<string> passTtyrecUrl, string corsProxy)
        {
            this.InitializeComponent();
            //this.Opened += SignInContentDialog_Opened;
            //this.Closing += SignInContentDialog_Closing;
            ttyrecList = new List<string>();
            CORS_PROXY= corsProxy;

            // Create PeopleFiltered collection and copy data from original People collection
            TtyrecListFiltered = new SilentObservableCollection<string>();

            // Set the ListView's ItemsSource property to the PeopleFiltered collection
            ServerSelectionComboBox.ItemsSource = hostsites.Keys.ToList();
            ServerSelectionComboBox.SelectedIndex = 0;
            TTyrecSelectionListView.ItemsSource = TtyrecListFiltered;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += Timer_Tick;

            PassTtyrecUrl = passTtyrecUrl;

            this.LayoutUpdated += Focus;
        }

        private void Timer_Tick(object sender, object e)
        {
            _timer.Stop();
            TtyrecListFiltered.Clear();
            TtyrecListFiltered.AddRange(
                ttyrecList.Where(
                    ttyrecItem => ttyrecItem.Contains(TTyrecFilterTextBox.Text)));
        }
        private void TTyrecFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _timer.Stop();
            _timer.Start();
        }

        private async void TTyrecFilterTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Check if the key pressed was the Enter key
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // Submit the form or trigger the action
                await SearchTTyrecs();
            }
        }

        private async void Dialog_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Check if the key pressed was the Enter key
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                // Submit the form or trigger the action
                this.Hide();
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Ensure the user name and password fields aren't empty. If a required field
            // is empty, set args.Cancel = true to keep the dialog open.

            if (string.IsNullOrEmpty(UrlTextBox.Text))
            {
                args.Cancel = true;
                //errorTextBlock.Text = "User name is required.";
            }

            // If you're performing async operations in the button click handler,
            // get a deferral before you await the operation. Then, complete the
            // deferral when the async operation is complete.

            ContentDialogButtonClickDeferral deferral = args.GetDeferral();
            //SaveSettings();
            PassTtyrecUrl(UrlTextBox.Text);
            deferral.Complete();
        }

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //cancel any ongoing download?
        }

        void Focus(object sender, object e)
        {
#if __WASM__
            WebAssemblyRuntime.InvokeJS("document.evaluate(\" /html/body/div/div/div[3]/div\", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.addEventListener(\"click\", focusTtyrecDownload);");
            WebAssemblyRuntime.InvokeJS("focusTtyrecDownload()");
#endif
        }
        private void TTyrecSelectionListView_ItemSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedKey = e.AddedItems.First().ToString();
            UrlTextBox.Text = selectedKey.Contains("http") ? selectedKey : DownloadedWebsite + selectedKey;
        }
        private async Task SearchTTyrecs()
        {
            //if (comboBox1.SelectedIndex == -1) MessageBox.Show("Server not selected");
            //else
            //{
            //linkList.Clear();

            var website = hostsites[ServerSelectionComboBox.SelectedItem.ToString()] + PlayerNameTextBox.Text + '/';
            var proxyWebsite = CORS_PROXY + website;
            if (await CheckUrlExists(proxyWebsite) && PlayerNameTextBox.Text != "")
            {

                var config = AngleSharp.Configuration.Default
                .With(new AngleSharp.Io.Network.HttpClientRequester()) // only requester
                .WithDefaultLoader();
                IBrowsingContext context = BrowsingContext.New(config);
                var document = await context.OpenAsync(proxyWebsite);

                //if (!doc.DocumentNode.SelectNodes("//a[@href]").Any(node => node.InnerText.Contains("ttyrec")))
                //{
                //    MessageBox.Show("No TTyRecs Found");
                //}
                //reutnr?

                ttyrecList.Clear();
                foreach (var cell in document.GetElementsByTagName("a"))
                {
                    if (cell.TextContent.Contains(".ttyrec"))
                    {
                        ttyrecList.Add(cell.TextContent);
                    }

                }
                DownloadedWebsite = website;
                TtyrecListFiltered.AddRange(ttyrecList);
            }
            else
            {

                //Console.WriteLine("ITsnot wokring");
                //MessageBox.Show("Not a Valid Player Name");
            }
            //}
        }

        private async void Button_Click_SearchTTyrecs(object sender, RoutedEventArgs e)
        {
            await SearchTTyrecs();
        }

        private async Task<bool> CheckUrlExists(string url)
        {
            if (!hostsites.Values.Any(host => url.Contains(host)))
                return false;
            try
            {
                using var client = new HttpClient();
                client.Timeout = new TimeSpan(0, 0, 5);
                var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Head, url);
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode) //Good requests
                {
                    return true;
                }
                else
                {
                    Console.WriteLine(string.Format("The remote server has thrown an internal error. Url is not valid: {0}", url));
                    return false;
                }
            }
            catch
            {
                Console.WriteLine(string.Format("The remote server has thrown an internal error. Url is not valid: {0}", url));
                return false;
            }
        }

        private void TextBox_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsLetterOrDigit(c));
        }
    }
}
