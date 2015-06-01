using IctBaden.RasPi.Interop;

namespace IctBaden.RasPi.Comm
{
    /// <summary>
    /// Class for I2C communication.
    /// Requres libi2c-dev to be installed (sudo apt-get install libi2c-dev).
    /// Check weather /dev/i2c exists.
    /// 
    /// Load drivers adding following lines to /etc/modules
    /// i2c-bmc2708
    /// i2c-dev
    /// 
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
            return (Libc.ioctl_dword(file, Libc.I2C_SLAVE, (ulong)address) >= 0);
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
            return Libc.write(file, buf, 1) == 1;
        }

        /// <summary>
        /// Write buffers bytes to the slave.  
        /// </summary>
        /// <param name="data">buffer with bytes to write</param>
        /// <returns></returns>
        public bool Write(byte[] data)
        {
            return Libc.write(file, data, data.Length) == data.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="register"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteRegister(byte register, byte value)
        {
            //var mem = Marshal.AllocCoTaskMem(1);
            //Marshal.WriteByte(mem, value);
            //var ctrl = new Libc.i2c_smbus_ioctl_data
            //               {
            //                   read_write = Libc.I2C_SMBUS_WRITE,
            //                   command = register,
            //                   size = Libc.I2C_SMBUS_BYTE,
            //                   data = mem
            //               };
            //var ok = (Libc.ioctl_smbus(file, Libc.I2C_SMBUS, ref ctrl) >= 0);
            //Marshal.FreeCoTaskMem(mem);
            //return ok;

            return Libc.write(file, new[] { register, value }, 2) >= 0;
        }

        /// <summary>
        /// Read one byte from the slave.  
        /// </summary>
        /// <returns>Data byte read or 0 if failed.</returns>
        public byte Read()
        {
            byte[] buf = { 0 };
            return (Libc.read(file, buf, 1) != 1) ? (byte)0 : buf[0];
        }

        /// <summary>
        /// Read count bytes from the slave.  
        /// </summary>
        /// <param name="count">byte count</param>
        /// <returns>Buffer with data or empty buffer if failed.</returns>
        public byte[] Read(int count)
        {
            var buf = new byte[count];
            return (Libc.read(file, buf, count) != count) ? new byte[0] : buf;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="register"></param>
        /// <returns></returns>
        public byte ReadRegister(byte register)
        {
            //var mem = Marshal.AllocCoTaskMem(2);
            //Marshal.WriteByte(mem, 0, register);
            //Marshal.WriteByte(mem, 1, 0x55);
            //var ctrl = new Libc.i2c_smbus_ioctl_data
            //               {
            //                 read_write = Libc.I2C_SMBUS_READ,
            //                 command = register,
            //                 size = Libc.I2C_SMBUS_BYTE,
            //                 data = mem
            //               };

            //byte value = 0;
            //var res = Libc.ioctl_smbus(file, Libc.I2C_SMBUS, ref ctrl);
            //Console.WriteLine("file=" + file);
            //Console.WriteLine("res=" + res);
            //Console.WriteLine("errno=" + Marshal.GetLastWin32Error());
            //if (res >= 0)
            //{
            //    value = Marshal.ReadByte(mem);
            //}
            //Marshal.FreeCoTaskMem(mem);

            Libc.write(file, new[] {register}, 1);
            var buffer = new byte[1];
            Libc.read(file, buffer, 1);
            return buffer[0];
        }
    }
}

