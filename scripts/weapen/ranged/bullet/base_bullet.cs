using Godot;
using System;

public partial class base_bullet : base_life
{
    public Vector2 Velocity;
    public float Damage;
    public float LifeTime;
    public override void _Ready()
    {
        base._Ready();
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        body.Velocity = Velocity;
        LifeTime -= (float)delta;
        if (LifeTime <= 0.0f)
        {
            QueueFree();
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        // CharacterBody2D 在 Godot 4 推荐用 MoveAndSlide(), 它会根据 body.Velocity 移动
        body.MoveAndSlide();

        // 遍历所有滑动碰撞, 检测是否撞到敌人
        int collisionCount = body.GetSlideCollisionCount();
        for (int i = 0; i < collisionCount; i++)
        {
            var collision = body.GetSlideCollision(i);
            var collider = collision.GetCollider();
            CollisionChecked(collider as Node);
        }
    }
    public void CollisionChecked(Node collider)
    {
        if (collider == null) return;
        
        base_enemy enemy = FindParentOfType<base_enemy>(collider);
        if (enemy != null)
        {
            enemy.TakeHit(this, Damage);
            QueueFree();
        }
    }
}
