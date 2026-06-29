extends Node

@onready var sprite: Sprite2D = $"../sprite"
@onready var shape: CollisionShape2D = $"../shape"
@onready var light_occluder: LightOccluder2D = $"../light_occluder"

func _ready() -> void:
	# 设置碰撞形状为精灵的矩形
	var rect_shape: RectangleShape2D = RectangleShape2D.new()
	rect_shape.size = sprite.texture.get_size()
	shape.shape = rect_shape

	# 堆石山
	set_light_occluder_polygon(rect_shape)

func set_light_occluder_polygon(rect_shape: RectangleShape2D) -> void:
	# 设置光遮挡器的形状为精灵的矩形
	var polygon: PackedVector2Array = PackedVector2Array()
	polygon.append(Vector2(rect_shape.get_rect().position.x, rect_shape.get_rect().position.y))
	polygon.append(Vector2(rect_shape.get_rect().position.x + rect_shape.get_rect().size.x, rect_shape.get_rect().position.y))
	polygon.append(Vector2(rect_shape.get_rect().position.x + rect_shape.get_rect().size.x, rect_shape.get_rect().position.y + rect_shape.get_rect().size.y))   
	polygon.append(Vector2(rect_shape.get_rect().position.x, rect_shape.get_rect().position.y + rect_shape.get_rect().size.y))
	polygon.append(Vector2(rect_shape.get_rect().position.x, rect_shape.get_rect().position.y))
	light_occluder.occluder.polygon = polygon
