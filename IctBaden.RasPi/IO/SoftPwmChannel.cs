using System.Diagnostics;

namespace IctBaden.RasPi.IO
{
    internal class SoftPwmChannel : Output, IPwmChannel
    {
        private readonly SoftPwm _pwm;
        private double _percent;

        internal SoftPwmChannel(SoftPwm pwm, Gpio gpio)
            : base(gpio)
        {
            _pwm = pwm;
            _percent = 50.0;
        }

        public Output Output => this;
        public uint OutputGpio => GetGpio().Bit;

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
