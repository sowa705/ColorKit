using System.Numerics;

namespace ColorKit;

public struct Color
{
    public float R;
    public float G;
    public float B;
    public float A;

    public float X => R;
    public float Y => G;
    public float Z => B;

    public TransferFunction TransferFunction = TransferFunction.Linear;
    public ColorSpace ColorSpace = ColorSpace.sRGB;

    public Color(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
    public Color ConvertColorSpace(ColorSpace colorSpace)
    {
        if (ColorSpace == colorSpace)
        {
            return this;
        }
        var initialTransfer = TransferFunction;
        // make sure the gamma is linear
        var linearColor = ConvertTransferFunction(TransferFunction.Linear);
        var startcolor = new Vector3(linearColor.R, linearColor.G, linearColor.B);
        var xyzcolor = Vector3.Zero;
        var finalcolor = Vector3.Zero;

        // convert to CIE XYZ
        if (ColorSpace == ColorSpace.sRGB)
        {
            xyzcolor = ColorSpaceConverters.LinearsRGBToFloatCieXyz(startcolor);
        }
        if (ColorSpace == ColorSpace.DisplayP3)
        {
            xyzcolor = ColorSpaceConverters.LinearDisplayP3ToFloatCieXyz(startcolor);
        }
        if (ColorSpace == ColorSpace.REC2020)
        {
            xyzcolor = ColorSpaceConverters.LinearREC2020ToFloatCieXyz(startcolor);
        }
        if (ColorSpace == ColorSpace.CIEXYZ)
        {
            xyzcolor = startcolor;
        }

        // convert to final space

        if (colorSpace == ColorSpace.sRGB)
        {
            finalcolor = ColorSpaceConverters.FloatCieXyzToLinearsRGB(xyzcolor);

        }
        if (colorSpace == ColorSpace.DisplayP3)
        {
            finalcolor = ColorSpaceConverters.FloatCieXyzToLinearDisplayP3(xyzcolor);
        }
        if (ColorSpace == ColorSpace.REC2020)
        {
            xyzcolor = ColorSpaceConverters.FloatCieXyzToLinearREC2020(startcolor);
        }
        if (colorSpace == ColorSpace.CIEXYZ)
        {
            finalcolor = xyzcolor;
        }

        finalcolor.X = MathF.Max(finalcolor.X,0);
        finalcolor.Y = MathF.Max(finalcolor.Y,0);
        finalcolor.Z = MathF.Max(finalcolor.Z,0);

        var preGamma = new Color(finalcolor.X, finalcolor.Y, finalcolor.Z, A) { TransferFunction = TransferFunction.Linear, ColorSpace = colorSpace};

        return preGamma.ConvertTransferFunction(initialTransfer);
    }

    public Color ConvertTransferFunction(TransferFunction transferFunction)
    {
        if (TransferFunction == transferFunction)
        {
            return this;
        }
        var tempColor = this;
        // covert to linear
        if (TransferFunction == TransferFunction.sRGB)
        {
            tempColor = new Color(MathF.Pow(tempColor.R,2.2f), MathF.Pow(tempColor.G,2.2f),MathF.Pow(tempColor.B,2.2f) , tempColor.A);
        }
        // convert to desired gamma
        if (transferFunction == TransferFunction.sRGB)
        {
            tempColor = new Color(MathF.Pow(tempColor.R,1/2.2f), MathF.Pow(tempColor.G,1/2.2f),MathF.Pow(tempColor.B,1/2.2f) , tempColor.A) {TransferFunction = transferFunction, ColorSpace = ColorSpace};
        }

        return tempColor;
    }

    public static implicit operator Vector4(Color color)
    {
        // convert to linear/P3
        var tempColor = color.ConvertTransferFunction(TransferFunction.Linear).ConvertColorSpace(ColorSpace.DisplayP3);

        return new Vector4(tempColor.R, tempColor.G, tempColor.B, tempColor.A);
    }

    public static Color operator *(Color left, float right)
    {
        // convert to linear gamma
        var tempColor = left.ConvertTransferFunction(TransferFunction.Linear);
        // multiply
        tempColor = new Color(tempColor.R * right, tempColor.G * right, tempColor.B * right, tempColor.A)
        {
            TransferFunction = tempColor.TransferFunction, ColorSpace = tempColor.ColorSpace
        };

        return tempColor;
    }

    public float GetDifference(Color other)
    {
        var a = this.ConvertTransferFunction(TransferFunction.Linear).ConvertColorSpace(ColorSpace.CIEXYZ);
        var b = other.ConvertTransferFunction(TransferFunction.Linear).ConvertColorSpace(ColorSpace.CIEXYZ);

        return MathF.Abs(a.X - b.X) + MathF.Abs(a.Y - b.Y) + MathF.Abs(a.Z - b.Z);
    }

    public static Color FromTemperature(float kelvin)
    {
        // Clamp the kelvin range between 1000K and 40000K for practical usage
        kelvin = Math.Clamp(kelvin, 1000.0f, 40000.0f);

        float x, y;

        if (kelvin <= 4000)
        {
            x = -0.2661239f * MathF.Pow(10, 9) / MathF.Pow(kelvin, 3)
                - 0.2343580f * MathF.Pow(10, 6) / MathF.Pow(kelvin, 2)
                + 0.8776956f * MathF.Pow(10, 3) / kelvin
                + 0.179910f;
        }
        else
        {
            x = -3.0258469f * MathF.Pow(10, 9) / MathF.Pow(kelvin, 3)
                + 2.1070379f * MathF.Pow(10, 6) / MathF.Pow(kelvin, 2)
                + 0.2226347f * MathF.Pow(10, 3) / kelvin
                + 0.240390f;
        }

        // Calculate y from x using approximation
        y = -3.000f * MathF.Pow(x, 2) + 2.870f * x - 0.275f;

        // Calculate z as 1 - (x + y)
        var z = 1.0f - (x + y);

        // Convert to CIE XYZ values (normalized)
        var Y = 1.0f; // Assume Y (luminance) is 1 for simplicity
        var X = (Y / y) * x;
        var Z = (Y / y) * z;

        var xyzColor = new Color(X, Y, Z, 1) { ColorSpace = ColorSpace.CIEXYZ, TransferFunction = TransferFunction.Linear };

        return xyzColor.ConvertColorSpace(ColorSpace.DisplayP3);
    }


    public static Color Random()
    {
        var hue = System.Random.Shared.NextSingle();
        var sat = System.Random.Shared.NextSingle()*0.4f+0.6f;
        var val = System.Random.Shared.NextSingle()*0.4f+0.6f;

        return FromHSV(hue * 360f, sat, val);
    }

    public static Color FromHSV(float hue, float saturation, float value)
    {
        if (MathF.Abs(saturation) < double.Epsilon)
        {
            return new Color(value, value, value, 1);
        }

        hue = hue % 360;
        hue /= 60;
        var hi = (int)Math.Floor(hue);
        var f = hue - hi;

        value = value * 255;
        var v = value;
        var p = value * (1 - saturation);
        var q = value * (1 - saturation * f);
        var t = value * (1 - saturation * (1 - f));

        switch (hi)
        {
            case 0:
                return new Color(v / 255, t / 255, p / 255, 1);
            case 1:
                return new Color(q / 255, v / 255, p / 255, 1);
            case 2:
                return new Color(p / 255, v / 255, t / 255, 1);
            case 3:
                return new Color(p / 255, q / 255, v / 255, 1);
            case 4:
                return new Color(t / 255, p / 255, v / 255, 1);
            default:
                return new Color(v / 255, p / 255, q / 255, 1);
        }
    }


    public override string ToString()
    {
        return $"R:{R:0.000}, G:{G:0.000}, B:{B:0.000} A:{A:0.000}, TF:{TransferFunction}, ColorSpace:{ColorSpace}";
    }
}
