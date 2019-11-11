using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace Window
{
    public class Widow_Display
    {
        string gamelocation;
        public Form1 form;
        public Widow_Display()
        {
           
            Application.EnableVisualStyles();           
            //Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1();
            if (!File.Exists(@"..\..\..\..\FrameGenerator\Extra\config.ini"))
            {
              
                Thread t = new Thread((System.Threading.ThreadStart)(() => {

                    form.folderBrowserDialog1.Description = "Choose your game location";

                    FolderBrowserDialog dlg = new FolderBrowserDialog();


                 
                        DialogResult result = dlg.ShowDialog();


                        if (result == DialogResult.OK && Directory.Exists(dlg.SelectedPath + @"\source\rltiles\mon"))
                        {

                                gamelocation = dlg.SelectedPath;
                            //break;
                            

                        }                                                                             
               
                    }));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
         
                using (StreamWriter outputFile = new StreamWriter(@"..\..\..\..\FrameGenerator\Extra\config.ini"))
                {
                        outputFile.WriteLine(gamelocation);
                }


            }
         
            System.Threading.Thread workerThread = new System.Threading.Thread(() => Application.Run(form));
            workerThread.Start();


        }
        public void Update_Window_Image(Bitmap bmp)
        {         
                form.pictureBox1.Image = bmp;
            
            //form.update(bmp);         
            
        }



    }
}
