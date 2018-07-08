using System;
using System.Text.RegularExpressions;

namespace LibRetro.Native
{
    public static class NativeHelper
    {
        private static readonly IHelper PlatformHelper = 
            IsLinux() ? (new LinuxHelper()) : (IHelper)(new WindowsHelper());
        
        private static readonly Regex ArgumentsRegex = 
            new Regex(@"%(?:\d+\$)?[+-]?(?:[ 0]|'.{1})?-?\d*(?:\.\d+)?([bcdeEufFgGosxX])", RegexOptions.Compiled);
        
        public static IntPtr LoadLibrary(string fileName)
        {
            return PlatformHelper.LoadLibrary(fileName);
        }

        public static void FreeLibrary(IntPtr handle)
        {
            PlatformHelper.FreeLibrary(handle);
        }

        public static IntPtr GetProcAddress(IntPtr dllHandle, string name)
        {
            return PlatformHelper.GetProcAddress(dllHandle, name);
        }

        public static int Sprintf(out string buffer, string format, params IntPtr[] args)
        {
            return PlatformHelper.Sprintf(out buffer, format, args);
        }

        public static int GetFormatArgumentCount(string format)
        {
            var argumentsToPush = 0;

            var matches = ArgumentsRegex.Matches(format);

            foreach (Match match in matches)
            {
                switch (match.Captures[1].Value)
                {
                    case "b":
                        argumentsToPush += 1;
                        break;
                    case "d":
                        argumentsToPush += 1;
                        break;
                    case "f":
                        argumentsToPush += 2;
                        break;
                    case "u":
                        argumentsToPush += 1;
                        break;
                    case "s":
                        argumentsToPush += 1;
                        break;
                    case "m":
                        argumentsToPush += 2;
                        break;
                    default:
                        throw new NotImplementedException(
                            $"Placeholder '{match.Value}' not implemented"
                        );
                }
            }

            return argumentsToPush;
        }

        public static bool IsLinux()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix;
        }
    }
}