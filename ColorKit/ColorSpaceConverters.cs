using System.Numerics;

namespace ColorKit;

internal static class ColorSpaceConverters
{
    private static readonly float[,] MatP3 = new float[,]
    {
        {  2.49349691f, -0.93138362f, -0.40271078f },
        { -0.82948897f,  1.76266406f,  0.02362469f },
        {  0.03584583f, -0.07617239f,  0.95688452f }
    };

    private static readonly float[,] MatP3Inverse = new float[,]
    {
        {  0.515102f,  0.291965f,  0.157153f },
        {  0.241182f,  0.692236f,  0.066581f },
        { -0.001050f,  0.041881f,  0.784377f }
    };

    private static readonly float[,] MatSRGB = new float[,]
    {
        {  3.2406f, -1.5372f, -0.4986f },
        { -0.9689f,  1.8758f,  0.0415f },
        {  0.0557f, -0.2040f,  1.0570f }
    };

    private static readonly float[,] MatSRGBInverse = new float[,]
    {
        {  0.4124564f,  0.3575761f,  0.1804375f },
        {  0.2126729f,  0.7151522f,  0.0721750f },
        {  0.0193339f,  0.1191920f,  0.9503041f }
    };

    private static readonly float[,] MatREC2020 = new float[,]
    {
        {  1.716651187f, -0.355670783f, -0.253366281f },
        { -0.666684351f,  1.616481236f,  0.015768545f },
        {  0.017639857f, -0.042770613f,  0.942103121f }
    };

    private static readonly float[,] MatREC2020Inverse = new float[,]
    {
        {  0.636958048f,  0.144616916f,  0.168880975f },
        {  0.262700212f,  0.677998071f,  0.059301718f },
        {  0.000000000f,  0.028072693f,  1.060985057f }
    };

    public static Vector3 FloatCieXyzToLinearDisplayP3(Vector3 color)
    {
        return TransformColor(color, MatP3);
    }

    public static Vector3 LinearDisplayP3ToFloatCieXyz(Vector3 color)
    {
        return TransformColor(color, MatP3Inverse);
    }

    public static Vector3 FloatCieXyzToLinearsRGB(Vector3 color)
    {
        return TransformColor(color, MatSRGB);
    }

    public static Vector3 LinearsRGBToFloatCieXyz(Vector3 color)
    {
        return TransformColor(color, MatSRGBInverse);
    }

    public static Vector3 FloatCieXyzToLinearREC2020(Vector3 color)
    {
        return TransformColor(color, MatREC2020);
    }

    public static Vector3 LinearREC2020ToFloatCieXyz(Vector3 color)
    {
        return TransformColor(color, MatREC2020Inverse);
    }

    private static Vector3 TransformColor(Vector3 color, float[,] matrix)
    {
        float X = color.X, Y = color.Y, Z = color.Z;

        var r = matrix[0, 0] * X + matrix[0, 1] * Y + matrix[0, 2] * Z;
        var g = matrix[1, 0] * X + matrix[1, 1] * Y + matrix[1, 2] * Z;
        var b = matrix[2, 0] * X + matrix[2, 1] * Y + matrix[2, 2] * Z;

        return new Vector3(r, g, b);
    }
}
