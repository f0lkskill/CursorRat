extends AnimationTree

class_name base_cat_animationPlayer

@export var face_idle:AnimationPlayer
@export var ear_idle:AnimationPlayer
@export var breathe_idle:AnimationPlayer
@export_range(0,10) var blink_min:float
@export_range(0,10) var blink_max:float

func _ready() -> void:
	face_idle.current_animation = "face_idle"
	ear_idle.current_animation = "ear_swing"
	breathe_idle.current_animation = "breathe_idle"

	# face_idle.active = false

	roll_blink()

func roll_blink():
	if not is_instance_valid(self):
		return

	face_idle.play("face_idle", -1, randf_range(1, 2))

	# 创建随机时间段定时器
	await get_tree(). \
		create_timer(randf_range(blink_min, blink_max)) \
		.timeout
	
	roll_blink()
