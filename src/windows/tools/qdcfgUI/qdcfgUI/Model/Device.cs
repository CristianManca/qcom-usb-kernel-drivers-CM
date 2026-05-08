using System.Collections.ObjectModel;
using QCUtility;

namespace QCDevice
{
    public abstract class LogDevice : CNotifyPropertyChanged
    {
        private uint enableLog = 0;
        public uint EnableLog
        {
            get { return enableLog; }
            set
            {
                if (enableLog != value)
                {
                    enableLog = value;
                    NotifyPropertyChanged("EnableLog");
                }
            }
        }

        private string deviceName;
        public string DeviceName
        {
            get { return deviceName; }
            set
            {
                if (string.IsNullOrEmpty(deviceName) || deviceName != value)
                {
                    deviceName = value;
                    NotifyPropertyChanged(GetPropertyChangedMessage());
                }
            }
        }

        public byte DeviceType { get; set; }

        public abstract string GetPropertyChangedMessage();
        public override string ToString()
        {
            return DeviceName;
        }
    }

    public class Device: LogDevice
    {
        public override string GetPropertyChangedMessage()
        {
            return "Name";
        }
    }

    public class ParentDevice: LogDevice
    {
        private ObservableCollection<Device> devices;
        public ObservableCollection<Device> Devices
        {
            get { return devices; }
            set
            {
                if (devices != value)
                {
                    devices = value;
                }
            }
        }

        public ParentDevice()
        {
            Devices = new ObservableCollection<Device>();
        }

        public override string GetPropertyChangedMessage()
        {
            return "ParentName";
        }
    }
}
