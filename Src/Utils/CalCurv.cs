using System;

public class CalCurv
{
    public class CurvResult
    {
        public double[,] sag;
        public double[,] sph;
        public double[,] cyl;
        public double[,] ax;
        public double[,] H;
        public double[,] K;
        public double[,] k1;
        public double[,] k2;
        public double[,] theta;
        public double cellSize;

        public CurvResult(int n, double d)
        {
            sag = new double[n, n];
            sph = new double[n, n];
            cyl = new double[n, n];
            ax = new double[n, n];
            H = new double[n, n];
            K = new double[n, n];
            k1 = new double[n, n];
            k2 = new double[n, n];
            theta = new double[n, n];
            cellSize = d;
        }
    }

    public static CurvResult Cal(double[,] heightMap, double d = 1)
    {
        double maxR = 30;

        if (heightMap is null) return null;
        var n = heightMap.GetLength(0);
        if (n != heightMap.GetLength(1))
            throw new ArgumentException("原始数据不是行列相同的矩阵");
        var CurvResult = new CurvResult(n, d);

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == 0 || i == n - 1 || j == 0 || j == n - 1)
                {
                    // 边界点无法计算二阶导数，设为 NaN 或 0
                    CurvResult.H[i, j] = double.NaN;
                    CurvResult.K[i, j] = double.NaN;
                    CurvResult.k1[i, j] = double.NaN;
                    CurvResult.k2[i, j] = double.NaN;
                    CurvResult.theta[i, j] = double.NaN;
                    continue;
                }

                // 有限差分计算一阶导数
                double p = (heightMap[i, j + 1] - heightMap[i, j - 1]) / (2 * d);
                double q = (heightMap[i + 1, j] - heightMap[i - 1, j]) / (2 * d);

                // 二阶导数
                double r = (heightMap[i, j + 1] - 2 * heightMap[i, j] + heightMap[i, j - 1]) / (d * d);
                double t = (heightMap[i + 1, j] - 2 * heightMap[i, j] + heightMap[i - 1, j]) / (d * d);
                double s = (heightMap[i + 1, j + 1] - heightMap[i + 1, j - 1] -
                            heightMap[i - 1, j + 1] + heightMap[i - 1, j - 1]) / (4 * d * d);

                double p2 = p * p, q2 = q * q;
                double denom1 = 1 + p2 + q2;
                double denom_sqrt = Math.Sqrt(denom1);
                double denom1_sq = denom1 * denom1;
                double denom2 = 2 * denom_sqrt * denom1; // 2*(1+p²+q²)^(3/2)

                // 高斯曲率 K
                double K = (r * t - s * s) / denom1_sq;

                // 平均曲率 H
                double H = ((1 + q2) * r - 2 * p * q * s + (1 + p2) * t) / denom2;

                // 主曲率
                double discriminant = H * H - K;
                double k1, k2;

                if (discriminant >= 0)
                {
                    double sqrtD = Math.Sqrt(discriminant);
                    k1 = H + sqrtD;
                    k2 = H - sqrtD;
                }
                else
                {
                    // 数值误差导致负判别式
                    k1 = k2 = H;
                }

                // 计算主方向（最大曲率方向）
                // 使用渐近方向或 Hessian 特征向量方法
                // 简化：主方向角度 θ 满足 tan(2θ) = 2s / (r - t)
                double theta = 0.0;
                if (Math.Abs(r - t) > 1e-10 || Math.Abs(s) > 1e-10)
                {
                    // 使用 atan2 避免除零
                    theta = 0.5 * Math.Atan2(2 * s, r - t); // 弧度
                }

                // 存储结果
                double n2 = ((float)n - 1) / 2;
                if (Math.Pow(i - n2, 2) + Math.Pow(j - n2, 2) < Math.Pow(maxR / d, 2))
                {
                    CurvResult.sag[i, j] = heightMap[i, j];
                    CurvResult.sph[i, j] = k1;
                    CurvResult.cyl[i, j] = k1 - k2;
                    var thetaD = theta / Math.PI * 180;
                    CurvResult.ax[i, j] = (-thetaD + 270) % 180;
                }
                CurvResult.H[i, j] = H;
                CurvResult.K[i, j] = K;
                CurvResult.k1[i, j] = k1;
                CurvResult.k2[i, j] = k2;
                CurvResult.theta[i, j] = theta;
            }
        }

        return CurvResult;
    }
}
