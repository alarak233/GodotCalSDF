using Godot;
using System;

public partial class Main : Control
{
    [Export]
    private AcceptDialog _testAcceptDialog;

    public override void _Ready()
    {
        var testButton = GetNode<Button>("CenterContainer/GridContainer/TestButton");
        testButton.Pressed += OnTestButtonPressed;
    }

    private void OnTestButtonPressed()
    {
        if (_testAcceptDialog is not null)
        {
            _testAcceptDialog.DialogText = "Hello";
            _testAcceptDialog.PopupCentered();
        }
    }
}
