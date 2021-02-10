using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using FrameGenerator;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using Android;
using Putty;

namespace DCSSReplay.Droid
{
    [Activity(Label = "DCSSReplay", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private void CheckAppPermissions()
        {
            if ((int)Build.VERSION.SdkInt < 23)
            {
                return;
            }
            else
            {
                if (PackageManager.CheckPermission(Manifest.Permission.ReadExternalStorage, PackageName) != Permission.Granted
                    && PackageManager.CheckPermission(Manifest.Permission.WriteExternalStorage, PackageName) != Permission.Granted)
                {
                    var permissions = new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage };
                    RequestPermissions(permissions, 1);
                }
            }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            //var term = new Terminal(80, 24);
            //var i = term.GetLine(1);
            ExtractExtraFileFolder();
            //var files = Directory.GetFiles(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "*", SearchOption.AllDirectories);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App(Assets.Open("neopolispart1.ttyrec")));
        }

        private void ExtractExtraFileFolder()
        {
            CheckAppPermissions();
            try
            {
                //File.Delete(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/Extra");
                var path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), @"Extra.zip");
                using (var asset = Assets.Open("Extra.zip"))
                using (var dest = System.IO.File.Create(path))
                    asset.CopyTo(dest);
                FileStream fileStreamIn = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                var zipInStream = new ZipInputStream(fileStreamIn);
                var entry = zipInStream.GetNextEntry();
                while (entry != null && entry.CanDecompress)
                {
                    var outputFile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"/" + entry.Name;
                    var outputDirectory = Path.GetDirectoryName(outputFile);

                    if (!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }

                    if (entry.IsFile)
                    {
                        var fileStreamOut = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
                        int size;
                        byte[] buffer = new byte[4096];
                        do
                        {
                            size = zipInStream.Read(buffer, 0, buffer.Length);
                            fileStreamOut.Write(buffer, 0, size);
                        } while (size > 0);
                        fileStreamOut.Close();
                    }

                    entry = zipInStream.GetNextEntry();
                }
                zipInStream.Close();
                fileStreamIn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}