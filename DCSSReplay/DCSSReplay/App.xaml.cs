using System;
using Xamarin.Forms;
using DCSSReplay.Services;
using SkiaSharpFormsDemos.Bitmaps;
using System.IO;

namespace DCSSReplay
{
    public partial class App : Application
    {

        public App(Stream file)
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new ImageView(file);
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
