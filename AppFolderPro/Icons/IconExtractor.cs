namespace AppFolderPro.Icons;

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

public static class IconExtractor
{
    [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
    private static extern int SHGetFileInfo(
        string pszPath,
        uint dwFileAttributes,
        out SHFILEINFO psfi,
        uint cbFileInfo,
        uint uFlags);

    [StructLayout(LayoutKind.Sequential)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    private const uint SHGFI_ICON = 0x000000100;
    private const uint SHGFI_LARGEICON = 0x000000000;

    public static BitmapSource GetIcon(string filePath)
    {
        
        
        // 1. SHGetFileInfo 방식 시도
        SHFILEINFO shinfo = new SHFILEINFO();
        int result = SHGetFileInfo(filePath, 0, out shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);

        if (result != 0 && shinfo.hIcon != IntPtr.Zero)
        {
            try
            {
                var icon = System.Drawing.Icon.FromHandle(shinfo.hIcon);
                return IconToBitmapSource(icon);
            }
            finally
            {
                // 핸들 해제
                DestroyIcon(shinfo.hIcon);
            }
        }

        // 2. SHGetFileInfo 실패 시, Icon.ExtractAssociatedIcon 방식 사용
        Console.WriteLine("SHGetFileInfo 실패. ExtractAssociatedIcon 방식 시도.");

        if (filePath.EndsWith(".lnk"))
        {
            // 바로 가기 파일 처리
            var shell = new IWshRuntimeLibrary.WshShell();
            var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(filePath);

            // TargetPath에서 아이콘 추출
            return IconToBitmapSource(Icon.ExtractAssociatedIcon(shortcut.TargetPath));
        }
        else if (filePath.EndsWith(".url"))
        {
            // URL 파일 처리
            return IconToBitmapSource(Icon.ExtractAssociatedIcon(filePath));
        }

        throw new FileNotFoundException("아이콘 추출 실패: " + filePath);
    }

    public static void SaveIconAsPng(string filePath, string savePath)
    {
        
        
        BitmapSource source = GetIcon(filePath);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(source));
        using (var stream = File.Create(savePath))
        {
            encoder.Save(stream);
        }
    }

    private static BitmapSource IconToBitmapSource(Icon icon)
    {
        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
    }

    [DllImport("User32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);
}
