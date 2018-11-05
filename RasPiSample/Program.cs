using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IctBaden.RasPi.Display;
using IctBaden.RasPi.IO;
using IctBaden.RasPi.Sensor;
using IctBaden.RasPi.System;

namespace RasPiSample
{
    internal class Program
    {
        private static List<string> _devices;
        private static CharacterDisplayI2C _lcd;

        private static void Main()
        {
            Console.WriteLine("Raspi Sample Core " + Assembly.GetEntryAssembly().GetName().Version);
            
            Console.WriteLine("1-wire Temperature Sensors");
            _devices = OneWireTemp.GetDevices();
            Console.WriteLine(_devices.Count + " device(s) found");
            foreach (var device in _devices)
            {
                var temp = OneWireTemp.ReadDeviceTemperature(device);
                Console.WriteLine(device + " = " + temp);
            }

            Console.WriteLine("Digital I/O");

            var inputs = new[] { /* GPIO */ 17, 27, 22, 18 };
            var outputs = new[] { /* GPIO */ 7, 8, 9, 10, 11, 23, 24, 25 };
            var io = new DigitalIo(inputs, outputs);
            if (!io.Initialize())
            {
                Console.WriteLine("Failed to initialize IO");
                return;
            }

            Console.WriteLine("PWM");
            var pwm = new SoftPwm();
            if (!pwm.Initialize())
            {
                Console.WriteLine("Failed to initialize PWM");
                return;
            }

            Console.WriteLine("I2C");
            const string deviceName = "/dev/i2c-1";
            // ReSharper disable once InconsistentlySynchronizedField
            _lcd = new CharacterDisplayI2C();
            lock (_lcd)
            {
                if (!_lcd.Open(deviceName, 0x27))
                {
                    Console.WriteLine("Failed to open I2C");
                    return;
                }

                _lcd.Print($"RasPi {ModelInfo.Revision:X}={ModelInfo.Name}");
                _lcd.SetCursor(1, 2);
                _lcd.Print("äöüßgjpqyÄÖÜ 0°");
            }

            var oldInp = io.GetInputs();

            var toggleBacklight = new Input(Gpio.Gpio17);
            var setOutputs = new Input(Gpio.Gpio27);
            var readTemps = new Input(Gpio.Gpio22);
            var startPwm = new Input(Gpio.Gpio18);

            var out0 = new Output(Gpio.Gpio7);

            var out3 = pwm.OpenChannel(Gpio.Gpio25);

            Help();

            Task.Run(() => UpdateTemp());

            while (true)
            {
                try
                {
                    if (Console.KeyAvailable)
                    {
                        Console.ReadKey();
                        Help();
                    }
                }
                catch
                {
                    // ignore
                }

                var newInp = io.GetInputs();
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
                    // exit if all buttons are pressed
                    break;
                }

                if (toggleBacklight)
                {
                    lock (_lcd)
                    {
                        _lcd.Backlight = !_lcd.Backlight;
                    }
                }
                if (setOutputs)
                {
                    for (var ix = 0; ix < io.OutputsCount; ix++)
                    {
                        Console.WriteLine("Set out {0}", ix);
                        io.SetOutput(ix, true);
                        Thread.Sleep(100);
                    }
                    for (var ix = 0; ix < io.OutputsCount; ix++)
                    {
                        Console.WriteLine("Reset out {0}", ix);
                        io.SetOutput(ix, false);
                        Thread.Sleep(100);
                    }
                }
                if (readTemps)
                {
                    foreach (var device in _devices)
                    {
                        var temp = OneWireTemp.ReadDeviceTemperature(device);
                        Console.WriteLine(device + " = " + temp);
                    }
                }

                if (startPwm)
                {
                    for (var p = 0; p <= 100; p++)
                    {
                        out3.SetPercent(p);
                        Thread.Sleep(50);
                    }
                }

                out0.Set(setOutputs && readTemps);
            }

            lock (_lcd)
            {
                _lcd.Clear();
            }
            Console.WriteLine("done.");
        }

        private static void UpdateTemp()
        {
            var oldTemop = 0.0f;
            while (true)
            {
                lock (_lcd)
                {
                    var newTemp = _devices.Count > 0
                        ? OneWireTemp.ReadDeviceTemperature(_devices[0])
                        : 0.0f;
                    if (Math.Abs(newTemp - oldTemop) >= 0.1f)
                    {
                        oldTemop = newTemp;
                        _lcd.SetCursor(1, 2);
                        _lcd.Print($"Temp = {newTemp:F2}°C      ");
                    }
                }

                Task.Delay(TimeSpan.FromSeconds(2)).Wait();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static void Help()
        {
            Console.WriteLine();
            Console.WriteLine("Button 1:    Toggle backlight");
            Console.WriteLine("Button 2:    Set outputs");
            Console.WriteLine("Button 3:    Read all temperatures");
            Console.WriteLine("Button 2+3:  Set output 0");
            Console.WriteLine("Button 4:    PWM on output 3");
            Console.WriteLine("All buttons: Exit");
            Console.WriteLine();
        }

    }

}
