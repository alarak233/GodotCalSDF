using Godot;
using System;

public partial class Main : Control
{
    private FileDialog _myFileDialog;
    private Label _pathLabel;
    private PtsData _ptsData;

    public override void _Ready()
    {
        _myFileDialog = GetNode<FileDialog>("MyFileDialog");
        _myFileDialog.FileSelected += OnFileSelected;

        _pathLabel = GetNode<Label>("CenterContainer/GridContainer/PathLabel");

        var calButton = GetNode<Button>("CenterContainer/GridContainer/CalButton");
        calButton.Pressed += CalButtonCommand;
    }


    public void OnFileSelected(string filePath)
    {
        try
        {
            _ptsData = new PtsData(filePath);
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
}