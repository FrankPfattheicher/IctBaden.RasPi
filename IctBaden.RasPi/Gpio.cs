using System;
using System.Collections.Generic;

namespace IctBaden.RasPi
{
    public class Gpio
    {
        // ReSharper disable InconsistentNaming
        private static unsafe class RawGpio
        {
            private const uint BCM2708_PERI_BASE = 0x20000000;
            private const uint GPIO_BASE = BCM2708_PERI_BASE + 0x200000; /* GPIO controller */
            
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
            
            public static void SET_GPIO_ALT(uint g, uint a)
            {
                *(gpio + (((g) / 10))) |= (((a) <= 3 ? (a) + 4 : (a) == 4 ? 3u : 2) << (int)(((g) % 10) * 3));
            }
            
            /// <summary>
            /// sets   bits which are 1 ignores bits which are 0
            /// </summary>
            public static uint GPIO_SET
            {
                set { *(gpio + 7) = value; }
            }
            
            /// <summary>
            /// clears bits which are 1 ignores bits which are 0
            /// </summary>
            public static uint GPIO_CLR
            {
                set { *(gpio + 10) = value; }
            }
            
            /// <summary>
            /// Get data 
            /// </summary>
            public static uint GPIO_IN0 { get { return *(gpio + 13); } }
            
            internal static void Initialize()
            {
                /* open /dev/mem */
                if ((mem_fd = Libc.open("/dev/mem", Libc.O_RDWR | Libc.O_SYNC)) < 0)
                {
                    throw new Exception("can't open /dev/mem");
                }
                
                /* mmap GPIO */
                // Allocate MAP block
                if ((gpio_mem = Libc.malloc(BLOCK_SIZE + (PAGE_SIZE - 1))) == NULL)
                {
                    throw new Exception("allocation error");
                }
                
                // Make sure pointer is on 4K boundary
                if ((ulong)gpio_mem % PAGE_SIZE != 0)
                    gpio_mem += PAGE_SIZE - ((ulong)gpio_mem % PAGE_SIZE);
                
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
                    throw new Exception(string.Format("mmap error {0}", (int)gpio_map));
                }
                
                // Always use volatile pointer!
                gpio = (uint*)gpio_map;
            }
          
        }
        // ReSharper restore InconsistentNaming

        Dictionary<uint, uint> ioMode = new Dictionary<uint, uint>();
        private int[] inputAssignment = { 17, 27, 22, 18 };
        private int[] outputAssignment = { 7, 8, 9, 10, 11, 23, 24, 25 };

        public int[] InputAssignment
        {
            get
            {
                return inputAssignment;
            }
            set
            {
                inputAssignment = value;
                Initialize();
            }
        }

        public int[] OutputAssignment
        {
            get
            {
                return outputAssignment;
            }
            set
            {
                outputAssignment = value;
                Initialize();
            }
        }

        public Dictionary<uint, uint> IoMode
        {
            get
            {
                return ioMode;
            }
            set
            {
                ioMode = value;
                Initialize();
            }
        }

        public int Inputs { get { return inputAssignment.Length; } }

        public int Outputs { get { return outputAssignment.Length; } }

        public bool Initialize()
        {
            try
            {
                RawGpio.Initialize();
            } catch (Exception)
            {
                return false;
            }

            foreach (var mode in ioMode)
            {
                RawGpio.INP_GPIO(mode.Key);
                RawGpio.SET_GPIO_ALT(mode.Key, mode.Value);
            }

            foreach (var input in inputAssignment)
            {
                RawGpio.INP_GPIO((uint)input);
            }
            
            foreach (var output in outputAssignment)
            {
                RawGpio.INP_GPIO((uint)output); // must use INP_GPIO before we can use OUT_GPIO
                RawGpio.OUT_GPIO((uint)output);
            }
            
            return true;
        }

        public void SetOutput(int index, bool value)
        {
            if ((index < 0) || (index >= Outputs))
            {
                throw new ArgumentException("Output out of range", "index");
            }
            if (value)
            {
                RawGpio.GPIO_SET = (uint)(1 << outputAssignment [index]);
            } else
            {
                RawGpio.GPIO_CLR = (uint)(1 << outputAssignment [index]);
            }
        }

        public bool GetInput(int index)
        {
            if ((index < 0) || (index >= Inputs))
            {
                throw new ArgumentException("Input out of range", "index");
            }

            return (RawGpio.GPIO_IN0 & (uint)(1 << inputAssignment [index])) != 0;
        }

        public ulong GetInputs()
        {
            ulong inputs = 0;
            for (var ix = 0; ix < Inputs; ix++)
            {
                if (GetInput(ix))
                {
                    inputs |= (ulong)1 << ix;
                }
            }
            return inputs;
        }
    }
}
