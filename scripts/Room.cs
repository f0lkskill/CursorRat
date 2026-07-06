using Godot;
using System;
using System.Collections.Generic;
#region 修改提示
//DoorPlace ->门的位置,[上][下][左][右]门
//
//TpPlace ->传送的位置,[上][下][左][右] 特别注意:上是指玩家从上门出传到下门前的位置,以中心位置为相对坐标移动
//
//MonsterNum ->怪物生成数量范围,[最少][最多] 闭区间
//
//SpawnPlaceNum ->怪物生成点位数量,对应房间节点下的1,2,3...节点,最多n个
//
//Door ->门的场景
//
//MonsterScene ->怪物的场景列表,对应生成怪物
//
//MonsterPercent ->怪物场景对应生成权重,和固定为1,权重越大生成概率越大
//
//
//
#endregion

public partial class Room : Node2D
{

    public int RoomType { get; set; }
    public int GridX { get; set; }//房间坐标
    public int GridY { get; set; }

    public Vector2 WorldPosition;

    private Camera2D camera;
    private Sprite2D roomSprite;
    //修改房间贴图必看!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!----出门后对应的tp坐标[tp到门检测的外面,顺便加载场景,防止回去卡死][人话就是检测区+碰撞区加起来 * 2][就是玩家相对往上tp实现换房间]
    
    [Export] public Vector2[] DoorPlace = new Vector2[4];//门的坐标按照上下左右排列,门的相对位置
    [Export] public Vector2[] TpPlace = new Vector2[4];//传送的坐标按照上下左右排列,传送的相对位置

    
    
    [Export] public Vector2 MonsterNum { get; set; }//怪物生成数量范围
    [Export] public int SpawnPlaceNum { get; set; }//怪物生成点位数量
    //必看!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    [Export] public PackedScene[] Door { get; set; }//门的场景,上下左右
    [Export]
    public PackedScene[] MonsterScene { get; set; } //怪物列表,生成怪物
    [Export] public double[] MonsterPercent { get; set; }//怪物场景对应生成权重,和固定为1,权重越大生成概率越大
    // 添加一个列表来追踪所有生成的门
    private List<Node2D> doors = new List<Node2D>();

    Stage stage;//获取父节点

    public virtual Vector2 GetRoomSize()
    {
        // 尝试从 Sprite2D 获取纹理尺寸
        var sprite = GetNodeOrNull<Sprite2D>("sprite");
        if (sprite != null && sprite.Texture != null)
        {
            // 考虑缩放
            return sprite.Texture.GetSize() * sprite.Scale;
        }

        // 如果没有 Sprite2D，返回默认尺寸
        return new Vector2(64, 64);
    }

    public void Initialize(int roomType, int x, int y)
    {
        RoomType = roomType;
        GridX = x;
        GridY = y;
    }

    public override void _Ready()
    {
       
       // GD.Print($"Room [{GridX}, {GridY}] _Ready 被调用");

      

        // 获取 Sprite2D
        roomSprite = GetNodeOrNull<Sprite2D>("sprite");
        roomSprite.Visible = false;

      

    }

    private void DeferredGenerateDoor()
    {
        if (stage == null)
        {
            // 房间结构：Stage → roomsContainer → Room
            // 所以需要获取父节点的父节点
            Node parent = GetParent();  // roomsContainer
            if (parent != null)
            {
                stage = parent.GetParent() as Stage;  // Stage
            }
        }

        GD.Print($"Room [{GridX}, {GridY}] - stage: {(stage != null ? "存在" : "null")}, Door: {(Door != null ? "存在" : "null")}");

        if (stage == null || Door == null) return;

        GenerateDoor();
    }
    public override void _Process(double delta)
    {

    }
    //定义房间位置
    public void SetGridPosition(int x, int y)
    {
        GridX = x;
        GridY = y;
    }
    //生成门
    private void GenerateDoor()
    {
        // 先清除旧的门（如果有的话）
        ClearDoors();

        if (GridY != 0)
        {
            if (stage.Room[GridX, GridY - 1] != 0)
            {
                Node2D door = Door[0].Instantiate<Node2D>();
                door.Position = new Vector2(DoorPlace[0].X, DoorPlace[0].Y);
                AddChild(door);
                doors.Add(door); // 记录门节点
            }
        }
        if (GridY != 6)
        {
            if (stage.Room[GridX, GridY + 1] != 0)
            {
                Node2D door = Door[1].Instantiate<Node2D>();
                door.Position = new Vector2(DoorPlace[1].X, DoorPlace[1].Y);
              //  door.Scale = new Vector2(1, -1);
                AddChild(door);
                doors.Add(door);
            }
        }
        if (GridX != 0)
        {
            if (stage.Room[GridX - 1, GridY] != 0)
            {
                Node2D door = Door[2].Instantiate<Node2D>();
             //   door.Rotation = (Mathf.Pi * 3) / 2;
                door.Position = new Vector2(DoorPlace[2].X, DoorPlace[2].Y);
                AddChild(door);
                doors.Add(door);
            }
        }
        if (GridX != 6)
        {
            if (stage.Room[GridX + 1, GridY] != 0)
            {
                Node2D door = Door[3].Instantiate<Node2D>();
              //  door.Rotation = Mathf.Pi / 2;
                door.Position = new Vector2(DoorPlace[3].X, DoorPlace[3].Y);
                AddChild(door);
                doors.Add(door);
            }
        }
    }
    // 添加清除所有门的方法
    private void ClearDoors()
    {
        foreach (Node2D door in doors)
        {
            if (IsInstanceValid(door))
            {
                door.QueueFree();
            }
        }
        doors.Clear();
    }

    //摄像机生成
    //进入房间
    private void _on_area_2d_body_entered(Node2D body)
    {
        if (body.GetParent() is player)
        {
            #region 生成门等
            //进入,纹理可视化
            roomSprite.Visible = true;

            // 创建摄像机
            camera = new Camera2D();
            camera.Name = "PlayerCamera";

            // 设置为当前摄像机
            camera.Enabled = true;
            // 检查相机是否可用
            if (camera != null && camera.IsInsideTree() && camera.Enabled)
            {
                camera.MakeCurrent();
            }


            //调整摄像机缩放
            if (roomSprite != null && roomSprite.Texture != null)
            {
                FitCameraToSprite();
            }


            // 添加为子节点
            AddChild(camera);


            //生成门
            // 延迟生成门，确保 stage 已经初始化完成
            CallDeferred(nameof(DeferredGenerateDoor));
            // 先确保 stage 已初始化
            if (stage == null)
            {
                Node parent = GetParent();
                if (parent != null)
                {
                    stage = parent.GetParent() as Stage;
                }
            }

            if (stage == null)
            {
                GD.Print("stage 为 null，跳过碰撞更新");
                return;
            }
            //调整碰撞箱,把有门的禁用
            var up2 = GetNodeOrNull<StaticBody2D>("up2");
            var down2 = GetNodeOrNull<StaticBody2D>("down2");
            var left2 = GetNodeOrNull<StaticBody2D>("left2");
            var right2 = GetNodeOrNull<StaticBody2D>("right2");
            if (GridY != 0)
            {
                if (stage.Room[GridX, GridY - 1] != 0)
                {
                    up2.CollisionLayer = 0;
                    up2.CollisionMask = 0;//禁用碰撞
                }
            }
            if (GridY != 6)
            {
                if (stage.Room[GridX, GridY + 1] != 0)
                {
                    down2.CollisionLayer = 0;
                    down2.CollisionMask = 0;//禁用碰撞
                }
            }
            if (GridX != 0)
            {
                if (stage.Room[GridX - 1, GridY] != 0)
                {
                    left2.CollisionLayer = 0;
                    left2.CollisionMask = 0;//禁用碰撞
                }
            }
            if (GridX != 6)
            {
                if (stage.Room[GridX + 1, GridY] != 0)
                {
                    right2.CollisionLayer = 0;
                    right2.CollisionMask = 0;//禁用碰撞
                }
            }
            #endregion

            #region 生成怪物
            SpawnMonsters();

            #endregion
        }
    }


    #region 生成怪物
    private void SpawnMonsters()
    {
        // 1. 验证数据
        if (!ValidateMonsterData())
            return;

        // 2. 收集所有可用的生成点位
        List<Node2D> availableSpawnPoints = new List<Node2D>();
        for (int i = 1; i <= SpawnPlaceNum; i++)
        {
            string name = i.ToString();
            Node2D spawnPoint = GetNodeOrNull<Node2D>(name);
            if (spawnPoint != null)
            {
                availableSpawnPoints.Add(spawnPoint);
            }
        }

        if (availableSpawnPoints.Count == 0)
        {
            GD.PrintErr("没有可用的怪物生成点位！");
            return;
        }

        // 3. 计算要生成的怪物数量（在 MonsterNum 范围内随机）
        int monsterCount = GetRandomMonsterCount();
        if (monsterCount <= 0)
        {
            GD.Print("本次生成怪物数量为0");
            return;
        }

        // 4. 限制数量不超过可用点位数量
        monsterCount = Math.Min(monsterCount, availableSpawnPoints.Count);

        GD.Print($"🎯 计划生成 {monsterCount} 个怪物（可用点位: {availableSpawnPoints.Count} 个）");

        // 5. 计算总权重（用于怪物类型选择）
        double totalWeight = CalculateTotalWeight();
        if (totalWeight <= 0)
        {
            GD.PrintErr("总权重为0！");
            return;
        }
        // 6. 随机选择点位并生成怪物
        Random random = new Random();
        List<Node2D> selectedPoints = new List<Node2D>();

        for (int i = 0; i < monsterCount; i++)
        {
            // 6.1 从剩余点位中随机选择一个
            int pointIndex = random.Next(0, availableSpawnPoints.Count);
            Node2D selectedPoint = availableSpawnPoints[pointIndex];

            // 6.2 从列表中移除已选中的点位（防止重复使用）
            availableSpawnPoints.RemoveAt(pointIndex);

            // 6.3 根据权重选择怪物类型
            int monsterIndex = GetWeightedRandomIndex(totalWeight);

            // 6.4 实例化并生成怪物
            if (monsterIndex >= 0 && monsterIndex < MonsterScene.Length)
            {
                PackedScene scene = MonsterScene[monsterIndex];
                if (scene != null)
                {
                    Node2D monster = scene.Instantiate<Node2D>();
                    monster.Position = selectedPoint.Position;
                    monster.Scale = selectedPoint.Scale;
                    AddChild(monster);

                    GD.Print($"✅ 生成怪物 {i + 1}/{monsterCount} → 点位 {selectedPoint.Name} → 怪物索引 {monsterIndex}");
                }
            }
        }

        GD.Print($"🎉 怪物生成完成！共生成 {monsterCount} 个怪物");
    }

    // 验证怪物数据
    // 验证怪物数据
    private bool ValidateMonsterData()
    {
        if (MonsterScene == null || MonsterScene.Length == 0)
        {
            GD.PrintErr("❌ MonsterScene 为空，无法生成怪物！");
            return false;
        }

        if (MonsterPercent == null || MonsterPercent.Length != MonsterScene.Length)
        {
            GD.PrintErr("❌ MonsterPercent 长度与 MonsterScene 不匹配！");
            return false;
        }

        // 检查是否有权重为负值
        foreach (double weight in MonsterPercent)
        {
            if (weight < 0)
            {
                GD.PrintErr($"❌ 权重不能为负数: {weight}");
                return false;
            }
        }

        // 验证权重总和是否接近1（允许浮点数误差）
        double totalWeight = 0;
        foreach (double weight in MonsterPercent)
        {
            totalWeight += weight;
        }

        if (Math.Abs(totalWeight - 1.0) > 0.0001) // 允许0.0001的误差
        {
            GD.PrintErr($"❌ 权重总和必须为1，当前总和: {totalWeight}");
            return false;
        }

        return true;
    }

    // 获取随机怪物数量（在 MonsterNum 范围内等概率）
    private int GetRandomMonsterCount()
    {
        int minCount = (int)MonsterNum.X;
        int maxCount = (int)MonsterNum.Y;

        // 确保范围有效
        if (minCount < 0) minCount = 0;
        if (maxCount < minCount) maxCount = minCount;

        if (minCount == 0 && maxCount == 0)
        {
            return 0;
        }

        // 等概率随机
        Random random = new Random();
        return random.Next(minCount, maxCount + 1); // +1 因为 Next 不包含上限
    }

    // 计算总权重
    // 计算总权重
    private double CalculateTotalWeight()
    {
        double total = 0;
        foreach (double weight in MonsterPercent)
        {
            total += weight;
        }
        return total;
    }

    // 根据权重随机选择怪物索引
    // 根据权重随机选择怪物索引
    private int GetWeightedRandomIndex(double totalWeight)
    {
        if (totalWeight <= 0)
        {
            GD.PrintErr("总权重为0，无法选择怪物");
            return 0;
        }

        Random random = new Random();
        double randomValue = random.NextDouble() * totalWeight; // 使用 NextDouble 生成 [0.0, 1.0) 之间的随机数

        double cumulative = 0;
        for (int i = 0; i < MonsterPercent.Length; i++)
        {
            cumulative += MonsterPercent[i];
            if (randomValue < cumulative)
            {
                return i;
            }
        }

        // 保底返回最后一个
        return MonsterPercent.Length - 1;
    }
    #endregion
    private void _on_area_2d_body_exited(Node2D body)
    {
        if (body.GetParent() is player)
        {
            if (camera != null)
            {
                roomSprite.Visible = false;
                // 从父节点移除
                RemoveChild(camera);

                // 释放内存
                camera.QueueFree();
                camera = null;

                ClearDoors();
            }
        }
    }
    private void FitCameraToSprite()
    {
        if (roomSprite == null || roomSprite.Texture == null)
        {
            //GD.PrintErr("roomSprite 或纹理为空，无法调整摄像机缩放");
            return;
        }

        // 获取纹理实际尺寸（考虑缩放）
        Vector2 textureSize = roomSprite.Texture.GetSize() * roomSprite.Scale;

        // 获取视口尺寸
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;

        // 计算每个轴需要的缩放值
        float scaleX = viewportSize.X / textureSize.X;
        float scaleY = viewportSize.Y / textureSize.Y;

        // 选择较小的缩放值，确保整个纹理可见（保持宽高比）
        float scale = Mathf.Min(scaleX, scaleY);
       
        // 应用缩放
        camera.Zoom = Vector2.One * scale;

       // GD.Print($"纹理尺寸: {textureSize}, 视口尺寸: {viewportSize}, 应用缩放: {scale}");
    }



    //出房间--------------------------
    //上
    private void _on_up_body_entered_up(Node2D body)
    {
        var playerNode = body.GetParent() as player;
        if (playerNode != null && playerNode is player)
        {
            Vector2[] targetTpPlace = stage.GetRoomTpPlace(GridX, GridY - 1);
            // 获取目标房间的世界坐标
            Vector2 targetWorldPos = stage.RoomGlobalPlace[GridX, GridY - 1];
            body.GlobalPosition = new Vector2(//这里对应坐标是下门的坐标,所以是下门的传送坐标
                targetWorldPos.X + targetTpPlace[1].X,
                targetWorldPos.Y + targetTpPlace[1].Y
            );
          //  GD.Print(targetWorldPos.X + TpPlace[1].X, targetWorldPos.Y + TpPlace[1].Y);
            
        }
    }

    //下
    private void _on_down_body_entered_down(Node2D body)
    {
        var playerNode = body.GetParent() as player;
        if (playerNode != null && playerNode is player)
        {
            Vector2[] targetTpPlace = stage.GetRoomTpPlace(GridX, GridY + 1);
            Vector2 targetWorldPos = stage.RoomGlobalPlace[GridX, GridY + 1];
            body.GlobalPosition = new Vector2(//这里对应坐标是上门的坐标,所以是上门的传送坐标
               targetWorldPos.X + targetTpPlace[0].X,
                targetWorldPos.Y + targetTpPlace[0].Y
            );
         //   GD.Print(targetWorldPos.X + TpPlace[0].X, targetWorldPos.Y + TpPlace[0].Y);
           
        }
    
    }
    //左
    private void _on_left_body_entered_left(Node2D body)
    {
        var playerNode = body.GetParent() as player;
        if (playerNode != null && playerNode is player)
        {

            Vector2[] targetTpPlace = stage.GetRoomTpPlace(GridX - 1, GridY);
            Vector2 targetWorldPos = stage.RoomGlobalPlace[GridX - 1, GridY];


            GD.Print(TpPlace[3].X, TpPlace[3].Y);
        
        Vector2 finalPos = new Vector2(
                targetWorldPos.X + targetTpPlace[3].X,
                targetWorldPos.Y + targetTpPlace[3].Y
            );


            body.GlobalPosition = finalPos;
        //    GD.Print(targetWorldPos.X + TpPlace[3].X, targetWorldPos.Y + TpPlace[3].Y);
          
        }

    
    }
    //右
    private void _on_right_body_entered_right(Node2D body)
    {
        var playerNode = body.GetParent() as player;
        if (playerNode != null && playerNode is player)
        {
            Vector2[] targetTpPlace = stage.GetRoomTpPlace(GridX + 1, GridY);
            Vector2 targetWorldPos = stage.RoomGlobalPlace[GridX + 1, GridY];
            body.GlobalPosition = new Vector2(// 这里对应坐标是左门的坐标,所以是左门的传送坐标
              targetWorldPos.X + targetTpPlace[2].X,
                targetWorldPos.Y + targetTpPlace[2].Y
            );
        //    GD.Print(targetWorldPos.X + TpPlace[2].X, targetWorldPos.Y + TpPlace[2].Y);
           
        }
    }
    
}
