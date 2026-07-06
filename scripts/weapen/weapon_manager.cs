using Godot;
using System;
using System.Collections.Generic;

public partial class weapon_manager : Node2D
{
    public base_weapon current_weapon;
    public Sprite2D hand;
    public List<base_weapon> weapons = new List<base_weapon>();
	public override void _Ready()
	{
        // 初始化武器列表
        init_weapon_list();
        current_weapon = weapons[0];

        // 获取节点
        hand = GetNode<Sprite2D>("hand");
	}

	public override void _Process(double delta)
    {
        base._Process(delta);
        Vector2 target_position = GetGlobalMousePosition();
        // 朝向鼠标
        Rotation = Mathf.Atan2(target_position.Y - GlobalPosition.Y, target_position.X - GlobalPosition.X);

        // 切换武器
        if (Input.IsActionJustPressed("change_weapon"))
        {
            change_weapon();
        }

        if (!weapons.Contains(current_weapon))
        {
            current_weapon = null;
        }

        foreach (var weapon in weapons)
        {
            if (current_weapon != weapon)
            {
                weapon.Visible = false;
            }
            else
            {
                weapon.Visible = true;
            }
            current_weapon.GlobalPosition = hand.GlobalPosition;
            current_weapon.body.Velocity = Vector2.Zero;
        }
    }

    public void change_weapon()
    {
        // 切换武器
        int current_index = weapons.IndexOf(current_weapon);
        if (weapons.Count - 1 < current_index + 1)
        {
            current_index = 0;
        }
        else
        {
            current_index++;
        }
        current_weapon = weapons[current_index];
    }

    public void init_weapon_list()
    {
        foreach (var weapon in GetChildren())
        {
            // 初始化武器列表
            if (weapon is base_weapon)
            {
                weapons.Add(weapon as base_weapon);
            }
        }
    }
}