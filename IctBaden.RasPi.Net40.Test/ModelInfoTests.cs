using IctBaden.RasPi.System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IctBaden.RasPi.Test
{
    [TestClass]
    public class ModelInfoTests
    {
        
        private const string CpuInfoB2 = @"processor	: 0
model name	: ARMv6-compatible processor rev 7 (v6l)
BogoMIPS	: 2.57
Features	: half thumb fastmult vfp edsp java tls 
CPU implementer	: 0x41
CPU architecture: 7
CPU variant	: 0x0
CPU part	: 0xb76
CPU revision	: 7

Hardware	: BCM2708
Revision	: 000f
Serial		: 000000006ff33293
";

        private const string MemInfoB2 = @"MemTotal:         449448 kB
MemFree:          385032 kB
MemAvailable:     755260 kB
Buffers:           30096 kB
Cached:           405296 kB
SwapCached:            0 kB
Active:           309444 kB
Inactive:         215344 kB
Active(anon):      89712 kB
Inactive(anon):    26560 kB
Active(file):     219732 kB
Inactive(file):   188784 kB
Unevictable:           4 kB
Mlocked:               4 kB
SwapTotal:        102396 kB
SwapFree:         102396 kB
Dirty:                 0 kB
Writeback:             0 kB
AnonPages:         89408 kB
Mapped:            99444 kB
Shmem:             26872 kB
Slab:              24756 kB
SReclaimable:      13352 kB
SUnreclaim:        11404 kB
KernelStack:        1504 kB
PageTables:         3020 kB
NFS_Unstable:          0 kB
Bounce:                0 kB
WritebackTmp:          0 kB
CommitLimit:      577120 kB
Committed_AS:     884200 kB
VmallocTotal:    1114112 kB
VmallocUsed:           0 kB
VmallocChunk:          0 kB
CmaTotal:           8192 kB
CmaFree:            6792 kB
";

        private const string CpuInfoB3 = @"processor	: 0
model name	: ARMv7 Processor rev 4 (v7l)
BogoMIPS	: 38.40
Features	: half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt vfpd32 lpae evtstrm crc32 
CPU implementer	: 0x41
CPU architecture: 7
CPU variant	: 0x0
CPU part	: 0xd03
CPU revision	: 4

processor	: 1
model name	: ARMv7 Processor rev 4 (v7l)
BogoMIPS	: 38.40
Features	: half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt vfpd32 lpae evtstrm crc32 
CPU implementer	: 0x41
CPU architecture: 7
CPU variant	: 0x0
CPU part	: 0xd03
CPU revision	: 4

processor	: 2
model name	: ARMv7 Processor rev 4 (v7l)
BogoMIPS	: 38.40
Features	: half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt vfpd32 lpae evtstrm crc32 
CPU implementer	: 0x41
CPU architecture: 7
CPU variant	: 0x0
CPU part	: 0xd03
CPU revision	: 4

processor	: 3
model name	: ARMv7 Processor rev 4 (v7l)
BogoMIPS	: 38.40
Features	: half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt vfpd32 lpae evtstrm crc32 
CPU implementer	: 0x41
CPU architecture: 7
CPU variant	: 0x0
CPU part	: 0xd03
CPU revision	: 4

Hardware	: BCM2835
Revision	: a020d3
Serial		: 00000000ccc1fd19
";


        private const string MemInfoB3 = @"MemTotal:         949448 kB
MemFree:          385032 kB
MemAvailable:     755260 kB
Buffers:           30096 kB
Cached:           405296 kB
SwapCached:            0 kB
Active:           309444 kB
Inactive:         215344 kB
Active(anon):      89712 kB
Inactive(anon):    26560 kB
Active(file):     219732 kB
Inactive(file):   188784 kB
Unevictable:           4 kB
Mlocked:               4 kB
SwapTotal:        102396 kB
SwapFree:         102396 kB
Dirty:                 0 kB
Writeback:             0 kB
AnonPages:         89408 kB
Mapped:            99444 kB
Shmem:             26872 kB
Slab:              24756 kB
SReclaimable:      13352 kB
SUnreclaim:        11404 kB
KernelStack:        1504 kB
PageTables:         3020 kB
NFS_Unstable:          0 kB
Bounce:                0 kB
WritebackTmp:          0 kB
CommitLimit:      577120 kB
Committed_AS:     884200 kB
VmallocTotal:    1114112 kB
VmallocUsed:           0 kB
VmallocChunk:          0 kB
CmaTotal:           8192 kB
CmaFree:            6792 kB
";



        [TestMethod]
        public void DetectModelB2()
        {
            ModelInfo.Decode(CpuInfoB2, MemInfoB2);

            Assert.AreEqual(0x0F, ModelInfo.RevisionCode);
            Assert.AreEqual("BCM2708", ModelInfo.Hardware);
            Assert.AreEqual("2B", ModelInfo.Name);
            Assert.AreEqual(512, ModelInfo.RamSizeMb);
        }

        [TestMethod]
        public void DetectModelB3Plus()
        {
            ModelInfo.Decode(CpuInfoB3, MemInfoB3);

            Assert.AreEqual(0xA020d3, ModelInfo.RevisionCode);
            Assert.AreEqual("BCM2835", ModelInfo.Hardware);
            Assert.AreEqual("3B+", ModelInfo.Name);
            Assert.AreEqual(1024, ModelInfo.RamSizeMb);
        }

    }
}
