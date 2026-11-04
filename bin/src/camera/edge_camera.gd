extends Camera2D

class_name edge_camera

@export var edge_margin:float = 60.0 # 边缘感应范围
@export var impulse_strength:float = 1400.0 # 撞击冲量
@export var friction:float = 2.5 # 摩擦系数，越大减速越快
@export var max_speed:float = 2000.0 # 最大移动速度
@export var start_at_view_center:bool = true # 初始定位到画面中心

var _velocity:Vector2 = Vector2.ZERO
var _last_edge_dir:Vector2 = Vector2.ZERO
var _last_mouse:Vector2 = Vector2.ZERO

func _ready() -> void:
	if start_at_view_center:
		position = get_viewport().get_visible_rect().size * 0.5
	make_current()
	_last_mouse = get_viewport().get_mouse_position()

func _process(delta: float) -> void:
	var view_size := get_viewport().get_visible_rect().size
	var mouse := get_viewport().get_mouse_position()
	var edge_dir := _get_edge_direction(mouse, view_size)

	# 鼠标撞到新的边缘时施加冲量
	if edge_dir != Vector2.ZERO and edge_dir != _last_edge_dir:
		var mouse_speed:float = (mouse - _last_mouse).length() / maxf(delta, 0.001)
		var depth := _get_edge_depth(mouse, view_size)
		var impact:float = impulse_strength * (0.6 + depth / edge_margin) + mouse_speed * 0.5
		_velocity += edge_dir * impact
		_velocity = _velocity.limit_length(max_speed)
	_last_edge_dir = edge_dir

	# 参考摩擦模型逐渐减速，形成惯性滑动
	_velocity *= exp(-friction * delta)
	if _velocity.length() < 1.0:
		_velocity = Vector2.ZERO
	position += _velocity * delta
	_last_mouse = mouse

func _get_edge_direction(mouse:Vector2, view_size:Vector2) -> Vector2:
	var dir := Vector2.ZERO
	if mouse.x <= edge_margin:
		dir.x = -1.0
	elif mouse.x >= view_size.x - edge_margin:
		dir.x = 1.0
	if mouse.y <= edge_margin:
		dir.y = -1.0
	elif mouse.y >= view_size.y - edge_margin:
		dir.y = 1.0
	return dir.normalized() if dir != Vector2.ZERO else dir

func _get_edge_depth(mouse:Vector2, view_size:Vector2) -> float:
	var depth := 0.0
	depth = max(depth, edge_margin - mouse.x)
	depth = max(depth, mouse.x - (view_size.x - edge_margin))
	depth = max(depth, edge_margin - mouse.y)
	depth = max(depth, mouse.y - (view_size.y - edge_margin))
	return clampf(depth, 0.0, edge_margin)
