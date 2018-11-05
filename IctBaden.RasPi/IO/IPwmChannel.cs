// ReSharper disable UnusedMember.Global
namespace IctBaden.RasPi.IO
{
    public interface IPwmChannel
    {
        uint OutputGpio { get; }
        double GetPercent();
        void SetPercent(double percent);
        void Close();
    }
}