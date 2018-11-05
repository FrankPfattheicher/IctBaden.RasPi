using System.Diagnostics;
using IctBaden.RasPi.System;
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace IctBaden.RasPi.IO
{
    /// <summary>
    /// RPi GPIO: J8 40-pin header
    /// --------------------------------
    ///         +3V3  1  2   +5V
    /// GPIO2   SDA1  3  4   +5V
    /// GPIO3   SCL1  5  6   GND
    /// GPIO4   GCLK  7  8   TXD0  GPIO14
    ///          GND  9  10  RXD0  GPIO15
    /// GPIO17  GEN0  11 12  GEN1  GPIO18
    /// GPIO27  GEN2  13 14  GND
    /// GPIO22  GEN3  15 16  GEN4  GPIO23
    ///         +3V3  17 18  GEN5  GPIO24
    /// GPIO10  MOSI  19 20  GND
    /// GPIO9   MISO  21 22  GEN6  GPIO25
    /// GPIO11  SCLK  23 24  CE0_N GPIO8
    ///          GND _25_26_ CE1_N GPIO7
    /// EEPROM ID_SD  27 28  ID_SC EEPROM
    /// GPIO5         29 30  GND
    /// GPIO6         31 32        GPIO12
    /// GPIO13        33 34  GND
    /// GPIO19        35 36        GPIO16
    /// GPIO26        37 38        GPIO20
    ///          GND  39 40        GPIO21
    /// --------------------------------
    /// </summary>
    [DebuggerDisplay("GPIO{Bit} P{Header} Pin {Pin}")]
    public class Gpio
    {
        public Gpio(uint bit, uint header, uint pin)
        {
            Bit = bit;
            Header = header;
            Pin = pin;
        }

        public uint Bit { get; private set; }
        public uint Header { get; private set; }
        public uint Pin { get; private set; }

        public bool IsSupported
        {
            get
            {
                if ((Header == 5) && !ModelInfo.HasHeaderP5)
                    return false;
                if ((Header == 0) && !ModelInfo.Name.StartsWith("A"))
                    return false;
                return (Pin <= 26) || ModelInfo.HasHeaderJ8;
            }
        }

        public uint Mask => (uint)(1 << (int)Bit);

        public static Gpio Gpio0, P0Pin2 = new Gpio(0, 0, 2);
        public static Gpio Gpio1, P0Pin3 = new Gpio(1, 0, 3);

        public static Gpio Gpio2, P1Pin3 = new Gpio(2, 1, 3);
        public static Gpio Gpio3, P1Pin5 = new Gpio(3, 1, 5);
        public static Gpio Gpio4, P1Pin7 = new Gpio(4, 1, 7);
        public static Gpio Gpio14, P1Pin8 = new Gpio(14, 1, 8);
        public static Gpio Gpio15, P1Pin10 = new Gpio(15, 1, 10);
        public static Gpio Gpio17, P1Pin11 = new Gpio(17, 1, 11);
        public static Gpio Gpio18, P1Pin12 = new Gpio(18, 1, 12);
        public static Gpio Gpio27, P1Pin13 = new Gpio(27, 1, 13);
        public static Gpio Gpio22, P1Pin15 = new Gpio(22, 1, 15);
        public static Gpio Gpio23, P1Pin16 = new Gpio(23, 1, 16);
        public static Gpio Gpio24, P1Pin18 = new Gpio(24, 1, 18);
        public static Gpio Gpio10, P1Pin19 = new Gpio(10, 1, 19);
        public static Gpio Gpio9, P1Pin21 = new Gpio(9, 1, 21);
        public static Gpio Gpio25, P1Pin22 = new Gpio(25, 1, 22);
        public static Gpio Gpio11, P1Pin23 = new Gpio(11, 1, 23);
        public static Gpio Gpio8, P1Pin24 = new Gpio(8, 1, 24);
        public static Gpio Gpio7, P1Pin26 = new Gpio(7, 1, 26);

        public static Gpio Gpio5, P1Pin29 = new Gpio(5, 1, 29);
        public static Gpio Gpio6, P1Pin31 = new Gpio(6, 1, 31);
        public static Gpio Gpio12, P1Pin32 = new Gpio(12, 1, 32);
        public static Gpio Gpio13, P1Pin33 = new Gpio(13, 1, 33);
        public static Gpio Gpio19, P1Pin35 = new Gpio(19, 1, 35);
        public static Gpio Gpio16, P1Pin36 = new Gpio(16, 1, 36);
        public static Gpio Gpio26, P1Pin37 = new Gpio(26, 1, 37);
        public static Gpio Gpio20, P1Pin38 = new Gpio(20, 1, 38);
        public static Gpio Gpio21, P1Pin40 = new Gpio(21, 1, 40);

        public static Gpio Gpio28, P5Pin3 = new Gpio(28, 5, 3);
        public static Gpio Gpio29, P5Pin4 = new Gpio(29, 5, 4);
        public static Gpio Gpio30, P5Pin5 = new Gpio(30, 5, 5);
        public static Gpio Gpio31, P5Pin6 = new Gpio(31, 5, 6);

    }
}