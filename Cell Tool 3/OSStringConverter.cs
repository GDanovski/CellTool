using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cell_Tool_3
{
    class OSStringConverter
    {
        public static string StringToDir(string input)
        {
            string output = "";

            switch (System.Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                    output = input.Replace("\\", "/");
                    break;
                case PlatformID.Unix:
                    output = input.Replace("\\", "/");
                    break;
                default:
                    output = input.Replace("/", "\\");
                    break;
            }

            return output;
        }

        public static string GetWinString(string input)
        {
            return input.Replace("/", "\\");
        }
    }
}
