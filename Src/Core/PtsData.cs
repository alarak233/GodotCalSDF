using Godot;
using System;
using System.IO;

public record AllTexture
{
    public ImageTexture SphTexture { get; init; }
    public ImageTexture CylTexture { get; init; }
    public ImageTexture AxTexture { get; init; }
    public ImageTexture SagTexture { get; init; }
}
public class PtsData
{
    public double[,] LPtsMatrix { get; private set; }
    public double[,] RPtsMatrix { get; private set; }
    public double LCrib { get; private set; }
    public double RCrib { get; private set; }
    public double LD { get; private set; }
    public double RD { get; private set; }
    public CalCurv.CurvResult LCurv { get; private set; }
    public CalCurv.CurvResult RCurv { get; private set; }
    public AllTexture LTexture { get; private set; }
    public AllTexture RTexture { get; private set; }

    public PtsData(string filePath)
    {
        ReadSDF(filePath);
        LCurv = CalCurv.Cal(LPtsMatrix, LD);
        RCurv = CalCurv.Cal(RPtsMatrix, RD);
        LTexture = GenAllTexture(LCurv);
        RTexture = GenAllTexture(RCurv);
    }

    private void ReadSDF(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        if (extension != ".csv" && extension != ".sdf" && extension != ".txt")
            throw new ArgumentException("不支持的文件类型");
        var lines = File.ReadAllLines(filePath);
        for (int i = 0; i < lines.Length; i++)
        {
            if (!lines[i].StartsWith("SURFMT=")) continue;

            var paras = lines[i].Split('=')[1].Split(';');
            if (paras.Length < 6)
                throw new ArgumentException("sdf数据错误:参数不足");

            var LR = paras[1];
            if (!int.TryParse(paras[3], out int rows) || !double.TryParse(paras[5], out double crib))
                throw new ArgumentException("sdf数据错误:参数错误");

            switch (LR)
            {
                case "R":
                    RCrib = crib;
                    RD = RCrib / (rows - 1);
                    RPtsMatrix = ReadMatrix(lines, i, rows);
                    break;
                case "L":
                    LCrib = crib;
                    LD = LCrib / (rows - 1);
                    LPtsMatrix = ReadMatrix(lines, i, rows);
                    break;
                default:
                    throw new ArgumentException("sdf数据错误:方向错误");
            }

            i += rows;
        }
    }
    private static double[,] ReadMatrix(string[] lines, int startIndex, int rows)
    {
        var matrix = new double[rows, rows];
        for (int i = 0; i < rows; i++)
        {
            var curIndex = i + startIndex + 1;
            if (!lines[curIndex].StartsWith("ZZ="))
                throw new ArgumentException($"sdf数据错误:{curIndex}行错误");
            var sags = lines[curIndex].Split('=')[1].Split(';');
            if (sags.Length < rows)
                throw new ArgumentException($"sdf数据错误:行数错误");
            for (int j = 0; j < rows; j++)
            {
                if (!double.TryParse(sags[j], out matrix[i, j]))
                    throw new ArgumentException($"sdf数据错误:{curIndex}行错误");
            }
        }
        return matrix;
    }

    private static AllTexture GenAllTexture(CalCurv.CurvResult curvResult)
    {
        return new AllTexture
        {
            SphTexture = HeatMapDrawer.GenHeatMapTexture(curvResult?.sph),
            CylTexture = HeatMapDrawer.GenHeatMapTexture(curvResult?.cyl),
            AxTexture = HeatMapDrawer.GenHeatMapTexture(curvResult?.ax),
            SagTexture = HeatMapDrawer.GenHeatMapTexture(curvResult?.sag),
        };
    }
}
