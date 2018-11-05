using System;
using IctBaden.RasPi.Interop;

namespace IctBaden.RasPi.IO
{
    public class Input
    {
        private readonly Gpio _gpio;
        private readonly DigitalIo _io;
        private readonly int _index;

        [Obsolete("Use Input(Gpio gpio) instead")]
        public Input(DigitalIo io, int index)
        {
            _io = io;
            _index = index;
        }
        public Input(Gpio gpio)
        {
            _gpio = gpio;
        }

        /// <summary>
        /// Returns the input's current value.
        /// </summary>
        /// <param name="input">Input to be queried</param>
        public static implicit operator bool(Input input)
        {
            if (input._gpio != null)
            {
                return (RawGpio.GPIO_IN0 & input._gpio.Mask) != 0;
            }
            return input._io.GetInput(input._index);
        }

    }
}
