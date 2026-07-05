using Godot;
using System;

public partial class spawn : Node2D
{
    [Export]
    // 敌人素材组
    public PackedScene[] EnemyScenes;
    
    // 敌人节点
    public Node2D enemies;

    // 玩家节点
    public player player;

	public override void _Ready()
    {
        enemies = GetNode<Node2D>("enemies");
        player = GetNode<player>("player");

        if (EnemyScenes == null || EnemyScenes.Length == 0)
        {
            GD.Print("spawn: EnemyScenes is empty or null");
            return;
        }

        // spawn敌人实例, 生成5次, 每次随机选取敌人素材组中的一个
        // 注意: GD.RandRange(int, int) 是两端闭区间, 所以最大值必须是 Length - 1
        for (int i = 0; i < 10; i++)
        {
           int enemyIndex = GD.RandRange(0, EnemyScenes.Length - 1);
           base_enemy enemy = EnemyScenes[enemyIndex].Instantiate() as base_enemy;
           if (enemy == null)
           {
               GD.Print($"spawn: failed to instantiate enemy at index {enemyIndex}");
               continue;
           }
           enemies.AddChild(enemy);

           // 防御性: AddChild 后 base_life._Ready 应该已执行, body 已初始化
           if (enemy.body != null)
           {
               enemy.body.GlobalPosition = new Vector2(GD.RandRange(-1000, 1000), GD.RandRange(-1000, 1000));
           }
           enemy.PlayerManager = player;
        }
    }
}