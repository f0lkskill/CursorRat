extends base_enemy

class_name base_cat_enemy

@export var hit_face_time:float = 0.4 # 受伤表情持续时间

@onready var _hit_face_sprite:Sprite2D = $head/hit_face

var _hit_face_tween:Tween

func _ready() -> void:
	super._ready()
	_hit_face_sprite.visible = false
	_hit_face_sprite.modulate.a = 0.0

func hit(damage:float):
	super.hit(damage)
	show_hit_face()

func show_hit_face() -> void:
	# 受伤表情淡入、停留后淡出，避免直接切换过于突兀
	if _hit_face_tween and _hit_face_tween.is_valid():
		_hit_face_tween.kill()
	_hit_face_sprite.visible = true
	_hit_face_sprite.modulate.a = 0.0
	_hit_face_tween = create_tween()
	_hit_face_tween.tween_property(_hit_face_sprite, "modulate:a", 1.0, 0.06)
	_hit_face_tween.tween_interval(hit_face_time)
	_hit_face_tween.tween_property(_hit_face_sprite, "modulate:a", 0.0, 0.15)
	_hit_face_tween.finished.connect(func(): _hit_face_sprite.visible = false)
