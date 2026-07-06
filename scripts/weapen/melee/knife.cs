using Godot;
using System;

public partial class knife : base_melee
{
    private Vector2 target_position;
    private Tween tween;
    private bool in_animation = false;

    [Export] public float swing_speed = 1.0f;

	public override void _Ready()
    {
        base._Ready();
    }

	public override void _Process(double delta)
    {
        base._Process(delta);
        target_position = GetGlobalMousePosition();

        if (!in_animation)
            Rotation = Mathf.Atan2(target_position.Y - GlobalPosition.Y, target_position.X - GlobalPosition.X);

        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            swing();
        }
    }

    public void setFlag()
    {
        in_animation = false;
    }

    public void swing()
    {
        in_animation = true;
        tween = GetTree().CreateTween();
        tween.TweenProperty(this, "rotation", -1 *Mathf.Pi, swing_speed * 0.1f);
        tween.TweenProperty(this, "rotation", 0.0f, swing_speed * 0.05f);
        tween.TweenCallback(Callable.From(setFlag));
        tween.Play();
    }
}
