using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace IctBaden.RasPi.Sensor
{
    /// <summary>
    /// Connect devices1
    /// 
    /// P1 PIN 1 (3v3)  ---+----------------+        +---------+
    ///                    |                +----(3)-+ VDD     |
    ///                    +---[4k7]---+             |         |
    /// P1 PIN 7 (GPIO4) --------------+---------(2)-+ DS18S20 |
    ///                                              |         |
    ///                                     +----(1)-+ GND     |
    /// P1 PIN 25 (GND) --------------------+        +---------+
    /// 
    /// Install drivers: /etc/modules
    /// w1-gpio pullup=1
    /// w1-therm
    /// 
    /// Activate pullup: /boot/config.txt
    /// dtoverlay=w1-gpio-pullup
    /// 
    /// Check found devices
    /// cd /sys/bus/w1/devices
    /// ls
    /// 
    /// 28-000005db9cea
    /// 
    /// 28-[di]     - DALLAS DS18B20
    /// 
    /// read device
    /// cat 28-[found device id]/w1_slave
    /// 
    /// 08 03 4b 46 7f ff 08 10 9e : crc=9e YES
    /// 08 03 4b 46 7f ff 08 10 9e t=48500
    /// </summary>
    public static class OneWireTemp
    {
        private const string DevicesDirectory = "/sys/bus/w1/devices";

        public static List<string> GetDevices()
        {
            var devices = new List<string>();
            try
            {
                devices = Directory
                    .EnumerateDirectories(DevicesDirectory, "28-*")
                    .Select(Path.GetFileName)
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return devices;
        }


        /// <summary>
        /// Reads one temperature sensor device.
        /// </summary>
        /// <param name="deviceId">The id of the device to read.</param>
        /// <returns>
        /// Temperature value in °C
        /// or -1000.0 if the given device could not be found
        /// or -1001.0 if the device readout is not valid
        /// </returns>
        public static float ReadDeviceTemperature(string deviceId)
        {
            var deviceFile = Path.Combine(DevicesDirectory, deviceId, "w1_slave");
            if (!File.Exists(deviceFile))
            {
                return -1000f;
            }
            var response = File.ReadAllText(deviceFile);
            var tempPos = response.IndexOf("t=", StringComparison.OrdinalIgnoreCase);

            if ((response.IndexOf("crc=", StringComparison.OrdinalIgnoreCase) == -1)
                || (response.IndexOf("YES", StringComparison.OrdinalIgnoreCase) == -1)
                || (tempPos == -1))
            {
                return -1001f;
            }

            if (!float.TryParse(response.Substring(tempPos + 2), out var temp))
            {
                return -1001f;
            }

            return temp / 1000f;
        }

    }
}
