// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable StringLiteralTypo
namespace IctBaden.RasPi.System
{
    public class RevisionCode
    {
        public bool IsValid { get; private set; }
        public bool NewStyle { get; private set; }
        // ReSharper disable once InconsistentNaming
        public int MemorySiteMB { get; private set; }
        public string Manufacturer { get; private set; }
        public RaspiSoCs Processor { get; private set; }
        public string Type { get; private set; }
        public int Revision { get; private set; }        
        
        private RevisionCode()
        {
            
        }
        
        public static RevisionCode Decode(ulong code)
        {
            // ReSharper disable CommentTypo

            //  uuuuuuuuFMMMCCCCPPPPTTTTTTTTRRRR
            
            /*
                uuuuuuuu	Unused	Unused
                F	        New flag
                                1: new-style revision
                                0: old-style revision
                MMM	        Memory size
                                0: 256 MB
                                1: 512 MB
                                2: 1 GB
                CCCC	    Manufacturer
                                0: Sony UK
                                1: Egoman
                                2: Embest
                                3: Sony Japan
                                4: Embest
                                5: Stadium
                PPPP	    Processor
                                0: BCM2835
                                1: BCM2836
                                2: BCM2837
                TTTTTTTT	Type
                                0: A
                                1: B
                                2: A+
                                3: B+
                                4: 2B
                                5: Alpha (early prototype)
                                6: CM1
                                8: 3B
                                9: Zero
                                a: CM3
                                c: Zero W
                                d: 3B+
                                e: 3A+
                RRRR	    Revision
                                0, 1, 2, etc.             
            */
            // ReSharper restore CommentTypo

            const ulong revisionMask     = 0x0000000F;
            const ulong typeMask         = 0x00000FF0;
            const ulong processorMask    = 0x0000F000;
            const ulong manufacturerMask = 0x000F0000;
            const ulong memorySizeMask   = 0x00700000;
            const ulong newStyleMask     = 0x00800000;

            var rc = new RevisionCode
            {
                IsValid = true,
                NewStyle = (code & newStyleMask) != 0
            };
            
            switch ((code & memorySizeMask) >> 20)
            {
                case 0: 
                    rc.MemorySiteMB = 256;
                    break;
                case 1: 
                    rc.MemorySiteMB = 512;
                    break;
                case 2: 
                    rc.MemorySiteMB = 1024;
                    break;
                default:
                    rc.IsValid = false;
                    break;
            }

            var manufacturers = new[]
            {
                "Sony UK",
                "Egoman",
                "Embest",
                "Sony Japan",
                "Embest",
                "Stadium"
            };

            var manufacturer = (int)((code & manufacturerMask) >> 16);
            if (manufacturer < manufacturers.Length)
            {
                rc.Manufacturer = manufacturers[manufacturer];
            }
            else
            {
                rc.Manufacturer = "<unknown>";
                rc.IsValid = false;
            }

            switch ((code & processorMask) >> 12)
            {
                case 0: 
                    rc.Processor = RaspiSoCs.BCM2835;
                    break;
                case 1: 
                    rc.Processor = RaspiSoCs.BCM2836;
                    break;
                case 2: 
                    rc.Processor = RaspiSoCs.BCM2837;
                    break;
                default:
                    rc.IsValid = false;
                    break;
            }

            var types = new[]
            {
                "A",
                "B",
                "A+",
                "B+",
                "B",
                "Alpha",
                "CM1",
                "B",
                "Zero",
                "CM3",
                "Zero W",
                "B+",
                "A+"
            };
            var type = (int)((code & typeMask) >> 4);
            if (type < types.Length)
            {
                rc.Type = types[type];
            }
            else
            {
                rc.Type = "<unknown>";
                rc.IsValid = false;
            }

        rc.Revision = (int) (code & revisionMask);         
            
            return rc;
        }

    }
}
