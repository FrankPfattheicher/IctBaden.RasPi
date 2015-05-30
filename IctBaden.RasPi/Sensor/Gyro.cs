namespace IctBaden.RasPi.Sensor
{
    /// <summary>
    /// MPU 6050
    /// I2C address 0x68
    /// 
    /// Pin 1 - 3.3V connect to VCC
    /// Pin 3 - SDA connect to SDA
    /// Pin 5 - SCL connect to SCL
    /// Pin 6 - Ground connect to GND
    /// </summary>
    public class Gyro
    {
        public struct Vector
        {
            public double X { get; set; }

            public double Y { get; set; }

            public double Z { get; set; }
        }

        // Registers
        // ReSharper disable InconsistentNaming
        private const byte PWR_MGMT_1 = 0x6B;

        // ReSharper restore InconsistentNaming

        // ReSharper disable once InconsistentNaming
        private readonly I2C i2c;

        public Gyro()
        {
            i2c = new I2C();
        }

        public bool Open(string deviceName, int address)
        {
            if (!i2c.Open(deviceName, address))
            {
                return false;
            }

            // reset SLEEP to start measurement
            i2c.WriteRegister(PWR_MGMT_1, 0x00);
            return true;
        }

        private ushort ReadWord(byte address)
        {
            var high = (ushort)i2c.ReadRegister(address);
            var low = (ushort)i2c.ReadRegister((byte)(address + 1));
            return (ushort)((high << 8) + low);
        }

        private double ReadValue(byte address)
        {
            var val = ReadWord(address);
            if (val >= 0x8000)
            {
                return -((0xFFFF - val) + 1) / (double)0x8000;
            }
            return val / (double)0x8000;
        }

        public Vector ReadAccelerometer()
        {
            var result = new Vector
            {
                X = ReadValue(0x3B),
                Y = ReadValue(0x3D),
                Z = ReadValue(0x3F)
            };
            return result;
        }

    }
}