using System.Diagnostics;

namespace IctBaden.RasPi.IO
{
    internal class SoftPwmChannel : IPwmChannel
    {
        private readonly SoftPwm _pwm;
        public readonly Output Output;
        private double _percent;

        public SoftPwmChannel(SoftPwm pwm, uint gpio)
        {
            _pwm = pwm;
            Output = new Output(Gpio.FromGpio(gpio));
            _percent = 50.0;
        }

        public uint OutputGpio => Output.GetGpio().Bit;

        public double GetPercent()
        {
            return _percent;
        }

        public void SetPercent(double percent)
        {
            if (percent < 0 || percent > 100.0)
            {
                Trace.TraceError("SoftPwmChannel.SetPercent: Percent value must be in range 0..100");
                return;
            }
            _percent = percent;
        }

        public void Close()
        {
            _pwm.ShutdownChannel(this);
        }
    }
}
