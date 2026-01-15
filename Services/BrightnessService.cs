using System.Management;

namespace LapKeys.Services;

public static class BrightnessService
{
    private static ManagementScope? _scope;
    private static bool _isInitialized;
    private static bool _isSupported = true;
    private static int _lastSetBrightness = -1;

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
                int brightness = Convert.ToInt32(obj["CurrentBrightness"]);
                _lastSetBrightness = brightness;
                return brightness;
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
                _lastSetBrightness = brightness;
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
        int current = _lastSetBrightness >= 0 ? _lastSetBrightness : GetBrightness();
        if (current < 0) return -1;

        int newLevel = Math.Min(100, current + step);
        if (SetBrightness(newLevel))
            return newLevel;
        return -1;
    }

    public static int DecreaseBrightness(int step = 10)
    {
        int current = _lastSetBrightness >= 0 ? _lastSetBrightness : GetBrightness();
        if (current < 0) return -1;

        int newLevel = Math.Max(0, current - step);
        if (SetBrightness(newLevel))
            return newLevel;
        return -1;
    }
}
