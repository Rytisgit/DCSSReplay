using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Window
{
    public class Widow_Display
    {
        public string Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TtyRecMonkey");
        string gamelocation;
        public Form1 form;
        public Widow_Display()
        {

            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1();
            if (!File.Exists(Folder + @"\config.ini"))
            {

                Thread t = new Thread((System.Threading.ThreadStart)(() =>
                {

                    //form.folderBrowserDialog1.Description = "Choose your game location";

                    //FolderBrowserDialog dlg = new FolderBrowserDialog();



                    //DialogResult result = dlg.ShowDialog();


                    //if (result == DialogResult.OK && Directory.Exists(dlg.SelectedPath + @"\source\rltiles\mon"))
                    //{

                    //    gamelocation = dlg.SelectedPath;
                    //    //break;


                    //}

                    gamelocation = @"..\..\..\Extra";
                    StreamWriter outputFile = new StreamWriter(Folder + @"\config.ini", false);
                    outputFile.WriteLine(gamelocation);
                    outputFile.Close();


                }));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();



            }

            System.Threading.Thread workerThread = new System.Threading.Thread(() => Application.Run(form));
            workerThread.Start();


        }
        public void Update_Window_Image(Bitmap bmp)
        {
             // form.pictureBox1.Image = bmp;

            form.update(bmp);
      
        }



    }
}
