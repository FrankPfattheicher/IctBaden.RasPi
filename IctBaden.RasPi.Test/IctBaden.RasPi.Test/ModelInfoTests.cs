using IctBaden.RasPi.System;
using Xunit;
// ReSharper disable StringLiteralTypo

namespace IctBaden.RasPi.Test
{
    public class ModelInfoTests
    {
        [Fact]
        public void DetectModelB2()
        {
            const string cpuInfo = @"processor	: 0
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

            ModelInfo.Decode(cpuInfo);

            Assert.Equal(0x0F, ModelInfo.RevisionCode);
            Assert.Equal("BCM2708", ModelInfo.Hardware);
            Assert.Equal("B2", ModelInfo.Name);
            Assert.Equal(512, ModelInfo.RamSizeMb);
        }
        
        [Fact]
        public void DetectModelB3plus()
        {
            const string cpuInfo = @"processor	: 0
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

            ModelInfo.Decode(cpuInfo);

            Assert.Equal(0xA000000, ModelInfo.RevisionCode);
            Assert.Equal("BCM2708", ModelInfo.Hardware);
            Assert.Equal("B3+", ModelInfo.Name);
            Assert.Equal(1024, ModelInfo.RamSizeMb);
        }
        
    }
}
