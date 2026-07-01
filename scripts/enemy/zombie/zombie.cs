using Godot;
using System;

public partial class zombie : base_enemy
{
	public override void _Ready()
    {
        base._Ready();
        // 降低敌人自己的击退强度, 避免被弹飞太远
        KnockbackStrength = 300.0f;
        KnockbackDuration = 0.2f;
    }

	public override void _PhysicsProcess(double delta)
    {
        // 让 base_enemy 先处理击退/无敌计时
        base._PhysicsProcess(delta);

        if (PlayerManager == null)
        {
            return;
        }

        CharacterBody2D player_body = PlayerManager.body;

        // 向玩家移动
        Vector2 direction = player_body.GlobalPosition - body.GlobalPosition;
        direction = direction.Normalized();
        Vector2 velocity = direction * Speed;

        // 击退期间降低主动移动, 避免持续推挤导致"粘在一起"
        if (IsKnockbackActive())
        {
            velocity *= 0.2f;
        }

        // 叠加击退速度 (反向推开 + 玩家接触反向推开)
        velocity = ApplyKnockback(velocity);
        body.Velocity = velocity;
        body.MoveAndSlide();

        // 1) 敌人之间互相推开 (公共逻辑, 由 base_enemy 提供)
        ApplyEnemySeparation();

        // 2) 与玩家碰撞: 让自己也被反向推开 (不扣血)
        int collisionCount = body.GetSlideCollisionCount();
        for (int i = 0; i < collisionCount; i++)
        {
            var collision = body.GetSlideCollision(i);
            var collider = collision.GetCollider();
            if (collider is Node node)
            {
                player hitPlayer = FindParentOfType<player>(node);
                if (hitPlayer != null)
                {
                    TakeHit(hitPlayer, 0);
                }
            }
        }
    }
}
