using IctBaden.RasPi.Interop;

namespace IctBaden.RasPi.IO
{
    public class Input
    {
        private readonly Gpio _gpio;

        internal Input(Gpio gpio)
        {
            _gpio = gpio;

            // set pin mode to input
            RawGpio.INP_GPIO(_gpio.Mask);
        }

        /// <summary>
        /// Returns the input's current value.
        /// </summary>
        /// <param name="input">Input to be queried</param>
        public static implicit operator bool(Input input)
        {
            return (RawGpio.GPIO_IN0 & input._gpio.Mask) != 0;
        }

    }
}
