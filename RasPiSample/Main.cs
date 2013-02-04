using System;
using System.Threading;
using IctBaden.RasPi;

namespace GPIO
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Digital I/O");

            var io = new Gpio();
            if (!io.Initialize())
            {
                Console.WriteLine("Failed to initialize IO");
                return;
            }

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
                
            Console.WriteLine("I2C");

            const string deviceName = "/dev/i2c-1";  
            
            var lcd = new LcdI2C();
            lcd.Open(deviceName, 0x27);

            lcd.Print("Hello äöüßÄÖÜ 0°");
            lcd.SetCursor(1, 2);
            lcd.Print("Raspberry Pi");

            Thread.Sleep(500);
            lcd.Backlight = false;
            Thread.Sleep(500);
            lcd.Backlight = true;

        }



    }
        
}
