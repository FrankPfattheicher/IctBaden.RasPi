// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
namespace IctBaden.RasPi.Display
{
    public class OledProfile
    {
        /// <summary>
        /// List of possible I2C addresses.
        /// First is the default address.
        /// </summary>
        public byte[] Address { get; private set; }
        
        /// <summary>
        /// Internal driver specific offset to use.
        /// </summary>
        public int Offset { get; private set; }
        
        /// <summary>
        /// Width of display in pixels.
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Height of display in pixels.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Size of the display described by the length of its diagonal, in inches.
        /// </summary>
        public float Size;
        
        
        public static OledProfile WemosShield64x48 = new OledProfile
        {
            Address = new byte[]
            {
                0x3C,     // 011110+SA0+RW - 0x3C or 0x3D
                0x3D   
            },
            Offset = 32,
            Width = 64,
            Height = 48,
            Size = 0.66f
        };
        
        public static OledProfile AZDeliveryOLED128x64 = new OledProfile
        {
            Address = new byte[]
            {
                0x3C,    // marked as 78 and 7A on PCB
                0x3D   
            },
            Offset = 0,
            Width = 128,
            Height = 64,
            Size = 0.96f
        };
        
    }
}