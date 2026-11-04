extends Node2D

class_name enemy_manager

@export var player:player

var enemies:Array

func init_ememy_info():
	for e in enemies:
		e.dtarget = player

func init() -> void:
	enemies = get_children()
	init_ememy_info()

func _input(event: InputEvent) -> void:
	# 只处理鼠标左键按下
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		_try_hit(get_global_mouse_position())

func _try_hit(cp:Vector2) -> void:
	if player == null:
		return
	# 遍历敌人，检测点击是否落在猫猫身体范围内（头部与双耳）
	for e in enemies:
		if not is_instance_valid(e):
			continue
		var rect := Rect2()
		var has_rect := false
		for path in ["head", "head/right_ear", "head/left_ear"]:
			var sprite := e.get_node_or_null(path) as Sprite2D
			if sprite == null or sprite.texture == null:
				continue
			var size:Vector2 = sprite.texture.get_size() * sprite.global_scale
			var r := Rect2(sprite.global_position - size * 0.5, size)
			rect = r if not has_rect else rect.merge(r)
			has_rect = true
		if has_rect and rect.has_point(cp):
			e.hit(player.damage) # 使用玩家伤害触发受击
