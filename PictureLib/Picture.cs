using Sdcb.LibRaw;

namespace PictureLib
{
    /// <summary>
    /// Represents a RAW image file and provides functionality to open and extract metadata using LibRaw.
    /// Supports various RAW formats (ARW, CR2, DNG, etc.) from different camera models.
    /// </summary>
    public sealed class Picture : IDisposable
    {
        private RawContext? _rawContext;
        private ProcessedImage? _processedImage;
        private bool _disposed;

        public Picture(string path) 
        {
            Path = path;
        }

        public string Path { get; }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Colors { get; private set; }
        public string? CameraModel { get; private set; }
        public bool IsLoaded { get; private set; }

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
                var imageParams = _rawContext.ImageParams;
                CameraModel = imageParams.Model;
                Colors = imageParams.Colors;

                // Process the image to get dimensions
                _rawContext.Unpack();
                _rawContext.DcrawProcess();
                _processedImage = _rawContext.MakeDcrawMemoryImage();

                Width = _processedImage.Width;
                Height = _processedImage.Height;
                IsLoaded = true;
            }
            catch (Exception)
            {
                Close();
                throw;
            }
        }

        public void Close()
        {
            _processedImage?.Dispose();
            _processedImage = null;
            _rawContext?.Dispose();
            _rawContext = null;
            IsLoaded = false;
            Width = 0;
            Height = 0;
            Colors = 0;
            CameraModel = null;
        }

        public string GetImageInfo()
        {
            if (!IsLoaded)
            {
                return "Image not loaded";
            }

            return $"{Path}: {Width}x{Height} ({Colors} colors) - Camera: {CameraModel}";
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Close();
            _disposed = true;
        }
    }
}
