using QCDevice;
using QCUtility;
using QDCFGUtility;
using QCDriverConstant;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace qdcfgUI
{
    public class MainWindowViewModel : CNotifyPropertyChanged
    {
        public static readonly uint defaultFlags = Convert.ToUInt32("7FFFFFFF", 16);
        public static readonly uint defaultLevel = 0xFF;
        public static readonly uint currMaxLevel = 0x06;
        public static readonly uint defaultFileSize = 100;

        private readonly object deviceLock = new object();

        private ObservableCollection<WPPConstantItem> debugFlags;
        public ObservableCollection<WPPConstantItem> DebugFlags
        {
            get { return debugFlags; }
            set
            {
                if (debugFlags != value)
                {
                    debugFlags = value;
                }
            }
        }

        private ObservableCollection<WPPConstantItem> debugLevels;
        public ObservableCollection<WPPConstantItem> DebugLevels
        {
            get { return debugLevels; }
            set
            {
                if (debugLevels != value)
                {
                    debugLevels = value;
                }
            }
        }

        private readonly QDCFG.PRINTF_DELEGATE textOuputDelegate;

        private List<QDCFGCommand> commandList;  // consider removing this for better performance
        private ObservableCollection<QDCFGCommand> exposedCommands;
        public ObservableCollection<QDCFGCommand> ExposedCommands
        {
            get { return exposedCommands; }
            set
            {
                if (exposedCommands != value)
                {
                    exposedCommands = value;
                }
            }
        }

        private string newEntryValue;
        public string NewEntryValue
        {
            get { return newEntryValue; }
            set
            {
                if (newEntryValue != value)
                {
                    newEntryValue = value;
                    NotifyPropertyChanged("NewEntryValue");
                }
            }
        }

        private LogDevice selectedDevice;
        public LogDevice SelectedDevice
        {
            get { return selectedDevice; }
            set
            {
                if (selectedDevice == null || selectedDevice != value)
                {
                    selectedDevice = value;
                    NotifyPropertyChanged("SelectedDevice");
                }
            }
        }

        private QDCFGCommand? selectedCommand = null;
        public QDCFGCommand? SelectedCommand
        {
            get { return selectedCommand; }
            set
            {
                selectedCommand = value;
                NotifyPropertyChanged("SelectedCommand");
            }
        }

        //Collection to hold parent devices
        private ObservableCollection<ParentDevice> parentDevices;
        public ObservableCollection<ParentDevice> ParentDevices
        {
            get { return parentDevices; }
            set
            {
                if (parentDevices != value)
                {
                    parentDevices = value;
                }
            }
        }

        //Displays output from qdcfg cmd line tool
        private string logOutput = string.Empty;
        public string LogOutput
        {
            get { return logOutput; }
            set
            {
                if (logOutput != value)
                {
                    logOutput = value;
                    NotifyPropertyChanged("LogOutput");
                }
            }
        }

        //Global setting flags
        private string flags;
        public string Flags
        {
            get { return flags; }
            set { /* do nothing here */ }
        }

        private uint flagVal;
        public uint FlagVal
        {
            get { return flagVal; }
            set
            {
                flagVal = value;
                flags = string.Format("0x{0:X8}", flagVal);
                NotifyPropertyChanged("Flags");
            }
        }

        //Global setting log level
        private uint level;
        public uint Level
        {
            get { return level; }
            set
            {
                if (level != value)
                {
                    level = value;
                    NotifyPropertyChanged("Level");
                }
            }
        }

        //Global setting file size
        private string fileSize;
        public string FileSize
        {
            get { return fileSize; }
            set
            {
                if (fileSize != value)
                {
                    fileSize = value;
                    NotifyPropertyChanged("FileSize");
                }
            }
        }

        //Action taken when enabling/disabling per device log
        private RelayCommand enableLogClickCommand;
        public RelayCommand EnableLogClickCommand
        {
            get
            {
                if (enableLogClickCommand == null)
                {
                    enableLogClickCommand = new RelayCommand(OnEnableLogClickCommand);
                }

                return enableLogClickCommand;
            }
        }

        //Action taken when enabling/disabling logging session
        private RelayCommand enableSessionClickCommand;
        public RelayCommand EnableSessionClickCommand
        {
            get
            {
                if (enableSessionClickCommand == null)
                {
                    enableSessionClickCommand = new RelayCommand(OnEnableSessionClickCommand);
                }

                return enableSessionClickCommand;
            }
        }

        private RelayCommand parseLogsCommand;
        public RelayCommand ParseLogsCommand
        {
            get
            {
                if (parseLogsCommand == null)
                {
                    parseLogsCommand = new RelayCommand(onParseLogsCommand);
                }

                return parseLogsCommand;
            }
        }

        //Action taken when setting entry values
        private RelayCommand applyEntryValueCommand;
        public RelayCommand ApplyEntryValueCommand
        {
            get
            {
                if (applyEntryValueCommand == null)
                {
                    applyEntryValueCommand = new RelayCommand(OnApplyEntryValueClickCommand);
                }
                return applyEntryValueCommand;
            }
        }

        //Action taken when setting entry values
        private RelayCommand fetchEntryValueClickCommand;
        public RelayCommand FetchEntryValueClickCommand
        {
            get
            {
                if (fetchEntryValueClickCommand == null)
                {
                    fetchEntryValueClickCommand = new RelayCommand(OnFetchEntryValueClickCommand);
                }
                return fetchEntryValueClickCommand;
            }
        }

        public MainWindowViewModel()
        {
            ParentDevices = new ObservableCollection<ParentDevice>();
            ExposedCommands = new ObservableCollection<QDCFGCommand>();

            QCDEVMON.ConfigureFeatures();
            QCDEVMON.SetDeviceNotificationCallback();
            QCDEVMON.LaunchDeviceMonitor();
            QCDEVMON.DeviceDiscovered += QCDEVMON_DeviceDiscovered;
            UpdateSessionState();
            UpdateGlobalSetting(false);

            textOuputDelegate = PrintConsole;
            QDCFG.setPrintDelegate(textOuputDelegate);

            commandList = new List<QDCFGCommand>();
            uint entrySize = QDCFG.getSupportedEntries();
            for (uint i=0; i<entrySize; i++)
            {
                QDCFGCommand cmd = new QDCFGCommand();
                cmd.Entry = new string('\0', QDCFG.getEntryLen(i));
                cmd.Usage = new string('\0', QDCFG.getUsageLen(i));
                QDCFG.getCommand(ref cmd, i);
                if (Convert.ToBoolean(cmd.IsExposed) == true)
                {
                    commandList.Add(cmd);
                }
            }

            DebugFlags = new ObservableCollection<WPPConstantItem>(WPPConstantItem.GetWPPConstants("WPP_DRV_MASK"));
            DebugLevels = new ObservableCollection<WPPConstantItem>(WPPConstantItem.GetWPPConstants("WPP_LEVEL"));

            SelectedDevice = new Device();
            SelectedDevice.DeviceName = "";
            SelectedDevice.DeviceType = 0xff;
            OnDeviceSelected(SelectedDevice);
        }

        //Callback when a device arrives/departs
        private void QCDEVMON_DeviceDiscovered(object sender, TypedQDEV device)
        {
            byte type = device.type;
            QDEV dev = device.dev;
            string deviceName = dev.DevDesc;
            string parentName = dev.ParentDev;

            if (string.IsNullOrEmpty(deviceName) || string.IsNullOrEmpty(parentName))
            {
                Console.WriteLine("Empty device name");
                return;
            }
            if (deviceName.Contains("ADB"))
            {
                Console.WriteLine("ADB device");
                return;
            }
            if (!deviceName.Contains("Qualcomm") && !deviceName.Contains("QDSS"))
            {
                Console.WriteLine("Non-Qualcomm device");
                return;
            }
            if (!parentName.Contains("Qualcomm") || parentName.Contains("ADB"))
            {
                Console.WriteLine("Parent is either a Non-Qualcomm device or ADB");
                return;
            }

            byte devState = dev.Flag;

            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (deviceLock)
                {
                    ParentDevice pDevice = ParentDevices.FirstOrDefault(name => name.DeviceName == parentName);
                    if (pDevice == null && devState == DevFlag.ARRIVAL)
                    {
                        //Add parent device; and child device
                        ParentDevice parent = new ParentDevice() { DeviceName = parentName, DeviceType = DevFlag.DEV_TYPE_FILT };
                        Device child = new Device() { DeviceName = deviceName, DeviceType = type };
                        UpdateLoggingState(parent);
                        UpdateLoggingState(child);
                        parent.Devices.Add(child);
                        ParentDevices.Add(parent);
                        ParentDevices.Sort(p => p.DeviceName);
                    }
                    else if (pDevice != null && devState == DevFlag.ARRIVAL)
                    {
                        //Parent device is present; add only child device to corresponding parent
                        Device child = new Device() { DeviceName = deviceName, DeviceType = type };
                        UpdateLoggingState(child);
                        pDevice.Devices.Add(child);
                        pDevice.Devices.Sort(c => c.DeviceName);
                    }
                    else if (pDevice != null && devState == DevFlag.DEPARTURE)
                    {
                        //Parent device is present; remove device from corresponding parent
                        Device cDevice = pDevice.Devices.FirstOrDefault(name => name.DeviceName == deviceName);
                        if (cDevice != null)
                        {
                            pDevice.Devices.Remove(cDevice);
                        }

                        //if all devices are removed from a parent, remove the parent
                        if (pDevice.Devices.Count == 0)
                        {
                            ParentDevices.Remove(pDevice);
                        }
                    }
                }
            });
        }

        //Method to enable/disable logging for parent/child device
        private void OnEnableLogClickCommand(object state)
        {
            if (state == null) return;

            Type type = state.GetType();
            if (type.Name.Equals("ParentDevice"))
            {
                ParentDevice pDevice = (ParentDevice)state;
                if (pDevice == null) return;

                Console.WriteLine("Logging for Parent Device : {0}", pDevice.DeviceName);
                SetLogging(false, pDevice.EnableLog ^ 1, pDevice.DeviceName);
                UpdateLoggingState(pDevice);
            }
            else if (type.Name.Equals("Device"))
            {
                Device currentDevice = (Device)state;
                if (currentDevice == null) return;

                Console.WriteLine("Logging for Child Device : {0}", currentDevice.DeviceName);
                SetLogging(false, currentDevice.EnableLog ^ 1, currentDevice.DeviceName);
                UpdateLoggingState(currentDevice);
            }
        }

        //Method to call qdcfg.exe cmd line tool
        private void SetLogging(bool isGlobalSetting, uint enableLog, string name = null)
        {
            int argLen = isGlobalSetting ? 4 : 1;
            string[] arguments = new string[argLen];
            arguments[0] = enableLog.ToString();
            if (isGlobalSetting)
            {
                arguments[1] = FlagVal.ToString("X");
                arguments[2] = Level.ToString("X");
                arguments[3] = string.IsNullOrEmpty(FileSize) ? defaultFileSize.ToString() : FileSize;
            }
            ClearConsole();
            QDCFG.executeCommand("QCDriverDebugMask", 's', arguments, argLen, !isGlobalSetting, name);
        }

       public static void RunETLParsingFile(
       string tracepdbPath,
       string tracefmtPath,
       string pdbPath,
       string etlPath,
       string tmfOutputDirectory, // TMF folder for storing the .tmf files and .sum file temporarily
       string logOutputFile)
        {
            String flavor;
            if (RuntimeInformation.OSArchitecture == Architecture.X86)
                flavor = "i386";
            else if (RuntimeInformation.OSArchitecture == Architecture.X64)
                flavor = "amd64";
            else if(RuntimeInformation.OSArchitecture == Architecture.Arm)
                flavor = "arm";
            else
                flavor = "arm64";

            Directory.CreateDirectory(tmfOutputDirectory); // Creating TMF output directory

            string[] driverFolders = Directory.GetDirectories(pdbPath);
            foreach (var driverFolder in driverFolders)
            {
                string flavorPath = Path.Combine(driverFolder, flavor);
                if (!Directory.Exists(flavorPath))
                {
                    flavorPath = Path.Combine(Path.Combine(driverFolder, "6x"), flavor);
                    if (!Directory.Exists(flavorPath))
                    {
                        Console.WriteLine($"Skipping {driverFolder} (no amd64 folder found)");
                        continue;
                    }
                }
                string[] pdbFiles = Directory.GetFiles(flavorPath, "*.pdb", SearchOption.TopDirectoryOnly);
                if (pdbFiles.Length == 0)
                {
                    Console.WriteLine($"No PDB files found in: {flavorPath}");
                    continue;
                }
                foreach (var pdbFile in pdbFiles) // Copy PDB to TMF folder (if not already the same)
                {
                    Console.WriteLine($"Processing PDB: {pdbFile}");
                    string pdbFileName = Path.GetFileName(pdbFile);
                    string pdbCopyPath = Path.Combine(tmfOutputDirectory, pdbFileName);
                    if (!pdbCopyPath.Equals(pdbFile, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            File.Copy(pdbFile, pdbCopyPath, true);
                            Console.WriteLine("File copied successfully.");
                        }
                        catch (IOException ioEx)
                        {
                            Console.WriteLine($"IO error while copying file: {ioEx.Message}");
                        }
                        catch (UnauthorizedAccessException uaEx)
                        {
                            Console.WriteLine($"Access denied: {uaEx.Message}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unexpected error: {ex.Message}");
                        }

                    }
                    // Running tracepdb via cmd.exe
                    Console.WriteLine("Generating TMF files...");
                    string cmdTracePdbPath = $"{tracepdbPath} -f {pdbCopyPath} -p {tmfOutputDirectory}";
                    RunViaCmd(cmdTracePdbPath);
                }
            }
            // Running tracefmt via cmd.exe
            Console.WriteLine("Extracting logs...");
            Console.WriteLine($"FMT Path: {tracefmtPath} ETL Path: {etlPath} LogOutput Path: {logOutputFile} tmfOutPut path: {tmfOutputDirectory}");
            string cmdTraceFmtPath = $"{tracefmtPath} {etlPath} -o {logOutputFile} -p {tmfOutputDirectory}";
            RunViaCmd(cmdTraceFmtPath);
            string sumFile = logOutputFile + ".sum";
            if(File.Exists(sumFile))
            {
                try
                {
                    File.Delete(sumFile);
                    Console.WriteLine($"Deleted Summary File: {sumFile}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not delete Summary File: {sumFile}");
                }
            }
            Console.WriteLine($"Logs extracted to: {logOutputFile}");
            try
            {
                Directory.Delete(tmfOutputDirectory, true);
                Console.WriteLine($"Deleted TMFS Directory: {tmfOutputDirectory}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to Delete TMFS Directory: {tmfOutputDirectory}");
            }
        }
        private static void RunViaCmd(string command)
        {
            Console.WriteLine($"Command: {command}");
            var processInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {command}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (var process = new Process())
            {
                process.StartInfo = processInfo;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new Exception(
                        $"Command failed with exit code {process.ExitCode}\nCommand: {command}\nError: {error}\nOutput: {output}");
                }
            }
        }

        //Method to enable/disable logging session
        private void OnEnableSessionClickCommand(object state)
        {
            if (!sessionRunning)
            {
                Console.WriteLine("Enabling session...");
                SetLogging(true, 1);
            }
            else
            {
                Console.WriteLine("Disabling session...");
                SetLogging(true, 0);
                LoggingDisabled = true;
            }
            UpdateSessionState();
            UpdateGlobalSetting(false);
        }

        private void onParseLogsCommand(object state)
        {
            if(LoggingDisabled)
            {
                string tracefmtPath = @"C:\Program Files (x86)\Qualcomm\QUD\DriverPackage\Qualcomm\Tools\tracefmt.exe";
                string tracefmtQuotedPath = $"\"{tracefmtPath}\"";
                string tracepdbPath = @"C:\Program Files (x86)\Qualcomm\QUD\DriverPackage\Qualcomm\Tools\tracepdb.exe";
                string tracepdbQuotedPath = $"\"{tracepdbPath}\"";
                string pdbPath = @"C:\Program Files (x86)\Qualcomm\QUD\DriverPackage\Qualcomm\fre\Windows10\";
                string etlPath = @"C:\QCDriverLog.etl";
                string TMFPath = @"C:\TMFS";
                string outputPath = Path.Combine(@"C:\", $"QCDriverLog_{Environment.MachineName}_QUDv{QDCFG.getQUDVersion()}_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}.txt");
                Console.WriteLine("Parsing Logs...");
                RunETLParsingFile(tracepdbQuotedPath, tracefmtQuotedPath, pdbPath, etlPath, TMFPath, outputPath);
                SetLogging(true, 2);
            }
            LoggingDisabled = false;
        }

        private void OnApplyEntryValueClickCommand(object param)
        {
            ClearConsole();
            char[] splitor = { ' ' };
            string[] arguments = NewEntryValue.Trim().Split(splitor, StringSplitOptions.RemoveEmptyEntries);
            int retCode = QDCFG.executeCommand(SelectedCommand.Value.Entry, 's', arguments, arguments.Length, !string.IsNullOrEmpty(SelectedDevice.DeviceName), SelectedDevice.DeviceName); // this will block
            if (SelectedCommand.Value.Entry == "QCDriverDebugMask")
            {
                UpdateSessionState();
                UpdateGlobalSetting(false);
                UpdateLoggingState(SelectedDevice);
            }
            MessageBox.Show(retCode == 0 ? "Success!" : "Failed! Error " + retCode);
        }

        private void OnFetchEntryValueClickCommand(object param)
        {
            uint value = 0;
            uint error = QDCFG.InspectEntry(SelectedDevice.DeviceName, SelectedCommand.Value.Entry, ref value);
            if (error == 0)
            {
                NewEntryValue = value.ToString("X");
            }
            else if (error == 2)
            {
                NewEntryValue = "";
                MessageBox.Show("Error reading value! Entry not initilized!");
            }
            else
            {
                NewEntryValue = "";
                MessageBox.Show("Error reading value! " + error);
            }
        }

        public void OnDeviceSelected(LogDevice dev)
        {
            SelectedDevice = dev;
            ExposedCommands.Clear();
            SelectedCommand = null;
            if (dev != null)
            {
                foreach (QDCFGCommand cmd in commandList)
                {
                    if (QDCFG.DeviceTypeCompatible(SelectedDevice.DeviceType, cmd.DevType))
                    {
                        ExposedCommands.Add(cmd);
                    }
                }
            }
        }

        public void OnMaskChecked(WPPConstantItem mask, bool isChecked)
        {
            if (mask != null)
            {
                if (isChecked)
                {
                    FlagVal = FlagVal | mask.Value;
                }
                else
                {
                    FlagVal = FlagVal ^ mask.Value;
                }
            }
        }

        public bool loggingDisabled = false;

        public bool LoggingDisabled
        {
            get { return loggingDisabled; }
            set
            {
                if (loggingDisabled != value)
                {
                    loggingDisabled = value;
                    NotifyPropertyChanged("LoggingDisabled");
                }
            }
        }
        public bool sessionRunning = false;
        public bool SessionRunning
        {
            get { return sessionRunning; }
            set
            {
                if (sessionRunning != value)
                {
                    sessionRunning = value;
                    NotifyPropertyChanged("SessionRunning");
                }
            }
        }

        private void UpdateSessionState()
        {
            if (QDCFG.InspectTracing() == 0)
            {
                // session is running
                SessionRunning = true;
            }
            else
            {
                // session is inactive
                SessionRunning = false;
            }
        }

        private uint UpdateLoggingState(LogDevice device)
        {
            uint errorCode = 0;
            if (device != null)
            {
                device.EnableLog = QDCFG.InspectLogging(device.DeviceName, ref errorCode);
            }
            return errorCode;
        }

        private void UpdateGlobalSetting(bool useDefault)
        {
            TracingConfig config = new TracingConfig();
            if (!useDefault && QDCFG.GetTracingConfig(ref config) == 0)
            {
                FlagVal = config.Flags;
                if (config.Level >= currMaxLevel)
                {
                    config.Level = defaultLevel;
                }
                Level = config.Level;
                FileSize = config.MaxFile.ToString();
            }
            else
            {
                FlagVal = defaultFlags;
                Level = defaultLevel;
                FileSize = defaultFileSize.ToString();
            }
        }

        public void PrintConsole(string message)
        {
            LogOutput += message;
        }

        public void ClearConsole()
        {
            LogOutput = "";
        }
    }

    public class SessionToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return "Disable Session";
            }
            else
            {
                return "Enable Session";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }

    public class LogParseToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return "Parse Logs";
            }
            else
            {
                return "Parsing";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }

    public class MaskIsCheckedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            uint value = (uint)values[0];
            uint refValue = System.Convert.ToUInt32(values[1] as string, 16);
            return (value & refValue) != 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;    // OneWay Binding Only
        }
    }
}
