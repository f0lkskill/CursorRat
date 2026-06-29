using Godot;
using System;

public partial class base_life : Node
{
    [Export]
	public float Speed = 100.0f;
    [Export]
    public base_health HealthManager;
}