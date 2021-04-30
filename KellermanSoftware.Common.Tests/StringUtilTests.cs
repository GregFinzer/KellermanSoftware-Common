using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KellermanSoftware.Common.Tests
{
    [TestFixture]
    public class StringUtilTests
    {
        [Test]
        public void CanInsertSpaces()
        {
            string value = "ThereCanBeOnly1Highlander";
            string expected = "There Can Be Only 1 Highlander";
            string result = StringUtil.InsertSpaces(value);
            Console.WriteLine(result);
            Assert.AreEqual(expected,result);
        }
    }
}
