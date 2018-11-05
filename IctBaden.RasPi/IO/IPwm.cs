// ReSharper disable UnusedMember.Global
namespace IctBaden.RasPi.IO
{
    public interface IPwm
    {
        bool Initialize();
        void Shutdown();
        IPwmChannel OpenChannel(uint gpio);
        void ShutdownChannel(IPwmChannel channel);
    }
}