using Godot;
using System;
using System.Collections.Generic;

public partial class BoosRoom : Node2D
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
    //必看!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    [Export] public PackedScene Door { get; set; }
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
                Node2D door = Door.Instantiate<Node2D>();
                door.Position = new Vector2(DoorPlace[0].X, DoorPlace[0].Y);
                AddChild(door);
                doors.Add(door); // 记录门节点
            }
        }
        if (GridY != 6)
        {
            if (stage.Room[GridX, GridY + 1] != 0)
            {
                Node2D door = Door.Instantiate<Node2D>();
                door.Position = new Vector2(DoorPlace[1].X, DoorPlace[1].Y);
                door.Scale = new Vector2(1, -1);
                AddChild(door);
                doors.Add(door);
            }
        }
        if (GridX != 0)
        {
            if (stage.Room[GridX - 1, GridY] != 0)
            {
                Node2D door = Door.Instantiate<Node2D>();
                door.Rotation = (Mathf.Pi * 3) / 2;
                door.Position = new Vector2(DoorPlace[2].X, DoorPlace[2].Y);
                AddChild(door);
                doors.Add(door);
            }
        }
        if (GridX != 6)
        {
            if (stage.Room[GridX + 1, GridY] != 0)
            {
                Node2D door = Door.Instantiate<Node2D>();
                door.Rotation = Mathf.Pi / 2;
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

        }
    }
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

            // 获取目标房间的世界坐标
            Vector2 targetWorldPos = stage.RoomGlobalPlace[GridX, GridY - 1];
            body.GlobalPosition = new Vector2(//这里对应坐标是下门的坐标,所以是下门的传送坐标
                targetWorldPos.X + TpPlace[1].X,
                targetWorldPos.Y + TpPlace[1].Y
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

            Vector2 targetWorldPos = stage.RoomGlobalPlace[GridX, GridY + 1];
            body.GlobalPosition = new Vector2(//这里对应坐标是上门的坐标,所以是上门的传送坐标
               targetWorldPos.X + TpPlace[0].X,
                targetWorldPos.Y + TpPlace[0].Y
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


            Vector2 targetWorldPos = stage.RoomGlobalPlace[GridX - 1, GridY];


            GD.Print(TpPlace[3].X, TpPlace[3].Y);

            Vector2 finalPos = new Vector2(
                    targetWorldPos.X + TpPlace[3].X,
                    targetWorldPos.Y + TpPlace[3].Y
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

            Vector2 targetWorldPos = stage.RoomGlobalPlace[GridX + 1, GridY];
            body.GlobalPosition = new Vector2(// 这里对应坐标是左门的坐标,所以是左门的传送坐标
              targetWorldPos.X + TpPlace[2].X,
                targetWorldPos.Y + TpPlace[2].Y
            );
            //    GD.Print(targetWorldPos.X + TpPlace[2].X, targetWorldPos.Y + TpPlace[2].Y);

        }
    }

}

