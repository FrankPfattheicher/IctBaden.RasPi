using System;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global

namespace IctBaden.RasPi.Interop
{
    // ReSharper disable InconsistentNaming
    internal static unsafe class Libc
    {
        //constants for libc open
        public const int O_RDONLY = 0;
        public const int O_WRONLY = 1;
        public const int O_RDWR = 02;

        public const int O_CREAT = 00100;       // create file if it doesn't exist
        public const int O_EXCL = 00200;        // exclusive use flag
        public const int O_NOCTTY = 00400;      // do not assign a controlling terminal
        public const int O_TRUNC = 01000;       // truncate flag
        public const int O_APPEND = 02000;      // set append mode
        public const int O_NONBLOCK = 04000;    // no delay
        public const int O_SYNC = 010000;

        [DllImport("libc.so.6")]
        public static extern int open(string file, int mode /*, int permissions */);

        [DllImport("libc.so.6")]
        public static extern int close(int file);

        [DllImport("libc.so.6", SetLastError = true)]
        public static extern int read(int file, [MarshalAs(UnmanagedType.LPArray)] byte[] addr, int count);

        [DllImport("libc.so.6")]
        public static extern int write(int file, [MarshalAs(UnmanagedType.LPArray)] byte[] addr, int count);


        public const int SEEK_SET = 0; // Seek from beginning of file.
        public const int SEEK_CUR = 1; // Seek from current position.
        public const int SEEK_END = 2; // Seek from end of file.

        [DllImport("libc.so.6")]
        public static extern int lseek(int file, int offset, int whence);
        
        //constants for ioctl (see i2c-dev.h)
        // /dev/i2c-X ioctl commands.  The ioctl's parameter is always an
        // unsigned long, except for:
        // - I2C_FUNCS, takes pointer to an unsigned long
        // - I2C_RDWR, takes pointer to struct i2c_rdwr_ioctl_data
        // - I2C_SMBUS, takes pointer to struct i2c_smbus_ioctl_data

        public const int I2C_RETRIES = 0x0701;  // number of times a device address should
        // be polled when not acknowledging
        public const int I2C_TIMEOUT = 0x0702;  // set timeout in units of 10 ms

        // NOTE: Slave address is 7 or 10 bits, but 10-bit addresses
        // are NOT supported! (due to code brokenness)

        public const int I2C_SLAVE = 0x0703;        // Use this slave address
        public const int I2C_SLAVE_FORCE = 0x0706;  // Use this slave address, even if it
        // is already in use by a driver!
        public const int I2C_TENBIT = 0x0704;       // 0 for 7 bit addrs, != 0 for 10 bit
        public const int I2C_FUNCS = 0x0705;        // Get the adapter functionality mask
        public const int I2C_RDWR = 0x0707;         // Combined R/W transfer (one STOP only)
        public const int I2C_PEC = 0x0708;          // != 0 to use PEC with SMBus
        public const int I2C_SMBUS = 0x0720;        // SMBus transfer

        // Data for SMBus Messages
        public const int I2C_SMBUS_BLOCK_MAX = 32;      // As specified in SMBus standard
        //union i2c_smbus_data {
        //        __u8 byte;
        //        __u16 word;
        //        __u8 block[I2C_SMBUS_BLOCK_MAX + 2]; /* block[0] is used for length */
        //                               /* and one more for user-space compatibility */
        //};

        /* i2c_smbus_xfer read or write markers */

        public const byte I2C_SMBUS_READ = 1;

        public const byte I2C_SMBUS_WRITE = 0;

        // SMBus transaction types (size parameter in the above functions)
        // Note: these no longer correspond to the (arbitrary) PIIX4 internal codes!
        public const byte I2C_SMBUS_QUICK = 0;
        public const byte I2C_SMBUS_BYTE = 1;
        public const byte I2C_SMBUS_BYTE_DATA = 2;
        public const byte I2C_SMBUS_WORD_DATA = 3;
        public const byte I2C_SMBUS_PROC_CALL = 4;
        public const byte I2C_SMBUS_BLOCK_DATA = 5;
        public const byte I2C_SMBUS_I2C_BLOCK_BROKEN = 6;
        public const byte I2C_SMBUS_BLOCK_PROC_CALL = 7;          // SMBus 2.0
        public const byte I2C_SMBUS_I2C_BLOCK_DATA = 8;


        [DllImport("libc.so.6", EntryPoint = "ioctl", SetLastError = true)]
        public static extern int ioctl_smbus(int file, int command, ref i2c_smbus_ioctl_data data);

        // This is the structure as used in the I2C_SMBUS ioctl call
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct i2c_smbus_ioctl_data
        {
            public byte read_write;
            public byte command;
            public ushort size;
            public IntPtr data;     // i2c_smbus_data
        };

        // This is the structure as used in the I2C_RDWR ioctl call
        //        public struct i2c_rdwr_ioctl_data 
        //        {
        //            struct i2c_msg msgs;    /* pointers to i2c_msgs */
        //            ushort nmsgs;           /* number of i2c_msgs */
        //        };

        public const int I2C_RDRW_IOCTL_MAX_MSGS = 42;


        [DllImport("libc.so.6", EntryPoint = "ioctl")]
        public static extern int ioctl_dword(int file, int command, ulong data);

        // constants for CD control
        public const int CDROMPAUSE = 0x5301;
        public const int CDROMRESUME = 0x5302;
        public const int CDROMPLAYMSF = 0x5303;
        public const int CDROMPLAYTRKIND = 0x5304;
        public const int CDROMREADTOCHDR = 0x5305;
        public const int CDROMREADTOCENTRY = 0x5306;
        public const int CDROMSTOP = 0x5307;
        public const int CDROMSTART = 0x5308;
        public const int CDROMEJECT = 0x5309;
        public const int CDROMVOLCTRL = 0x530a;
        public const int CDROMSUBCHNL = 0x530b;
        public const int CDROMREADMODE2 = 0x530c;
        public const int CDROMREADMODE1 = 0x530d;
        public const int CDROMREADAUDIO = 0x530e;
        public const int CDROMEJECT_SW = 0x530f;
        public const int CDROMMULTISESSION = 0x5310;
        public const int CDROM_GET_MCN = 0x5311;
        public const int CDROM_GET_UPC = CDROM_GET_MCN;
        public const int CDROMRESET = 0x5312;
        public const int CDROMVOLREAD = 0x5313;
        public const int CDROMREADRAW = 0x5314;

        public const int CDROMREADCOOKED = 0x5315;
        public const int CDROMSEEK = 0x5316;

        public const int CDROMPLAYBLK = 0x5317;

        public const int CDROMREADALL = 0x5318;

        public const int CDROMGETSPINDOWN = 0x531d;
        public const int CDROMSETSPINDOWN = 0x531e;

        public const int CDROMCLOSETRAY = 0x5319;
        public const int CDROM_SET_OPTIONS = 0x5320;
        public const int CDROM_CLEAR_OPTIONS = 0x5321;
        public const int CDROM_SELECT_SPEED = 0x5322;
        public const int CDROM_SELECT_DISC = 0x5323;
        public const int CDROM_MEDIA_CHANGED = 0x5325;
        public const int CDROM_DRIVE_STATUS = 0x5326;
        public const int CDROM_DISC_STATUS = 0x5327;
        public const int CDROM_CHANGER_NSLOTS = 0x5328;
        public const int CDROM_LOCKDOOR = 0x5329;
        public const int CDROM_DEBUG = 0x5330;
        public const int CDROM_GET_CAPABILITY = 0x5331;

        public const int CDROMAUDIOBUFSIZ = 0x5382;

        public const int DVD_READ_STRUCT = 0x5390;
        public const int DVD_WRITE_STRUCT = 0x5391;
        public const int DVD_AUTH = 0x5392;

        public const int CDROM_SEND_PACKET = 0x5393;
        public const int CDROM_NEXT_WRITABLE = 0x5394;
        public const int CDROM_LAST_WRITTEN = 0x5395;

        //constants for mmap
        public const int PROT_NONE = 0x0;      // page can not be accessed 
        public const int PROT_READ = 0x1;      // page can be read
        public const int PROT_WRITE = 0x2;     // page can be written
        public const int PROT_EXEC = 0x4;       // page can be executed 

        // see /usr/include/mman-generic.h
        public static int MAP_SHARED = 0x01;   // Share changes
        public static int MAP_PRIVATE = 0x02;  // Changes are private
        public static int MAP_TYPE = 0x0F;     // Mask for type of mapping
        public static int MAP_FIXED = 0x10;    // Interpret addr exactly
        public static int MAP_ANONYMOUS = 0x20; // don't use a file

        // see /usr/include/mman.h
        public static int MAP_LOCKED = 0x2000; // pages are locked
        public static int MAP_NORESERVE = 0x4000; // don't check for reservations
         
        public static void* MAP_FAILED = (void*) -1;

        [DllImport("libc.so.6", SetLastError = true)]
        public static extern void* mmap(void* addr, uint length, int prot, int flags, int fd, uint offset);

        [DllImport("libc.so.6", SetLastError = true)]
        public static extern int munmap(void* addr, uint length);

        [DllImport("libc.so.6")]
        public static extern byte* malloc(uint size);

        [DllImport("libc.so.6")]
        public static extern void free(byte* buffer);

        [DllImport("libc.so.6")]
        public static extern void* memset(byte* buffer, byte data, uint size);


        [DllImport("libc.so.6")]
        public static extern int getpid();
    }
    // ReSharper restore InconsistentNaming
}

