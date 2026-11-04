extends base_cat_enemy

class_name shooter_cat_enemy

const BULLET_SCENE := preload("res://bin/scene/base_bullet.tscn")
const ANGRY_FACE := preload("res://bin/assets/image/enemy/cat/angry_face.png")
const MOUTH_OPEN := preload("res://bin/assets/image/enemy/cat/base_mouce_open.png")
const MOUTH_CLOSE := preload("res://bin/assets/image/enemy/cat/base_mouce_close.png")

@export var bullet_system:BulletSystem # 子弹配置资源
@export var fire_interval:float = 1.5 # 发射间隔（秒）
@export var fire_scale:float = 0.8 # 发射时 scale-y 反馈倍率
@export var mouth_open_time:float = 0.2 # 张嘴持续时间

@onready var fire_timer:Timer = $FireTimer
@onready var face_sprite:Sprite2D = $head/face
@onready var mouth_sprite:Sprite2D = $head/mouce

var _base_scale:Vector2

func _ready() -> void:
	super._ready()
	# 枪手猫使用愤怒表情
	face_sprite.texture = ANGRY_FACE
	_default_face = ANGRY_FACE
	mouth_sprite.texture = MOUTH_CLOSE
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
	play_mouth_feedback()
	play_fire_feedback()

func play_mouth_feedback() -> void:
	# 发射时短暂切换为张嘴贴图
	mouth_sprite.texture = MOUTH_OPEN
	await get_tree().create_timer(mouth_open_time).timeout
	if is_instance_valid(mouth_sprite):
		mouth_sprite.texture = MOUTH_CLOSE

func play_fire_feedback() -> void:
	# scale-y 先缩小再恢复，表示发射了一颗子弹
	var tween := create_tween()
	tween.tween_property(self, "scale:y", _base_scale.y * fire_scale, 0.08)
	tween.tween_property(self, "scale:y", _base_scale.y, 0.15)
