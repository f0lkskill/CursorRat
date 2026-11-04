extends base_cat_enemy

class_name shooter_cat_enemy

const BULLET_SCENE := preload("res://bin/scene/base_bullet.tscn")

@export var bullet_system:BulletSystem # 子弹配置资源
@export var fire_interval:float = 1.5 # 发射间隔（秒）
@export var fire_scale:float = 0.8 # 发射时 scale-y 反馈倍率

@onready var fire_timer:Timer = $FireTimer

var _base_scale:Vector2

func _ready() -> void:
	super._ready()
	_base_scale = scale
	fire_timer.wait_time = fire_interval
	fire_timer.timeout.connect(_fire)
	fire_timer.start()

func _fire() -> void:
	if dtarget == null or bullet_system == null:
		return
	# 朝玩家方向发射子弹
	var dir:Vector2 = (dtarget.global_position - global_position).normalized()
	var bullet:base_bullet = BULLET_SCENE.instantiate()
	bullet.bullet_system = bullet_system
	bullet.direction = dir
	get_tree().current_scene.add_child(bullet)
	bullet.global_position = global_position
	play_fire_feedback()

func play_fire_feedback() -> void:
	# scale-y 先缩小再恢复，表示发射了一颗子弹
	var tween := create_tween()
	tween.tween_property(self, "scale:y", _base_scale.y * fire_scale, 0.08)
	tween.tween_property(self, "scale:y", _base_scale.y, 0.15)
