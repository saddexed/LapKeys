using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using LapKeys.Models;
using LapKeys.Native;

namespace LapKeys.Services;

public class HotkeyService : IDisposable
{
    private readonly Dictionary<int, HotkeyBinding> _registeredHotkeys = new();
    private IntPtr _windowHandle;
    private HwndSource? _hwndSource;
    private bool _disposed;

    public event EventHandler<HotkeyBinding>? HotkeyPressed;

    public void Initialize(Window window)
    {
        _windowHandle = new WindowInteropHelper(window).Handle;
        _hwndSource = HwndSource.FromHwnd(_windowHandle);
        _hwndSource?.AddHook(WndProc);
    }

    public bool RegisterHotkey(HotkeyBinding binding)
    {
        if (_windowHandle == IntPtr.Zero)
            return false;

        uint modifiers = ConvertModifiers(binding.Modifiers);
        uint vk = (uint)KeyInterop.VirtualKeyFromKey(binding.Key);

        if (NativeMethods.RegisterHotKey(_windowHandle, binding.Id, modifiers | NativeMethods.MOD_NOREPEAT, vk))
        {
            _registeredHotkeys[binding.Id] = binding;
            return true;
        }

        return false;
    }

    public bool UnregisterHotkey(int id)
    {
        if (_windowHandle == IntPtr.Zero)
            return false;

        if (NativeMethods.UnregisterHotKey(_windowHandle, id))
        {
            _registeredHotkeys.Remove(id);
            return true;
        }

        return false;
    }

    public void UnregisterAllHotkeys()
    {
        foreach (var id in _registeredHotkeys.Keys.ToList())
        {
            UnregisterHotkey(id);
        }
    }

    public bool UpdateHotkey(HotkeyBinding newBinding)
    {
        if (_registeredHotkeys.ContainsKey(newBinding.Id))
        {
            UnregisterHotkey(newBinding.Id);
        }

        return RegisterHotkey(newBinding);
    }

    public IReadOnlyList<HotkeyBinding> GetRegisteredHotkeys()
    {
        return _registeredHotkeys.Values.ToList();
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            if (_registeredHotkeys.TryGetValue(id, out var binding))
            {
                HotkeyPressed?.Invoke(this, binding);
                handled = true;
            }
        }

        return IntPtr.Zero;
    }

    private static uint ConvertModifiers(ModifierKeys modifiers)
    {
        uint result = 0;

        if (modifiers.HasFlag(ModifierKeys.Alt))
            result |= NativeMethods.MOD_ALT;
        if (modifiers.HasFlag(ModifierKeys.Control))
            result |= NativeMethods.MOD_CONTROL;
        if (modifiers.HasFlag(ModifierKeys.Shift))
            result |= NativeMethods.MOD_SHIFT;
        if (modifiers.HasFlag(ModifierKeys.Windows))
            result |= NativeMethods.MOD_WIN;

        return result;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        UnregisterAllHotkeys();
        _hwndSource?.RemoveHook(WndProc);
        _hwndSource?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
