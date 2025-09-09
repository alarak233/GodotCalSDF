using Godot;
using System;
using System.Threading;

public partial class Main : Control
{
    private PtsData _ptsData;
    private FileDialog _myFileDialog;
    private Label _pathLabel;
    private TextureRect _sphTexture;
    private TextureRect _cylTexture;
    private TextureRect _axTexture;
    private TextureRect _sagTexture;
    private CheckButton _lrCheckButton;

    public override void _Ready()
    {
        _myFileDialog = GetNode<FileDialog>("MyFileDialog");
        _myFileDialog.FileSelected += OnFileSelected;

        _pathLabel = GetNode<Label>("CenterContainer/GridContainer/PathLabel");

        _sphTexture = GetNode<TextureRect>("CenterContainer/GridContainer/TabContainer/sph/sphTexture");
        _cylTexture = GetNode<TextureRect>("CenterContainer/GridContainer/TabContainer/cyl/cylTexture");
        _axTexture = GetNode<TextureRect>("CenterContainer/GridContainer/TabContainer/ax/axTexture");
        _sagTexture = GetNode<TextureRect>("CenterContainer/GridContainer/TabContainer/sag/sagTexture");

        _lrCheckButton = GetNode<CheckButton>("CenterContainer/GridContainer/HBoxContainer/LRCheckButton");
        _lrCheckButton.Toggled += UpdateTexture;

        var calButton = GetNode<Button>("CenterContainer/GridContainer/CalButton");
        calButton.Pressed += CalButtonCommand;
    }


    public void OnFileSelected(string filePath)
    {
        try
        {
            _ptsData = new PtsData(filePath);
            UpdateTexture(_lrCheckButton.ButtonPressed);
            _pathLabel.Text = "sdf路径:" + filePath;
        }
        catch (Exception e)
        {
            _pathLabel.Text = e.Message;
        }
    }

    public void CalButtonCommand()
    {
        _myFileDialog.PopupCentered();
    }

    public void UpdateTexture(bool ButtonPressed)
    {
        var curTexture = _ptsData?.LTexture;
        _lrCheckButton.Text = "L";
        if (ButtonPressed)
        {
            curTexture = _ptsData?.RTexture;
            _lrCheckButton.Text = "R";
        }
        _sphTexture.Texture = curTexture?.SphTexture;
        _cylTexture.Texture = curTexture?.CylTexture;
        _axTexture.Texture = curTexture?.AxTexture;
        _sagTexture.Texture = curTexture?.SagTexture;
    }
}