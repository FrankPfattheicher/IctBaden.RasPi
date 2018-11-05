using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace IctBaden.RasPi.System
{
    public static class ModelInfo
    {
        public static string Serial { get; private set; }
        public static int Model { get; private set; }
        public static string Hardware { get; private set; }
        public static int Revision { get; private set; }
        public static string Name { get; private set; }
        public static int RamSizeMb { get; private set; }
        public static bool HardFloat { get; private set; }
        public static bool HasHeaderP5 { get; private set; }
        public static bool HasHeaderJ8 { get; private set; }
        /// <summary>
        /// number of pins on P1 header - Zero means NO P1 header
        /// </summary>
        public static int HasHeaderP1Pins { get; private set; }
        public static bool HasAudio { get; private set; }
        public static bool HasHdmi { get; private set; }

        // https://github.com/google/periph/blob/master/host/rpi/rpi.go
        // https://elinux.org/RPi_HardwareHistory

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
            var hardwareInfo = new Regex(@"Hardware\s+\:\s+(\w+)\s+").Match(cpuinfo);
            Hardware = (hardwareInfo.Success) ? hardwareInfo.Groups[1].Value : "<unknown>";

            var revInfo = new Regex(@"Revision\s+\:\s+(.*)\s+").Match(cpuinfo);
            Revision = (revInfo.Success) ? int.Parse(revInfo.Groups[1].Value, NumberStyles.HexNumber) : -1;

            var serialInfo = new Regex(@"Serial\s+\:\s+(.*)\s+").Match(cpuinfo);
            Serial = (serialInfo.Success) ? serialInfo.Groups[1].Value : "";

            // see https://www.raspberrypi.org/documentation/hardware/raspberrypi/revision-codes/README.md
            Model = 1;
            switch (Revision)
            {
                case 0x00000002:
                    Name = "B1";
                    RamSizeMb = 256;
                    break;
                case 0x00000003:
                    Name = "B1+";
                    RamSizeMb = 256;
                    break;
                case 0x00000004:
                    Name = "B2";
                    RamSizeMb = 256;
                    HasHeaderP5 = true;
                    break;
                case 0x00000005:
                    Name = "B2";
                    RamSizeMb = 256;
                    HasHeaderP5 = true;
                    break;
                case 0x00000006:
                    Name = "B2";
                    RamSizeMb = 256;
                    HasHeaderP5 = true;
                    break;
                case 0x00000007:
                    Name = "A";
                    RamSizeMb = 256;
                    break;
                case 0x00000008:
                    Name = "A";
                    RamSizeMb = 256;
                    break;
                case 0x00000009:
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
                case 0xA21041:
                    Model = 2;
                    Name = "2B";
                    RamSizeMb = 1024;
                    HasHeaderJ8 = true;
                    break;
                case 0xA22042:
                    Model = 2;
                    Name = "2C";
                    RamSizeMb = 1024;
                    HasHeaderJ8 = true;
                    break;
                default:
                    Model = 3;
                    Name = "<unknown>";
                    break;
            }

            HardFloat = Directory.Exists("/lib/arm-linux-gnueabihf");
        }


    }
}