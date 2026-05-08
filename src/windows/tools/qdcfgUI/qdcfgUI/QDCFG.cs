using System;
using System.Runtime.InteropServices;
using QCDevice;

namespace QDCFGUtility
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TracingConfig
    {
        public uint Flags { get; set; }
        public uint Level { get; set; }
        public uint MaxFile { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct QDCFGCommand
    {
        public string Entry { get; set; }
        public string Usage { get; set; }
        public int IsExposed { get; set; }
        public byte DevType { get; set; }
        public int DefaultVal { get; set; }
        public string Description { get; set; }
    }

    public static class QDCFG
    {
        [DllImport("qdcfg.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint InspectEntry(string deviceFriendlyName, string entryName, ref uint entryValue);

        [DllImport("qdcfg.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint InspectTracing();

        [DllImport("qdcfg.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint InspectLogging(string deviceName, ref uint errorCode);

        [DllImport("qdcfg.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint GetTracingConfig(ref TracingConfig config);

        [DllImport("qdcfg.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int processCommand(int argc, string[] argv);

        [DllImport("qdcfg.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint getSupportedEntries();

        [DllImport("qdcfg.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int getCommand(ref QDCFGCommand buffer, uint index);

        [DllImport("qdcfg.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int getEntryLen(uint index);

        [DllImport("qdcfg.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int getUsageLen(uint index);

        [DllImport("qdcfg.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int executeCommand(string entry, char action, string[] arguments, int argCount, bool device, string deviceName);

        [DllImport("qdcfg.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getQUDVersionNum();

        public static string getQUDVersion()
        {
            IntPtr ptr = getQUDVersionNum();
            return Marshal.PtrToStringAnsi(ptr);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PRINTF_DELEGATE(string message);

        [DllImport("qdcfg.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void setPrintDelegate(PRINTF_DELEGATE printer);

        /* Check if the type from device discovery callback matches a type defined in qdcfg library */
        // TODO: find another way to check command-device compatibility
        public static bool DeviceTypeCompatible(byte deviceType, byte commandType)
        {
            switch(deviceType)
            {
                case DevFlag.DEV_TYPE_NET:
                    return (0x01 & commandType) != 0;
                case DevFlag.DEV_TYPE_PORTS:
                    return (0x02 & commandType) != 0;
                case DevFlag.DEV_TYPE_USB:
                    return (0x04 & commandType) != 0;
                case DevFlag.DEV_TYPE_MDM:
                    return (0x02 & commandType) != 0;
                case DevFlag.DEV_TYPE_FILT:
                    return (0x08 & commandType) != 0;
                default:
                    return commandType == 255;   // Compatible with all devices
            }
        }
    }
}