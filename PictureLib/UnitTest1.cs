using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Adapter;
using Sdcb.LibRaw;

namespace PictureLib
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Console.WriteLine("Sdcb.LibRaw supported cameras:");
            foreach (string model in RawContext.SupportedCameras)
            {
                Console.WriteLine(model);
            }
        }
    }
}
