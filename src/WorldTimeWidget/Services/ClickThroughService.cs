using System.Runtime.InteropServices;

namespace WorldTimeWidget.Services;

/// <summary>
/// Включает/выключает расширенный стиль окна <c>WS_EX_TRANSPARENT</c> на переданном <c>hwnd</c> —
/// клики мышью "проваливаются" сквозь окно к тому, что находится под ним. Используется для
/// настройки <see cref="Models.AppSettings.IsClickThrough"/> (см. spec.md, доработка v1.2).
/// Окно уже создано с <c>AllowsTransparency="True"</c> (WPF сам проставляет
/// <c>WS_EX_LAYERED</c>) — здесь только добавляется/убирается один бит поверх текущих
/// extended styles, без перезаписи остальных.
/// </summary>
public static class ClickThroughService
{
    private const int GWL_EXSTYLE = -20;
    private const long WS_EX_TRANSPARENT = 0x00000020L;

    public static void SetClickThrough(IntPtr hwnd, bool enabled)
    {
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        var exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
        var newExStyle = enabled ? exStyle | WS_EX_TRANSPARENT : exStyle & ~WS_EX_TRANSPARENT;

        if (newExStyle != exStyle)
        {
            SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(newExStyle));
        }
    }

    // GetWindowLongPtr/SetWindowLongPtr существуют только в 64-битном user32.dll — на 32-битных
    // системах нужно звать GetWindowLong/SetWindowLong. Выбираем по разрядности процесса.
    private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex) =>
        IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : GetWindowLong32(hWnd, nIndex);

    private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong) =>
        IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : SetWindowLong32(hWnd, nIndex, dwNewLong);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
}
