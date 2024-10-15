using System;
using NUnit.Framework;

namespace tgalib_core.Tests;

[TestFixture]
public class BitsExtractorTest
{
    [Test]
    public void TestExtractByte()
    {
        Assert.That(19, Is.EqualTo(BitsExtractor.Extract(0xCC, 2, 5)));
    }

    [Test]
    public void TestExtractByteException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BitsExtractor.Extract(0xCC, 2, 7));
    }
}
