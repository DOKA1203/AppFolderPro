using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using AppFolderPro.Databases;

namespace AppFolderPro.Icons;

public class IconGenerator
{
    private static readonly string LocalApplicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppFolderPro");
    private static readonly string BaseImagePath = Path.Combine(LocalApplicationData, "base.png");
    public static string GenerateIcon(int folderId)
    {
        var iconsDir = Path.Combine(LocalApplicationData, "icons", $"{folderId}");
        var filePaths = GetFilePathsFromDatabase(folderId);

        // Load base background image
        using var background = new Bitmap(BaseImagePath);


        // Draw icons on background
        DrawIconsOnBackground(background, filePaths);

        // Remove old .ico files
        if (Directory.Exists(iconsDir))
        {
            foreach (var oldIco in Directory.GetFiles(iconsDir, "*.ico"))
                File.Delete(oldIco);
        }
        else
        {
            Directory.CreateDirectory(iconsDir);
        }

        // Generate a random name and save as ICO
        var ranName = GetRandomString(15);
        var icoPath = Path.Combine(iconsDir, ranName + ".ico");
        SaveAsIco(background, icoPath, 256);
        return ranName;
    }
    /// <summary>
    /// Retrieves file paths from the database for the given folder ID.
    /// </summary>
    private static List<string> GetFilePathsFromDatabase(int folderId)
    {
        var manager = new FolderDatabaseManager();
        var folder = manager.GetFolderById(folderId);

        if (folder != null && folder.Files.Any())
        {
            return folder.Files.Select(file => file.Icon).ToList();
        }
        
        Console.WriteLine($"Folder with ID {folderId} not found or has no files.");
        return new List<string>();
    }

    /// <summary>
    /// Draws up to 4 icons onto the background image at predefined positions.
    /// </summary>
    static void DrawIconsOnBackground(Bitmap background, List<string> filePaths)
    {
        var k = 110; // Icon size
        var n = 10;  // Padding
        var positions = new Point[]
        {
            new (n, n),
            new (128 + n, n),
            new (n, 128 + n),
            new (128 + n, 128 + n)
        };

        using var g = Graphics.FromImage(background);
        g.CompositingMode = CompositingMode.SourceOver;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;

        var count = Math.Min(filePaths.Count, 4);
        for (var i = 0; i < count; i++)
        {
            var iconPath = filePaths[i];
            using var iconImg = new Bitmap(iconPath);
            using var resized = new Bitmap(iconImg, new Size(k, k));
            g.DrawImage(resized, positions[i]);
        }
    }

    /// <summary>
    /// Generates a random lowercase string of the specified length.
    /// </summary>
    private static string GetRandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz";
        var rand = new Random();
        var sb = new StringBuilder(length);
        for (var i = 0; i < length; i++)
            sb.Append(chars[rand.Next(chars.Length)]);
        return sb.ToString();
    }

    /// <summary>
    /// Saves the image as a single-size ICO (256x256) by embedding a PNG in the ICO file.
    /// </summary>
    private static void SaveAsIco(Bitmap image, string filePath, int size)
    {
        using var resized = new Bitmap(image, new Size(size, size));
        using var pngStream = new MemoryStream();
        resized.Save(pngStream, ImageFormat.Png);
        var pngData = pngStream.ToArray();

        using var fs = new FileStream(filePath, FileMode.Create);
        using var bw = new BinaryWriter(fs);

        // ICONDIR
        bw.Write((short)0);      // Reserved
        bw.Write((short)1);      // Type = 1 (ICO)
        bw.Write((short)1);      // Count = 1

        // ICONDIRENTRY
        bw.Write((byte)size);    // Width
        bw.Write((byte)size);    // Height
        bw.Write((byte)0);       // No color palette
        bw.Write((byte)0);       // Reserved
        bw.Write((short)0);      // Color planes
        bw.Write((short)32);     // Bits per pixel
        bw.Write(pngData.Length);// Image size
        bw.Write(22);            // Offset: 6 bytes ICONDIR + 16 bytes ICONDIRENTRY = 22 bytes

        // Write image data (PNG)
        bw.Write(pngData);
    }
}