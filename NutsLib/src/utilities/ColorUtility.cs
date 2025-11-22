using OpenTK.Mathematics;

namespace NutsLib;

public static class ColorUtility
{
    public static Vector3 RgbToHsv(Vector3 rgb)
    {
        float r = rgb.X;
        float g = rgb.Y;
        float b = rgb.Z;

        float cmax = MathF.Max(r, MathF.Max(g, b));
        float cmin = MathF.Min(r, MathF.Min(g, b));
        float delta = cmax - cmin;

        float h = 0f;
        if (delta != 0f)
        {
            if (cmax == r)
            {
                h = 60f * ((g - b) / delta % 6f);
            }
            else if (cmax == g)
            {
                h = 60f * (((b - r) / delta) + 2f);
            }
            else if (cmax == b)
            {
                h = 60f * (((r - g) / delta) + 4f);
            }
        }

        float s = (cmax == 0f) ? 0f : (delta / cmax);
        float v = cmax;

        return new Vector3(h, s, v);
    }

    /// <summary>
    /// Takes 0-360 h, 0-1 s, 0-1 v.
    /// </summary>
    public static Vector3 HsvToRgb(double h, double S, double V)
    {
        int r; int g; int b;

        double H = h;

        while (H < 0) { H += 360; }
        ;

        while (H >= 360) { H -= 360; }
        ;

        double R, G, B;
        if (V <= 0)
        { R = G = B = 0; }
        else if (S <= 0)
        {
            R = G = B = V;
        }
        else
        {
            double hf = H / 60.0;
            int i = (int)Math.Floor(hf);
            double f = hf - i;
            double pv = V * (1 - S);
            double qv = V * (1 - (S * f));
            double tv = V * (1 - (S * (1 - f)));
            switch (i)
            {

                // Red is the dominant color.

                case 0:
                    R = V;
                    G = tv;
                    B = pv;
                    break;

                // Green is the dominant color.

                case 1:
                    R = qv;
                    G = V;
                    B = pv;
                    break;
                case 2:
                    R = pv;
                    G = V;
                    B = tv;
                    break;

                // Blue is the dominant color.

                case 3:
                    R = pv;
                    G = qv;
                    B = V;
                    break;
                case 4:
                    R = tv;
                    G = pv;
                    B = V;
                    break;

                // Red is the dominant color.

                case 5:
                    R = V;
                    G = pv;
                    B = qv;
                    break;

                // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                case 6:
                    R = V;
                    G = tv;
                    B = pv;
                    break;
                case -1:
                    R = V;
                    G = pv;
                    B = qv;
                    break;

                // The color is not defined, we should throw an error.

                default:
                    R = G = B = V;
                    break;
            }
        }
        r = Math.Clamp((int)(R * 255.0), 0, 255);
        g = Math.Clamp((int)(G * 255.0), 0, 255);
        b = Math.Clamp((int)(B * 255.0), 0, 255);

        return new Vector3(r / 255f, g / 255f, b / 255f);
    }
}