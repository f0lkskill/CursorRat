extends Camera2D

class_name edge_camera

@export var edge_margin:float = 60.0 # 边缘感应范围
@export var impulse_strength:float = 1400.0 # 撞击冲量
@export var friction:float = 6.0 # 摩擦系数，越大减速越快
@export var bounce:float = 0.4 # 撞到显示器边界的反弹系数
@export var max_speed:float = 2000.0 # 最大移动速度
@export var use_screen_as_map:bool = true # 地图尺寸取整个显示器
@export var map_size:Vector2 = Vector2(1920, 1080) # 地图尺寸
@export var center_window_on_start:bool = true # 初始把窗口放在显示器中央

# 窗口客户区左上角在显示器（地图）中的坐标，真正移动的是这个窗口
var _window_pos:Vector2 = Vector2.ZERO
var _velocity:Vector2 = Vector2.ZERO
var _window_size:Vector2 = Vector2.ZERO
var _half_view:Vector2 = Vector2.ZERO
var _min_window:Vector2 = Vector2.ZERO
var _max_window:Vector2 = Vector2.ZERO
var _last_edge_dir:Vector2 = Vector2.ZERO
var _last_mouse:Vector2 = Vector2.ZERO
var _last_os_mouse:Vector2 = Vector2.ZERO
var _last_applied_window_pos:Vector2i = Vector2i(-999999, -999999)

func _ready() -> void:
	if use_screen_as_map:
		var screen := DisplayServer.screen_get_size()
		if screen.x > 0 and screen.y > 0:
			map_size = Vector2(screen)
	_update_window_metrics()

	if center_window_on_start:
		_window_pos = (map_size - _window_size) * 0.5
	else:
		_window_pos = Vector2(DisplayServer.window_get_position())
	_window_pos = _window_pos.clamp(_min_window, _max_window)

	# 摄像机只负责把窗口这块“洞”对应到地图上，真正移动的是游戏窗口
	limit_left = 0
	limit_top = 0
	limit_right = int(map_size.x)
	limit_bottom = int(map_size.y)
	_apply_window_position()
	_sync_camera()
	make_current()

	_last_mouse = get_viewport().get_mouse_position()
	_last_os_mouse = Vector2(DisplayServer.mouse_get_position())
	_last_edge_dir = _get_edge_direction(_last_mouse)

func _process(delta: float) -> void:
	var view_size := get_viewport().get_visible_rect().size
	if view_size != _window_size:
		_update_window_metrics()
		_window_pos = _window_pos.clamp(_min_window, _max_window)
		_apply_window_position()
		_sync_camera()

	var mouse := get_viewport().get_mouse_position()
	var os_mouse := Vector2(DisplayServer.mouse_get_position())
	var edge_dir := _get_edge_direction(mouse)

	# 鼠标撞到新的窗口边缘时，把冲量施加到整个游戏窗口上
	if edge_dir != Vector2.ZERO and edge_dir != _last_edge_dir:
		var mouse_speed:float = (os_mouse - _last_os_mouse).length() / maxf(delta, 0.001)
		var depth := _get_edge_depth(mouse)
		var impact:float = impulse_strength * (0.6 + depth / edge_margin) + mouse_speed * 0.5
		_velocity += edge_dir * impact
		_velocity = _velocity.limit_length(max_speed)
	_last_edge_dir = edge_dir

	# 参考摩擦模型逐渐减速，形成惯性滑动
	_velocity *= exp(-friction * delta)
	if _velocity.length() < 1.0:
		_velocity = Vector2.ZERO

	if _velocity != Vector2.ZERO:
		_window_pos += _velocity * delta
		_bounce_at_map_edges()
		_apply_window_position()
	_sync_camera()

	_last_mouse = mouse
	_last_os_mouse = os_mouse

func _update_window_metrics() -> void:
	_window_size = get_viewport().get_visible_rect().size
	_half_view = _window_size * 0.5
	_min_window = Vector2.ZERO
	_max_window = map_size - _window_size
	# 窗口比显示器还大时固定在中心
	if _max_window.x < _min_window.x:
		var center_x := (map_size.x - _window_size.x) * 0.5
		_min_window.x = center_x
		_max_window.x = center_x
	if _max_window.y < _min_window.y:
		var center_y := (map_size.y - _window_size.y) * 0.5
		_min_window.y = center_y
		_max_window.y = center_y

func _apply_window_position() -> void:
	var target := Vector2i(_window_pos.round())
	if target == _last_applied_window_pos:
		return
	DisplayServer.window_set_position(target)
	_last_applied_window_pos = target

func _sync_camera() -> void:
	# 窗口移动后反向同步摄像机，让地图内容始终固定在显示器坐标上
	position = _window_pos + _half_view

func _bounce_at_map_edges() -> void:
	if _window_pos.x <= _min_window.x:
		_window_pos.x = _min_window.x
		if _velocity.x < 0.0:
			_velocity.x = -_velocity.x * bounce
	elif _window_pos.x >= _max_window.x:
		_window_pos.x = _max_window.x
		if _velocity.x > 0.0:
			_velocity.x = -_velocity.x * bounce
	if _window_pos.y <= _min_window.y:
		_window_pos.y = _min_window.y
		if _velocity.y < 0.0:
			_velocity.y = -_velocity.y * bounce
	elif _window_pos.y >= _max_window.y:
		_window_pos.y = _max_window.y
		if _velocity.y > 0.0:
			_velocity.y = -_velocity.y * bounce

func _get_edge_direction(mouse:Vector2) -> Vector2:
	var dir := Vector2.ZERO
	if mouse.x <= edge_margin:
		dir.x = -1.0
	elif mouse.x >= _window_size.x - edge_margin:
		dir.x = 1.0
	if mouse.y <= edge_margin:
		dir.y = -1.0
	elif mouse.y >= _window_size.y - edge_margin:
		dir.y = 1.0
	return dir.normalized() if dir != Vector2.ZERO else dir

func _get_edge_depth(mouse:Vector2) -> float:
	var depth := 0.0
	depth = max(depth, edge_margin - mouse.x)
	depth = max(depth, mouse.x - (_window_size.x - edge_margin))
	depth = max(depth, edge_margin - mouse.y)
	depth = max(depth, mouse.y - (_window_size.y - edge_margin))
	return clampf(depth, 0.0, edge_margin)
