namespace IctBaden.RasPi.IO
{
    public class PwmChannel
    {
        private readonly Pwm pwm;
        private readonly int channel;
        private readonly uint gpio;

        internal PwmChannel(Pwm pwm, int channel, uint gpio)
        {
            this.pwm = pwm;
            this.channel = channel;
            this.gpio = gpio;
        }

        public bool SetPercent(double percent)
        {
            return pwm.SetChannelPercent(channel, gpio, percent);
        }

        public void Close()
        {
            pwm.ShutdownChannel(channel);
        }
    }
}