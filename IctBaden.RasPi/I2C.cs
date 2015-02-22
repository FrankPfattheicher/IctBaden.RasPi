namespace IctBaden.RasPi
{
    /// <summary>
    /// Class for I2C communication.
    /// Requres libi2c-dev to be installed (sudo apt-get install libi2c-dev).
    /// Check weather /dev/i2c exists.
    /// </summary>
    public class I2C
    {
        private int file = -1;

        public bool Open(string deviceName, int address)
        {
            // Open up the I2C bus  
            file = Libc.open(deviceName, Libc.O_RDWR);
            if (file == -1)
            {
                return false;
            }

            // Specify the address of the slave device.  
            if (Libc.ioctl_dword(file, Libc.I2C_SLAVE, (ulong)address) < 0)
            {
                return false;
            }

            return true;
        }

        public void Close()
        {
            if (file == -1)
                return;

            Libc.close(file);
            file = -1;
        }

        /// <summary>
        /// Write one byte to the slave.  
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Write(byte data)
        {
            byte[] buf = { data };
            if (Libc.write(file, buf, 1) != 1)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Write buffers bytes to the slave.  
        /// </summary>
        /// <param name="data">buffer with bytes to write</param>
        /// <returns></returns>
        public bool Write(byte[] data)
        {
            if (Libc.write(file, data, data.Length) != data.Length)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read one byte from the slave.  
        /// </summary>
        /// <returns>Data byte raed or 0 if failed.</returns>
        public byte Read()
        {
            byte[] buf = { 0 };
            if (Libc.read(file, buf, 1) != 1)
            {
                return 0;
            }
            return buf[0];
        }

        /// <summary>
        /// Read count bytes from the slave.  
        /// </summary>
        /// <param name="count">byte count</param>
        /// <returns>Buffer with data or empty buffer if failed.</returns>
        public byte[] Read(int count)
        {
            var buf = new byte[count];
            if (Libc.read(file, buf, count) != count)
            {
                return new byte[0];
            }
            return buf;
        }
    }
}

