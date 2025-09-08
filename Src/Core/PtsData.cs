using Godot;
using System;
using System.IO;

public class PtsData
{
    public double[,] LPtsMatrix { get; private set; }
    public double[,] RPtsMatrix { get; private set; }
    public double LCrib { get; private set; }
    public double RCrib { get; private set; }

    public PtsData(string filePath)
    {
        ReadSDF(filePath);
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
                    RPtsMatrix = ReadMatrix(lines, i, rows);
                    break;
                case "L":
                    LCrib = crib;
                    LPtsMatrix = ReadMatrix(lines, i, rows);
                    break;
                default:
                    throw new ArgumentException("sdf数据错误:方向错误");
            }

            i += rows;
        }
    }
    private double[,] ReadMatrix(string[] lines, int startIndex, int rows)
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
}
