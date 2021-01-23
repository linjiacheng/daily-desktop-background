using System;
using System.Runtime.InteropServices;
using DailyDesktopBackground.Helpers;
using ImageMagick;

namespace DailyDesktopBackground.Helper
{
    public class WallpaperHelper
    {

        public static void SetWallpaper(IDesktopWallpaper wallpaper, string monitorId, string imagePath, Uri uri)
        {
            using (var image = DownloadImage(uri))
            {
                image.Write(imagePath, MagickFormat.Jpg);
            }

            wallpaper.SetPosition(DesktopWallpaperPosition.Fill);
            wallpaper.SetWallpaper(monitorId, imagePath);
        }

        private static MagickImage DownloadImage(Uri uri)
        {
            using var stream = new System.Net.WebClient().OpenRead(uri.ToString());
            return new MagickImage(stream);
        }
    }
}
