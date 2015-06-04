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
        public static bool HasHeaderP5 { get; private set; }
        public static bool HasHeaderJ8 { get; private set; }
        public static bool HardFloat { get; private set; }

        static ModelInfo()
        {
            const string infoFileName = "/proc/cpuinfo";
            if (!File.Exists(infoFileName)) 
                return;

            var cpuinfo = File.ReadAllText(infoFileName);
            Decode(cpuinfo);
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
                    HasHeaderP5 = true;
                    break;
                case 5: 
                    Name = "B2"; 
                    RamSizeMb = 256;
                    HasHeaderP5 = true;
                    break;
                case 6: 
                    Name = "B2"; 
                    RamSizeMb = 256;
                    HasHeaderP5 = true;
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
                    HasHeaderJ8 = true;
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
                    HasHeaderP5 = true;
                    break;
                case 14: 
                    Name = "B2";
                    RamSizeMb = 512;
                    HasHeaderP5 = true;
                    break;
                case 15: 
                    Name = "B2";
                    RamSizeMb = 512;
                    HasHeaderP5 = true;
                    break;
                case 0xA01041:
                    Name = "2B";
                    RamSizeMb = 1024;
                    HasHeaderJ8 = true;
                    break;
                default: 
                    Name = "<unknown>"; 
                    break;
            }

            HardFloat = Directory.Exists("/lib/arm-linux-gnueabihf");
        }

        
    }
}