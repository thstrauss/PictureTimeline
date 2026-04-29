
using PictureLib;

namespace PictureLibTests
{
    /// <summary>
    /// Tests for the Picture class, including integration tests that use real RAW images
    /// from the user's Pictures directory (e.g., CR2, ARW, DNG formats).
    /// </summary>
    internal sealed class PictureTests
    {
        private string? _sampleImagePath;
        private static readonly string[] RawExtensions = { ".cr2", ".arw", ".dng", ".raw", ".nef", ".nrw", ".rw2" };

        [SetUp]
        public void Setup()
        {
            // Find the first available RAW image from Pictures directory
            // Supports Canon (CR2), Sony (ARW), Adobe (DNG), and other RAW formats
            var picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (Directory.Exists(picturesPath))
            {
                var rawFiles = Directory.GetFiles(picturesPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => RawExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .FirstOrDefault();

                _sampleImagePath = rawFiles;
            }
        }

        [Test]
        public void Constructor_WithPath_SetsPicturePath()
        {
            string testPath = @"C:\test.raw";
            using var picture = new Picture(testPath);

            Assert.AreEqual(testPath, picture.Path);
            Assert.IsFalse(picture.IsLoaded);
        }

        [Test]
        public void IsLoaded_InitiallyFalse()
        {
            using var picture = new Picture(@"C:\test.raw");
            Assert.IsFalse(picture.IsLoaded);
        }

        [Test]
        public void Dimensions_InitiallyZero()
        {
            using var picture = new Picture(@"C:\test.raw");
            Assert.AreEqual(0, picture.Width);
            Assert.AreEqual(0, picture.Height);
            Assert.AreEqual(0, picture.Colors);
        }

        [Test]
        public void CameraModel_InitiallyNull()
        {
            using var picture = new Picture(@"C:\test.raw");
            Assert.IsNull(picture.CameraModel);
        }

        [Test]
        public void Open_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            using var picture = new Picture(@"C:\nonexistent\file.raw");

            Assert.Throws<FileNotFoundException>(() => picture.Open());
        }

        [Test]
        public void Open_WithEmptyPath_ThrowsFileNotFoundException()
        {
            using var picture = new Picture("");

            Assert.Throws<FileNotFoundException>(() => picture.Open());
        }

        [Test]
        public void Close_ResetsDimensions()
        {
            using var picture = new Picture(@"C:\test.raw");
            picture.Close();

            Assert.IsFalse(picture.IsLoaded);
            Assert.AreEqual(0, picture.Width);
            Assert.AreEqual(0, picture.Height);
            Assert.AreEqual(0, picture.Colors);
            Assert.IsNull(picture.CameraModel);
        }

        [Test]
        public void GetImageInfo_WhenNotLoaded_ReturnsNotLoadedMessage()
        {
            using var picture = new Picture(@"C:\test.raw");

            Assert.AreEqual("Image not loaded", picture.GetImageInfo());
        }

        [Test]
        public void Dispose_CallsClose()
        {
            var picture = new Picture(@"C:\test.raw");
            picture.Dispose();

            Assert.IsFalse(picture.IsLoaded);
            Assert.AreEqual(0, picture.Width);
        }

        [Test]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            var picture = new Picture(@"C:\test.raw");
            picture.Dispose();
            picture.Dispose(); // Should not throw

            Assert.Pass();
        }

        [Test]
        public void Close_CanBeCalledMultipleTimes()
        {
            using var picture = new Picture(@"C:\test.raw");
            picture.Close();
            picture.Close(); // Should not throw

            Assert.Pass();
        }

        [Test]
        [Category("Integration")]
        public void Open_WithValidSampleRawImage_LoadsImageSuccessfully()
        {
            if (string.IsNullOrEmpty(_sampleImagePath))
            {
                Assert.Inconclusive("No sample RAW images found in Pictures directory");
            }

            using var picture = new Picture(_sampleImagePath);
            picture.Open();

            Assert.IsTrue(picture.IsLoaded);
            Assert.Greater(picture.Width, 0);
            Assert.Greater(picture.Height, 0);
            Assert.Greater(picture.Colors, 0);
        }

        [Test]
        [Category("Integration")]
        public void Open_WithValidSampleRawImage_PopulatesImageInfo()
        {
            if (string.IsNullOrEmpty(_sampleImagePath))
            {
                Assert.Inconclusive("No sample RAW images found in Pictures directory");
            }

            using var picture = new Picture(_sampleImagePath);
            picture.Open();

            var imageInfo = picture.GetImageInfo();
            Assert.That(imageInfo, Does.Contain("x"));
            Assert.That(imageInfo, Does.Contain("colors"));
            Assert.That(imageInfo, Does.Not.Contain("not loaded"));
        }

        [Test]
        [Category("Integration")]
        public void Close_AfterOpeningValidRawImage_ResetsAllProperties()
        {
            if (string.IsNullOrEmpty(_sampleImagePath))
            {
                Assert.Inconclusive("No sample RAW images found in Pictures directory");
            }

            using var picture = new Picture(_sampleImagePath);
            picture.Open();

            Assert.IsTrue(picture.IsLoaded);
            var widthBeforeClose = picture.Width;
            Assert.Greater(widthBeforeClose, 0);

            picture.Close();

            Assert.IsFalse(picture.IsLoaded);
            Assert.AreEqual(0, picture.Width);
            Assert.AreEqual(0, picture.Height);
            Assert.AreEqual(0, picture.Colors);
        }

        [Test]
        [Category("Integration")]
        public void Open_WithValidSampleRawImage_CanBeOpenedAgain()
        {
            if (string.IsNullOrEmpty(_sampleImagePath))
            {
                Assert.Inconclusive("No sample RAW images found in Pictures directory");
            }

            using var picture = new Picture(_sampleImagePath);

            picture.Open();
            var firstWidth = picture.Width;
            picture.Close();

            picture.Open();
            var secondWidth = picture.Width;

            Assert.AreEqual(firstWidth, secondWidth);
            Assert.IsTrue(picture.IsLoaded);
        }

        [Test]
        [Category("Integration")]
        public void Open_WithValidSampleRawImage_ExtractsCameraModel()
        {
            if (string.IsNullOrEmpty(_sampleImagePath))
            {
                Assert.Inconclusive("No sample RAW images found in Pictures directory");
            }

            using var picture = new Picture(_sampleImagePath);
            picture.Open();

            Assert.IsNotNull(picture.CameraModel);
            Assert.IsNotEmpty(picture.CameraModel);
        }
    }
}
