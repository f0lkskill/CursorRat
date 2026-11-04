extends base_life

class_name player

@export var damage:float = 10.0 # 玩家攻击伤害

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	# Input.set_custom_mouse_cursor(null)
	super._ready() # 初始化玩家生命值


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	position = get_global_mouse_position()
