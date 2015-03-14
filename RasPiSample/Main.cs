using System;
using System.Linq;
using System.Threading;
using IctBaden.RasPi;

namespace RasPiSample
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("1-wire Temperature Sensors");
            var devices = OneWireTemp.GetDevices();
            Console.WriteLine(devices.Count + " device(s) found");
            foreach (var device in devices)
            {
                var temp = OneWireTemp.GetTemperature(device);
                Console.WriteLine(device + " = " + temp);
            }
            
            
            



            var io = new Gpio();
            if (!io.Initialize())
            {
                Console.WriteLine("Failed to initialize IO");
                return;
            }


            const string deviceName = "/dev/i2c-1";
            var lcd = new LcdI2C();
            if (!lcd.Open(deviceName, 0x27))
            {
                Console.WriteLine("Failed to open I2C");
                return;
            }

            Console.WriteLine("I2C");
            lcd.Print("RasPi " + ModelInfo.Name);
            lcd.SetCursor(1, 2);
            lcd.Print("äöüßgjpqyÄÖÜ 0°");

            Thread.Sleep(500);
            lcd.Backlight = false;
            Thread.Sleep(500);
            lcd.Backlight = true;


            Console.WriteLine("Digital I/O");
            Console.WriteLine("Inputs = {0:X8}", io.GetInputs());

            for (var repeat = 1; repeat <= 3; repeat++)
            {
                Console.WriteLine("Cycle {0}", repeat);
                for (var ix = 0; ix < io.Outputs; ix++)
                {
                    Console.WriteLine("Set out {0}", ix);
                    io.SetOutput(ix, true);
                    Thread.Sleep(100);
                }
                for (var ix = 0; ix < io.Outputs; ix++)
                {
                    Console.WriteLine("Reset out {0}", ix);
                    io.SetOutput(ix, false);
                    Thread.Sleep(100);
                }
            }


        }



    }
        
}
