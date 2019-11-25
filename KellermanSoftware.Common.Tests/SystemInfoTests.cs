using System;
using NUnit.Framework;

namespace KellermanSoftware.Common.Tests
{
    [TestFixture]
    public class SystemInfoTests
    {
        [Test]
        public void CanGetCpuInfo()
        {
            Console.WriteLine(SystemInfo.GetCPUInfo());
            Assert.IsFalse(SystemInfo.GetCPUInfo().Contains("Unknown"));
        }

        [Test]
        public void CanGetFreeSpace()
        {
            Console.WriteLine(SystemInfo.GetFreeSpace(FileUtil.GetCurrentDriveLetter()));
            Assert.IsFalse(SystemInfo.GetFreeSpace(FileUtil.GetCurrentDriveLetter()).Contains("Unknown"));
        }

        [Test]
        public void CanGetTotalRAM()
        {
            Console.WriteLine(SystemInfo.GetTotalRAM());
            Assert.IsFalse(SystemInfo.GetTotalRAM().Contains("Unknown"));
        }

        [Test]
        public void CanGetOSVersion()
        {
            Console.WriteLine(SystemInfo.GetOSVersion());
            Assert.IsFalse(SystemInfo.GetOSVersion().Contains("Unknown"));
        }
    }
}
