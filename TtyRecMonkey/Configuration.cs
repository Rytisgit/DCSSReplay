// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.

using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using TtyRecMonkey.Properties;

namespace TtyRecMonkey
{
    [Serializable]
    class ConfigurationData1
    {
        public bool ChunksForceGC = false; // force a garbage collection to unload chunks -- bad idea!
        public int ChunksTargetMemoryMB = 100; // for all 3 expected 'active' chunks
        public int TimeStepLengthMS = 5000;
        public int MaxDelayBetweenPackets = 500;//millisecondss
        public int framerateControlTimeout = 5;
        public bool OpenNone = true;
        public bool OpenFileSelect = false;
        public bool OpenDownload = false;


        public ConfigurationData1()
        {
        }

    }

    static class Configuration
    {
        static readonly string DataFile = "data.cfg";

        private static ConfigurationData1 Defaults = new ConfigurationData1();
        public static ConfigurationData1 Main { get; private set; }

        private static ConfigurationData1 Load(Stream data)
        {
            var bf = new BinaryFormatter();
            var o = bf.Deserialize(data);
            var cd1 = (ConfigurationData1)o;
            return cd1;
        }

        public static void Load(Form towhineat)
        {
            retry:
            Main = new ConfigurationData1();
            try
            {
                using (var data = File.OpenRead(DataFile)) Main = Load(data);
            }
            catch (FileNotFoundException)
            {
                Main = new ConfigurationData1();
            }
            catch (Exception e)
            {
                var result = MessageBox.Show
                    (towhineat
                    , "There was a problem loading your configuration:\n"
                    + e.Message
                    , "Load Error"
                    , MessageBoxButtons.AbortRetryIgnore
                    , MessageBoxIcon.Error
                    );
                switch (result)
                {
                    case DialogResult.Retry: goto retry;
                    case DialogResult.Abort: Application.Exit(); break;
                    case DialogResult.Ignore: break;
                    default: throw new ApplicationException("Should never happen!");
                }
            }
        }

        public static void Save(Form towhineat)
        {
        retry:
            var stream = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(stream, Main);
            stream.Position = 0;

            try
            {
                Load(stream);
            }
            catch (Exception e)
            {
                var result = MessageBox.Show
                    (towhineat
                    , "There was a problem saving your configuration:\n"
                    + "Serialization was successful, but deserializing the result threw an exception!\n"
                    + "Save aborted.  The exception was:\n"
                    + e.Message
                    , "Save Error"
                    , MessageBoxButtons.RetryCancel
                    , MessageBoxIcon.Error
                    );
                if (result == DialogResult.Retry) goto retry;
                return;
            }

            File.WriteAllBytes(DataFile, stream.ToArray());
        }
    }
}
