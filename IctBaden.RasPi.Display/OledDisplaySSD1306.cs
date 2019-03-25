using System;
using IctBaden.RasPi.Comm;

// ReSharper disable InconsistentNaming

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace IctBaden.RasPi.Display
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// This is a port of ThingPulse ESP8266 OLED SSD1306
    /// https://github.com/ThingPulse/esp8266-oled-ssd1306
    /// </summary>
    public class OledDisplaySSD1306
    {
        // SSD1306 commands
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Local
        // ReSharper disable IdentifierTypo
        private const byte SETCONTRAST = 0x81;
        private const byte DISPLAYALLON_RESUME = 0xA4;
        private const byte DISPLAYALLON = 0xA5;
        private const byte NORMALDISPLAY = 0xA6;
        private const byte INVERTDISPLAY = 0xA7;
        private const byte DISPLAYOFF = 0xAE;
        private const byte DISPLAYON = 0xAF;
        private const byte SETDISPLAYOFFSET = 0xD3;
        private const byte SETCOMPINS = 0xDA;
        private const byte SETVCOMDETECT = 0xDB;
        private const byte SETDISPLAYCLOCKDIV = 0xD5;
        private const byte SETPRECHARGE = 0xD9;
        private const byte SETMULTIPLEX = 0xA8;
        private const byte SETLOWCOLUMN = 0x00;
        private const byte SETHIGHCOLUMN = 0x10;
        private const byte SETSTARTLINE = 0x40;
        private const byte MEMORYMODE = 0x20;
        private const byte COLUMNADDR = 0x21;
        private const byte PAGEADDR = 0x22;
        private const byte COMSCANINC = 0xC0;
        private const byte COMSCANDEC = 0xC8;
        private const byte SEGREMAPOFF = 0xA0;
        private const byte SEGREMAPON = 0xA1;
        private const byte CHARGEPUMP = 0x8D;
        private const byte EXTERNALVCC = 0x01;
        private const byte SWITCHCAPVCC = 0x02;

        // Scrolling constants
        private const byte ACTIVATE_SCROLL = 0x2F;
        private const byte DEACTIVATE_SCROLL = 0x2E;
        private const byte SET_VERTICAL_SCROLL_AREA = 0xA3;
        private const byte RIGHT_HORIZONTAL_SCROLL = 0x26;
        private const byte LEFT_HORIZONTAL_SCROLL = 0x27;
        private const byte VERTICAL_AND_RIGHT_HORIZONTAL_SCROLL = 0x29;

        private const byte VERTICAL_AND_LEFT_HORIZONTAL_SCROLL = 0x2A;
        // ReSharper restore IdentifierTypo
        // ReSharper restore UnusedMember.Local
        // ReSharper restore InconsistentNaming


        /// <summary>
        /// Display width in pixel
        /// </summary>
        public byte Width { get; private set; }

        /// <summary>
        /// Display height in pixel
        /// </summary>
        public byte Height { get; private set; }


        private readonly I2C _device;
        private readonly byte _offset;
        private readonly byte _pages;
        private readonly byte[] _buffer;

        private OledColor _color;
        private byte[] _font = OledFonts.ArialPlain10;

        /// <summary>
        /// Creates a display interface
        /// </summary>
        /// <param name="device">Reference to I2C device to use</param>
        /// <param name="profile">The profile of the attached display</param>
        public OledDisplaySSD1306(I2C device, OledProfile profile)
        {
            _device = device;
            _offset = (byte) profile.Offset;
            Width = (byte) profile.Width;
            Height = (byte) profile.Height;
            _pages = (byte) (Height / 8);
            _color = OledColor.White;
            _buffer = new byte[Width * _pages + 1];
            _buffer[0] = 0x40; // data bytes
        }

        /// <summary>
        /// Clears the entire display
        /// </summary>
        public void Clear()
        {
            for (var ix = 1; ix < _buffer.Length; ix++)
            {
                _buffer[ix] = 0;
            }

            UpdateDisplay();
        }

        /// <summary>
        /// Sets contrast of display pixels
        /// </summary>
        /// <param name="percent">0..100 percent value</param>
        public void SetContrast(uint percent)
        {
            var value = 0xFF * percent / 100;
            Cmd(SETCONTRAST, (byte) value); // set contrast control register)
        }

        /// <summary>
        /// Set one display pixel with the given color.
        /// </summary>
        /// <param name="x">Horizontal position</param>
        /// <param name="y">Vertical position</param>
        public void SetPixel(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;

            switch (_color)
            {
                case OledColor.White:
                    _buffer[x + (y / 8) * Width] |= (byte) (1 << (byte) (y & 7));
                    break;
                case OledColor.Black:
                    _buffer[x + (y / 8) * Width] &= (byte) ~(1 << (byte) (y & 7));
                    break;
                case OledColor.Inverse:
                    _buffer[x + (y / 8) * Width] ^= (byte) (1 << (byte) (y & 7));
                    break;
            }
        }

        private static void Swap(ref int a, ref int b)
        {
            var temp = a;
            a = b;
            b = temp;
        }

        /// <summary>
        /// Set the color fot the next pixel and line operations
        /// </summary>
        /// <param name="color"></param>
        public void SetColor(OledColor color)
        {
            _color = color;
        }

        /// <summary>
        /// Select the font for DrawText operations
        /// </summary>
        /// <param name="font"></param>
        public void SetFont(byte[] font)
        {
            _font = font;
        }

        /// <summary>
        /// Draw line with given color.
        /// Attention! Not including endpoint.
        /// Bresenham's algorithm - thx wikipedia and Adafruit_GFX
        /// </summary>
        /// <param name="x0">Horizontal start position 1..width-1</param>
        /// <param name="y0">Horizontal end position 1..width-1</param>
        /// <param name="x1">Vertical start position 1..height-1</param>
        /// <param name="y1">Vertical end position 1..height-1</param>
        public void DrawLine(int x0, int y0, int x1, int y1)
        {
            var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }

            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            var dx = x1 - x0;
            var dy = Math.Abs(y1 - y0);

            var err = dx / 2;
            var yStep = (y0 < y1) ? 1 : -1;

            for (; x0 <= x1; x0++)
            {
                if (steep)
                {
                    SetPixel(y0, x0);
                }
                else
                {
                    SetPixel(x0, y0);
                }

                err -= dy;
                if (err < 0)
                {
                    y0 += yStep;
                    err += dx;
                }
            }
        }

        public void DrawRect(int x, int y, int width, int height)
        {
            var left = x;
            var right = x + width - 1;
            var top = y;
            var bottom = y + height - 1;
            DrawLine(left, top, right, top);
            DrawLine(left, top, left, bottom);
            DrawLine(right, top, right, bottom);
            DrawLine(left, bottom, right, bottom);
        }

        public void FillRect(int x, int y, int width, int height)
        {
            var left = x;
            var right = x + width - 1;
            for (var top = y; top <= y + height - 1; top++)
            {
                DrawLine(left, top, right, top);
            }
        }

        public void DrawBuffer(byte[] buffer)
        {
            var len = Math.Min(_buffer.Length - 1, buffer.Length);
            Console.WriteLine($"Array.Copy(length {len})");
            Array.Copy(buffer, 0, _buffer, 1, len);

            UpdateDisplay();
        }

        /// <summary>
        /// Updates physical display from drawing buffer.
        /// </summary>
        public void UpdateDisplay()
        {
            Cmd(COLUMNADDR,
                _offset, // column start address (0 = reset)
                (byte) (_offset + Width - 1)); // column end address
            Cmd(PAGEADDR,
                0, // page start address (0 = reset)
                (byte) (_pages - 1)); // page end address

            _device.Write(_buffer);
        }

        /// <summary>
        /// Initializes display hardware.
        /// </summary>
        public void Initialize()
        {
            Cmd(DISPLAYOFF); // turn off oled panel
            Cmd(SETDISPLAYCLOCKDIV, 0x80); // set display clock divide ratio/oscillator frequency

            Cmd(SETMULTIPLEX, (byte) (Height - 1)); // set multiplex ratio(1 to 64)

            Cmd(SETDISPLAYOFFSET, 0x00); // set display offset, no offset
            Cmd(SETSTARTLINE); // set start line address
            Cmd(CHARGEPUMP, 0x14); // set Charge Pump enable/disable

            Cmd(MEMORYMODE, 0x00); // 0x00 act like ks0108
            Cmd(SEGREMAPON); // segment remap
            Cmd(COMSCANDEC); // mirror the screen
            Cmd(SETCOMPINS, 0x12); // set com pins hardware configuration        

            Cmd(SETCONTRAST, 0xCF); // set contrast control register

            Cmd(SETPRECHARGE, 0xF1); // set pre-charge period

            Cmd(SETVCOMDETECT, 0x40); // set Vcomh

            Cmd(DISPLAYALLON_RESUME);
            Cmd(NORMALDISPLAY); // set normal display

            Cmd(DISPLAYON); // turn on oled panel
        }

        private void Cmd(byte cmd)
        {
            _device.Write(new byte[] {0x00, cmd});
        }

        private void Cmd(byte cmd, byte param)
        {
            _device.Write(new byte[] {0x00, cmd, param});
        }

        private void Cmd(byte cmd, byte param1, byte param2)
        {
            _device.Write(new byte[] {0x00, cmd, param1, param2});
        }


        private void DrawInternal(int xMove, int yMove, int width, int height, byte[] data, uint offset, uint bytesInData)
        {
            if (width < 0 || height < 0) return;
            if (yMove + height < 0 || yMove > Height) return;
            if (xMove + width < 0 || xMove > Width) return;

            var rasterHeight = 1 + ((height - 1) >> 3); // fast ceil(height / 8.0)
            var yOffset = yMove & 7;

            bytesInData = bytesInData == 0 ? (uint) (width * rasterHeight) : bytesInData;

            var initYMove = yMove;
            var initYOffset = yOffset;


            for (uint i = 0; i < bytesInData; i++)
            {
                // Reset if next horizontal drawing phase is started.
                if (i % rasterHeight == 0)
                {
                    yMove = initYMove;
                    yOffset = initYOffset;
                }

                var currentByte = data[offset + i];
                var displayBufferSize = (_buffer.Length - 1);
                var xPos = (int) (xMove + (i / rasterHeight));
                var yPos = (int) (((yMove >> 3) + (i % rasterHeight)) * Width);

                // int yScreenPos = yMove + yOffset;
                var dataPos = xPos + yPos;

                if (dataPos >= 0 && dataPos < displayBufferSize && xPos >= 0 && xPos < Width)
                {
                    if (yOffset >= 0)
                    {
                        switch (_color)
                        {
                            case OledColor.White:
                                _buffer[dataPos] |= (byte) (currentByte << yOffset);
                                break;
                            case OledColor.Black:
                                _buffer[dataPos] &= (byte) (~(currentByte << yOffset));
                                break;
                            case OledColor.Inverse:
                                _buffer[dataPos] ^= (byte) (currentByte << yOffset);
                                break;
                        }

                        if (dataPos < (displayBufferSize - Width))
                        {
                            switch (_color)
                            {
                                case OledColor.White:
                                    _buffer[dataPos + Width] |= (byte) (currentByte >> (8 - yOffset));
                                    break;
                                case OledColor.Black:
                                    _buffer[dataPos + Width] &= (byte) (~(currentByte >> (8 - yOffset)));
                                    break;
                                case OledColor.Inverse:
                                    _buffer[dataPos + Width] ^= (byte) (currentByte >> (8 - yOffset));
                                    break;
                            }
                        }
                    }
                    else
                    {
                        // Make new offset position
                        yOffset = -yOffset;

                        switch (_color)
                        {
                            case OledColor.White:
                                _buffer[dataPos] |= (byte) (currentByte >> yOffset);
                                break;
                            case OledColor.Black:
                                _buffer[dataPos] &= (byte) (~(currentByte >> yOffset));
                                break;
                            case OledColor.Inverse:
                                _buffer[dataPos] ^= (byte) (currentByte >> yOffset);
                                break;
                        }

                        // Prepare for next iteration by moving one block up
                        yMove -= 8;

                        // and setting the new yOffset
                        yOffset = 8 - yOffset;
                    }
                }
            }
        }

        /// <summary>
        /// Draw a text on the display using current selected font.
        /// Text is not wrapped automatically but line breaks are respected.
        /// </summary>
        /// <param name="xPos">Left position of text</param>
        /// <param name="yPos">Top position of text</param>
        /// <param name="text">Text to draw</param>
        public void DrawString(int xPos, int yPos, string text)
        {
            var lineHeight = _font[OledFonts.HEIGHT_POS];
            var textLines = text
                .Replace("\r", "")
                .Split('\n');
            for(var line = 0; line < textLines.Length; line++)
            {
                var textPart = textLines[line];
                var length = textPart.Length;
                DrawStringInternal(xPos, yPos + line * lineHeight, textPart, length, GetStringWidth(textPart));
            }
        }

        /// <summary>
        /// Returns the width of the given text using current font.
        /// </summary>
        /// <param name="text">Text to measure</param>
        /// <returns>Width in pixel</returns>
        public uint GetStringWidth(string text)
        {
            uint firstChar = _font[OledFonts.FIRST_CHAR_POS];
            uint stringWidth = 0;
            uint maxWidth = 0;
            var length = text.Length;
            while (length-- > 0)
            {
                var width = OledFonts.JUMPTABLE_START 
                            + (text[length] - firstChar) * OledFonts.JUMPTABLE_BYTES 
                            + OledFonts.JUMPTABLE_WIDTH;
                
                stringWidth += _font[width];
                if (text[length] == 10)
                {
                    maxWidth = Math.Max(maxWidth, stringWidth);
                    stringWidth = 0;
                }
            }

            return Math.Max(maxWidth, stringWidth);
        }

        /// <summary>
        /// Gets the height of a text line using the current font.
        /// </summary>
        /// <returns>Height of text line in pixel</returns>
        public uint GetTextHeight()
        {
            return _font[OledFonts.HEIGHT_POS];
        }


        private void DrawStringInternal(int xPos, int yPos, string text, int textLength, uint textWidth)
        {
            uint textHeight = _font[OledFonts.HEIGHT_POS];
            uint firstChar = _font[OledFonts.FIRST_CHAR_POS];
            var sizeOfJumpTable = (uint) (_font[OledFonts.CHAR_NUM_POS] * OledFonts.JUMPTABLE_BYTES);

            // Don't draw anything if it is not on the screen.
            if (xPos + textWidth < 0 || xPos > Width)
            {
                return;
            }

            if (yPos + textHeight < 0 || yPos > Width)
            {
                return;
            }

            for (var j = 0; j < textLength; j++)
            {
                var code = (byte) text[j];
                if (code < firstChar) continue;
                
                var charCode = (byte) (code - firstChar);

                // 4 Bytes per char code
                var msbJumpToChar =
                    _font[OledFonts.JUMPTABLE_START + charCode * OledFonts.JUMPTABLE_BYTES]; // MSB  \ JumpAddress
                var lsbJumpToChar =
                    _font[
                        OledFonts.JUMPTABLE_START + charCode * OledFonts.JUMPTABLE_BYTES +
                        OledFonts.JUMPTABLE_LSB]; // LSB /
                var charByteSize =
                    _font[
                        OledFonts.JUMPTABLE_START + charCode * OledFonts.JUMPTABLE_BYTES +
                        OledFonts.JUMPTABLE_SIZE]; // Size
                var currentCharWidth =
                    _font[
                        OledFonts.JUMPTABLE_START + charCode * OledFonts.JUMPTABLE_BYTES +
                        OledFonts.JUMPTABLE_WIDTH]; // Width

                // Test if the char is drawable
                if (!(msbJumpToChar == 255 && lsbJumpToChar == 255))
                {
                    // Get the position of the char data
                    var charDataPosition = (uint) (OledFonts.JUMPTABLE_START + sizeOfJumpTable +
                                                    ((msbJumpToChar << 8) + lsbJumpToChar));
                    DrawInternal(xPos, yPos, currentCharWidth, (int) textHeight, _font, charDataPosition,
                        charByteSize);
                }

                xPos += currentCharWidth;
            }
        }
        
    }
}
