using Godot;
using System;

public partial class base_enemy : base_life
{
    [Export]
    public player PlayerManager;

    public override void _PhysicsProcess(double delta)
    {
        // GD.Print(Speed);
        // 公共: 处理击退/无敌计时
        UpdateInvincibilityAndKnockbackTimers(delta);
        CollisionShape2D coll = body.GetNode<CollisionShape2D>("shape");
        CollisionShape2D area = body.GetNode<CollisionShape2D>("area/shape");
        area.Shape = coll.Shape;
        // area.GetParent<Area2D>().Scale = new Vector2(texture_scale, texture_scale);
        area.Position += offset;
    }

    /// 敌人之间互相推开: 子类在自己的 MoveAndSlide() 之后调用。
    protected void ApplyEnemySeparation()
    {
        if (body == null) return;

        int collisionCount = body.GetSlideCollisionCount();
        for (int i = 0; i < collisionCount; i++)
        {
            var collision = body.GetSlideCollision(i);
            var collider = collision.GetCollider();
            if (collider is Node node)
            {
                player otherEnemy = FindParentOfType<player>(node);
                if (otherEnemy != null)
                {
                    // 以对方为"攻击者", TakeHit 会用位置差算出远离方向
                    TakeHit(otherEnemy, 0);
                }
            }
        }
    }
}