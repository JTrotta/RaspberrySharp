using System;

namespace RaspberrySharp.IO.Interop
{
    [Flags]
    public enum MemoryProtection
    {
        None = 0,
        Read = 1,
        Write = 2,
        ReadWrite = Read | Write
    }
}
