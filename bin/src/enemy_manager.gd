extends Node2D

class_name enemy_manager

@export var player:player
@export var click_damage:float = 10.0 # 点击造成的伤害

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
		var cp:Vector2 = get_global_mouse_position()
		# 遍历敌人，检测点击是否落在敌人头部范围内
		for e in enemies:
			if not is_instance_valid(e):
				continue
			var head:Sprite2D = e.get_node_or_null("head")
			if head == null or head.texture == null:
				continue
			# 按头部贴图大小计算点击判定矩形
			var size:Vector2 = head.texture.get_size() * head.global_scale
			var rect := Rect2(head.global_position - size * 0.5, size)
			if rect.has_point(cp):
				e.hit(click_damage) # 触发受击
