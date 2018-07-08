using System;

namespace LibRetro.Types
{
    [Flags]
    public enum RetroMod
    {
        None = 0x0000,

        Shift = 0x01,
        Ctrl = 0x02,
        Alt = 0x04,
        Meta = 0x08,

        NumLock = 0x10,
        CapsLock = 0x20,

        ScrollLock = 0x40
    }
}
