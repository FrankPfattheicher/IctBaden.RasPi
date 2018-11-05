// ReSharper disable UnusedMember.Global
namespace IctBaden.RasPi.IO
{
    public interface IPwm
    {
        bool Initialize();
        void Shutdown();
        IPwmChannel OpenChannel(Gpio gpio);
        void ShutdownChannel(IPwmChannel channel);
    }
}