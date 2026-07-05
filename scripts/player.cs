using Godot;
using System;

public partial class player : base_life
{
    public override void _Ready()
    {
        base._Ready();
    }

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity;

        // 俯视图移动, left/right/up/down
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down").Normalized();
        if (direction != Vector2.Zero)
        {
            velocity = direction * Speed;
        } else
        {
            velocity = Vector2.Zero;
        }

        // 处理无敌时间计时与击退衰减
        UpdateInvincibilityAndKnockbackTimers(delta);

        // 叠加击退速度到最终 velocity 上
        velocity = ApplyKnockback(velocity);
        body.Velocity = velocity;

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

    public override void _Process(double delta)
    {
        base._Process(delta);
       
    }

    public void CollisionChecked(Node collider)
    {
        if (collider == null) return;

        base_enemy enemy = FindParentOfType<base_enemy>(collider);
        if (enemy != null)
        {
            TakeHit(enemy, 10);
        }
    }
}