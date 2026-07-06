using Godot;

public partial class smile_gun : base_ranged
{
    [Export]
    public base_enemy user;
	public override void _Ready()
    {
        base._Ready();
    }

	public override void _Process(double delta)
    {
        base._Process(delta);

        target_position = user.PlayerManager.body.GlobalPosition;
        Rotation = Mathf.Atan2(target_position.Y - GlobalPosition.Y, target_position.X - GlobalPosition.X);
    }
}
