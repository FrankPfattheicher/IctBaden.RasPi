// ReSharper disable UnusedMember.Global
namespace IctBaden.RasPi.IO
{
    public interface IPwm
    {
        bool Initialize();
        IPwmChannel OpenChannel(uint gpio);
        void Shutdown();
    }
}