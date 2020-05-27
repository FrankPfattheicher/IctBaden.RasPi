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
    internal static class Program
    {
        private static List<string> _devices;
        private static CharacterDisplayI2C _lcd;

        private static void Main()
        {
            Console.WriteLine("Raspi Sample Core " + Assembly.GetAssembly(typeof(Program))!.GetName().Version);
            
            Console.WriteLine("1-wire Temperature Sensors");
            _devices = OneWireTemp.GetDevices();
            Console.WriteLine(_devices.Count + " device(s) found");
            foreach (var device in _devices)
            {
                var temp = OneWireTemp.ReadDeviceTemperature(device);
                Console.WriteLine(device + " = " + temp);
            }

            Console.WriteLine("Digital I/O");

            var io = new DigitalIo();
            if (!io.Initialize())
            {
                Console.WriteLine("Failed to initialize IO");
                return;
            }

            var in0 = io.CreateInput(Gpio.Gpio17);
            var in1 = io.CreateInput(Gpio.Gpio27);
            var in2 = io.CreateInput(Gpio.Gpio22);
            var in3 = io.CreateInput(Gpio.Gpio18);

            var out0 = io.CreateOutput(Gpio.Gpio7);
            var out1 = io.CreateOutput(Gpio.Gpio8);
            var out2 = io.CreateOutput(Gpio.Gpio9);
            var out3 = io.CreateOutput(Gpio.Gpio10);
            var out4 = io.CreateOutput(Gpio.Gpio11);
            var out5 = io.CreateOutput(Gpio.Gpio23);
            var out6 = io.CreateOutput(Gpio.Gpio24);
            var out7 = io.CreateOutput(Gpio.Gpio25);
            var outputs = new List<Output> { out0, out1, out2, out3, out4, out5, out6, out7 };

            Console.WriteLine("PWM");
            var pwm = new SoftPwm();
            if (!pwm.Initialize())
            {
                Console.WriteLine("Failed to initialize PWM");
                return;
            }

            var pwm0 = pwm.OpenChannel(Gpio.Gpio25);

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

                _lcd.Print($"RasPi {ModelInfo.RevisionCode:X}={ModelInfo.Name}");
                _lcd.SetCursor(1, 2);
                // ReSharper disable once StringLiteralTypo
                _lcd.Print("äöüßgjpqyÄÖÜ 0°");
            }

            var oldInp = io.GetAllInputs();

            Help();

            Task.Run(UpdateTemp);

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

                var newInp = io.GetAllInputs();
                if (newInp == oldInp)
                {
                    Thread.Sleep(50);
                    continue;
                }

                Console.WriteLine("Digital I/O");
                Console.WriteLine("Inputs = {0:X8}", newInp);

                oldInp = newInp;
                if (in0 && in1 && in2 && in3)
                {
                    // exit if all buttons are pressed
                    break;
                }

                if (in0)
                {
                    lock (_lcd)
                    {
                        _lcd.Backlight = !_lcd.Backlight;
                    }
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
                if (in2)
                {
                    foreach (var device in _devices)
                    {
                        var temp = OneWireTemp.ReadDeviceTemperature(device);
                        Console.WriteLine(device + " = " + temp);
                    }
                }

                if (in3)
                {
                    for (var p = 0; p <= 100; p++)
                    {
                        pwm0.SetPercent(p);
                        Thread.Sleep(50);
                    }
                }

                out0.Set(in1 && in2);
            }

            lock (_lcd)
            {
                _lcd.Clear();
            }
            Console.WriteLine("done.");
            Environment.Exit(0);
        }

        private static void UpdateTemp()
        {
            var oldTemp = 0.0f;
            while (true)
            {
                lock (_lcd)
                {
                    var newTemp = _devices.Count > 0
                        ? OneWireTemp.ReadDeviceTemperature(_devices[0])
                        : 0.0f;
                    if (Math.Abs(newTemp - oldTemp) >= 0.1f)
                    {
                        oldTemp = newTemp;
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
