using System;
using System.Threading;

namespace IctBaden.RasPi
{
    public class LcdI2C : ILcd
    {
        // SainSmart IIC LCD1602 Module Display
        // I2C address 0x27
        //
        // signal assignment 
        //        LCD - CHIP
        //         RS - P0
        //         RW - P1
        //         EN - P2
        //         BL - P3
        //         D4 - P4
        //         D5 - P5
        //         D6 - P6
        //         D7 - P7

        private const byte LcdRs = 0x01;
        private const byte LcdRw = 0x02;
        private const byte LcdEn = 0x04;
        private const byte LcdBl = 0x08;

        bool backlight;
        public bool Backlight
        {
            get
            {
                return backlight;
            }
            set
            {
                backlight = value;
                WriteNibble(0);
            }
        }
        public int Lines { get { return 2; } }
        public int Columns { get { return 16; } }

        private I2C i2c;

        public LcdI2C()
        {
            backlight = true;
            i2c = new I2C();
        }

        public bool Open(string deviceName, int address)
        {
            if (!i2c.Open(deviceName, address))
            {
                return false;
            }

            Initialize();
            return true;
        }

        private void Initialize()
        {
            WriteNibble(0x00);
            Thread.Sleep(20);
            WriteNibble(0x30);    // Interface auf 4-Bit setzen
            Thread.Sleep(5);
            WriteNibble(0x30);    // Interface auf 4-Bit setzen
            Thread.Sleep(5);
            WriteNibble(0x30);    // Interface auf 4-Bit setzen
            Thread.Sleep(1);
            WriteNibble(0x20);
            Thread.Sleep(1);
            
            WriteCmd(0x28);    // 2-zeilig, 5x8-Punkt-Matrix
            WriteCmd(0x06);    // Kursor nach rechts wandernd, kein Display shift
            WriteCmd(0x14);    // Cursor/Display-Shift
            Clear();
            WriteCmd(0x0C);    // Display ein

            // charcter set
            // Ä
            WriteCmd(0x40 + 0);
            WriteData(0x15);
            WriteData(0x0A);
            WriteData(0x11);
            WriteData(0x11);
            WriteData(0x1F);
            WriteData(0x11);
            WriteData(0x11);
            WriteData(0x00);
            // Ö
            WriteData(0x11);
            WriteData(0x0E);
            WriteData(0x11);
            WriteData(0x11);
            WriteData(0x11);
            WriteData(0x11);
            WriteData(0x0E);
            WriteData(0x00);
            // Ü
            WriteData(0x11);
            WriteData(0x00);
            WriteData(0x11);
            WriteData(0x11);
            WriteData(0x11);
            WriteData(0x11);
            WriteData(0x0E);
            WriteData(0x00);
            // ß
            WriteData(0x0E);
            WriteData(0x11);
            WriteData(0x1E);
            WriteData(0x11);
            WriteData(0x11);
            WriteData(0x1E);
            WriteData(0x10);
            WriteData(0x00);

            WriteCmd(0x80);
        }

        public void Clear()
        {
            WriteCmd(0x01);    // Display löschen 
            Thread.Sleep(20);
        }

        public void SetCursor(int col, int row)
        {
            if((col < 1) || (col > Columns))
                throw new ArgumentException("invalid position", "col");
            if((row < 1) || (row > Lines))
                throw new ArgumentException("invalid position", "row");

            int[] row_offset = { 0x00, 0x40, 0x14, 0x54 };
            int addr = col - 1 + row_offset[row - 1];
            WriteCmd((byte)(0x80 | addr));
        }

        public void Print(string text)
        {

            foreach (var txch in text)
            {
                byte ch = (byte)txch;
                switch(txch)
                {
                    case 'ä': ch = 0xE1; break;
                    case 'ö': ch = 0xEF; break;
                    case 'ü': ch = 0xF5; break;
                    case 'ß': ch = 0x03; break;
                    case 'Ä': ch = 0x00; break;
                    case 'Ö': ch = 0x01; break;
                    case 'Ü': ch = 0x02; break;
                    case '°': ch = 0xDF; break;
                }
                WriteData(ch);
            }
        }

        private void WriteCmd(byte data)
        {
            WriteNibble((byte)(data & 0xf0));
            WriteNibble((byte)((data << 4) & 0xf0));
        }
        private void WriteData(byte data)
        {
            WriteNibble((byte)((data & 0xf0) | LcdRs));
            WriteNibble((byte)(((data << 4) & 0xf0) | LcdRs));
        }
        private void WriteNibble(byte nibble)
        {
            if(backlight)
                nibble |= LcdBl;
            i2c.Write((byte)nibble);
            i2c.Write((byte)(nibble | LcdEn));
            i2c.Write((byte)nibble);
        }

    }
}

