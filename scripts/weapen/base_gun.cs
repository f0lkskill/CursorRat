using Godot;
using System;

public partial class base_gun : base_life
{
    [Export] public PackedScene bullet_scene;
    [Export] public float bullet_speed = 300.0f;
    [Export] public float bullet_damage = 10.0f;
    [Export] public float bullet_life_time = 1.0f;
    [Export] public Node2D target;

    public override void _Ready()
    {
        base._Ready();
        
    }
    public void fire()
    {
        if (target == null)
        {
            return;
        }
        Vector2 targetPos = target.GlobalPosition;
        Vector2 dir = (targetPos - body.GlobalPosition).Normalized();
        Vector2 bulletPos = body.GlobalPosition + dir * 100.0f;
        base_bullet bullet = bullet_scene.Instantiate() as base_bullet;
        bullet.Position = bulletPos;
        bullet.Rotation = Mathf.Atan2(dir.Y, dir.X);
        bullet.Velocity = dir * bullet_speed;
        GetParent().AddChild(bullet);
    }
}