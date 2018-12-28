using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using IctBaden.RasPi.Display;
using IctBaden.RasPi.IO;
using IctBaden.RasPi.Sensor;
using IctBaden.RasPi.System;

namespace RasPiSample
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Raspi Sample .NET Framework / Mono " + Assembly.GetEntryAssembly().GetName().Version);

            Console.WriteLine("1-wire Temperature Sensors");
            var devices = OneWireTemp.GetDevices();
            Console.WriteLine(devices.Count + " device(s) found");
            foreach (var device in devices)
            {
                var temp = OneWireTemp.ReadDeviceTemperature(device);
                Console.WriteLine(device + " = " + temp);
            }
            

            var io = new DigitalIo();
            if (!io.Initialize())
            {
                Console.WriteLine("Failed to initialize IO");
                return;
            }

            var in0 = io.CreateInput(Gpio.Gpio17);
            var in1 = io.CreateInput(Gpio.Gpio27);

            var out0 = io.CreateOutput(Gpio.Gpio7);
            var out1 = io.CreateOutput(Gpio.Gpio8);
            var out2 = io.CreateOutput(Gpio.Gpio9);
            var out3 = io.CreateOutput(Gpio.Gpio10);
            var out4 = io.CreateOutput(Gpio.Gpio11);
            var out5 = io.CreateOutput(Gpio.Gpio23);
            var out6 = io.CreateOutput(Gpio.Gpio24);
            var out7 = io.CreateOutput(Gpio.Gpio25);
            var outputs = new List<Output> { out0, out1, out2, out3, out4, out5, out6, out7 };

            const string deviceName = "/dev/i2c-1";
            var lcd = new CharacterDisplayI2C();
            if (!lcd.Open(deviceName, 0x27))
            {
                Console.WriteLine("Failed to open I2C");
                return;
            }

            Console.WriteLine("I2C");
            lcd.Print("RasPi " + ModelInfo.Name);
            lcd.SetCursor(1, 2);
            lcd.Print("äöüßgjpqyÄÖÜ 0°");

            var oldInp = io.GetAllInputs();
            while (true)
            {
                var newInp = io.GetAllInputs();

                if (newInp == oldInp)
                {
                    Thread.Sleep(50);   
                    continue;
                }

                Console.WriteLine("Digital I/O");
                Console.WriteLine("Inputs = {0:X8}", newInp);

                oldInp = newInp;
                if ((newInp & 0x0F) == 0x0F)
                {
                    break;
                }

                if (in0)
                {
                    lcd.Backlight = !lcd.Backlight;
                }
                if (in1)
                {
                    for (var ix = 0; ix < outputs.Count; ix++)
                    {
                        Console.WriteLine("Set out {0}", ix);
                        outputs[ix].Set(true);
                        Thread.Sleep(100);
                    }
                    for (var ix = 0; ix < outputs.Count; ix++)
                    {
                        Console.WriteLine("Reset out {0}", ix);
                        outputs[ix].Set(false);
                        Thread.Sleep(100);
                    }
                }

            }

        }



    }
        
}
