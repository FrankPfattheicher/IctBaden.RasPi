using IctBaden.RasPi.System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IctBaden.RasPi.Test
{
    [TestClass]
    public class ModelInfoTests
    {
        [TestMethod]
        public void DetectModelB2()
        {
            const string cpuinfo = @"processor	: 0
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

            ModelInfo.Decode(cpuinfo);

            Assert.AreEqual(0x0F, ModelInfo.Revision);
            Assert.AreEqual("BCM2708", ModelInfo.Hardware);
            Assert.AreEqual("B2", ModelInfo.Name);
            Assert.AreEqual(512, ModelInfo.RamSizeMb);
        }
    }
}
