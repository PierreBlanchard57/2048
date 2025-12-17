using Godot;
using System;

public partial class Tile : ColorRect
{
    private int value;
    private Label label;
    private CenterContainer centerContainer;
    public Tile(int value)
    {
        this.value = value;
    }
    public override void _Ready()
    {
        int size= (608 - Game.MARGIN * (Game.GRIDSIZE + 1)) / Game.GRIDSIZE;
        Size = new Vector2(size,size);
        Color = new Color(1, 1, 1, 1);

        centerContainer = new CenterContainer();
        centerContainer.Size = new Vector2(size,size);
        AddChild(centerContainer);
        label = new Label();
        label.Text = value.ToString();
        label.ZIndex = 5;
        centerContainer.AddChild(label);
        //label.Position = new Vector2(100,100);
        label.Set("theme_override_colors/font_color", new Color(0, 0, 0, 1));
        label.Set("theme_override_font_sizes/font_size", 40);
    }
    public void doubleValue()
    {
        this.value *= 2;
    }
    public int getValue()
    {
        return value;
    }
    public Label GetLabel()
    {
        return label;
    }
}
