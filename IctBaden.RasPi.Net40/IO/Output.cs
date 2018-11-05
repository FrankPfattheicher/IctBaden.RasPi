using System;
using IctBaden.RasPi.Interop;

namespace IctBaden.RasPi.IO
{
    public class Output
    {
        private readonly Gpio _gpio;
        private readonly DigitalIo _io;
        private readonly int _index;

        [Obsolete("Use Output(Gpio gpio) instead")]
        public Output(DigitalIo io, int index)
        {
            _io = io;
            _index = index;
        }

        public Output(Gpio gpio)
        {
            _gpio = gpio;
        }

        /// <summary>
        /// Returns output's current value.
        /// </summary>
        /// <param name="output">Output to be queried</param>
        public static implicit operator bool(Output output)
        {
            if (output._gpio != null)
            {
                return (RawGpio.GPIO_IN0 & output._gpio.Mask) != 0;
            }
            return output._io.GetOutput(output._index);
        }

        /// <summary>
        /// Sets the output to the given value.
        /// </summary>
        /// <param name="value">True to set output</param>
        public void Set(bool value)
        {
            if (_gpio != null)
            {
                if (value)
                {
                    RawGpio.GPIO_SET = _gpio.Mask;
                }
                else
                {
                    RawGpio.GPIO_CLR = _gpio.Mask;
                }
            }
            else
            {
                _io.SetOutput(_index, value);
            }
        }

    }
}
