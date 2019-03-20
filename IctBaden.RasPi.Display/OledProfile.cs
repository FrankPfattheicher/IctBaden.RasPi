// ReSharper disable UnusedMember.Global
namespace IctBaden.RasPi.Display
{
    public class OledProfile
    {
        public int Offset { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        
        public static OledProfile WemosShield64X48 = new OledProfile
        {
            Offset = 32,
            Width = 64,
            Height = 48
        };
    }
}