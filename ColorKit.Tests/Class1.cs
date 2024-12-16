using System.Numerics;
using ColorKit;
using Xunit;
using Xunit.Abstractions;

namespace ColorKit.Tests;

public class ColorTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ColorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Color_FromTemperature_ReturnsCorrectColor()
    {
        var color = Color.FromTemperature(6504); //D65
        var expectedColor = new Color(0.9504f, 1.0000f, 1.0890f,1) { TransferFunction = TransferFunction.Linear, ColorSpace = ColorSpace.DisplayP3};

        _testOutputHelper.WriteLine(color.ToString());
        _testOutputHelper.WriteLine(expectedColor.ToString());
        _testOutputHelper.WriteLine($"Diff: {color.GetDifference(expectedColor)}");

        Assert.True(color.GetDifference(expectedColor) < 0.1f);
    }

    [Fact]
    public void Color_Conversion_Lossless()
    {
        var color = Colors.Orange;

        color = color.ConvertTransferFunction(TransferFunction.sRGB);
        color = color.ConvertColorSpace(ColorSpace.DisplayP3);
        color = color.ConvertColorSpace(ColorSpace.sRGB);
        color = color.ConvertTransferFunction(TransferFunction.Linear);

        _testOutputHelper.WriteLine(color.ToString());
        _testOutputHelper.WriteLine(Colors.Orange.ToString());
        _testOutputHelper.WriteLine($"Diff: {color.GetDifference(Colors.Orange)}");

        Assert.True(color.GetDifference(Colors.Orange) < 0.055f);
    }

    [Fact]
    public void Color_Conversion_Lossy()
    {
        var color = Colors.Orange;
        color.ColorSpace = ColorSpace.DisplayP3; //assume the orange is orange-er

        var srgbcolor = color.ConvertColorSpace(ColorSpace.sRGB); // gamut gets crushed
        color = srgbcolor.ConvertColorSpace(ColorSpace.DisplayP3);

        _testOutputHelper.WriteLine(color.ToString());
        _testOutputHelper.WriteLine(srgbcolor.ToString());
        _testOutputHelper.WriteLine(Colors.Orange.ToString());
        _testOutputHelper.WriteLine($"Diff: {color.GetDifference(Colors.Orange)}");

        Assert.True(color.GetDifference(Colors.Orange) < 0.2f);
    }
}
