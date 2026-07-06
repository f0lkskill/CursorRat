using Godot;
using System;
using System.Collections.Generic;

public partial class base_melee : base_weapon
{
    public override void _Ready()
    {
        base._Ready();
        CollisionShape2D coll = body.GetNode<CollisionShape2D>("shape");
        CollisionShape2D area = body.GetNode<CollisionShape2D>("area/shape");
        area.Shape = coll.Shape;
        // coll.QueueFree();
        area.GetParent<Area2D>().Scale = new Vector2(texture_scale, texture_scale);
        area.Position += offset;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!this.Visible) return;

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
            enemy.TakeHit(this, damage);
            // QueueFree();
        }
    }
}