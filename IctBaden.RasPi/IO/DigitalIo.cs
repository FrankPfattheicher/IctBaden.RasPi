using System;
using IctBaden.RasPi.Interop;

namespace IctBaden.RasPi.IO
{
    public class DigitalIo
    {
        /// <summary>
        /// Initialize digital io library
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            try
            {
                RawGpio.Initialize();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Factory method to create digital input
        /// </summary>
        /// <param name="gpio"></param>
        /// <returns></returns>
        public Input CreateInput(Gpio gpio)
        {
            return new Input(gpio);
        }

        /// <summary>
        /// Factory method to create digital output
        /// </summary>
        /// <param name="gpio"></param>
        /// <returns></returns>
        public Output CreateOutput(Gpio gpio)
        {
            return new Output(gpio);
        }

        public ulong GetAllInputs()
        {
            ulong inputs = RawGpio.GPIO_IN0;
            return inputs;
        }

    }
}
