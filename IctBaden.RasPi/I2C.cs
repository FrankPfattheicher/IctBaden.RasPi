namespace IctBaden.RasPi
{
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
            if(file == -1)
                return;

            Libc.close(file);
            file = -1;
        }

        public bool Write(byte data)
        {
            // Write a byte to the slave.  
            byte[] buf = { data };  
            if (Libc.write(file, buf, 1) != 1)
            {  
                return false;
            }  
            return true;
        }

        public bool Write(byte[] data)
        {
            // Write a byte to the slave.  
            if (Libc.write(file, data, data.Length) != data.Length)
            {  
                return false;
            }  
            return true;
        }

        public int Read()
        {
            byte[] buf = { 0 };  
            // Read a byte from the slave.  
            if (Libc.read(file, buf, 1) != 1)  
            {  
                return 0;
            }  
            return buf[0];
        }
    }
}

