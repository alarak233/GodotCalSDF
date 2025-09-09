using Godot;
using System;

public partial class HeatMapDrawer : Node
{
    public static ImageTexture GenHeatMapTexture(double[,] pts)
    {
        var colorMap = CreateJetColormap();
        var image = GenHeatMapImage(pts, colorMap);
        return ImageTexture.CreateFromImage(image);

    }
    private static Image GenHeatMapImage(double[,] pts, Color[] colorMap)
    {
        if (pts is null) return null;
        int n = pts.GetLength(0);
        if (n != pts.GetLength(1))
            throw new ArgumentException("用于绘制热力图的矩阵行列数不相同");

        double minPt = double.MaxValue;
        double maxPt = double.MinValue;
        foreach (var pt in pts)
        {
            if (pt < minPt && pt != 0) minPt = pt;
            if (pt > maxPt && pt != 0) maxPt = pt;
        }

        var image = Image.CreateEmpty(n, n, false, Image.Format.Rgb8);
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                var color = MapValueToColor(pts[y, x], minPt, maxPt, colorMap);
                image.SetPixel(x, y, color);
            }
        }

        return image;
    }
    private static Color[] CreateJetColormap(int size = 256)
    {
        Color[] colors = new Color[size];
        for (int i = 0; i < size; i++)
        {
            float x = (float)i / (size - 1) * 4;
            float r = Mathf.Clamp(Mathf.Min(x - 1.5f, -x + 4.5f), 0, 1);
            float g = Mathf.Clamp(MathF.Min(x - 0.5f, -x + 3.5f), 0, 1);
            float b = Mathf.Clamp(Mathf.Min(x + 0.5f, -x + 2.5f), 0, 1);
            colors[i] = new Color(r, g, b);
        }
        return colors;
    }
    private static Color MapValueToColor(double value, double minVal, double maxVal, Color[] colorMap)
    {
        double t = Mathf.Clamp((value - minVal) / (maxVal - minVal), 0, 1);
        int index = (int)(t * (colorMap.Length - 1));
        return colorMap[index];
    }
}
