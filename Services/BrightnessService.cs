using System.Management;

namespace LapKeys.Services;

public static class BrightnessService
{
    private static ManagementScope? _scope;
    private static ManagementEventWatcher? _brightnessWatcher;
    private static bool _isInitialized;
    private static bool _isSupported = true;

    public static event Action<int>? BrightnessChanged;

    public static bool IsSupported => _isSupported;

    private static void Initialize()
    {
        if (_isInitialized) return;

        try
        {
            _scope = new ManagementScope("root\\WMI");
            _scope.Connect();
            _isInitialized = true;
        }
        catch
        {
            _isSupported = false;
            _isInitialized = true;
        }
    }

    public static void StartWatching()
    {
        Initialize();
        if (!_isSupported || _brightnessWatcher != null) return;

        try
        {
            var query = new WqlEventQuery("SELECT * FROM WmiMonitorBrightnessEvent");
            _brightnessWatcher = new ManagementEventWatcher(_scope, query);
            _brightnessWatcher.EventArrived += OnBrightnessChanged;
            _brightnessWatcher.Start();
        }
        catch
        {
        }
    }

    public static void StopWatching()
    {
        if (_brightnessWatcher != null)
        {
            _brightnessWatcher.Stop();
            _brightnessWatcher.EventArrived -= OnBrightnessChanged;
            _brightnessWatcher.Dispose();
            _brightnessWatcher = null;
        }
    }

    private static void OnBrightnessChanged(object sender, EventArrivedEventArgs e)
    {
        try
        {
            int brightness = Convert.ToInt32(e.NewEvent["Brightness"]);
            BrightnessChanged?.Invoke(brightness);
        }
        catch
        {
        }
    }

    public static int GetBrightness()
    {
        Initialize();
        if (!_isSupported) return -1;

        try
        {
            using var searcher = new ManagementObjectSearcher(_scope, 
                new ObjectQuery("SELECT CurrentBrightness FROM WmiMonitorBrightness"));
            
            foreach (ManagementObject obj in searcher.Get())
            {
                return Convert.ToInt32(obj["CurrentBrightness"]);
            }
        }
        catch
        {
            _isSupported = false;
        }

        return -1;
    }

    public static int[] GetBrightnessLevels()
    {
        Initialize();
        if (!_isSupported) return Array.Empty<int>();

        try
        {
            using var searcher = new ManagementObjectSearcher(_scope,
                new ObjectQuery("SELECT Level FROM WmiMonitorBrightness"));

            foreach (ManagementObject obj in searcher.Get())
            {
                var levels = obj["Level"] as byte[];
                if (levels != null)
                {
                    return levels.Select(b => (int)b).ToArray();
                }
            }
        }
        catch
        {
        }

        return Enumerable.Range(0, 101).ToArray();
    }

    public static bool SetBrightness(int brightness)
    {
        Initialize();
        if (!_isSupported) return false;

        brightness = Math.Clamp(brightness, 0, 100);

        try
        {
            using var searcher = new ManagementObjectSearcher(_scope,
                new ObjectQuery("SELECT * FROM WmiMonitorBrightnessMethods"));

            foreach (ManagementObject obj in searcher.Get())
            {
                obj.InvokeMethod("WmiSetBrightness", new object[] { 1, brightness });
                return true;
            }
        }
        catch
        {
            _isSupported = false;
        }

        return false;
    }

    public static int IncreaseBrightness(int step = 10)
    {
        int current = GetBrightness();
        if (current < 0) return -1;

        int newLevel = Math.Min(100, current + step);
        if (SetBrightness(newLevel))
            return newLevel;
        return -1;
    }

    public static int DecreaseBrightness(int step = 10)
    {
        int current = GetBrightness();
        if (current < 0) return -1;

        int newLevel = Math.Max(0, current - step);
        if (SetBrightness(newLevel))
            return newLevel;
        return -1;
    }
}
