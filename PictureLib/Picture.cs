using Sdcb.LibRaw;
using Sdcb.LibRaw.Natives;
using System.Drawing;
using System.Drawing.Imaging;

namespace PictureLib
{
    /// <summary>
    /// Represents an image file and provides functionality to open and extract metadata.
    /// Supports various RAW formats (ARW, CR2, DNG, etc.) using LibRaw and JPEG formats using System.Drawing.
    /// </summary>
    public sealed class Picture : IDisposable
    {
        private RawContext? _rawContext;
        private Bitmap? _bitmap;

        public Picture(string path) 
        {
            Path = path;
        }

        public string Path { get; }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public DateTime CaptureDate { get; private set; }

        public void Open()
        {
            if (string.IsNullOrEmpty(Path) || !File.Exists(Path))
            {
                throw new FileNotFoundException($"Image file not found: {Path}");
            }

            try
            {
                string extension = System.IO.Path.GetExtension(Path).ToLowerInvariant();

                if (extension == ".jpg" || extension == ".jpeg")
                {
                    OpenJpeg();
                }
                else
                {
                    OpenRaw();
                }
            }
            catch (Exception)
            {
                Close();
                throw;
            }
        }

        private void OpenRaw()
        {
            _rawContext = RawContext.OpenFile(Path);

            LibRawImageOtherParams otherParams = _rawContext.ImageOtherParams;

            CaptureDate = otherParams.Timestamp > 0 ? UnixTimeStampToDateTime(otherParams.Timestamp) : DateTime.MinValue;

            Width = _rawContext.Width;
            Height = _rawContext.Height;
        }

        private void OpenJpeg()
        {
            _bitmap = new Bitmap(Path);

            Width = _bitmap.Width;
            Height = _bitmap.Height;

            // Try to extract EXIF date from JPEG
            CaptureDate = ExtractExifDate(_bitmap);
        }

        private static DateTime ExtractExifDate(Bitmap bitmap)
        {
            try
            {
                // EXIF tag for DateTime is 0x0132 (306 in decimal)
                const int exifDateTimeTag = 0x0132;

                if (bitmap.PropertyIdList.Contains(exifDateTimeTag))
                {
                    PropertyItem? exifDate = bitmap.GetPropertyItem(exifDateTimeTag);
                    if (exifDate != null)
                    {
                        string dateString = System.Text.Encoding.ASCII.GetString(exifDate.Value).Trim('\0');
                        if (DateTime.TryParseExact(dateString, "yyyy:MM:dd HH:mm:ss", 
                            System.Globalization.CultureInfo.InvariantCulture, 
                            System.Globalization.DateTimeStyles.None, 
                            out DateTime result))
                        {
                            return result;
                        }
                    }
                }
            }
            catch
            {
                // If EXIF extraction fails, return MinValue
            }

            return DateTime.MinValue;
        }

        public void Close()
        {
            _rawContext?.Dispose();
            _rawContext = null;
            _bitmap?.Dispose();
            _bitmap = null;
            Width = 0;
            Height = 0;
            CaptureDate = DateTime.MinValue;
        }

        public string GetImageInfo()
        {
            if (_rawContext == null && _bitmap == null)
            {
                return "Image not loaded";
            }

            var dateString = CaptureDate != DateTime.MinValue 
                ? CaptureDate.ToString("yyyy-MM-dd HH:mm:ss") 
                : "Unknown date";

            return $"{Path}: {Width}x{Height} - Date: {dateString}";
        }

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
