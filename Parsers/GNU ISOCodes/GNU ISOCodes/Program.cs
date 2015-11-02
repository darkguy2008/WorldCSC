using Qube.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace GNU_ISOCodes
{
    class Program
    {
        public static string AppPath { get { return new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName + "\\"; } }

        static void Main(string[] args)
        {
            Application app = new Application();
            app.Main(args);
        }
    }
}
