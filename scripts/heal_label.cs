using Godot;
using System;

public partial class heal_label : Label
{
    public float heal_amount;
    private double _elapsed;
    private const double _duration = 1.0;
    private Vector2 _startPos;
    private Color _baseColor;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        if (heal_amount > 0)
        {
            Text = $"+{heal_amount}";
            _baseColor = new Color(0, 1, 0, 1);
        }
        else
        {
            Text = $"-{Math.Abs(heal_amount)}";
            _baseColor = new Color(1, 0, 0, 1);
        }
        SetModulate(_baseColor);
        _startPos = Position;
        _elapsed = 0.0;
    }

	public override void _Process(double delta)
    {
        _elapsed += delta;
        double t = Math.Min(_elapsed / _duration, 1.0);

        // 1s内淡出, 旋转掉落
        float alpha = 1.0f - (float)t;
        Color c = _baseColor;
        c.A = alpha;
        SetModulate(c);

        // 上浮 + 轻微旋转
        Position = _startPos + new Vector2(0, (float)(-40.0 * t));
        Rotation = (float)(t * 0.2);

        if (t >= 1.0)
        {
            QueueFree();
        }
    }
}