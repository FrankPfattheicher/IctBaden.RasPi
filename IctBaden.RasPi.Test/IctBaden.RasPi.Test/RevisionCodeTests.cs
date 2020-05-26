using IctBaden.RasPi.System;
using Xunit;
// ReSharper disable InconsistentNaming

namespace IctBaden.RasPi.Test
{
    public class RevisionCodeTests
    {

        [Fact]
        public void RevisionCode0xA22042ShouldResultIn2Bv12()
        {
            var rc = RevisionCode.Decode(0xA22042);
            
            Assert.True(rc.IsValid);
            Assert.Equal(2, rc.Revision);
            Assert.Equal("B", rc.Type);
            Assert.Equal("Embest", rc.Manufacturer);
            Assert.Equal(RaspiSoCs.BCM2837, rc.Processor);
            Assert.Equal(1024, rc.MemorySiteMB);
        }
        
    }
}