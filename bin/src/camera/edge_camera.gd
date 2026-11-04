extends Camera2D

class_name edge_camera

@export var edge_margin:float = 60.0 # 边缘感应范围
@export var impulse_strength:float = 1400.0 # 撞击冲量
@export var friction:float = 6.0 # 摩擦系数，越大减速越快
@export var bounce:float = 0.4 # 撞到地图边界的反弹系数
@export var max_speed:float = 2000.0 # 最大移动速度
@export var use_screen_as_map:bool = true # 地图尺寸取整个显示器
@export var map_size:Vector2 = Vector2(1920, 1080) # 地图尺寸
@export var start_at_map_center:bool = true # 初始定位到地图中心

var _velocity:Vector2 = Vector2.ZERO
var _last_edge_dir:Vector2 = Vector2.ZERO
var _last_mouse:Vector2 = Vector2.ZERO
var _min_cam:Vector2 = Vector2.ZERO
var _max_cam:Vector2 = Vector2.ZERO

func _ready() -> void:
	if use_screen_as_map:
		var screen := DisplayServer.screen_get_size()
		if screen.x > 0 and screen.y > 0:
			map_size = Vector2(screen)
	# 窗口只是地图上的一层蒙版，限制摄像机在地图范围内
	limit_left = 0
	limit_top = 0
	limit_right = int(map_size.x)
	limit_bottom = int(map_size.y)
	var half_view := get_viewport().get_visible_rect().size * 0.5
	_min_cam = half_view
	_max_cam = map_size - half_view
	# 视野比地图大时固定在中心
	if _min_cam.x > _max_cam.x:
		var center_x := map_size.x * 0.5
		_min_cam.x = center_x
		_max_cam.x = center_x
	if _min_cam.y > _max_cam.y:
		var center_y := map_size.y * 0.5
		_min_cam.y = center_y
		_max_cam.y = center_y
	if start_at_map_center:
		position = map_size * 0.5
	position = position.clamp(_min_cam, _max_cam)
	make_current()
	_last_mouse = get_viewport().get_mouse_position()
	# 避免启动时鼠标正好在边缘而立刻弹动
	_last_edge_dir = _get_edge_direction(_last_mouse, get_viewport().get_visible_rect().size)

func _process(delta: float) -> void:
	var view_size := get_viewport().get_visible_rect().size
	var mouse := get_viewport().get_mouse_position()
	var edge_dir := _get_edge_direction(mouse, view_size)

	# 鼠标撞到新的窗体边缘时施加冲量
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
	_bounce_at_map_edges()
	_last_mouse = mouse

func _bounce_at_map_edges() -> void:
	if position.x <= _min_cam.x:
		position.x = _min_cam.x
		if _velocity.x < 0.0:
			_velocity.x = -_velocity.x * bounce
	elif position.x >= _max_cam.x:
		position.x = _max_cam.x
		if _velocity.x > 0.0:
			_velocity.x = -_velocity.x * bounce
	if position.y <= _min_cam.y:
		position.y = _min_cam.y
		if _velocity.y < 0.0:
			_velocity.y = -_velocity.y * bounce
	elif position.y >= _max_cam.y:
		position.y = _max_cam.y
		if _velocity.y > 0.0:
			_velocity.y = -_velocity.y * bounce

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
