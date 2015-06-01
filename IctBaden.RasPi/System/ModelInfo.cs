using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace IctBaden.RasPi.System
{
    public static class ModelInfo
    {
        public static string Hardware { get; private set; }
        public static int Revision { get; private set; }
        public static string Name { get; private set; }
        public static int RamSizeMb { get; private set; }

        static ModelInfo()
        {
            const string infoFileName = "/proc/cpuinfo";
            if (File.Exists(infoFileName))
            {
                var cpuinfo = File.ReadAllText(infoFileName);
                Decode(cpuinfo);
            }
        }

        public static void Decode(string cpuinfo)
        {
            var hardwareInfo = new Regex(@"Hardware\s+\:\s+(.*)\s+").Match(cpuinfo);
            Hardware = (hardwareInfo.Success) ? hardwareInfo.Groups[1].Value : "<unknown>";

            var revInfo = new Regex(@"Revision\s+\:\s+(.*)\s+").Match(cpuinfo);
            Revision = (revInfo.Success) ? int.Parse(revInfo.Groups[1].Value, NumberStyles.AllowHexSpecifier) : -1;

            switch (Revision)
            {
                case 2:
                    Name = "B1";
                    RamSizeMb = 256;
                    break;
                case 3: 
                    Name = "B1+"; 
                    RamSizeMb = 256;
                    break;
                case 4: 
                    Name = "B2"; 
                    RamSizeMb = 256;
                    break;
                case 5: 
                    Name = "B2"; 
                    RamSizeMb = 256;
                    break;
                case 6: 
                    Name = "B2"; 
                    RamSizeMb = 256;
                    break;
                case 7: 
                    Name = "A"; 
                    RamSizeMb = 256;
                    break;
                case 8: 
                    Name = "A"; 
                    RamSizeMb = 256;
                    break;
                case 9: 
                    Name = "A"; 
                    RamSizeMb = 256;
                    break;
                case 10:
                    Name = "B+";
                    RamSizeMb = 512;
                    break;
                case 11:
                    Name = "Compute";
                    RamSizeMb = 512;
                    break;
                case 12:
                    Name = "A+";
                    RamSizeMb = 256;
                    break;
                case 13: 
                    Name = "B2"; 
                    RamSizeMb = 512;
                    break;
                case 14: 
                    Name = "B2";
                    RamSizeMb = 512;
                    break;
                case 15: 
                    Name = "B2";
                    RamSizeMb = 512;
                    break;
                case 0xA01041:
                    Name = "2B";
                    RamSizeMb = 1024;
                    break;
                default: 
                    Name = "<unknown>"; 
                    break;
            }
        }
    }
}