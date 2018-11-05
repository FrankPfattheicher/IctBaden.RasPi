using System.Collections.Generic;
using System.Threading;

namespace IctBaden.RasPi.IO
{
    public class SoftPwm : IPwm
    {
        private Thread _pwmThread;
        private readonly List<IPwmChannel> _channels = new List<IPwmChannel>();

        public bool Initialize()
        {
            _pwmThread = new Thread(Pwm) {Priority = ThreadPriority.Highest};
            _pwmThread.Start();
            return true;
        }

        private void Pwm()
        {
            var percent = 0.0;
            while (_pwmThread != null)
            {
                if (_channels.Count == 0)
                {
                    Thread.Sleep(100);
                    continue;
                }

                lock(_channels)
                {
                    foreach (var pwmChannel in _channels)
                    {
                        var channel = (SoftPwmChannel) pwmChannel;
                        channel.Output.Set(channel.GetPercent() >= percent);
                    }
                }

                Thread.Sleep(2);
                percent += 10.0;
                if (percent >= 100.0) percent = 0.0;
            }
        }

        public IPwmChannel OpenChannel(Gpio gpio)
        {
            var channel = new SoftPwmChannel(this, gpio);
            lock (_channels)
            {
                _channels.Add(channel);
            }
            return channel;
        }

        public void ShutdownChannel(IPwmChannel channel)
        {
            lock (_channels)
            {
                _channels.Remove(channel);
            }
        }

        public void Shutdown()
        {
            _pwmThread = null;
        }
    }
}
