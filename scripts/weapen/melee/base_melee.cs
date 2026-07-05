using Godot;
using System;
using System.Collections.Generic;

public partial class base_melee : base_weapon
{
    public override void _Ready()
    {
        base._Ready();
        CollisionPolygon2D coll = body.GetNode<CollisionPolygon2D>("shape");
        body.GetNode<CollisionPolygon2D>("area/damage_coll").Polygon = coll.Polygon;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

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
        // GD.Print(collider);
        
        if (collider == null) return;
        
        base_enemy enemy = FindParentOfType<base_enemy>(collider);
        if (enemy != null)
        {
            if (enemy._invincibleTimer <= 0.0f)
            {
                enemy.TakeHit(this, damage);
            }
            // QueueFree();
        }
    }
}