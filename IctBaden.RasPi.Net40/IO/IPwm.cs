// ReSharper disable UnusedMember.Global
namespace IctBaden.RasPi.IO
{
    public interface IPwm
    {
        bool Initialize();
        IPwmChannel OpenChannel(Gpio gpio);
        void Shutdown();
    }
}