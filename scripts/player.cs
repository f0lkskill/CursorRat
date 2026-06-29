using Godot;
using System;

public partial class player : base_life
{
    public CharacterBody2D body;
    public TextureProgressBar health_bar;

    public override void _Ready()
    {
        body = GetNode<CharacterBody2D>("body");
        health_bar = body.GetNode<TextureProgressBar>("health_bar");

        health_bar.MaxValue = HealthManager.MaxHealth;
        health_bar.Value = HealthManager.Health;
    }

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity;

        // 俯视图移动, left/right/up/down
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        if (direction != Vector2.Zero)
        {
            velocity = direction * Speed;
        } else
        {
            velocity = Vector2.Zero;
        }

        body.Velocity = velocity;

		body.MoveAndSlide();
	}
    public override void _Process(double delta)
    {
        health_bar.Value = HealthManager.Health;
        HealthManager.TakeDamage(0.1f);
    }
}