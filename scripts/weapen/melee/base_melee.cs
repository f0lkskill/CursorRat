using Godot;
using System;
using System.Collections.Generic;

public partial class base_melee : base_life
{
    // 挥砍参数
    [Export] public Vector2 target_position = Vector2.Zero; // 挥砍目标点（例如鼠标位置）
    [Export] public bool auto_swing = false;                // 是否自动挥砍
    [Export] public float swing_damage = 15.0f;             // 挥砍伤害
    [Export] public float swing_cooldown = 0.5f;            // 挥砍间隔
    [Export] public float swing_speed = 300.0f;             // 挥砍速度（越大单次挥砍移动越远）
    [Export] public float swing_duration = 0.12f;           // 单次挥砍持续时间
    [Export] public float swing_knockback = 200.0f;         // 挥砍对敌人的击退强度

    private float _swing_cooldown_timer = 0.0f;
    private float _swing_active_timer = 0.0f;               // 当前挥砍剩余时间
    private Vector2 _swing_direction = Vector2.Zero;        // 本次挥砍方向
    private HashSet<Node> _hit_this_swing = new HashSet<Node>(); // 本次挥砍已命中敌人，避免重复

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

        // 自动挥砍：冷却结束时触发一次 swing()
        if (auto_swing)
        {
            if (_swing_cooldown_timer <= 0.0f && _swing_active_timer <= 0.0f)
            {
                if (swing())
                {
                    _swing_cooldown_timer = swing_cooldown;
                }
            }
            if (_swing_cooldown_timer > 0.0f)
            {
                _swing_cooldown_timer -= (float)delta;
            }
        }

        // 正在挥砍：像子弹一样给 body.Velocity，用 MoveAndSlide + 碰撞数检测命中
        if (_swing_active_timer > 0.0f)
        {
            body.Velocity = _swing_direction * swing_speed;
            body.MoveAndSlide();

            int collisionCount = body.GetSlideCollisionCount();
            for (int i = 0; i < collisionCount; i++)
            {
                var collision = body.GetSlideCollision(i);
                var collider = collision.GetCollider() as Node;
                CollisionChecked(collider);
            }

            _swing_active_timer -= (float)delta;
            if (_swing_active_timer <= 0.0f)
            {
                // 挥砍结束：停止移动，武器留在原地（不销毁）
                body.Velocity = Vector2.Zero;
                _swing_direction = Vector2.Zero;
                _hit_this_swing.Clear();
            }
        }
    }

    /// 触发一次挥砍：根据 target_position 计算方向，启动挥砍窗口。
    public bool swing()
    {
        if (target_position == Vector2.Zero) return false;

        _swing_direction = (target_position - body.GlobalPosition).Normalized();
        if (_swing_direction == Vector2.Zero)
        {
            _swing_direction = Vector2.Right;
        }

        _swing_active_timer = swing_duration;
        _hit_this_swing.Clear();
        return true;
    }

    // 参考 base_bullet.CollisionChecked：检测碰撞对象是否为敌人，命中后施加伤害与击退。
    // 与子弹不同：武器自己不 QueueFree，且同一敌人在单次挥砍中只受一次伤害。
    public void CollisionChecked(Node collider)
    {
        if (collider == null) return;
        if (_hit_this_swing.Contains(collider)) return;

        base_enemy enemy = FindParentOfType<base_enemy>(collider);
        if (enemy == null) return;

        // 临时提高敌人击退强度，复用 base_life.TakeHit 的通用算法
        float original = enemy.KnockbackStrength;
        enemy.KnockbackStrength = swing_knockback;
        enemy.TakeHit(this, swing_damage);
        enemy.KnockbackStrength = original;

        _hit_this_swing.Add(collider);
    }
}