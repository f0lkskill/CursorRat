using Godot;
using System;

public partial class base_health : Node2D
{
    [Export]
    public float Health = 100;
    [Export]
    public float MaxHealth = 100;
    [Export]
    public PackedScene heal_label_scene;
    
    public void TakeDamage(float damage)
    {
        if (Health <= 0)
        {
            return;
        }
        Health -= damage;
        create_label(-damage);
    }
    public void Heal(float heal)
    {
        if (Health >= MaxHealth)
        {
            return;
        }
        Health += heal;
        create_label(heal);
    }
    public void Die()
    {
        TakeDamage(Health);
    }
    public bool IsAlive()
    {
        return Health > 0;
    }
    public void ResetHealth()
    {
        Heal(MaxHealth - Health);
    }
    public void create_label(float heal_amount)
    {
        heal_label label = heal_label_scene.Instantiate<heal_label>();
        label.heal_amount = heal_amount;
        label.GlobalPosition = GlobalPosition + new Vector2(GD.RandRange(-20, 20), GD.RandRange(-20, 20));
        GetTree().CurrentScene.AddChild(label);
    }
    public override void _Ready()
    {
        MaxHealth = Health;
    }
}