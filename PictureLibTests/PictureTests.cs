
using PictureLib;

namespace PictureLibTests
{
    internal sealed class PictureTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void PictureTest()
        {
            Picture picture = new Picture();
            Assert.IsNotNull(picture);
        }
    }
}
