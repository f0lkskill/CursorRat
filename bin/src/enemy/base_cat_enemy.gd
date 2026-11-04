extends base_enemy

class_name base_cat_enemy

const HIT_FACE := preload("res://bin/assets/image/enemy/cat/base_hited_face.png")

@export var hit_face_time:float = 0.4 # 受伤表情持续时间

var _default_face:Texture2D
var _hit_face_version:int = 0

func _ready() -> void:
	super._ready()
	var face := _get_face_sprite()
	if face:
		_default_face = face.texture

func hit(damage:float):
	super.hit(damage)
	show_hit_face()

func show_hit_face() -> void:
	var face := _get_face_sprite()
	if face == null:
		return
	_hit_face_version += 1
	var version := _hit_face_version
	# 受伤时短暂切换受伤表情，随后恢复默认表情
	face.texture = HIT_FACE
	await get_tree().create_timer(hit_face_time).timeout
	if is_instance_valid(face) and version == _hit_face_version:
		face.texture = _default_face

func _get_face_sprite() -> Sprite2D:
	return get_node_or_null("head/face") as Sprite2D
