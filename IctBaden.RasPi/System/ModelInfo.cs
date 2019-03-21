using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable MemberCanBePrivate.Global

namespace IctBaden.RasPi.System
{
    /// <summary>
    /// Model information about the current device.
    /// 
    /// https://github.com/google/periph/blob/master/host/rpi/rpi.go
    /// https://elinux.org/RPi_HardwareHistory
    /// </summary>
    public static class ModelInfo
    {
        /// <summary>
        /// Serial number of this board.
        /// </summary>
        public static string Serial { get; private set; }
        /// <summary>
        /// Generic model number 1, 2, 3, ...
        /// </summary>
        public static int Model { get; private set; }
        /// <summary>
        /// see https://www.raspberrypi.org/documentation/hardware/raspberrypi/revision-codes/README.md
        /// </summary>
        public static string Hardware { get; private set; }
        /// <summary>
        /// Board manufacturer.
        /// </summary>
        public static string Manufacturer { get; private set; }
        /// <summary>
        /// Old-style revision codes
        /// The first set of Raspberry Pi revisions were given sequential hex revision codes from 0002 to 0015.
        /// New-style revision codes
        /// With the launch of the Raspberry Pi 2, new-style revision codes were introduced.
        /// Rather than being sequential, each bit of the hex code represents a piece of information about the revision.
        /// uuuuuuuuFMMMCCCCPPPPTTTTTTTTRRRR
        /// see https://www.raspberrypi.org/documentation/hardware/raspberrypi/revision-codes/README.md
        /// </summary>
        public static int RevisionCode { get; private set; }
        /// <summary>
        /// Public known name of the model.
        /// </summary>
        public static string Name { get; private set; }
        /// <summary>
        /// RAM size in MB.
        /// </summary>
        public static int RamSizeMb { get; private set; }
        /// <summary>
        /// Device supports hardware floating point support (arm-hf).
        /// </summary>
        public static bool HardFloat { get; private set; }
        /// <summary>
        /// Physical addresses range starting from thi address for peripherals.
        /// </summary>
        public static uint PeripheralBaseAddress { get; private set; }
        /// <summary>
        /// Base address of the Broadcom VideoCore IV Architecture GPU.
        /// </summary>
        public static uint VideocoreBaseAddress { get; private set; }

        /// <summary>
        /// Device has P5 header.
        /// </summary>
        public static bool HasHeaderP5 { get; private set; }
        /// <summary>
        /// Device has J8 header.
        /// </summary>
        public static bool HasHeaderJ8 { get; private set; }
        /// <summary>
        /// Number of pins on P1 header (26, 40).
        /// Zero means NO P1 header (compute module).
        /// </summary>
        public static int HasHeaderP1Pins { get; private set; } 
        /// <summary>
        /// Device has 3.5mm audio output.
        /// </summary>
        public static bool HasAudio { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public static HdmiConnectors HdmiConnector { get; private set; }


        static ModelInfo()
        {
            const string cpuInfo = "/proc/cpuinfo";
            const string memInfo = "/proc/meminfo";
            if (File.Exists(cpuInfo) && File.Exists(memInfo))
            {
                Decode(File.ReadAllText(cpuInfo), File.ReadAllText(memInfo));
            }
        }


        public static void Decode(string cpuInfo, string memInfo)
        {
        /*
         * Hardware    : BCM2835
         * RevisionCode    : a02082
         * Serial      : 00000000765fc593
         */
            var hardwareInfo = new Regex(@"Hardware\s+\:\s+(\w+)\s+").Match(cpuInfo);
            Hardware = (hardwareInfo.Success) ? hardwareInfo.Groups[1].Value : "<unknown>";

            var revInfo = new Regex(@"Revision\s+\:\s+(.*)\s+").Match(cpuInfo);
            RevisionCode = (revInfo.Success) ? int.Parse(revInfo.Groups[1].Value, NumberStyles.HexNumber) : -1;

            var serialInfo = new Regex(@"Serial\s+\:\s+(.*)\s+").Match(cpuInfo);
            Serial = (serialInfo.Success) ? serialInfo.Groups[1].Value : "";

            Model = 1;
            switch (RevisionCode)
            {
                case 0x00000002:
                    Name = "1B";
                    RamSizeMb = 256;
                    break;
                case 0x00000003: 
                    Name = "1B+"; 
                    RamSizeMb = 256;
                    break;
                case 0x00000004: 
                    Name = "2B"; 
                    RamSizeMb = 256;
                    HasHeaderP5 = true;
                    break;
                case 0x00000005: 
                    Name = "2B"; 
                    RamSizeMb = 256;
                    HasHeaderP5 = true;
                    break;
                case 0x00000006: 
                    Name = "2B"; 
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
                    Name = "2B"; 
                    RamSizeMb = 512;
                    HasHeaderP5 = true;
                    break;
                case 14: 
                    Name = "2B";
                    RamSizeMb = 512;
                    HasHeaderP5 = true;
                    break;
                case 15: 
                    Name = "2B";
                    RamSizeMb = 0;
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
                case 0xA020D3:
                    Model = 3;
                    Name = "3B+";
                    RamSizeMb = 1024;
                    break;
                default:
                    Model = 3;
                    Name = "<unknown>"; 
                    break;
            }

            if (RamSizeMb == 0)
            {
                var memoryInfo = new Regex(@"MemTotal\:\s+(\w+)\s+").Match(memInfo);
                var ramSizeBytes = (memoryInfo.Success) ? long.Parse(memoryInfo.Groups[1].Value) : 0;
                RamSizeMb = 256;
                while ((RamSizeMb * 1024) < ramSizeBytes)
                {
                    RamSizeMb *= 2;
                }
            }
            HardFloat = Directory.Exists("/lib/arm-linux-gnueabihf");
        }

        
    }
}
