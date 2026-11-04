extends AnimationTree

class_name base_cat_animationPlayer

@export var face_idle:AnimationPlayer
@export var ear_idle:AnimationPlayer
@export var breathe_idle:AnimationPlayer
@export_range(0,10) var blink_min:float
@export_range(0,10) var blink_max:float
@export var breathe_base_speed:float = 100.0 # 呼吸动画的基准移动速度
@export var breathe_speed_min:float = 0.2 # 呼吸最慢倍率
@export var breathe_speed_max:float = 3.0 # 呼吸最快倍率

func _ready() -> void:
	face_idle.current_animation = "face_idle"
	ear_idle.current_animation = "ear_swing"
	breathe_idle.current_animation = "breathe_idle"

	# face_idle.active = false

	update_breathe_speed()
	roll_blink()

func _process(_delta: float) -> void:
	update_breathe_speed()

func update_breathe_speed() -> void:
	# 根据敌人移动速度线性调整呼吸动画快慢
	var enemy := get_parent() as base_enemy
	if enemy == null or enemy.life_system == null:
		return
	var rate:float = enemy.life_system.speed / breathe_base_speed
	breathe_idle.speed_scale = clampf(rate, breathe_speed_min, breathe_speed_max)

func roll_blink():
	if not is_instance_valid(self):
		return

	face_idle.play("face_idle", -1, randf_range(1, 2))

	# 创建随机时间段定时器
	await get_tree(). \
		create_timer(randf_range(blink_min, blink_max)) \
		.timeout
	
	roll_blink()
