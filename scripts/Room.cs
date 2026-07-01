using Godot;
using System;

public partial class Room : Node2D
{

    public int RoomType { get; set; }
    public int GridX { get; set; }
    public int GridY { get; set; }

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

    }
    public override void _Process(double delta)
    {

    }
}
