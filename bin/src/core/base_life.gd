extends base_obj

class_name base_life

const FLOATING_TEXT := preload("res://bin/scene/floating_text.tscn")

@export var life_system:LifeSystem

func initLifeSystem():
	# 单独运行场景时没有挂载资源，使用默认生命系统
	if life_system == null:
		life_system = LifeSystem.new()
	# 初始化生命值
	life_system.current_hp = life_system.max_hp

func hit(damage:float):
	if life_system == null:
		return
	# 扣血并限制最低为 0
	life_system.current_hp = max(life_system.current_hp - damage, 0.0)
	show_hit_feedback(damage)

func show_hit_feedback(damage:float):
	# 弹出红色伤害文本，旋转掉落并渐出
	if get_tree().current_scene == null:
		return
	var label:floating_text = FLOATING_TEXT.instantiate()
	get_tree().current_scene.add_child(label)
	label.setup(str(int(damage)), global_position + Vector2(0, -40))

func _ready() -> void:
	initLifeSystem()
