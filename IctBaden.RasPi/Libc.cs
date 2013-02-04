using System.Runtime.InteropServices;

namespace IctBaden.RasPi
{
    // ReSharper disable InconsistentNaming
    internal static unsafe class Libc
    {
        //constants for libc open
        public const int O_RDONLY = 0;
        public const int O_WRONLY = 1;
        public const int O_RDWR = 02;

        public const int O_CREAT = 00100;       // creat file if it doesn't exist
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
        
        [DllImport("libc.so.6")]
        public static extern int read(int file,[MarshalAs(UnmanagedType.LPArray)] byte[] addr, int count);

        [DllImport("libc.so.6")]
        public static extern int write(int file,[MarshalAs(UnmanagedType.LPArray)] byte[] addr, int count);

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

        // This is the structure as used in the I2C_SMBUS ioctl call
//        public struct i2c_smbus_ioctl_data 
//        {
//            byte read_write;
//            byte command;
//            ushort size;
//            union i2c_smbus_data data;
//        };
         
        // This is the structure as used in the I2C_RDWR ioctl call
//        public struct i2c_rdwr_ioctl_data 
//        {
//            struct i2c_msg msgs;    /* pointers to i2c_msgs */
//            ushort nmsgs;           /* number of i2c_msgs */
//        };

        public const int I2C_RDRW_IOCTL_MAX_MSGS = 42;


        [DllImport("libc.so.6", EntryPoint="ioctl")]
        public static extern int ioctl_dword(int file, int command, ulong data);


        //constants for mmap
        public const int PROT_NONE = 0x0;      // page can not be accessed 
        public const int PROT_READ = 0x1;      // page can be read
        public const int PROT_WRITE = 0x2;     // page can be written
        public const int PROT_EXEC= 0x4;       // page can be executed 

        public static int MAP_SHARED = 0x01;   // Share changes
        public static int MAP_PRIVATE = 0x02;  // Changes are private
        public static int MAP_TYPE = 0x0F;     // Mask for type of mapping
        public static int MAP_FIXED = 0x10;    // Interpret addr exactly
        public static int MAP_ANONYMOUS = 0x20;// don't use a file


        [DllImport("libc.so.6")]
        public static extern void* mmap(void* addr, uint length, int prot, int flags, int fd, uint offset);
        
        [DllImport("libc.so.6")]
        public static extern byte* malloc(uint size);
    }
    // ReSharper restore InconsistentNaming
}

