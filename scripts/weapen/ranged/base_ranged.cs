using Godot;
using System;

public partial class base_ranged : base_life
{
    [Export] public PackedScene bullet_scene;
    [Export] public float bullet_speed = 300.0f;
    [Export] public float bullet_damage = 10.0f;
    [Export] public float bullet_life_time = 1.0f;
    [Export] public Vector2 target_position = Vector2.Zero;
    [Export] public bool auto_fire = false;
    [Export] public float fire_cooldown = 0.5f;
    [Export] public Vector2 bullet_offset = Vector2.Zero;

    private float fire_cooldown_timer = 0.0f;

    public override void _Ready()
    {
        base._Ready();
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (auto_fire)
        {
            if (fire_cooldown_timer <= 0.0f)
            {
                fire();
                fire_cooldown_timer = fire_cooldown;
            }
            fire_cooldown_timer -= (float)delta;
        }
    }
    public void fire()
    {
        if (target_position == Vector2.Zero)
        {
            return;
        }
        Vector2 targetPos = target_position;
        Vector2 dir = (target_position - body.GlobalPosition).Normalized();
        base_bullet bullet = bullet_scene.Instantiate() as base_bullet;
        GetParent().GetParent().AddChild(bullet);
        bullet.Rotation = Mathf.Atan2(dir.Y, dir.X);
        bullet.Velocity = dir * bullet_speed;
        bullet.LifeTime = bullet_life_time;
        bullet.Damage = bullet_damage;
        bullet.GlobalPosition = body.GlobalPosition + bullet_offset;
    }
}