using Godot;
using System;

public partial class base_health : Resource
{
	[Export]
	public float Health = 100;
	[Export]
	public float MaxHealth = 100;
	[Export]
	public PackedScene heal_label_scene;
	
	public void TakeDamage(float damage, Node2D body)
	{
		if (Health <= 0)
		{
			return;
		}
		Health -= damage;
		create_label(-damage, body);
	}
	public void Heal(float heal, Node2D body)
	{
		if (Health >= MaxHealth)
		{
			return;
		}
		Health += heal;
		create_label(heal, body);
	}
	public void Die(Node2D body)
	{
		TakeDamage(Health, body);
	}
	public bool IsAlive()
	{
		return Health > 0;
	}
	public void ResetHealth(Node2D body)
	{
		Heal(MaxHealth - Health, body);
	}
	public void create_label(float heal_amount, Node2D body)
	{
		heal_label label = heal_label_scene.Instantiate<heal_label>();
		label.heal_amount = heal_amount;
		label.GlobalPosition = body.GlobalPosition + new Vector2(GD.RandRange(-20, 20), GD.RandRange(-20, 20));
		body.GetTree().CurrentScene.AddChild(label);
	}
}