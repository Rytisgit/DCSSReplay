using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Uno.Foundation;
using Windows.Storage;

namespace DCSSTV
{
    public enum SaveKeys
    {
        MaxPause,
        MinPause,
        ArrowJump,
        OpenOnStart,
        TileDataVersion
    }

    public sealed partial class SaveSettings : ContentDialog
    {
        public SaveSettings()
        {
            this.InitializeComponent();
            this.Opened += SignInContentDialog_Opened;
            //this.Closing += SignInContentDialog_Closing;
            this.LayoutUpdated += Focus;

        }
        void Focus(object sender, object e)
        {
            Console.WriteLine("Focussed something in the ttyrec download");
#if __WASM__
            WebAssemblyRuntime.InvokeJS("document.evaluate(\" /html/body/div/div/div[3]/div\", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.addEventListener(\"click\", focusSettings);");
            WebAssemblyRuntime.InvokeJS("focusSettings()");
            Console.WriteLine("FOCUSSSSSSSSSSSSS");
#endif
        }
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Ensure the user name and password fields aren't empty. If a required field
            // is empty, set args.Cancel = true to keep the dialog open.

            if (string.IsNullOrEmpty(ArrowJump.Text))
            {
                args.Cancel = true;
                //errorTextBlock.Text = "User name is required.";
            }
            else if (string.IsNullOrEmpty(MinPause.Text))
            {
                args.Cancel = true;
                //errorTextBlock.Text = "Password is required.";
            }
            else if (string.IsNullOrEmpty(MaxPause.Text))
            {
                args.Cancel = true;
                //errorTextBlock.Text = "Password is required.";
            }

            // If you're performing async operations in the button click handler,
            // get a deferral before you await the operation. Then, complete the
            // deferral when the async operation is complete.

            ContentDialogButtonClickDeferral deferral = args.GetDeferral();
            SaveSettingsLocally();
            deferral.Complete();
        }

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

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

        void SignInContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            MaxPause.Text = localSettings.Values[SaveKeys.MaxPause.ToString()].ToString();
            ArrowJump.Text = localSettings.Values[SaveKeys.ArrowJump.ToString()].ToString();
            MinPause.Text = localSettings.Values[SaveKeys.MinPause.ToString()].ToString();
            switch (localSettings.Values[SaveKeys.OpenOnStart.ToString()].ToString())
            {
                case "File": { OpenFile.IsChecked = true; break; }
                case "Download": { OpenDownload.IsChecked = true; break; }
                default: { None.IsChecked = true; break; }
            }
            switch (localSettings.Values[SaveKeys.TileDataVersion.ToString()].ToString())
            {
                case "Classic": { Classic.IsChecked = true; break; }
                case "2023": { Version2023.IsChecked = true; break; }
            }
        }

        private void SaveSettingsLocally()
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            //TODO doesn't handle empty settings
            localSettings.Values[SaveKeys.MaxPause.ToString()] = MaxPause.Text;
            localSettings.Values[SaveKeys.ArrowJump.ToString()] = ArrowJump.Text;
            localSettings.Values[SaveKeys.MinPause.ToString()] = MinPause.Text;
            if ((bool)OpenFile.IsChecked) localSettings.Values[SaveKeys.OpenOnStart.ToString()] = "File";
            else if ((bool)OpenDownload.IsChecked) localSettings.Values[SaveKeys.OpenOnStart.ToString()] = "Download";
            else localSettings.Values[SaveKeys.OpenOnStart.ToString()] = "None";
            if ((bool)Classic.IsChecked) localSettings.Values[SaveKeys.TileDataVersion.ToString()] = "Classic";
            else localSettings.Values[SaveKeys.TileDataVersion.ToString()] = "2023";
        }

        //TODO possibly use muxc:NumberBox instead
        private void TextBox_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
    }
}
