using Godot;
using System;
using System.Threading;

public partial class Main : Control
{
    [Export] private FileDialog myFileDialog;
    [Export] private Label pathLabel;
    [Export] private TabContainer tabContainer;
    [Export] private TextureRect sphTextureRect;
    [Export] private TextureRect cylTextureRect;
    [Export] private TextureRect axTextureRect;
    [Export] private TextureRect sagTextureRect;
    [Export] private LineEdit lindLineEdit;
    [Export] private LineEdit DRXLineEdit;
    [Export] private LineEdit DRYLineEdit;
    [Export] private LineEdit NRXLineEdit;
    [Export] private LineEdit NRYLineEdit;
    [Export] private CheckButton lrCheckButton;
    [Export] private Button getValueButton;
    [Export] private Label DRResultLabel;
    [Export] private Label NRResultLabel;
    [Export] private Button calButton;
    private FileDialog nativeFileDialog;
    private PtsData ptsData;

    public override void _Ready()
    {
        nativeFileDialog = new();
        nativeFileDialog.Access = FileDialog.AccessEnum.Filesystem;
        nativeFileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        nativeFileDialog.Filters = ["*.sdf", "*.txt", "*.csv"];
        nativeFileDialog.UseNativeDialog = true;
        AddChild(nativeFileDialog);
        nativeFileDialog.FileSelected += OnFileSelected;

        myFileDialog.FileSelected += OnFileSelected;

        getValueButton.Pressed += OnGetValueButtonPressed;

        lrCheckButton.Toggled += OnLRButtonToggled;

        calButton.Pressed += OnCalButtonPressed;

        tabContainer.TabChanged += OnTabContainerTabChanged;
    }

    public void OnFileSelected(string filePath)
    {
        pathLabel.Text = "sdf路径:" + filePath;
        try
        {
            ptsData = new PtsData(filePath);
            UpdateTexture(lrCheckButton.ButtonPressed);
            UpdateResultLabel();
        }
        catch (Exception e)
        {
            pathLabel.Text = e.Message;
        }
    }

    public void OnCalButtonPressed() =>
        // myFileDialog.PopupCentered();
        nativeFileDialog.PopupCentered();

    public void OnGetValueButtonPressed() =>
        UpdateResultLabel();

    public void OnLRButtonToggled(bool pressed)
    {
        UpdateTexture(pressed);
        var curDRX = DRXLineEdit.Text.Replace("-", "");
        DRXLineEdit.Text = pressed ? "-" + curDRX : curDRX;
        UpdateResultLabel();
    }

    public void OnTabContainerTabChanged(long tabIndex)
    {
        // GD.Print($"{tabIndex}");
        UpdateResultLabel();
    }

    public void UpdateTexture(bool pressed)
    {
        if (ptsData == null)
        {
            ClearTextures(); // 清空所有纹理
            return;
        }
        var curTexture = ptsData?.LTexture;
        lrCheckButton.Text = "L";
        if (pressed)
        {
            curTexture = ptsData?.RTexture;
            lrCheckButton.Text = "R";
        }
        sphTextureRect.Texture = curTexture?.SphTexture;
        cylTextureRect.Texture = curTexture?.CylTexture;
        axTextureRect.Texture = curTexture?.AxTexture;
        sagTextureRect.Texture = curTexture?.SagTexture;
    }

    private void ClearTextures()
    {
        sphTextureRect.Texture = null;
        cylTextureRect.Texture = null;
        axTextureRect.Texture = null;
        sagTextureRect.Texture = null;
    }

    public void UpdateResultLabel()
    {
        if (ptsData == null)
        {
            DRResultLabel.Text = "请先加载数据";
            NRResultLabel.Text = "请先加载数据";
            return;
        }

        if (!double.TryParse(lindLineEdit.Text, out double lind) ||
            lind < 1.0 || lind > 2.0)
        {
            DRResultLabel.Text = "折射率输入错误";
            NRResultLabel.Text = "折射率输入错误";
            return;
        }

        if (!int.TryParse(DRXLineEdit.Text, out int DRx) ||
        !int.TryParse(DRYLineEdit.Text, out int DRy) ||
        !int.TryParse(NRXLineEdit.Text, out int NRx) ||
        !int.TryParse(NRYLineEdit.Text, out int NRy))
        {
            DRResultLabel.Text = "坐标值输入错误";
            NRResultLabel.Text = "坐标值输入错误";
            return;
        }

        double DRResult;
        double NRResult;

        var isR = lrCheckButton.ButtonPressed;
        var curCurv = isR ? ptsData.RCurv : ptsData.LCurv;
        string tabName = tabContainer.GetChild(tabContainer.CurrentTab).Name;
        double[,] curMatrix = tabName switch
        {
            "sph" => curCurv.sph,
            "cyl" => curCurv.cyl,
            "ax" => curCurv.ax,
            "sag" => curCurv.sag,
            _ => null,
        };

        if (curMatrix is null || curMatrix.GetLength(0) % 2 == 0)
        {
            DRResultLabel.Text = "点云不存在";
            NRResultLabel.Text = "点云不存在";
            return;
        }
        var n = (curMatrix.GetLength(0) - 1) / 2;
        try
        {
            if (tabName == "sph" || tabName == "cyl")
            {
                DRResult = (lind - 1) * 1000 * curMatrix[n + DRy, n + DRx];
                NRResult = (lind - 1) * 1000 * curMatrix[n + NRy, n + NRx];
            }
            else
            {
                DRResult = curMatrix[n + DRy, n + DRx];
                NRResult = curMatrix[n + NRy, n + NRx];
            }
            DRResultLabel.Text = $"结果: {DRResult:f3}";
            NRResultLabel.Text = $"结果: {NRResult:f3}";
        }
        catch
        {
            DRResultLabel.Text = $"查询的结果不存在";
            NRResultLabel.Text = $"查询的结果不存在";
        }
    }
}