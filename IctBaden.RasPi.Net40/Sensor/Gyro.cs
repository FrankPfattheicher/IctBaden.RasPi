using IctBaden.RasPi.Comm;

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
        // ReSharper disable UnusedMember.Local
        private const byte INT_STATUS = 0x3A;
        private const byte ACCEL_XOUT_H = 0x3B;
        private const byte ACCEL_XOUT_L = 0x3C;
        private const byte ACCEL_YOUT_H = 0x3D;
        private const byte ACCEL_YOUT_L = 0x3E;
        private const byte ACCEL_ZOUT_H = 0x3F;
        private const byte ACCEL_ZOUT_L = 0x40;
        private const byte TEMP_OUT_H = 0x41;
        private const byte TEMP_OUT_L = 0x42;
        private const byte GYRO_XOUT_H = 0x43;
        private const byte GYRO_XOUT_L = 0x44;
        private const byte GYRO_YOUT_H = 0x45;
        private const byte GYRO_YOUT_L = 0x46;
        private const byte GYRO_ZOUT_H = 0x47;
        private const byte GYRO_ZOUT_L = 0x48;
        private const byte PWR_MGMT_1 = 0x6B;
        private const byte WHO_AM_I = 0x75;
        // ReSharper restore UnusedMember.Local
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
            return i2c.ReadRegister(WHO_AM_I) == address;
        }

        private ushort ReadWord(byte address)
        {
            var high = (ushort)i2c.ReadRegister(address);
            var low = (ushort)i2c.ReadRegister((byte)(address + 1));
            return (ushort)((high << 8) + low);
        }

        private double ReadValue(byte address)
        {
            var val = (long)ReadWord(address);
            if (val >= 0x8000)
            {
                val = -((0xFFFF - val) + 1);
            }
            return val / (double)0x4000;
        }

        public Vector ReadAccelerometer()
        {
            var result = new Vector
            {
                X = ReadValue(ACCEL_XOUT_H),
                Y = ReadValue(ACCEL_YOUT_H),
                Z = ReadValue(ACCEL_ZOUT_H)
            };
            return result;
        }

        public Vector ReadGyrometer()
        {
            var result = new Vector
            {
                X = ReadValue(GYRO_XOUT_H),
                Y = ReadValue(GYRO_YOUT_H),
                Z = ReadValue(GYRO_ZOUT_H)
            };
            return result;
        }

        public double ReadTemperature()
        {
            var val = (long)ReadWord(TEMP_OUT_H);
            if (val >= 0x8000)
            {
                val = -((0xFFFF - val) + 1);
            }
            return val / 340.0 + 36.53;
        }

    }
}