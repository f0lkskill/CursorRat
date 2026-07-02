using Godot;
using System;

public partial class Room : Node2D
{

    public int RoomType { get; set; }
    public int GridX { get; set; }//房间坐标
    public int GridY { get; set; }

  
    [Export] public PackedScene Door { get; set; }


    Stage stage;//获取父节点

    public virtual Vector2 GetRoomSize()
    {
        // 尝试从 Sprite2D 获取纹理尺寸
        var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
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
        GD.Print($"Room [{GridX}, {GridY}] _Ready 被调用");

        // 延迟生成门，确保 stage 已经初始化完成
        CallDeferred(nameof(DeferredGenerateDoor));
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



        //生成门
        if (GridY != 0)
        {
            if (stage.Room[GridX, GridY - 1] != 0)//上方
            {
                //生成门
                Node2D door = Door.Instantiate<Node2D>();
                door.Position = new Vector2(0, -121);
                AddChild(door);
            }
        }
        if (GridY != 6)
        {
            if (stage.Room[GridX, GridY + 1] != 0)//下方
            {
                //生成门
                Node2D door = Door.Instantiate<Node2D>();
                door.Position = new Vector2(0, 121);
                door.Scale = new Vector2(1, -1);//垂直翻转
                AddChild(door);
            }
        }
        if (GridX != 0)
        {
            if (stage.Room[GridX - 1, GridY] != 0)//左方
            {
                //生成门
                Node2D door = Door.Instantiate<Node2D>();
                door.Rotation = (Mathf.Pi * 3) / 2;//旋转270°
                door.Position = new Vector2(-198,0);
                AddChild(door);
            }
        }
        if (GridX != 6)
        {
            if (stage.Room[GridX + 1, GridY] != 0)//右方
            {
                //生成门
                Node2D door = Door.Instantiate<Node2D>();
                door.Rotation = Mathf.Pi / 2;//旋转90°
                door.Position = new Vector2(198,0);
                AddChild(door);
            }
        }
    }

}
