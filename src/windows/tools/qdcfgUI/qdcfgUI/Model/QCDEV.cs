using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
public struct QDEV_FEATURE_SETTING
{
    public uint Version;
    public uint Settings;
    public uint DeviceClass;
    [MarshalAs(UnmanagedType.LPTStr)]
    public string VID;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct QDEV_CB_PARAMS
{
    public IntPtr DevDesc;
    public IntPtr DevName;
    public IntPtr LIfName;
    public IntPtr Loc;
    public IntPtr DevPath;
    public IntPtr SerNum;
    public IntPtr SerNumMsm;
    public uint Mtu;
    public uint Flag;
    public uint Protocol;
    public IntPtr HwId;
    public IntPtr ParentDev;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct QDEV
{
    public string DevDesc;
    public string DevName;
    public string LIfName;
    public string SerNum;
    public byte Flag;
    public string ParentDev;
    /*public IntPtr Loc;
    public IntPtr DevPath;    
    public IntPtr SerNumMsm;
    public uint Mtu;    
    public uint Protocol;
    public IntPtr HwId;    */
}

unsafe public delegate void MyDevCB(ref QDEV_CB_PARAMS Params, ref IntPtr Context);

namespace QCDevice
{
    public static class DevFlag
    {
        public const uint MASK_DEV_TYPE = 0x0000FF00;
        public const uint MASK_DEV_STATE = 0x000000F0;
        public const uint MASK_QC_DRIVER = 0x0000000F;
        public const byte DEV_TYPE_NET = 0x01;
        public const byte DEV_TYPE_PORTS = 0x02;
        public const byte DEV_TYPE_USB = 0x03;
        public const byte DEV_TYPE_MDM = 0x04;
        public const byte DEV_TYPE_FILT = 0x09;
        public const byte DEPARTURE = 0x00;
        public const byte ARRIVAL = 0x01;
    }

    public static class QcConfig
    {
        public const uint DEV_FEATURE_INCLUDE_NONE_QC_PORTS = 0x00000001;
        public const uint DEV_FEATURE_SCAN_USB_WITH_VID = 0x00010000;
        public const uint DEV_CLASS_NET = 0x00000001;
        public const uint DEV_CLASS_PORTS = 0x00000002;
        public const uint DEV_CLASS_USB = 0x00000004;
    }

    public class TypedQDEV
    {
        public TypedQDEV(QDEV dev, byte type)
        {
            this.dev = dev;
            this.type = type;
        }

        public QDEV dev { get; }
        public byte type { get; }
    }

    public class QCDEVMON
    {
        public static MyDevCB discoveryCallBack;

        [DllImport("qcdev.dll", CharSet = CharSet.Auto)]
        public static extern void QDDLL_StartDeviceMonitor();

        [DllImport("qcdev.dll", CharSet = CharSet.Auto)]
        public static extern void QDDLL_stopDeviceMonitor();

        [DllImport("qcdev.dll", CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern void QDDLL_SetFeature([In] IntPtr Settings);

        [DllImport("qcdev.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void QDDLL_SetDeviceChangeCallback(MyDevCB DevCb);

        public static void LaunchDeviceMonitor()
        {
            QDDLL_StartDeviceMonitor();
        }

        unsafe public static void ConfigureFeatures()
        {
            QDEV_FEATURE_SETTING mySet = new QDEV_FEATURE_SETTING();
            mySet.Version = 1;
            mySet.Settings = (QcConfig.DEV_FEATURE_INCLUDE_NONE_QC_PORTS | QcConfig.DEV_FEATURE_SCAN_USB_WITH_VID);
            mySet.DeviceClass = (QcConfig.DEV_CLASS_NET | QcConfig.DEV_CLASS_PORTS | QcConfig.DEV_CLASS_USB);
            mySet.VID = "VID_05C6";
            int iSizeOfsStruct = Marshal.SizeOf(typeof(QDEV_FEATURE_SETTING));
            IntPtr pSettings = Marshal.AllocHGlobal(iSizeOfsStruct);
            Marshal.StructureToPtr(mySet, pSettings, false);

            QDDLL_SetFeature(pSettings);

            Marshal.FreeHGlobal(pSettings);
            pSettings = IntPtr.Zero;
        }

        public static void SetDeviceNotificationCallback()
        {
            discoveryCallBack = DeviceNotificationCB;
            QDDLL_SetDeviceChangeCallback(discoveryCallBack);
        }

        unsafe public static void DeviceNotificationCB(ref QDEV_CB_PARAMS Param, ref IntPtr Context)
        {
            byte devState;
            byte devType;
            byte isQcDriver;

            QDEV dev = new QDEV();
            dev.DevDesc = Marshal.PtrToStringAuto(Param.DevDesc);
            dev.ParentDev = Marshal.PtrToStringAuto(Param.ParentDev);
            dev.DevName = Marshal.PtrToStringAnsi(Param.DevName);
            dev.SerNum = Marshal.PtrToStringAuto(Param.SerNum);
            dev.LIfName = Marshal.PtrToStringAuto(Param.LIfName);

            devState = (byte)((Param.Flag & DevFlag.MASK_DEV_STATE) >> 4);
            devType = (byte)((Param.Flag & DevFlag.MASK_DEV_TYPE) >> 8);
            isQcDriver = (byte)(Param.Flag & DevFlag.MASK_QC_DRIVER);

            dev.Flag = devState;

            if (devState == DevFlag.ARRIVAL)
            {
                Console.WriteLine("ARRIVAL: <{0}> <{1}> <{2}>\n", dev.DevDesc, dev.DevName, dev.SerNum);
                Console.WriteLine("         Parent: <{0}>\n", dev.ParentDev);
                if (devType == DevFlag.DEV_TYPE_NET)
                {
                    Console.WriteLine("       : <{0}>\n", dev.LIfName);
                }
            }
            else
            {
                Console.WriteLine("DEPARTURE: <{0}> <{1}> <{2}>\n", dev.DevDesc, dev.DevName, dev.SerNum);
            }

            OnDeviceDiscovery(dev, devType);
        }

        protected static void OnDeviceDiscovery(QDEV devParams, byte devType)
        {
            EventHandler<TypedQDEV> handler = DeviceDiscovered;
            if (handler != null)
            {
                handler(null, new TypedQDEV(devParams, devType));
            }
        }

        public static event EventHandler<TypedQDEV> DeviceDiscovered;
    }
}
