using Godot;
using System;

public partial class MenuButton : Button
{
    public override void _Pressed()
    {
        GetTree().ChangeSceneToFile("res://MainMenu.tscn");
    }
}
