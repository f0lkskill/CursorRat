using Godot;
using System;
using System.Collections.Generic;
//using System.Numerics;

public partial class Stage : Node2D
{
    private Dictionary<(int, int), Node2D> roomDictionary = new Dictionary<(int, int), Node2D>();

    [Export] public PackedScene MonsterRoomScene { get; set; }  // 怪物房间
    [Export] public PackedScene ChestRoomScene { get; set; }    // 宝箱房间
    [Export] public PackedScene BarRoomScene { get; set; }      // 酒吧房间
    [Export] public PackedScene BossRoomScene { get; set; }     // Boss房间
    [Export] public PackedScene StartRoomScene { get; set; }    // 初始房间
    [Export] public float CellSpacing { get; set; } = 10f; // 房间之间的间距

    private  Vector2 StartPlace = new Vector2(0,0);
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
       
        //房间生成
        InitRoom();
        SpawnRooms();
      
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	//房间数组[0]空 [1]怪物房间 [2]箱子房间 [3]酒吧房间 [4]Boss房间 [5]初始房间(空房间)
    public int[,] Room = new int[7, 7];
    public Vector2[,] RoomGlobalPlace = new Vector2[7, 7];
    // 创建 Random 实例
    Random random = new Random();
    private struct MapBlock
    {
        public int x;
        public int y;
        public MapBlock(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    //生成除了某一数字的范围随机数,输入:[最小值][最大值][不要的数字]
    private int GetRandom(int a,int b, params int[] c)
    {
        while (true)
        {
            int num = random.Next(a, b);
            bool isExcluded = false;
            foreach (int ex in c)
            {
                if (num == ex)
                {
                    isExcluded = true;
                    break;
                }
            }
            if (!isExcluded) return num;
        }
    }
    //链接房间[中心房间][外部房间] 这里是外部房间默认向中心房间延申对齐防止不对应
    private void BindingRoom(MapBlock core_room,MapBlock out_room)
    {
        //等概率x/y方向延申
        int RandomDir = 0;//随机方向[0]x [1]y
        RandomDir = random.Next(0, 2);
        for (int w = 0; w < 2; w++) {//只运行2次
            if (RandomDir == 0) {
                //x延申
                for (int i = 0; i < Math.Abs(core_room.x - out_room.x); i++)
                {
                    Room[out_room.x + Math.Sign(core_room.x - out_room.x) * i, out_room.y] = 1;//Math.Sign----(-8 -> -1)(8 -> 1)(0 -> 0)
                }
                out_room.x = core_room.x;
                RandomDir = 1;
            
            }
            else if (RandomDir == 1)
            {
                //y延申
                for (int i = 0; i < Math.Abs(core_room.y - out_room.y); i++)
                {
                    Room[out_room.x, out_room.y + Math.Sign(core_room.y - out_room.y) * i] = 1;//Math.Sign----(-8 -> -1)(8 -> 1)(0 -> 0)
                }
                out_room.y = core_room.y;
                RandomDir = 0;

            }
        }
    }
    private void InitRoom()
	{
        
        //随机房间位置
        MapBlock BossPlace = new MapBlock
        {
            x = GetRandom(0,7,3),
            y = GetRandom(0,7,3)
        };
        MapBlock CheastPlace = new MapBlock
        {
            x = GetRandom(0, 7, 3 ,BossPlace.x),
            y = GetRandom(0, 7, 3, BossPlace.y)
        };
        MapBlock BarPlace = new MapBlock
        {
            x = GetRandom(0, 7, 3, BossPlace.x,CheastPlace.x),
            y = GetRandom(0, 7, 3, BossPlace.y,CheastPlace.y)
        };
        MapBlock StartPlace = new MapBlock
        {
            x = 3,
            y = 3
        };

    

        //链接房间
        BindingRoom(StartPlace, BossPlace);
        BindingRoom(StartPlace, CheastPlace);
        BindingRoom(StartPlace, BarPlace);
        //随机占位房间提高随机性
        int RandNum = random.Next(1, 4);//获取1~3个随机占位房间
        for (int i = 0; i < RandNum; i++)
        {
            MapBlock RandPlace = new MapBlock
            {
                x = GetRandom(0, 7, 3, BossPlace.x, CheastPlace.x),
                y = GetRandom(0, 7, 3, BossPlace.y, CheastPlace.y)
            };
            BindingRoom(StartPlace, RandPlace);
        }

        //标记房间
        Room[BossPlace.x, BossPlace.y] = 4;//boss房间
        Room[BarPlace.x, BarPlace.y] = 3;//商店房间
        Room[CheastPlace.x, CheastPlace.y] = 2;//宝箱房间
        Room[3, 3] = 5;//初始房间

     
    }

    //生成房间
    // 生成房间
    private void SpawnRooms()
    {
        Node2D roomsContainer = new Node2D();
        roomsContainer.Name = "RoomsContainer";
        AddChild(roomsContainer);

        // 清空字典
        roomDictionary.Clear();

        Dictionary<int, PackedScene> roomTypeToScene = new Dictionary<int, PackedScene>();
        if (MonsterRoomScene != null) roomTypeToScene[1] = MonsterRoomScene;
        if (ChestRoomScene != null) roomTypeToScene[2] = ChestRoomScene;
        if (BarRoomScene != null) roomTypeToScene[3] = BarRoomScene;
        if (BossRoomScene != null) roomTypeToScene[4] = BossRoomScene;
        if (StartRoomScene != null) roomTypeToScene[5] = StartRoomScene;

        // 先实例化所有房间并获取尺寸
        Node2D[,] roomInstances = new Node2D[7, 7];
        Vector2[,] roomSizes = new Vector2[7, 7];

        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                int roomType = Room[x, y];
                if (roomType > 0 && roomTypeToScene.ContainsKey(roomType))
                {
                    PackedScene scene = roomTypeToScene[roomType];
                    Node2D roomInstance = scene.Instantiate<Node2D>();

                    // 先添加到场景树（才能正确获取 Sprite2D 尺寸）
                    roomsContainer.AddChild(roomInstance);

                    // 获取房间尺寸
                    Vector2 size = GetSpriteSize(roomInstance);
                    roomSizes[x, y] = size;
                    roomInstances[x, y] = roomInstance;

                    // 添加到字典
                    roomDictionary[(x, y)] = roomInstance;

                    // 设置房间信息
                    roomInstance.Name = $"Room_{roomType}_{x}_{y}";
                    roomInstance.SetMeta("room_type", roomType);
                    roomInstance.SetMeta("grid_x", x);
                    roomInstance.SetMeta("grid_y", y);

                    if (roomInstance.HasMethod("Initialize"))
                    {
                        roomInstance.Call("Initialize", roomType, x, y);
                    }
                    if (roomInstance is Room room)
                    {
                        room.SetGridPosition(x, y);//设置数组的位置
                    }
                }
            }
        }

        // 计算每列的最大宽度和每行的最大高度
        float[] columnWidths = new float[7];
        float[] rowHeights = new float[7];

        for (int x = 0; x < 7; x++)
        {
            float maxWidth = 0;
            for (int y = 0; y < 7; y++)
            {
                if (roomInstances[x, y] != null)
                {
                    maxWidth = Math.Max(maxWidth, roomSizes[x, y].X);
                }
            }
            columnWidths[x] = maxWidth;
        }

        for (int y = 0; y < 7; y++)
        {
            float maxHeight = 0;
            for (int x = 0; x < 7; x++)
            {
                if (roomInstances[x, y] != null)
                {
                    maxHeight = Math.Max(maxHeight, roomSizes[x, y].Y);
                }
            }
            rowHeights[y] = maxHeight;
        }

        // 计算累积位置
        float[] xPositions = new float[7];
        float[] yPositions = new float[7];

        float currentX = 0;
        for (int x = 0; x < 7; x++)
        {
            xPositions[x] = currentX;
            if (columnWidths[x] > 0)
            {
                currentX += columnWidths[x] + CellSpacing;
            }
        }

        float currentY = 0;
        for (int y = 0; y < 7; y++)
        {
            yPositions[y] = currentY;
            if (rowHeights[y] > 0)
            {
                currentY += rowHeights[y] + CellSpacing;
            }
        }

       
        // 设置每个房间的位置（居中放置在格子中）
        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                if (roomInstances[x, y] != null)
                {
                    Vector2 roomSize = roomSizes[x, y];

                    // 在格子中居中
                    float xOffset = (columnWidths[x] - roomSize.X) / 2;
                    float yOffset = (rowHeights[y] - roomSize.Y) / 2;

                    Vector2 localPosition = new Vector2(
                        xPositions[x] + xOffset,
                        yPositions[y] + yOffset
                    );

                    roomInstances[x, y].Position = localPosition;

                    // 获取房间的全局居中坐标
                    Vector2 worldPos = roomInstances[x, y].GlobalPosition;
                    
                    RoomGlobalPlace[x,y] = worldPos;
                    if ((x == 3) && (y == 3))
                    {
                        StartPlace = roomInstances[x, y].GlobalPosition;
                    }
                }
            }
        }

        var playerNode = GetNode<player>("../player");
        
    
    //传送玩家
        playerNode.GlobalPosition = StartPlace;
    }

    // 获取 Sprite2D 的实际尺寸
    private Vector2 GetSpriteSize(Node2D room)
    {
        // 查找直接子节点的 Sprite2D
        var sprite = room.GetNodeOrNull<Sprite2D>("Sprite2D");
        if (sprite != null && sprite.Texture != null)
        {
            return sprite.Texture.GetSize() * sprite.Scale;
        }

        // 递归查找所有子节点中的 Sprite2D
        var sprites = new Godot.Collections.Array<Node>();
        FindSpritesRecursive(room, sprites);

        if (sprites.Count > 0 && sprites[0] is Sprite2D firstSprite && firstSprite.Texture != null)
        {
            return firstSprite.Texture.GetSize() * firstSprite.Scale;
        }

        GD.PrintErr($"房间 {room.Name} 没有找到有效的 Sprite2D 纹理！");
        return new Vector2(64, 64); // 默认尺寸
    }

    private void FindSpritesRecursive(Node node, Godot.Collections.Array<Node> results)
    {
        if (node is Sprite2D sprite && sprite.Texture != null)
        {
            results.Add(node);
            return; // 找到第一个就返回
        }

        foreach (Node child in node.GetChildren())
        {
            FindSpritesRecursive(child, results);
            if (results.Count > 0) return;
        }
    }
    public Vector2[] GetRoomTpPlace(int x, int y)
    {
        if (x < 0 || x >= 7 || y < 0 || y >= 7)
        {
            GD.PrintErr($"GetRoomTpPlace: 无效坐标 ({x}, {y})");
            return null;
        }

        if (roomDictionary.TryGetValue((x, y), out Node2D roomNode))
        {
            // 使用 Godot 的 Get 方法获取 TpPlace，适用于任何有 TpPlace 属性的节点
            var tpPlace = roomNode.Get("TpPlace");
            if (tpPlace.Obj is Vector2[] tpArray)
            {
                return tpArray;
            }

            GD.PrintErr($"房间 ({x}, {y}) 没有 TpPlace 属性");
            return null;
        }

        GD.PrintErr($"房间 ({x}, {y}) 不存在");
        return null;
    }
}

