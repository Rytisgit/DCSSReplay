using System.Linq;
using System.Security.Policy;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace DCSSTV
{
    public enum SaveKeys
    {
        MaxPause,
        MinPause,
        ArrowJump,
        OpenOnStart
    }

    public sealed partial class SaveSettings : ContentDialog
    {

        public SaveSettings()
        {
            this.InitializeComponent();
            this.Opened += SignInContentDialog_Opened;
            //this.Closing += SignInContentDialog_Closing;
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
        }

        //TODO possibly use muxc:NumberBox instead
        private void TextBox_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
    }
}
