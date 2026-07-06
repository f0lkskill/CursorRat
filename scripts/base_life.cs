using Godot;
using System;

public partial class base_life : Node2D
{
    [Export]
    public base_health HealthManager;
    [Export]
	public float Speed = 100.0f;

    [Export]
    public float InvincibleDuration = 0.8f;

    [Export]
    public float KnockbackStrength = 50.0f;

    [Export]
    public float KnockbackDuration = 0.25f;

    [Export]
    public Vector2 offset = Vector2.Zero;
    [Export]
    public float texture_scale = 1.0f;
    [Export]
    public bool is_object = false;

    // 节点
    public CharacterBody2D body;
    public TextureProgressBar health_bar;

    // 击退与无敌时间
    public float _invincibleTimer = 0.0f;
    protected Vector2 _knockbackVelocity = Vector2.Zero;
    protected float _knockbackTimer = 0.0f;
    protected float _knockbackDecay = 10.0f;

    public override void _Ready()
    {
        base._Ready();

        // 获取节点
        body = GetNode<CharacterBody2D>("body");


        if (!is_object)
        {        
            // 重置健康值
            base_health temp_health = new base_health();

            temp_health.MaxHealth = HealthManager.MaxHealth;
            temp_health.heal_label_scene = HealthManager.heal_label_scene;

            temp_health.Health = temp_health.MaxHealth;
            HealthManager = temp_health;

            health_bar = body.GetNode<TextureProgressBar>("health_bar");

            // 初始化健康条
            health_bar.Value = HealthManager.Health;
            health_bar.MaxValue = HealthManager.MaxHealth;
        }

        AnimatedSprite2D sprite = body.GetNode<AnimatedSprite2D>("sprite");
        sprite.Scale = new Vector2(texture_scale, texture_scale);
        sprite.Offset = offset;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (is_object) return;
        // 更新健康条
        health_bar.Value = HealthManager.Health;
        if (!HealthManager.IsAlive())
        {
            // body.Hide();
            QueueFree();
        }
    }

    /// 每帧调用，用于无敌时间与击退速度衰减。
    protected void UpdateInvincibilityAndKnockbackTimers(double delta)
    {
        if (_invincibleTimer > 0.0f)
        {
            _invincibleTimer = Mathf.Max(0.0f, _invincibleTimer - (float)delta);
        }

        if (_knockbackTimer > 0.0f)
        {
            _knockbackTimer = Mathf.Max(0.0f, _knockbackTimer - (float)delta);
            float decay = Mathf.Max(0.0f, 1.0f - _knockbackDecay * (float)delta);
            _knockbackVelocity *= decay;
        }
        else
        {
            _knockbackVelocity = Vector2.Zero;
        }
    }

    /// 将击退速度叠加到传入的 velocity 上，供子类设置 body.Velocity 使用。
    protected Vector2 ApplyKnockback(Vector2 velocity)
    {
        return velocity + _knockbackVelocity;
    }

    /// 通用受伤入口：无敌期间直接返回 false；否则扣血并施加击退。
    /// damage = 0 时不扣血，但仍然施加击退（用于敌人接触玩家时的反向推开）。
    public bool TakeHit(Node attacker, float damage, bool enableKnockback = true)
    {
        // GD.PrintRaw($"TakeHit: {damage}");
        if (_invincibleTimer > 0.0f) return false;

        // 有伤害时才扣血并设置较长无敌；纯击退时用较短的“推开冷却”，避免卡住不动
        if (damage > 0)
        {
            if (HealthManager == null) return false;
            HealthManager.TakeDamage(damage, body);
            _invincibleTimer = InvincibleDuration;
        }
        else
        {
            _invincibleTimer = 0.1f;
        }

        if (attacker != null && body != null)
        {
            Node2D attackerRef = FindAttackerBody2D(attacker);
            Vector2 attackerPos = attackerRef != null
                ? attackerRef.GlobalPosition
                : body.GlobalPosition;

            Vector2 knockbackDir = (body.GlobalPosition - attackerPos).Normalized();
            if (knockbackDir == Vector2.Zero)
            {
                knockbackDir = Vector2.Up;
            }

            if (enableKnockback)
            {
                knockbackDir = knockbackDir * KnockbackStrength;
            }

            if (enableKnockback)
            {
                _knockbackVelocity = knockbackDir;
                _knockbackTimer = KnockbackDuration;
            }

            // GD.Print($"TakeHit: {damage}, {knockbackDir}, KnockbackStrength: {KnockbackStrength}");
        }

        return true;
    }

    /// 是否正处于击退运动中（用于子类判断是否抑制主动移动）。
    protected bool IsKnockbackActive()
    {
        return _knockbackTimer > 0.0f && _knockbackVelocity.Length() > 10.0f;
    }

    /// 从任意节点向上追溯，尝试找到它所属的 base_life 的 body；找不到时退回到自身 Node2D。
    private Node2D FindAttackerBody2D(Node node)
    {
        Node current = node;
        while (current != null)
        {
            if (current is base_life bl && bl.body != null)
            {
                return bl.body;
            }
            current = current.GetParent();
        }
        return node as Node2D;
    }

    /// 向上遍历父节点链，查找第一个指定类型的节点。
    protected T FindParentOfType<T>(Node node) where T : class
    {
        Node current = node;
        while (current != null)
        {
            if (current is T found)
            {
                return found;
            }
            current = current.GetParent();
        }
        return null;
    }

    public bool IsInvincible()
    {
        return _invincibleTimer > 0.0f;
    }
}