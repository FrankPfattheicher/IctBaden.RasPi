using System;
using System.Diagnostics;
using IctBaden.RasPi.System;

namespace IctBaden.RasPi.Interop
{
    // ReSharper disable InconsistentNaming
    internal static unsafe class RawGpio
    {
        private static readonly uint BCM2708_PERI_BASE = (ModelInfo.Model == 1)
                                                       ? 0x20000000u        // RasPi 1
                                                       : 0x3F000000u;       // RasPi 2 up

        private static readonly uint GPIO_BASE = BCM2708_PERI_BASE + 0x200000;

        private const uint PAGE_SIZE = (4 * 1024);
        private const uint BLOCK_SIZE = (4 * 1024);
        private static int mem_fd;
        private static byte* gpio_mem;
        private static byte* gpio_map;

        // I/O access
        private static volatile uint* gpio;
        private static readonly void* NULL = (void*)0;

        // GPIO setup macros. Always use INP_GPIO(x) before using OUT_GPIO(x) or SET_GPIO_ALT(x,y)
        public static void INP_GPIO(uint g)
        {
            *(gpio + ((g) / 10)) &= ~(7u << (int)(((g) % 10) * 3));
        }

        public static void OUT_GPIO(uint g)
        {
            *(gpio + ((g) / 10)) |= 1u << (int)(((g) % 10) * 3);
        }

        // ReSharper disable once UnusedMember.Global
        public static void SET_GPIO_ALT(uint g, uint a)
        {
            *(gpio + (((g) / 10))) |= (((a) <= 3 ? (a) + 4 : (a) == 4 ? 3u : 2) << (int)(((g) % 10) * 3));
        }

        /// <summary>
        /// sets   bits which are 1 ignores bits which are 0
        /// </summary>
        public static uint GPIO_SET
        {
            set => *(gpio + 7) = value;
        }

        /// <summary>
        /// clears bits which are 1 ignores bits which are 0
        /// </summary>
        public static uint GPIO_CLR
        {
            set => *(gpio + 10) = value;
        }

        /// <summary>
        /// Get data 
        /// </summary>
        public static uint GPIO_IN0 => *(gpio + 13);

        internal static void Initialize()
        {
            if (IsInitialized) return;

            /* open /dev/gpiomem */
            if ((mem_fd = Libc.open("/dev/gpiomem", Libc.O_RDWR | Libc.O_SYNC)) < 0)
            {
                throw new Exception("can't open /dev/gpiomem");
            }

            /* mmap GPIO */
            // Allocate MAP block
            if ((gpio_mem = Libc.malloc(BLOCK_SIZE + (PAGE_SIZE - 1))) == NULL)
            {
                throw new Exception("allocation error");
            }

            // Make sure pointer is on 4K boundary
            if ((ulong) gpio_mem % PAGE_SIZE != 0)
            {
                gpio_mem += PAGE_SIZE - ((ulong)gpio_mem % PAGE_SIZE);
            }

            // Now map it
            gpio_map = (byte*)Libc.mmap(
              gpio_mem,
              BLOCK_SIZE,
              Libc.PROT_READ | Libc.PROT_WRITE,
              Libc.MAP_SHARED | Libc.MAP_FIXED,
              mem_fd,
              GPIO_BASE
            );

            if ((long)gpio_map < 0)
            {
                throw new Exception($"mmap error {(int)gpio_map}");
            }

            // Always use volatile pointer!
            gpio = (uint*)gpio_map;
        }

        // ReSharper disable once UnusedMember.Global
        public static void Close()
        {
            gpio = null;

            if (gpio_map != null)
            {
                if (Libc.munmap(gpio_map, BLOCK_SIZE) < 0)
                {
                    Trace.TraceError("RawGpio.Close: Failed to unmap GPIO");
                }
                gpio_map = null;
            }

            if (mem_fd != 0)
            {
                if (Libc.close(mem_fd) < 0)
                {
                    Trace.TraceError("RawGpio.Close: Failed to close /dev/gpiomem");
                }
                mem_fd = 0;
            }
        }

        public static bool IsInitialized => gpio != null;
    }
    // ReSharper restore InconsistentNaming
}
