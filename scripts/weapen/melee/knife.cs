using Godot;
using System;

public partial class knife : base_melee
{
    private Vector2 target_position;
        
	public override void _Ready()
    {
        base._Ready();
    }

	public override void _Process(double delta)
    {
        base._Process(delta);
        target_position = GetGlobalMousePosition();
        Rotation = Mathf.Atan2(target_position.Y - GlobalPosition.Y, target_position.X - GlobalPosition.X);
    }
}
