using IctBaden.RasPi.Interop;

namespace IctBaden.RasPi.IO
{
    public class Output
    {
        private readonly Gpio _gpio;

        internal Output(Gpio gpio)
        {
            _gpio = gpio;

            // set pin mode to output
            RawGpio.INP_GPIO(_gpio.Mask); // must use INP_GPIO before we can use OUT_GPIO
            RawGpio.OUT_GPIO(_gpio.Mask);
        }

        /// <summary>
        /// Returns output's current value.
        /// </summary>
        /// <param name="output">Output to be queried</param>
        public static implicit operator bool(Output output)
        {
            return (RawGpio.GPIO_IN0 & output._gpio.Mask) != 0;
        }

        /// <summary>
        /// Sets the output to the given value.
        /// </summary>
        /// <param name="value">True to set output</param>
        public void Set(bool value)
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

    }
}
