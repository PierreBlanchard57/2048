using Godot;
using System;

public partial class ResetButton : Button
{
    public override void _Pressed()
    {
        GetTree().ChangeSceneToFile("res://Game.tscn");
    }
}
