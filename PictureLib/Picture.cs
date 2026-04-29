using Sdcb.LibRaw;
using Sdcb.LibRaw.Natives;

namespace PictureLib
{
    /// <summary>
    /// Represents a RAW image file and provides functionality to open and extract metadata using LibRaw.
    /// Supports various RAW formats (ARW, CR2, DNG, etc.) from different camera models.
    /// </summary>
    public sealed class Picture : IDisposable
    {
        private RawContext? _rawContext;

        public Picture(string path) 
        {
            Path = path;
        }

        public string Path { get; }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Colors { get; private set; }
        public string? CameraModel { get; private set; }
        public DateTime CaptureDate { get; private set; }

        public void Open()
        {
            if (string.IsNullOrEmpty(Path) || !File.Exists(Path))
            {
                throw new FileNotFoundException($"Image file not found: {Path}");
            }

            try
            {
                _rawContext = RawContext.OpenFile(Path);

                // Get basic metadata
                LibRawImageParams imageParams = _rawContext.ImageParams;
                LibRawImageOtherParams otherParams = _rawContext.ImageOtherParams;

                CameraModel = imageParams.Model;
                Colors = imageParams.Colors;

                // Extract capture date from Unix timestamp
                // Timestamp is seconds since epoch (1970-01-01 00:00:00 UTC)
                if (otherParams.Timestamp > 0)
                {
                    CaptureDate = UnixTimeStampToDateTime(otherParams.Timestamp);
                }
                else
                {
                    CaptureDate = DateTime.MinValue;
                }

                Width = _rawContext.Width;
                Height = _rawContext.Height;
            }
            catch (Exception)
            {
                Close();
                throw;
            }
        }

        public void Close()
        {
            _rawContext?.Dispose();
            _rawContext = null;
            Width = 0;
            Height = 0;
            Colors = 0;
            CameraModel = null;
            CaptureDate = DateTime.MinValue;
        }

        public string GetImageInfo()
        {
            if (_rawContext == null)
            {
                return "Image not loaded";
            }

            var dateString = CaptureDate != DateTime.MinValue 
                ? CaptureDate.ToString("yyyy-MM-dd HH:mm:ss") 
                : "Unknown date";

            return $"{Path}: {Width}x{Height} ({Colors} colors) - Camera: {CameraModel} - Date: {dateString}";
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
