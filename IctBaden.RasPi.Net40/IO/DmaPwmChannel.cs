namespace IctBaden.RasPi.IO
{
    public class DmaPwmChannel : IPwmChannel
    {
        private readonly DmaPwm _dmaPwm;
        public readonly int Channel;
        public uint OutputGpio { get; }
        private double _percent;

        internal DmaPwmChannel(DmaPwm dmaPwm, int channel, uint gpio)
        {
            _dmaPwm = dmaPwm;
            Channel = channel;
            OutputGpio = gpio;
        }

        public double GetPercent()
        {
            return _percent;
        }

        public void SetPercent(double percent)
        {
            if (_dmaPwm.SetChannelPercent(Channel, OutputGpio, percent))
            {
                _percent = percent;
            }
        }

        public void Close()
        {
            _dmaPwm.ShutdownChannel(this);
        }
    }
}