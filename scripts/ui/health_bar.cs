using Godot;
using System;

public partial class health_bar : TextureProgressBar
{
	public override void _Process(double delta)
    {
        Label num = GetNode<Label>("num");
        num.Text = Value.ToString();
    }
}
