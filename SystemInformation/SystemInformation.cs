// ***************
// requires reference for System.Management.dll 
// ***************
using Microsoft.Win32;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Collections.Generic;

namespace SystemInformation
{
    public class SystemInformation
    {
        public static int AverageProcessorLoad
        {
            get
            {
                if (_monitoringLevel != MonitoringLevels.Paused)
                    return _averageProcessorLoad;
                return GetCurrentProcessorLoad ();
            }
        }

        public static int CurrentProcessorLoad
        {
            get
            {
                if (_monitoringLevel != MonitoringLevels.Paused)
                    return _currentProcessorLoad;
                return GetCurrentProcessorLoad ();
            }
        }

        public static long CurrentFreeMemory
        {
            get
            {
                if (_monitoringLevel != MonitoringLevels.Paused && _currentFreeMemory > 0)
                    return _currentFreeMemory;
                return GetCurrentFreeMemory ();
            }
        }

        public static long TotalMemory
        {
            get { loadInfo (); return _totalMemory; }
        }

        public static long ApplicationTotalMemory
        {
            get
            {
                if (_monitoringLevel != MonitoringLevels.Paused && _currentFreeMemory > 0)
                    return _applicationTotalMemory;
                return GetApplicationTotalMemory ();
            }
        }

        public static string OsName
        {
            get { loadInfo (); return _osName; }
        }

        public static int ProcessId
        {
            get { loadInfo (); return _processId; }
        }

        public static int LogicalProcessors
        {
            get { loadInfo (); return _logicalProcessors; }
        }

        public static int ProcessorCores
        {
            get { loadInfo (); return _processorCores; }
        }

        public static int Processors
        {
            get { loadInfo (); return _processors; }
        }

        public static string SystemName
        {
            get { loadInfo (); return _systemName; }
        }

        public static string ProcessorName
        {
            get { loadInfo (); return _processorName; }
        }

        public static string CurrentPublicIp
        {
            get { loadInfo (); return _currentPublicIp; }
        }

        public static string AwsInstanceType
        {
            get { loadInfo (); return _awsInstanceType; }
        }

        public static string AwsInstanceId
        {
            get { loadInfo (); return _awsInstanceId; }
        }

        public static string AwsAmiId
        {
            get { loadInfo (); return _awsAmiId; }
        }

        public static string AwsPublicIp
        {
            get { loadInfo (); return _awsPublicIp; }
        }

        public static bool IsRunningOnAWS
        {
            get { loadInfo (); return !String.IsNullOrEmpty (_awsInstanceId); }
        }

        public static string[] DiskInfo
        {
            get { loadInfo (); return _disks; }
        }

        private static volatile bool _loaded = false;

        private static string _processorName = null;
        private static string _systemName = null;
        private static string _osName = null;
        private static int _processors = 0;
        private static int _processorCores = 0;
        private static int _logicalProcessors = 0;
        private static int _processId = 0;
        private static long _totalMemory = 0;
        private static int _currentProcessorLoad = 0;
        private static int _averageProcessorLoad = 0;
        private static long _currentFreeMemory = 0;
        private static long _applicationTotalMemory = 0;
        private static string _currentPublicIp;
        private static string _awsInstanceType;
        private static string _awsInstanceId;
        private static string _awsAmiId;
        private static string _awsPublicIp;
        private static string[] _disks;

        static PerformanceCounter cpuCounter;
static PerformanceCounter ramCounter;



public static string getCurrentCpuUsage(){
            return cpuCounter.NextValue()+"%";
}

public static string getAvailableRAM(){
            return ramCounter.NextValue()+"Mb";
} 

        public static void loadInfo ()
        {
            if (!_loaded)
            {
                try
                {

                    //new PerformanceCounter ("Memory", "Available MBytes", String.Empty, Environment.MachineName).NextValue ();

                    cpuCounter = new PerformanceCounter ("Processor", "% Processor Time", "_Total", Environment.MachineName);
                    ramCounter = new PerformanceCounter ("Memory", "Available MBytes", true);


                    _processId = Process.GetCurrentProcess ().Id;

                    _osName = Environment.OSVersion.Platform.ToString ();
                    _systemName = Environment.MachineName;
                    //_processorName = "":
                    //_processorCores = Environment.ProcessorCount;
                    _logicalProcessors = Environment.ProcessorCount;


                    //// System & CPUs
                    //using (var query = new ManagementObjectSearcher ("SELECT name, SystemName, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor"))
                    //{
                    //    foreach (var info in query.Get ())
                    //    {
                    //        _processorName = info["name"].ToString ().Replace ("CPU           ", "CPU ").Replace ("  ", " ");
                    //        _systemName = info["SystemName"].ToString ();
                    //        _processorCores = Convert.ToInt32 (info["NumberOfCores"]);
                    //        _logicalProcessors = Convert.ToInt32 (info["NumberOfLogicalProcessors"]);
                    //    }
                    //}

                    //// CPUs
                    //using (var query = new ManagementObjectSearcher ("SELECT NumberOfProcessors FROM Win32_ComputerSystem"))
                    //{
                    //    foreach (var info in query.Get ())
                    //    {
                    //        _processors = Convert.ToInt32 (info["NumberOfProcessors"]);
                    //    }
                    //}

                    // Memory
                    //using (var query = new ManagementObjectSearcher ("SELECT Caption, TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                    //{
                    //    foreach (var info in query.Get ())
                    //    {
                    //        _totalMemory = Convert.ToInt64 (info["TotalVisibleMemorySize"]);
                    //        _osName = info["Caption"].ToString ();
                    //    }
                    //}

                    //// Disks
                    //using (var query = new ManagementObjectSearcher ("SELECT VolumeName, Size, FreeSpace FROM Win32_LogicalDisk"))
                    //{
                    //    var tmp = new List<string> ();
                    //    foreach (var info in query.Get ())
                    //    {
                    //        var sz = info["Size"];
                    //        var fs = info["FreeSpace"];
                    //        tmp.Add (info["VolumeName"] + " " + sz + " / " + fs);
                    //    }
                    //    _disks = tmp.ToArray ();
                    //}

                    var tmp = new List<string> ();
                    foreach (var d in System.IO.DriveInfo.GetDrives ())
                    {
                        if (!d.IsReady || d.DriveType == System.IO.DriveType.CDRom)
                            continue;
                        tmp.Add (String.Format ("{0} ({1}) {2} / {3} ({4}%)", d.Name, d.VolumeLabel ?? "", FormatAsGb (d.AvailableFreeSpace), FormatAsGb (d.TotalSize), (d.AvailableFreeSpace / d.TotalSize) * 100).Replace ("()", ""));
                    }
                    _disks = tmp.ToArray ();

                    ReloadNetworkInfo ();
                }
                catch
                {
                    _processorName = "NotFound";
                    _systemName = "NotFound";
                }
                _loaded = true;

                ReloadNetworkInfo ();
            }
        }

        public static bool IsLinux ()
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);            
        }

        public static string GetLinuxOsInfo ()
        {
            var info = System.IO.File.ReadAllText ("/proc/version");
            var ix = info.LastIndexOf ('#');
            if (ix > 0)
                info = info.Substring (0, ix);
            return info;
        }

        public static void GetLinuxCPUInfo ()
        {            
            string marker = null;
            foreach (var info in System.IO.File.ReadLines ("/proc/cpuinfo"))
            {
                if (!String.IsNullOrWhiteSpace (info))
                {
                    var data = info.Split (':');
                    if (data.Length < 2)
                        continue;
                    data[0] = data[0].Trim ();
                    switch (data[0])
                    {
                        case "processor":
                            {
                                if (marker != null)
                                    return;
                                marker = data[1].Trim ();
                            }
                            break;
                        case "model name":
                            _processorName = data[1].Trim ();
                            break;
                        case "physical id":
                            {
                                Int32.TryParse (data[1].Trim (), out _processors);
                                _processors++;                                
                            }
                            break;
                        case "cpu cores":
                            Int32.TryParse (data[1].Trim (), out _processorCores);
                            break;
                        case "siblings":
                            Int32.TryParse (data[1].Trim (), out _logicalProcessors);
                            break;
                    }
                }
            }            
        }

        public static void GetLinuxMemInfo ()
        {
            // http://www.redhat.com/advice/tips/meminfo.html/
            foreach (var info in System.IO.File.ReadLines ("/proc/meminfo"))
            {
                if (!String.IsNullOrWhiteSpace (info) && (info.IndexOf ("Mem", 0, 4, StringComparison.Ordinal) == 0 || info.IndexOf ("Swap", 0, 5, StringComparison.Ordinal) == 0))
                {
                    var data = info.Split (':');
                    if (data.Length < 2)
                        continue;
                    data[0] = data[0].Trim ();
                    switch (data[0])
                    {
                        case "MemTotal":
                            _totalMemory = ParseBytes (data[1].Trim ());
                            break;
                        case "MemFree":
                            _currentFreeMemory = ParseBytes (data[1].Trim ());
                            break;
                        case "SwapTotal":                            
                            break;
                        case "SwapFree":                            
                            break;
                    }
                }
            }
        }

        public static void GetLinuxCPUTime (string pId)
        {
            // http://stackoverflow.com/questions/29092242/get-processor-time-in-mono
            //https://web.archive.org/web/20130302063336/http://www.lindevdoc.org/wiki//proc/pid/stat
            //http://stackoverflow.com/questions/16726779/how-do-i-get-the-total-cpu-usage-of-an-application-from-proc-pid-stat
            //http://unix.stackexchange.com/questions/58539/top-and-ps-not-showing-the-same-cpu-result
            foreach (var info in System.IO.File.ReadLines ("/proc/" + pId + "/stat"))
            {
                if (!String.IsNullOrWhiteSpace (info))
                {
                    var data = info.Split (' ');
                    if (data.Length < 10)
                        continue;
                    
                }
            }
        }

        public static string FormatAsGb (long number)
        {
            return (number / (1024 * 1024 * 1024)).ToString ("N1") + " Gb";
        }

        public static long ParseBytes (string number)
        {
            if (String.IsNullOrWhiteSpace (number))
                return 0;
            int multiplier = 0;
            number = number.Trim ();
            var data = number.Split (' ');
            if (data.Length > 0)
            {
                if (data[1].Equals ("kb", StringComparison.OrdinalIgnoreCase))
                    multiplier = 1024;
                else if (data[1].Equals ("mb", StringComparison.OrdinalIgnoreCase))
                    multiplier = 1024 * 1024;
                else if (data[1].Equals ("gb", StringComparison.OrdinalIgnoreCase))
                    multiplier = 1024 * 1024 * 1024;
            }
            long bytesValue;
            Int64.TryParse (data[0], out bytesValue);
            if (multiplier > 0)
                bytesValue = bytesValue * multiplier;
            return bytesValue;
        }

        public static void ReloadNetworkInfo ()
        {
            _currentPublicIp = getCurrentIpAddress ();
            getAmazonWebServicesInfo ();
        }

        private static string getCurrentIpAddress ()
        {
            try
            {
                return System.Net.Dns.GetHostAddresses (System.Net.Dns.GetHostName ())
                           .Where (i => i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                           .Select (i => i.ToString ()).FirstOrDefault ();
            }
            catch
            {
                return "";
            }
        }

        private static void getAmazonWebServicesInfo ()
        {
            try
            {
                using (var web = new System.Net.WebClient ())
                {
                    _awsInstanceType = web.DownloadString ("http://169.254.169.254/latest/meta-data/instance-type");
                    _awsInstanceId = web.DownloadString ("http://169.254.169.254/latest/meta-data/instance-id");
                    _awsAmiId = web.DownloadString ("http://169.254.169.254/latest/meta-data/ami-id");
                    _awsPublicIp = web.DownloadString ("http://169.254.169.254/latest/meta-data/public-ipv4");                    
                }
                if (!String.IsNullOrWhiteSpace (_awsPublicIp))
                    _currentPublicIp = _awsPublicIp;
            }
            catch
            {
                _awsInstanceType = null;
                _awsInstanceId = null;
            }
        }

        public static long GetApplicationTotalMemory ()
        {
            /// NOTE: 
            /// PrivateMemorySize64 => Total app memory including memory that was swapped to disk by Windows
            /// WorkingSet64 => Total in RAM (without data that was swapped to disk by Windows)
            using (var p = Process.GetCurrentProcess ())
            {
                return p.PrivateMemorySize64;
            }
        }

        public static int GetCurrentProcessorLoad ()
        {
            try
            {
                int load = 0;
                int count = 0;
                var mosProcessor = new s ("SELECT LoadPercentage FROM Win32_Processor");
                foreach (var moProcessor in mosProcessor.Get ())
                {
                    count++;
                    load += Convert.ToInt32 (moProcessor["LoadPercentage"]);
                }
                return load / count;
            }
            catch
            {
                return 0;
            }
        }

        private static long GetCurrentFreeMemory ()
        {
            try
            {
                var mosProcessor = new ManagementObjectSearcher ("SELECT FreePhysicalMemory FROM Win32_OperatingSystem");
                foreach (var moProcessor in mosProcessor.Get ())
                {
                    return Convert.ToInt64 (moProcessor["FreePhysicalMemory"]);
                }
            }
            catch
            {
            }
            return 0;
        }

        private static System.Threading.Timer m_runningTask = null;
        private static MonitoringLevels _monitoringLevel = MonitoringLevels.Paused;
        private static int _historyLengthMinutes = 2;

        public enum MonitoringLevels { Paused = 0, VeryHigh = 2, High = 5, MidHigh = 10, Normal = 20, MidLow = 40, Low = 60 }

        public static void StartMonitoring (MonitoringLevels level, int historyLengthMinutes = 2)
        {
            // set timer            
            if (level == MonitoringLevels.Paused)
            {
                StopMonitoring ();
            }
            else if (m_runningTask != null)
            {
                if (_monitoringLevel != level)
                {
                    m_runningTask.Change (0, (int)level * 1000);
                }
            }
            else
            {
                loadInfo ();
                m_runningTask = new System.Threading.Timer (UpdateEvent, null, 0, (int)level * 1000);
            }
            if (historyLengthMinutes < 1)
                historyLengthMinutes = 1;
            _historyLengthMinutes = historyLengthMinutes;
            _monitoringLevel = level;
        }

        public static void StopMonitoring ()
        {
            if (m_runningTask != null)
                m_runningTask.Dispose ();
            m_runningTask = null;
        }

        static ConcurrentQueue<int> processorLoadHistory = new ConcurrentQueue<int> ();

        private static void UpdateEvent (object state)
        {
            int load = GetCurrentProcessorLoad ();
            processorLoadHistory.Enqueue (load);
            // calculate average value
            int avgLoad = 0;
            foreach (int v in processorLoadHistory)
            {
                avgLoad += v;
            }
            avgLoad = avgLoad / processorLoadHistory.Count;
            // trim queue size (+/- 2 minute of values)
            int sz = (_historyLengthMinutes * 60) / ((int)_monitoringLevel > 0 ? (int)_monitoringLevel : 1);
            if (sz < 2)
                sz = 2;
            while (processorLoadHistory.Count > sz)
            {
                processorLoadHistory.TryDequeue (out avgLoad);
            }
            // update values
            Interlocked.Exchange (ref _currentProcessorLoad, load);
            Interlocked.Exchange (ref _averageProcessorLoad, avgLoad);
            Interlocked.Exchange (ref _currentFreeMemory, GetCurrentFreeMemory ());
            Interlocked.Exchange (ref _applicationTotalMemory, GetApplicationTotalMemory ());
        }        
    }
}