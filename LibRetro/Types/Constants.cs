namespace LibRetro.Types
{
    public static class Constants
    {
        public const byte RetroApiVersion = 1;

        // Buttons
        public const byte RetroDeviceIdJoypadB = 0;
        public const byte RetroDeviceIdJoypadY = 1;
        public const byte RetroDeviceIdJoypadSelect = 2;
        public const byte RetroDeviceIdJoypadStart = 3;
        public const byte RetroDeviceIdJoypadUp = 4;
        public const byte RetroDeviceIdJoypadDown = 5;
        public const byte RetroDeviceIdJoypadLeft = 6;
        public const byte RetroDeviceIdJoypadRight = 7;
        public const byte RetroDeviceIdJoypadA = 8;
        public const byte RetroDeviceIdJoypadX = 9;
        public const byte RetroDeviceIdJoypadR = 11;
        public const byte RetroDeviceIdJoypadL2 = 12;
        public const byte RetroDeviceIdJoypadR2 = 13;
        public const byte RetroDeviceIdJoypadL3 = 14;
        public const byte RetroDeviceIdJoypadR3 = 15;
        public const byte RetroDeviceIdJoypadL = 10;

        // Analog
        public const byte RetroDeviceIndexAnalogLeft = 0;
        public const byte RetroDeviceIndexAnalogRight = 1;
        public const byte RetroDeviceIndexAnalogButton = 2;
     
        public const byte RetroDeviceIdAnalogX = 0;
        public const byte RetroDeviceIdAnalogY = 1;

        // Mouse
        public const byte RetroDeviceIdMouseX = 0;
        public const byte RetroDeviceIdMouseY = 1;
        public const byte RetroDeviceIdMouseLeft = 2;
        public const byte RetroDeviceIdMouseRight = 3;
        public const byte RetroDeviceIdMouseWheelup = 4;
        public const byte RetroDeviceIdMouseWheeldown = 5;
        public const byte RetroDeviceIdMouseMiddle = 6;
        public const byte RetroDeviceIdMouseHorizWheelup = 7;
        public const byte RetroDeviceIdMouseHorizWheeldown = 8;
        public const byte RetroDeviceIdMouseButton4 = 9;
        public const byte RetroDeviceIdMouseButton5 = 10;

        // Lightgun
        public const byte RetroDeviceIdLightgunScreenX = 13; /*Absolute Position*/
        public const byte RetroDeviceIdLightgunScreenY = 14;/*Absolute*/
        public const byte RetroDeviceIdLightgunIsOffscreen = 15;/*Status Check*/
        public const byte RetroDeviceIdLightgunTrigger = 2;
        public const byte RetroDeviceIdLightgunReload = 16;/*Forced off-screen shot*/
        public const byte RetroDeviceIdLightgunAuxA = 3;
        public const byte RetroDeviceIdLightgunAuxB = 4;
        public const byte RetroDeviceIdLightgunStart = 6;
        public const byte RetroDeviceIdLightgunSelect = 7;
        public const byte RetroDeviceIdLightgunAuxC = 8;
        public const byte RetroDeviceIdLightgunDpadUp = 9;
        public const byte RetroDeviceIdLightgunDpadDown = 10;
        public const byte RetroDeviceIdLightgunDpadLeft = 11;
        public const byte RetroDeviceIdLightgunDpadRight = 12;
        /* deprecated */
        public const byte RetroDeviceIdLightgunX = 0; /*Relative Position*/
        public const byte RetroDeviceIdLightgunY = 1; /*Relative*/
        public const byte RetroDeviceIdLightgunCursor = 3; /*Use Aux:A*/
        public const byte RetroDeviceIdLightgunTurbo = 4; /*Use Aux:B*/
        public const byte RetroDeviceIdLightgunPause = 5; /*Use Start*/

        /* Id values for POINTER. */
        public const byte RetroDeviceIdPointerX = 0;
        public const byte RetroDeviceIdPointerY = 1;
        public const byte RetroDeviceIdPointerPressed = 2;

        /* Passed to retro_get_memory_data/size().
         * If the memory type doesn't apply to the
         * implementation NULL/0 can be returned.
         */
        public const byte RetroMemoryMask = 0xff;

        /* Regular save RAM. This RAM is usually found on a game cartridge,
         * backed up by a battery.
         * If save game data is too complex for a single memory buffer,
         * the SAVE_DIRECTORY (preferably) or SYSTEM_DIRECTORY environment
         * callback can be used. */
        public const byte RetroMemorySaveRam = 0;

        /* Some games have a built-in clock to keep track of time.
         * This memory is usually just a couple of bytes to keep track of time.
         */
        public const byte RetroMemoryRtc = 1;

        /* System ram lets a frontend peek into a game systems main RAM. */
        public const byte RetroMemorySystemRam = 2;

        /* Video ram lets a frontend peek into a game systems video RAM (VRAM). */
        public const byte RetroMemoryVideoRam = 3;

        /* If set, this call is not part of the public libretro API yet. It can
 * change or be removed at any time. */
        public const int RetroEnvironmentExperimental = 0x10000;
        /* Environment callback to be used internally in frontend. */
        public const int RetroEnvironmentPrivate = 0x20000;

        /* File open flags
         * Introduced in VFS API v1 */
        public const byte RetroVfsFileAccessRead = (1 << 0); /* Read only mode */
        public const byte RetroVfsFileAccessWrite = (1 << 1); /* Write only mode, discard contents and overwrites existing file unless RETRO_VFS_FILE_ACCESS_UPDATE is also specified */
        public const byte RetroVfsFileAccessReadWrite = (RetroVfsFileAccessRead | RetroVfsFileAccessWrite);/* Read-write mode, discard contents and overwrites existing file unless RETRO_VFS_FILE_ACCESS_UPDATE is also specified*/
        public const byte RetroVfsFileAccessUpdateExisting = (1 << 2); /* Prevents discarding content of existing files opened for writing */

        /* These are only hints. The frontend may choose to ignore them. Other than RAM/CPU/etc use,
           and how they react to unlikely external interference (for example someone else writing to that file,
           or the file's server going down), behavior will not change. */
        public const byte RetroVfsFileAccessHintNone = (0);
        /* Indicate that the file will be accessed many times. The frontend should aggressively cache everything. */
        public const byte RetroVfsFileAccessHintFrequentAccess = (1 << 0);

        /* Seek positions */
        public const byte RetroVfsSeekPositionStart = 0;
        public const byte RetroVfsSeekPositionCurrent = 1;
        public const byte RetroVfsSeekPositionEnd = 2;
    }
}
