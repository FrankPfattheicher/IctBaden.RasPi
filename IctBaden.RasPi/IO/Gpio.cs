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
        private Gpio(uint bit, uint header, uint pin)
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

        public static Gpio Gpio0 = new Gpio(0, 0, 2);
        public static Gpio P0Pin2 = Gpio0;
        public static Gpio Gpio1 = new Gpio(1, 0, 3);
        public static Gpio P0Pin3 = Gpio1;

        public static Gpio Gpio2 = new Gpio(2, 1, 3);
        public static Gpio P1Pin3 = Gpio2;
        public static Gpio Gpio3 = new Gpio(3, 1, 5);
        public static Gpio P1Pin5 = Gpio3;
        public static Gpio Gpio4 = new Gpio(4, 1, 7);
        public static Gpio P1Pin7 = Gpio4;
        public static Gpio Gpio14 = new Gpio(14, 1, 8);
        public static Gpio P1Pin8 = Gpio14;
        public static Gpio Gpio15 = new Gpio(15, 1, 10);
        public static Gpio P1Pin10 = Gpio15;
        public static Gpio Gpio17 = new Gpio(17, 1, 11);
        public static Gpio P1Pin11 = Gpio17;
        public static Gpio Gpio18 = new Gpio(18, 1, 12);
        public static Gpio P1Pin12 = Gpio18;
        public static Gpio Gpio27 = new Gpio(27, 1, 13);
        public static Gpio P1Pin13 = Gpio27;
        public static Gpio Gpio22 = new Gpio(22, 1, 15);
        public static Gpio P1Pin15 = Gpio22;
        public static Gpio Gpio23 = new Gpio(23, 1, 16);
        public static Gpio P1Pin16 = Gpio23;
        public static Gpio Gpio24 = new Gpio(24, 1, 18);
        public static Gpio P1Pin18 = Gpio24;
        public static Gpio Gpio10 = new Gpio(10, 1, 19);
        public static Gpio P1Pin19 = Gpio10;
        public static Gpio Gpio9 = new Gpio(9, 1, 21);
        public static Gpio P1Pin21 = Gpio9;
        public static Gpio Gpio25 = new Gpio(25, 1, 22);
        public static Gpio P1Pin22 = Gpio25;
        public static Gpio Gpio11 = new Gpio(11, 1, 23);
        public static Gpio P1Pin23 = Gpio11;
        public static Gpio Gpio8 = new Gpio(8, 1, 24);
        public static Gpio P1Pin24 = Gpio8;
        public static Gpio Gpio7 = new Gpio(7, 1, 26);
        public static Gpio P1Pin26 = Gpio7;

        public static Gpio Gpio5 = new Gpio(5, 1, 29);
        public static Gpio P1Pin29 = Gpio5;
        public static Gpio Gpio6 = new Gpio(6, 1, 31);
        public static Gpio P1Pin31 = Gpio6;
        public static Gpio Gpio12 = new Gpio(12, 1, 32);
        public static Gpio P1Pin32 = Gpio12;
        public static Gpio Gpio13 = new Gpio(13, 1, 33);
        public static Gpio P1Pin33 = Gpio13;
        public static Gpio Gpio19 = new Gpio(19, 1, 35);
        public static Gpio P1Pin35 = Gpio19;
        public static Gpio Gpio16 = new Gpio(16, 1, 36);
        public static Gpio P1Pin36 = Gpio16;
        public static Gpio Gpio26 = new Gpio(26, 1, 37);
        public static Gpio P1Pin37 = Gpio26;
        public static Gpio Gpio20 = new Gpio(20, 1, 38);
        public static Gpio P1Pin38 = Gpio20;
        public static Gpio Gpio21 = new Gpio(21, 1, 40);
        public static Gpio P1Pin40 = Gpio21;

        public static Gpio Gpio28 = new Gpio(28, 5, 3);
        public static Gpio P5Pin3 = Gpio28;
        public static Gpio Gpio29 = new Gpio(29, 5, 4);
        public static Gpio P5Pin4 = Gpio29;
        public static Gpio Gpio30 = new Gpio(30, 5, 5);
        public static Gpio P5Pin5 = Gpio30;
        public static Gpio Gpio31 = new Gpio(31, 5, 6);
        public static Gpio P5Pin6 = Gpio31;

        public static Gpio FromGpio(uint gpio)
        {
            // TODO: seek...
            return new Gpio(gpio, 0, 0);
        }
    }
}