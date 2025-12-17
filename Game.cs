using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.ConstrainedExecution;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class Game : Node
{
    public static int GRIDSIZE = 4;
    public static int MARGIN = 2;
    private Tile[,] tiles = new Tile[GRIDSIZE,GRIDSIZE];
    private System.Collections.Generic.Dictionary<System.String,double> rad=new System.Collections.Generic.Dictionary<System.String,double>();
    [Export] private Label scoreLabel;
    [Export] private Label endScoreLabel;
    [Export] private Label bestScoreLabel;
    [Export] private ColorRect looseScreen;
    
    private int tileSize;
    private int score=0;
    private int bestScore = 0;
    private int deltaUntilLastInput = 0;
    private bool gameFinished = false;
    public override void _Ready()
    {
        tileSize = (608 - MARGIN * (GRIDSIZE + 1)) / GRIDSIZE;
        bestScore = readBestScore();
        bestScoreLabel.Text = "Meilleur Score: " + bestScore;
        base._Ready();
        for(int i = 0; i < 2; i++)
        {
            generateRandom();
        }
        //sin=y and cos=x
        rad.Add("up",Math.PI/2);
        rad.Add("down",3*Math.PI/2);
        rad.Add("left", 0);
        rad.Add("right", Math.PI);
    }
    private void generateRandom()
    {
        int x = 0;
        int y = 0;
        do
        {
            x = new Random().Next(0, GRIDSIZE);
            y = new Random().Next(0, GRIDSIZE);
        } while (tiles[x, y] != null);
        Tile tile = new Tile(2);
        AddChild(tile);
        tile.Position = new Vector2(x * tileSize + (x + 1) * MARGIN, y * tileSize + (y + 1) * MARGIN);
        tiles[x, y] = tile;
    }
    private bool isBoardFilled()
    {
        bool res = true;
        for (int y = 0;y < GRIDSIZE; y++){
            for(int x = 0; x < GRIDSIZE; x++)
            {
                if (tiles[x, y] == null) res = false;
            }
        }
        return res;
    }
    private bool isValidCoord(int n)
    {
        return n>=0 && n<=(GRIDSIZE-1);
    }
    private bool canMove()
    {
        bool res = false;
        for(int y = 0; y < GRIDSIZE; y++)
        {
            for(int x = 0; x < GRIDSIZE; x++)
            {
                if(tiles[x, y] != null)
                {
                    int value = tiles[x, y].getValue();
                    double radVal = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        int xVal = (int)Math.Cos(radVal) + x;
                        int yVal = (int)Math.Sin(radVal) + y;
                        if (isValidCoord(yVal) && isValidCoord(xVal))
                        {
                            if (tiles[xVal, yVal] != null)
                            {
                                if (tiles[xVal, yVal].getValue() == value) res = true;
                            }
                            else res = true;
                            
                        }
                        radVal += Math.PI / 2;
                    }
                }
                
            }
        }
        return res;
    }
    private void moveBoard(string direction)
    {
        int xVal = (int)Math.Cos(rad[direction]);
        int yVal= (int)Math.Sin(rad[direction]);
        int currentY = (yVal == -1) ? GRIDSIZE-1:0 ;
        if (xVal == 0)//vertical
        {
            for (int y = 0; y != (GRIDSIZE-1)*yVal; y+=yVal)
            {
                for (int x = 0; x != GRIDSIZE; x++)
                {
                    //int currentY = ((((1+yVal)/2)*y+((1-yVal)/2)*(GRIDSIZE-1-y))+GRIDSIZE)%GRIDSIZE;
                    if (tiles[x, currentY] == null && tiles[x, currentY + yVal] != null)
                    {
                        tiles[x, currentY] = tiles[x, currentY + yVal];
                        tiles[x, currentY + yVal] = null;
                        Tween tween = GetTree().CreateTween();
                        tween.TweenProperty(tiles[x, currentY], "position", new Vector2(x * tileSize + (x + 1) * MARGIN, currentY * tileSize + (currentY + 1) * MARGIN), 0.2);
                    }
                    if (tiles[x, currentY] != null && tiles[x, currentY + yVal] != null)
                    {
                        if (tiles[x, currentY].getValue() == tiles[x, currentY + yVal].getValue())
                        {
                            int value = tiles[x, currentY].getValue();
                            int xCopy = x;
                            int yCopy = currentY;
                            Tile next = tiles[xCopy, yCopy + yVal];
                            tiles[xCopy, yCopy + yVal] = null;
                            tiles[xCopy, yCopy].doubleValue();
                            Tile current = tiles[xCopy, yCopy];
                            Tween tween = GetTree().CreateTween();
                            tween.TweenProperty(next, "position", new Vector2(x * tileSize + (x + 1) * MARGIN, currentY * tileSize + (currentY + 1) * MARGIN), 0.1).Finished += () =>
                            {
                                RemoveChild(next);
                                current.GetLabel().Text=  current.getValue().ToString();
                                Tween tween2 = GetTree().CreateTween();
                                tween2.TweenProperty(current.GetLabel(), "theme_override_font_sizes/font_size", 60, 0.2);
                                tween2.TweenProperty(current.GetLabel(), "theme_override_font_sizes/font_size", 40, 0.2);
                            };
                            score += (int)(100 *Math.Log2(value));
                            scoreLabel.Text = "Score: " + score;
                        }
                    }
                }
                currentY += yVal;
            }
        }
        else//horizontal
        {
            int currentX = (xVal == -1) ? GRIDSIZE - 1 : 0;
            for(int x = 0; x != (GRIDSIZE-1)*xVal; x+=xVal)
            {
                for(int  y = 0; y != GRIDSIZE; y++)
                {
                    //int currentX= (GRIDSIZE - (1 - xVal) / 2 + x) % GRIDSIZE;
                    if (tiles[currentX, y] == null && tiles[currentX+xVal, y] != null)
                    {
                        tiles[currentX, y] = tiles[currentX+xVal, y];
                        tiles[currentX+xVal, y] = null;
                        Tween tween = GetTree().CreateTween();
                        tween.TweenProperty(tiles[currentX, y], "position", new Vector2(currentX * tileSize + (currentX + 1) * MARGIN, y * tileSize + (y + 1) * MARGIN), 0.2);
                    }
                    if (tiles[currentX, y] != null && tiles[currentX+xVal, y] != null)
                    {
                        if (tiles[currentX, y].getValue() == tiles[currentX+xVal, y].getValue())
                        {
                            int value = tiles[currentX, y].getValue();
                            int xCopy = currentX;
                            int yCopy = y;

                            Tile next = tiles[xCopy + xVal, yCopy];
                            tiles[xCopy + xVal, yCopy] = null;
                            tiles[xCopy, yCopy].doubleValue();
                            Tile current = tiles[xCopy, yCopy];
                            Tween tween = GetTree().CreateTween();
                            tween.TweenProperty(next, "position", new Vector2(currentX * tileSize + (currentX + 1) * MARGIN, y * tileSize + (y + 1) * MARGIN), 0.1).Finished += () =>
                            {
                                RemoveChild(next);
                                current.GetLabel().Text = current.getValue().ToString();
                                Tween tween2 = GetTree().CreateTween();
                                tween2.TweenProperty(current.GetLabel(), "theme_override_font_sizes/font_size", 60, 0.2);
                                tween2.TweenProperty(current.GetLabel(), "theme_override_font_sizes/font_size", 40, 0.2);
                                
                            };
                            score += (int)(100 * Math.Log2(value));
                            scoreLabel.Text = "Score: " + score;
                        }
                    }
                }
                currentX += xVal;
            }
        }
    }
    private int readBestScore()
    {
        var file=FileAccess.Open("res://data.json",FileAccess.ModeFlags.Read);
        if (file.GetLength() == 0)
        {
            file.Close();
            file = FileAccess.Open("res://data.json", FileAccess.ModeFlags.Write);
            var data = new Godot.Collections.Dictionary<string, int>();
            data.Add("bestScore", 0);
            string text = Json.Stringify(data);
            file.StoreLine(text);
            file.Close();
            return 0;

        }
        else
        {

            byte[] buffer = file.GetBuffer((int)file.GetLength());
            string text=Encoding.UTF8.GetString(buffer);
            Godot.Collections.Dictionary<string,Variant> dictionary= (Godot.Collections.Dictionary<string, Variant>)Json.ParseString(text);
            file.Close();
            return (int)dictionary["bestScore"];
        }
    }
    private void saveBestScore()
    {
        var file = FileAccess.Open("res://data.json", FileAccess.ModeFlags.Read);
        byte[] buffer = file.GetBuffer((int)file.GetLength());
        string text = Encoding.UTF8.GetString(buffer);
        Godot.Collections.Dictionary<string, Variant> dictionary = (Godot.Collections.Dictionary<string, Variant>)Json.ParseString(text);
        file.Close();
        dictionary["bestScore"] = score;
        file = FileAccess.Open("res://data.json", FileAccess.ModeFlags.Write);
        text = Json.Stringify(dictionary);
        file.StoreLine(text);
        file.Close();
    }
    public override void _Process(double delta)
    {
        if (!gameFinished)
        {
            if (deltaUntilLastInput != 0)
            {
                deltaUntilLastInput++;
                if (deltaUntilLastInput == 20) deltaUntilLastInput = 0;
            }
            else
            {
                
                if (!canMove())
                {
                    gameFinished = true;

                    if (score > bestScore)
                    {
                        saveBestScore();
                        endScoreLabel.Text = "Nouveau Record! : " + score;
                        endScoreLabel.Position= new Vector2 (endScoreLabel.Position.X-100, endScoreLabel.Position.Y);
                    }
                    else
                    {
                        endScoreLabel.Text = "Score: " + score;
                    }
                        looseScreen.Visible = true;
                }
            }
            bool generate = false;
            if (Input.IsActionJustPressed("up") && deltaUntilLastInput == 0)
            {
                deltaUntilLastInput++;
                moveBoard("up");
                generate = true;
            }
            if (Input.IsActionJustPressed("down") && deltaUntilLastInput == 0)
            {
                deltaUntilLastInput++;
                moveBoard("down");
                generate = true;
            }
            if (Input.IsActionJustPressed("left") && deltaUntilLastInput == 0)
            {
                deltaUntilLastInput++;
                moveBoard("left");
                generate = true;
            }
            if (Input.IsActionJustPressed("right") && deltaUntilLastInput == 0)
            {
                deltaUntilLastInput++;
                moveBoard("right");
                generate = true;
            }
            if (generate && !isBoardFilled())
            {
                generateRandom();
            }
        }
    }
}
