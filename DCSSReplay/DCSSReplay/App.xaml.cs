using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DCSSReplay.Services;
using DCSSReplay.Views;
using SkiaSharpFormsDemos.Bitmaps;
using SkiaSharp;

namespace DCSSReplay
{
    public partial class App : Application
    {

        public App(SKBitmap bmp)
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new ImageView(bmp);
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
