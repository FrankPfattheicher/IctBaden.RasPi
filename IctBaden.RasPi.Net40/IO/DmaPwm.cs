using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using IctBaden.RasPi.Interop;
using IctBaden.RasPi.System;

namespace IctBaden.RasPi.IO
{
    /// <summary>
    /// 
    /// This file is part of IctBaden.RasPi.IO
    /// 
    /// Copyright
    ///
    ///     Copyright (C)2015 Frank Pfattheicher (fpf@ict-baden.de)
    ///
    /// License
    ///
    ///     This program is free software: you can redistribute it and/or modify
    ///     it under the terms of the GNU Lesser General Public License as published
    ///     by the Free Software Foundation, either version 3 of the License, or
    ///     (at your option) any later version.
    ///
    ///     This program is distributed in the hope that it will be useful,
    ///     but WITHOUT ANY WARRANTY; without even the implied warranty of
    ///     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ///     GNU Lesser General Public License for more details at
    ///     http://www.gnu.org/licenses/lgpl-3.0-standalone.html
    ///
    /// Documentation
    ///
    ///     currently none
    ///
    /// DmaPwm.cs is based on pwm.c from Chris Hager, which is
    /// based on the excellent servod.c by Richard Hirst, provides flexible
    /// PWM via DMA for the Raspberry Pi, supporting a resolution of up to 1us,
    /// all 15 DMA channels, multiple GPIOs per channel.
    /// 
    /// Feedback is much appreciated.
    /// 
    /// Links:
    /// https://github.com/metachris/RPIO
    /// https://github.com/richardghirst/PiBits/tree/master/ServoBlaster
    ///
    ///
    /// SUBCYCLES
    /// ---------
    /// One second is divided into subcycles of user-defined length (within 2ms and 1s)
    /// which will be repeated endlessly. The subcycle length is set
    /// per DMA channel; the shorter the length of a subcycle, the less DMA memory will
    /// be used. Do not set below 2ms - we started seeing weird behaviors of the RPi.
    ///
    /// To use servos for instance, a typical subcycle time is 20ms (which will be repeated
    /// 50 times a second). Each subcycle includes the specific pulse(s) to set the servo
    /// to the correct position.
    ///
    /// You can add pulses to the subcycle, and they will be repeated accordingly (eg.
    /// a 100ms subcycle will be repeated 10 times per second; as are all the pulses
    /// within that subcycle). You can use any number of GPIOs, and set multiple pulses
    /// for each one. Longer subcycles use more DMA memory.
    ///
    ///
    /// PULSE WIDTH INCREMENT GRANULARITY
    /// ---------------------------------
    /// Another very important setting is the pulse width increment granularity, which
    /// defaults to 10µs and is used for _all_ DMA channels (since its passed to the PWM
    /// timing hardware). Under the hood you need to set the pulse widths as multiples
    /// of the increment-granularity. Eg. in order to set 500µs pulses with a granularity
    /// setting of 10µs, you'll need to set the pulse-width as 50 (50 * 10µs = 500µs).
    /// Less granularity needs more DMA memory.
    ///
    /// To achieve shorter pulses than 10µs, you simply need set a lower granularity.
    ///
    ///
    /// WARNING
    /// -------
    /// DmaPwm.cs is currently not yet fully tested. Setting very long or very short
    /// subcycle times may cause unreliable signals and system failure. 
    /// Please send feedback to fpf@ict-baden.de.
    ///
    ///</summary>
    public unsafe class DmaPwm : IPwm
    {
        // DMA Control Block Data Structure (p40): 8 words (256 bits)
        private struct DmaCb
        {
#pragma warning disable 414
#pragma warning disable 649
            // ReSharper disable NotAccessedField.Local
            public uint Info; // TI: transfer information
            public uint Src; // SOURCE_AD
            public uint Dst; // DEST_AD
            public uint Length; // TXFR_LEN: transfer length
            public uint Stride; // 2D stride mode
            public uint Next; // NEXTCONBK
            // ReSharper disable UnusedMember.Global
#pragma warning disable 169
            public uint Pad1; // _reserved_
            public uint Pad2;
#pragma warning restore 169
            // ReSharper restore UnusedMember.Global
            // ReSharper restore NotAccessedField.Local
#pragma warning restore 649
#pragma warning restore 414
        };

        // Memory mapping
        private struct VirtPhysPageMap
        {
            public byte* VirtAddr;
            public uint PhysAddr;
        };

        private struct Channel
        {
            public byte* VirtBase;
#pragma warning disable 649
            // ReSharper disable UnusedMember.Global
#pragma warning disable 169
            public uint* Sample;
            public DmaCb* Cb;
#pragma warning restore 169
            // ReSharper restore UnusedMember.Global
#pragma warning restore 649
            public VirtPhysPageMap* PageMap;
            public volatile uint* DmaReg;

            // Set by user
            public uint SubcycleTimeUs;

            // Set by system
            public uint NumSamples;
            public uint NumCbs;
            public uint NumPages;

            // Used only for control and percentage purposes
            public uint WidthMax;
        };

        // ReSharper disable UnusedMember.Local
        // ReSharper disable InconsistentNaming

        // PWM Memory Addresses
        private const uint PWM_CTL = (0x00 / 4);
        private const uint PWM_DMAC = (0x08 / 4);
        private const uint PWM_RNG1 = (0x10 / 4);
        private const uint PWM_FIFO = (0x18 / 4);

        private const uint PWMCLK_CNTL = 40;
        private const uint PWMCLK_DIV = 41;

        private const uint PWMCTL_MODE1 = (1 << 1);
        private const uint PWMCTL_PWEN1 = (1 << 0);
        private const uint PWMCTL_CLRF = (1 << 6);
        private const uint PWMCTL_USEF1 = (1 << 5);

        private const uint PWMDMAC_ENAB = ((uint)1 << 31);
        private const uint PWMDMAC_THRSHLD = ((15 << 8) | (15 << 0));

        // Standard page sizes
        private const int PAGE_SIZE = 4096;
        private const int PAGE_SHIFT = 12;

        // GPIO Memory Addresses
        private const int GPIO_FSEL0 = (0x00 / 4);
        private const int GPIO_SET0 = (0x1c / 4);
        private const int GPIO_CLR0 = (0x28 / 4);
        private const int GPIO_LEV0 = (0x34 / 4);
        private const int GPIO_PULLEN = (0x94 / 4);
        private const int GPIO_PULLCLK = (0x98 / 4);

        // GPIO Modes (IN=0, OUT=1)
        private const int GPIO_MODE_IN = 0;
        private const int GPIO_MODE_OUT = 1;

        // Memory Addresses
        private const uint DMA_BASE = 0x20007000;
        private const uint DMA_CHANNEL_INC = 0x100;
        private const uint DMA_LEN = 0x24;
        private const uint PWM_BASE = 0x2020C000;
        private const uint PWM_LEN = 0x28;
        private const uint CLK_BASE = 0x20101000;
        private const uint CLK_LEN = 0xA8;
        private const uint GPIO_BASE = 0x20200000;
        private const uint GPIO_LEN = 0x100;
        private const uint PCM_BASE = 0x20203000;
        private const uint PCM_LEN = 0x24;

        // Physical Addresses
        private const uint physGpClr0 = 0x7E200000 + 0x28;
        private const uint physGpSet0 = 0x7E200000 + 0x1c;

        private const uint physFifoAddr = (PWM_BASE | 0x7E000000) + 0x18;

        // Datasheet p. 51:
        private const uint DMA_NO_WIDE_BURSTS = (1 << 26);
        private const uint DMA_WAIT_RESP = (1 << 3);
        private const uint DMA_D_DREQ = (1 << 6);
        private readonly Func<uint, uint> DMA_PER_MAP = x => ((x) << 16);
        private const uint DMA_END = (1 << 1);
        private const uint DMA_RESET = ((uint)1 << 31);
        private const uint DMA_INT = (1 << 2);

        // Each DMA channel has 3 writeable registers:
        private const uint DMA_CS = (0x00 / 4);
        private const uint DMA_CONBLK_AD = (0x04 / 4);
        private const uint DMA_DEBUG = (0x20 / 4);

        // 15 DMA channels are usable on the RPi (0..14)
        private const int DMA_CHANNELS = 15;
        private static readonly Channel[] channels = new Channel[DMA_CHANNELS];

        // ReSharper restore InconsistentNaming
        // ReSharper restore UnusedMember.Local


        // ReSharper disable UnusedMember.Global
        public const int PulseWidthIncrementGranularityUsDefault = 10;
        public const int SubcycleTimeUsDefault = 20000; // 50 Hz
        // Subcycle minimum. We kept seeing no signals and strange behavior of the RPi
        public const uint SubcycleTimeUsMin = 2000;     // 500 Hz
        public const uint SubcycleTimeUsMax = 1000000;  // 1 Hz
        // ReSharper restore UnusedMember.Global

        private int _pulseWidthIncrementUs;

        private static volatile uint* _pwmReg;
        private static volatile uint* _pcmReg;
        private static volatile uint* _clkReg;
        private static volatile uint* _gpioReg;

        private static uint _gpioSetup; // bitfield for setup gpios (setup = out/low)

        public bool Initialize()
        {
            return Initialize(100);
        }

        // ReSharper disable once UnusedMember.Global
        public bool Initialize(int incrementUs)
        {
            if (ModelInfo.Model != 1)
            {
                Trace.TraceError("PWM: DMA based PWM is supported on model 1 hardware only");
                return false;
            }

            _pulseWidthIncrementUs = incrementUs;

            // Initialize common stuff
            _pwmReg = MapPeripheral(PWM_BASE, PWM_LEN);
            _pcmReg = MapPeripheral(PCM_BASE, PCM_LEN);
            _clkReg = MapPeripheral(CLK_BASE, CLK_LEN);
            _gpioReg = MapPeripheral(GPIO_BASE, GPIO_LEN);
            if (_pwmReg == null || _pcmReg == null || _clkReg == null || _gpioReg == null)
            {
                Trace.TraceError("PWM: Failed to map peripherals");
                return false;
            }

            // Initialise PWM
            _pwmReg[PWM_CTL] = 0;
            Thread.Sleep(TimeSpan.FromMilliseconds(0.01));
            _clkReg[PWMCLK_CNTL] = 0x5A000006; // Source=PLLD (500MHz)
            Thread.Sleep(TimeSpan.FromMilliseconds(0.1));
            _clkReg[PWMCLK_DIV] = 0x5A000000 | (50 << 12); // set pwm div to 50, giving 10MHz
            Thread.Sleep(TimeSpan.FromMilliseconds(0.1));
            _clkReg[PWMCLK_CNTL] = 0x5A000016; // Source=PLLD and enable
            Thread.Sleep(TimeSpan.FromMilliseconds(0.1));
            _pwmReg[PWM_RNG1] = (uint)incrementUs * 10;
            Thread.Sleep(TimeSpan.FromMilliseconds(0.01));
            _pwmReg[PWM_DMAC] = PWMDMAC_ENAB | PWMDMAC_THRSHLD;
            Thread.Sleep(TimeSpan.FromMilliseconds(0.01));
            _pwmReg[PWM_CTL] = PWMCTL_CLRF;
            Thread.Sleep(TimeSpan.FromMilliseconds(0.01));
            _pwmReg[PWM_CTL] = PWMCTL_USEF1 | PWMCTL_PWEN1;
            Thread.Sleep(TimeSpan.FromMilliseconds(0.01));

            return true;
        }

        private static uint* MapPeripheral(uint baseAddr, uint len)
        {
            var fd = Libc.open("/dev/mem", Libc.O_RDWR);

            if (fd < 0)
            {
                Trace.TraceError("PWM: Failed to open /dev/mem");
                return null;
            }

            var vaddr = Libc.mmap(null, len, Libc.PROT_READ | Libc.PROT_WRITE, Libc.MAP_SHARED, fd, baseAddr);
            if (vaddr == Libc.MAP_FAILED)
            {
                Trace.TraceError("PWM: Failed to map peripheral at {0:8X}\n", baseAddr);
                return null;
            }
            Libc.close(fd);

            return (uint*)vaddr;
        }


        internal bool InitChannel(int channel, uint subcycleTimeUs)
        {
            if (channel > (DMA_CHANNELS - 1))
            {
                Trace.TraceError("PWM: maximum channel is {0} (requested channel {1})\n", DMA_CHANNELS - 1, channel);
                return false;
            }
            if (channels[channel].VirtBase != null)
            {
                Trace.TraceError("PWM: channel {0} already initialized.\n", channel);
                return false;
            }
            if (subcycleTimeUs < SubcycleTimeUsMin)
            {
                //fatal("Error: subcycle time %dus is too small (min=%dus)\n", subcycleTimeUs, SUBCYCLE_TIME_US_MIN);
                return false;
            }

            // Setup Data
            channels[channel].SubcycleTimeUs = subcycleTimeUs;
            channels[channel].NumSamples = (uint)(channels[channel].SubcycleTimeUs / _pulseWidthIncrementUs);
            channels[channel].WidthMax = channels[channel].NumSamples;
            channels[channel].NumCbs = channels[channel].NumSamples * 2;
            channels[channel].NumPages = ((channels[channel].NumCbs * 32 + channels[channel].NumSamples * 4 + PAGE_SIZE -
                                            1) >> PAGE_SHIFT);

            // Initialize channel
            return InitVirtbase(channel) && MakePagemap(channel) && InitCtrlData(channel);
        }

        private static bool InitVirtbase(int channel)
        {
            channels[channel].VirtBase =
                (byte*)Libc.mmap(null, channels[channel].NumPages * PAGE_SIZE, Libc.PROT_READ | Libc.PROT_WRITE,
                    Libc.MAP_SHARED | Libc.MAP_ANONYMOUS | Libc.MAP_NORESERVE | Libc.MAP_LOCKED, -1, 0);

            if ((channels[channel].VirtBase == Libc.MAP_FAILED) || (channels[channel].VirtBase == null))
            {
                // ReSharper disable once RedundantAssignment
                var errno = Marshal.GetLastWin32Error();
                Trace.TraceError($"PWM: Failed to mmap physical pages: {errno}");
                return false;
            }
            if (((ulong)channels[channel].VirtBase & (PAGE_SIZE - 1)) != 0)
            {
                Trace.TraceError("PWM: Virtual address is not page aligned");
                return false;
            }
            return true;
        }

        private static bool MakePagemap(int channel)
        {
            channels[channel].PageMap =
                (VirtPhysPageMap*)Libc.malloc((uint)(channels[channel].NumPages * sizeof(VirtPhysPageMap)));

            if (channels[channel].PageMap == (VirtPhysPageMap*)0)
            {
                Trace.TraceError("PWM: Failed to malloc page_map");
                return false;
            }
            var memfd = Libc.open("/dev/mem", Libc.O_RDWR);
            if (memfd < 0)
            {
                Trace.TraceError("PWM: Failed to open /dev/mem");
                return false;
            }

            var pagemapFn = $"/proc/{Libc.getpid()}/pagemap";
            var fd = Libc.open(pagemapFn, Libc.O_RDONLY);
            if (fd < 0)
            {
                Trace.TraceError("PWM: Failed to open {0}", pagemapFn);
                return false;
            }

            if (Libc.lseek(fd, (int)((uint)channels[channel].VirtBase >> 9), Libc.SEEK_SET) !=
                (uint)channels[channel].VirtBase >> 9)
            {
                Trace.TraceError("PWM: Failed to seek on {0}", pagemapFn);
                return false;
            }

            for (var i = 0; i < channels[channel].NumPages; i++)
            {
                var pfn = new byte[8];
                channels[channel].PageMap[i].VirtAddr = channels[channel].VirtBase + (i * PAGE_SIZE);
                // Following line forces page to be allocated
                channels[channel].PageMap[i].VirtAddr[0] = 0;

                var read = Libc.read(fd, pfn, 8);
                if (read != pfn.Length)
                {
                    // ReSharper disable once RedundantAssignment
                    var errno = Marshal.GetLastWin32Error();
                    Trace.TraceError($"PWM: Failed to read {pagemapFn}: read={read}, errno={errno}");
                    return false;
                }
                var pfnLong = pfn[0]
                               + ((ulong)pfn[1] << 8)
                               + ((ulong)pfn[2] << 16)
                               + ((ulong)pfn[3] << 24)
                               + ((ulong)pfn[4] << 32)
                               + ((ulong)pfn[5] << 40)
                               + ((ulong)pfn[6] << 48)
                               + ((ulong)pfn[7] << 56);
                if (((pfnLong >> 55) & 0x1bf) != 0x10c)
                {
                    Trace.TraceError("PWM: Page {0} not present (pfn 0x{1:X}16llx)\n", i, pfn);
                    return false;
                }
                channels[channel].PageMap[i].PhysAddr = (uint)pfnLong << PAGE_SHIFT | 0x40000000;
            }
            Libc.close(fd);
            Libc.close(memfd);
            return true;
        }

        // Returns a pointer to the control block of this channel in DMA memory
        private static byte* GetCb(int channel)
        {
            return channels[channel].VirtBase + (sizeof(uint) * channels[channel].NumSamples);
        }

        // Memory mapping
        private static uint MemVirtToPhys(int channel, void* virt)
        {
            var offset = (uint)((long)virt - (long)channels[channel].VirtBase);
            return channels[channel].PageMap[offset >> PAGE_SHIFT].PhysAddr + (offset % PAGE_SIZE);
        }

        private bool InitCtrlData(int channel)
        {
            var cbp = (DmaCb*)GetCb(channel);
            var sample = (uint*)channels[channel].VirtBase;

            channels[channel].DmaReg = MapPeripheral(DMA_BASE, DMA_LEN) + (DMA_CHANNEL_INC * channel);
            if (channels[channel].DmaReg == null)
                return false;

            // Reset complete per-sample gpio mask to 0
            Libc.memset((byte*)sample, 0, channels[channel].NumSamples * sizeof(uint));

            // For each sample we add 2 control blocks:
            // - first: clear gpio and jump to second
            // - second: jump to next CB
            for (var ix = 0; ix < channels[channel].NumSamples; ix++)
            {
                cbp->Info = DMA_NO_WIDE_BURSTS | DMA_WAIT_RESP;
                cbp->Src = MemVirtToPhys(channel, sample + ix);
                // src contains mask of which gpios need change at this sample
                cbp->Dst = physGpClr0; // set each sample to clear set gpios by default
                cbp->Length = 4;
                cbp->Stride = 0;
                cbp->Next = MemVirtToPhys(channel, cbp + 1);
                cbp++;

                // Delay
                cbp->Info = DMA_NO_WIDE_BURSTS | DMA_WAIT_RESP | DMA_D_DREQ | DMA_PER_MAP(5);

                cbp->Src = MemVirtToPhys(channel, sample); // Any data will do
                cbp->Dst = physFifoAddr;
                cbp->Length = 4;
                cbp->Stride = 0;
                cbp->Next = MemVirtToPhys(channel, cbp + 1);
                cbp++;
            }

            // The last control block links back to the first (= endless loop)
            cbp--;
            cbp->Next = MemVirtToPhys(channel, GetCb(channel));

            // Initialize the DMA channel 0 (p46, 47)
            channels[channel].DmaReg[DMA_CS] = DMA_RESET; // DMA channel reset
            Thread.Sleep(TimeSpan.FromMilliseconds(0.01));
            channels[channel].DmaReg[DMA_CS] = DMA_INT | DMA_END; // Interrupt status & DMA end flag
            channels[channel].DmaReg[DMA_CONBLK_AD] = MemVirtToPhys(channel, GetCb(channel)); // initial CB
            channels[channel].DmaReg[DMA_DEBUG] = 7; // clear debug error flags
            channels[channel].DmaReg[DMA_CS] = 0x10880001; // go, mid priority, wait for outstanding writes

            return true;
        }

        // Sets a GPIO to either GPIO_MODE_IN(=0) or GPIO_MODE_OUT(=1)
        private static void GpioSetMode(uint gpio, uint mode)
        {
            var fsel = _gpioReg[GPIO_FSEL0 + gpio / 10];

            fsel &= (uint)~(7 << ((int)gpio % 10) * 3);
            fsel |= mode << ((int)gpio % 10) * 3;
            _gpioReg[GPIO_FSEL0 + gpio / 10] = fsel;
        }

        // Sets the gpio to input (level=1) or output (level=0)
        private static void GpioSet(uint gpio, bool level)
        {
            if (level)
                _gpioReg[GPIO_SET0] = (uint)(1 << (int)gpio);
            else
                _gpioReg[GPIO_CLR0] = (uint)(1 << (int)gpio);
        }

        // Set GPIO to OUTPUT, Low
        private static void InitGpio(uint gpio)
        {
            Trace.TraceInformation("PWM: InitGpio {0}", gpio);
            GpioSet(gpio, false);
            GpioSetMode(gpio, GPIO_MODE_OUT);
            _gpioSetup |= (uint)(1 << (int)gpio);
        }

        // ReSharper disable once UnusedMember.Global
        public IPwmChannel OpenChannel(Gpio gpio)
        {
            for (var ix = 0; ix < DMA_CHANNELS; ix++)
            {
                if (ChannelInUse(ix)) 
                    continue;

                if (InitChannel(ix, 2000))
                {
                    return new DmaPwmChannel(this, ix, gpio.Bit);
                }
            }
            return null;
        }

        public void ShutdownChannel(IPwmChannel channel)
        {
            channel.Close();
        }

        // Update the channel with another pulse within one full cycle. Its possible to
        // add more gpios to the same timeslots (widthStart). widthStart and width are
        // multiplied with pulse_width_incr_us to get the pulse width in microseconds [us].
        //
        // Be careful: if you try to set one GPIO to high and another one to low at the same
        // point in time, only the last added action (eg. set-to-low) will be executed on all pins.
        // To create these kinds of inverted signals on two GPIOs, either offset them by 1 step, or
        // use multiple DMA channels.
        internal bool SetChannelPercent(int channel, uint gpio, double percent)
        {
            var max = (int)channels[channel].WidthMax;
            var width = (int)((percent * max) / 100.0);
            if (width < 0)
            {
                width = 0;
            }
            if (width > max)
            {
                width = max;
            }
            return AddChannelPulse(channel, gpio, 0, width);
        }


        public bool AddChannelPulse(int channel, uint gpio, int widthStart, int width)
        {
            //Trace.TraceInformation("PWM: AddChannelPulse: channel={0}, gpio={1}, start={2}, width={3}", channel, gpio, widthStart, width);
            if (channels[channel].VirtBase == null)
            {
                Trace.TraceError($"PWM: channel {channel} has not been initialized with 'init_channel(..)'\n");
                return false;
            }
            if (widthStart < 0)
            {
                Trace.TraceError($"PWM: cannot add pulse to channel {widthStart}: widthStart less than zero {widthStart}");
                return false;
            }
            if (width < 0)
            {
                Trace.TraceError($"PWM: cannot add pulse to channel {width}: width less than zero {width}");
                return false;
            }
            if ((widthStart + width) > channels[channel].WidthMax)
            {
                Trace.TraceError($"PWM: cannot add pulse to channel {channel}: widthStart + width exceed max_width of {channels[channel].WidthMax}");
                return false;
            }

            if ((_gpioSetup & (1 << (int)gpio)) == 0)
            {
                InitGpio(gpio);
            }

            var cbp = (DmaCb*)((long)GetCb(channel) + (widthStart * 2));
            var dp = (uint*)channels[channel].VirtBase;

            // enable or disable gpio at this point in the cycle
            dp[widthStart] |= (uint)(1 << (int)gpio);
            //*(dp + width_start) |= 1 << gpio;
            cbp->Dst = physGpSet0;

            // Do nothing for the specified width
            int i;
            for (i = 1; i < width - 1; i++)
            {
                dp[widthStart + i] &= (uint)~(1 << (int)gpio);  // set just this gpio's bit to 0
                cbp += 2;
            }

            if ((widthStart + width) < channels[channel].WidthMax)
            {
                // Clear GPIO at end
                dp[widthStart + width] |= (uint)(1 << (int)gpio);
            }

            cbp->Dst = physGpClr0;
            return true;
        }

        // Clears all pulses for a specific gpio on this channel. Also sets the GPIO to Low.
        // ReSharper disable once UnusedMember.Global
        public bool ClearChannelGpio(int channel, uint gpio)
        {
            int i;
            var dp = (uint*)channels[channel].VirtBase;

            Trace.TraceInformation($"clear_channel_gpio: channel={channel}, gpio={gpio}");
            if (channels[channel].VirtBase == null)
            {
                Trace.TraceError($"PWM: channel {channel} has not been initialized with 'init_channel(..)'");
                return false;
            }
            if ((_gpioSetup & (1 << (int)gpio)) == 0)
            {
                Trace.TraceError($"PWM: cannot clear gpio {gpio}; not yet been set up");
                return false;
            }

            // Remove this gpio from all samples:
            for (i = 0; i < channels[channel].NumSamples; i++)
            {
                dp[i] &= (uint)~(1 << (int)gpio);  // set just this gpio's bit to 0
            }

            // Let DMA do one cycle before setting GPIO to low.
            //udelay(channels[channel].subcycle_time_us);

            GpioSet(gpio, false);
            return true;
        }

        // Shutdown -- its important to reset the DMA before quitting
        // ReSharper disable once UnusedMember.Global
        public void Shutdown()
        {
            for (var ix = 0; ix < DMA_CHANNELS; ix++)
            {
                ShutdownChannel(ix);
            }
        }

        private bool ChannelInUse(int channel)
        {
            return ((channels[channel].DmaReg != null) && (channels[channel].VirtBase != null));
        }

        internal void ShutdownChannel(DmaPwmChannel channel)
        {
            ShutdownChannel(channel.Channel);
        }
        internal void ShutdownChannel(int channelIndex)
        {
            if (ChannelInUse(channelIndex))
            {
                Trace.TraceError("PWM: Shutting down DMA channel {0}", channelIndex);
                ClearChannel(channelIndex);
                Thread.Sleep(TimeSpan.FromMilliseconds(channels[channelIndex].SubcycleTimeUs / 1000.0));
                channels[channelIndex].DmaReg[DMA_CS] = DMA_RESET;
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }
        }

        // Reset this channel to original state (all samples=0, all cbs=clr0)
        // ReSharper disable once UnusedMethodReturnValue.Local
        private static bool ClearChannel(int channel)
        {
            int i;
            var cbp = (DmaCb*)GetCb(channel);
            var dp = (uint*)channels[channel].VirtBase;

            Trace.TraceInformation("PWM: Clear channel {0}", channel);
            if (channels[channel].VirtBase == null)
            {
                Trace.TraceError("PWM: channel {0} has not been initialized with 'init_channel(..)'", channel);
                return false;
            }

            // First we have to stop all currently enabled pulses
            for (i = 0; i < channels[channel].NumSamples; i++)
            {
                cbp->Dst = physGpClr0;
                cbp += 2;
            }

            // Let DMA do one cycle to actually clear them
            Thread.Sleep(TimeSpan.FromMilliseconds(channels[channel].SubcycleTimeUs / 1000.0));

            // Finally set all samples to 0 (instead of gpio_mask)
            for (i = 0; i < channels[channel].NumSamples; i++)
            {
                dp[i] = 0;
            }

            return true;
        }


    }
}


