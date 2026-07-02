extends Node

@onready var sprite: Node2D = $"../sprite"
@onready var shape: CollisionShape2D = $"../shape"
@onready var light_occluder: LightOccluder2D = $"../light_occluder"

# 透明像素阈值 (0-255)，低于此值的像素视为透明
@export var alpha_threshold: int = 10

# 采样精度：每多少像素采样一次（值越大精度越低但性能越好）
@export var sample_step: int = 2

func _ready() -> void:
	var texture: Texture2D = get_current_texture()
	if texture == null:
		push_warning("sprite没有设置纹理！")
		return

	var sprite_scale: Vector2 = get_sprite_scale()
	var image: Image = texture.get_image()
	if image == null:
		push_warning("无法获取图像数据！")
		return

	# 设置碰撞形状（剔除透明像素）
	set_collision_shape_from_image(image, sprite_scale)

	# 设置光遮挡器（剔除透明像素）
	set_light_occluder_from_image(image, sprite_scale)

# 获取精灵当前显示的纹理
func get_current_texture() -> Texture2D:
	if sprite is AnimatedSprite2D:
		var anim_sprite: AnimatedSprite2D = sprite as AnimatedSprite2D
		if anim_sprite.sprite_frames != null and anim_sprite.sprite_frames.get_frame_count(anim_sprite.animation) > 0:
			return anim_sprite.sprite_frames.get_frame_texture(anim_sprite.animation, anim_sprite.frame)
	elif sprite is Sprite2D:
		return (sprite as Sprite2D).texture
	return null

# 获取精灵的缩放
func get_sprite_scale() -> Vector2:
	return sprite.scale

# 根据图像alpha通道生成碰撞形状
func set_collision_shape_from_image(image: Image, scale: Vector2) -> void:
	var polygon: PackedVector2Array = trace_alpha_contour(image, alpha_threshold, sample_step)
	if polygon.size() < 3:
		# 如果无法生成轮廓，退回到简单矩形
		var rect_shape: RectangleShape2D = RectangleShape2D.new()
		rect_shape.size = scale * Vector2(image.get_size())
		shape.shape = rect_shape
		return

	# 应用缩放
	var scaled_polygon: PackedVector2Array = PackedVector2Array()
	var center: Vector2 = Vector2(image.get_size()) / 2.0
	for p in polygon:
		scaled_polygon.append((p - center) * scale)

	# 使用ConvexPolygonShape2D作为碰撞形状
	var polygon_shape: ConvexPolygonShape2D = ConvexPolygonShape2D.new()
	polygon_shape.points = scaled_polygon
	shape.shape = polygon_shape

# 根据图像alpha通道生成光遮挡器
func set_light_occluder_from_image(image: Image, scale: Vector2) -> void:
	var polygon: PackedVector2Array = trace_alpha_contour(image, alpha_threshold, sample_step)
	if polygon.size() < 3:
		# 退回到简单矩形
		var size: Vector2 = Vector2(image.get_size()) * scale
		var half: Vector2 = size / 2.0
		var rect_polygon: PackedVector2Array = PackedVector2Array()
		rect_polygon.append(Vector2(-half.x, -half.y))
		rect_polygon.append(Vector2(half.x, -half.y))
		rect_polygon.append(Vector2(half.x, half.y))
		rect_polygon.append(Vector2(-half.x, half.y))
		light_occluder.occluder.polygon = rect_polygon
		return

	# 应用缩放并居中
	var scaled_polygon: PackedVector2Array = PackedVector2Array()
	var center: Vector2 = Vector2(image.get_size()) / 2.0
	for p in polygon:
		scaled_polygon.append((p - center) * scale)

	light_occluder.occluder.polygon = scaled_polygon

# 基于marching squares算法追踪alpha轮廓（简化版）
func trace_alpha_contour(image: Image, threshold: int, step: int) -> PackedVector2Array:
	var width: int = image.get_width()
	var height: int = image.get_height()
	step = clampi(step, 1, 16)

	# 生成像素采样网格，标记哪些是不透明的
	var grid_width: int = (width + step - 1) / step
	var grid_height: int = (height + step - 1) / step
	var opaque_cells: Array = []
	opaque_cells.resize(grid_width * grid_height)

	for gy in range(grid_height):
		for gx in range(grid_width):
			var px: int = gx * step
			var py: int = gy * step
			if px >= width or py >= height:
				opaque_cells[gy * grid_width + gx] = false
				continue
			var color: Color = image.get_pixel(px, py)
			opaque_cells[gy * grid_width + gx] = color.a > (float(threshold) / 255.0)

	# 从图像顶部开始寻找轮廓起点
	var start_pos: Vector2i = Vector2i(-1, -1)
	for gy in range(grid_height):
		for gx in range(grid_width):
			if opaque_cells[gy * grid_width + gx]:
				start_pos = Vector2i(gx, gy)
				break
		if start_pos.x != -1:
			break

	if start_pos.x == -1:
		return PackedVector2Array()

	# 使用方格行走算法追踪轮廓
	# 方向: 0=右, 1=下, 2=左, 3=上
	var directions: Array = [
		Vector2i(1, 0),
		Vector2i(0, 1),
		Vector2i(-1, 0),
		Vector2i(0, -1)
	]

	var polygon: PackedVector2Array = PackedVector2Array()
	var current: Vector2i = start_pos
	var direction: int = 0  # 初始向右
	var visited: Dictionary = {}
	var max_iterations: int = grid_width * grid_height * 4
	var iterations: int = 0

	while iterations < max_iterations:
		var key: String = "%d,%d,%d" % [current.x, current.y, direction]
		if visited.has(key):
			break
		visited[key] = true
		iterations += 1

		# 检查右侧是否有不透明单元格
		var right_dir: int = (direction + 1) % 4
		var right_cell: Vector2i = current + directions[right_dir]
		var front_cell: Vector2i = current + directions[direction]

		var has_right: bool = false
		if right_cell.x >= 0 and right_cell.x < grid_width and right_cell.y >= 0 and right_cell.y < grid_height:
			has_right = bool(opaque_cells[right_cell.y * grid_width + right_cell.x])

		var has_front: bool = false
		if front_cell.x >= 0 and front_cell.x < grid_width and front_cell.y >= 0 and front_cell.y < grid_height:
			has_front = bool(opaque_cells[front_cell.y * grid_width + front_cell.x])

		var step_f: float = float(step)
		if has_right:
			# 向右转
			var d: Vector2i = directions[direction]
			var dr: Vector2i = directions[right_dir]
			var corner: Vector2 = Vector2(
				(float(current.x) + 0.5) * step_f,
				(float(current.y) + 0.5) * step_f
			) + Vector2(
				(float(d.x) + float(dr.x)) * step_f * 0.5,
				(float(d.y) + float(dr.y)) * step_f * 0.5
			)
			polygon.append(corner)
			direction = right_dir
		elif not has_front:
			# 向左转
			var d: Vector2i = directions[direction]
			var dr: Vector2i = directions[right_dir]
			var corner: Vector2 = Vector2(
				(float(current.x) + 0.5) * step_f,
				(float(current.y) + 0.5) * step_f
			) + Vector2(
				(float(d.x) - float(dr.x)) * step_f * 0.5,
				(float(d.y) - float(dr.y)) * step_f * 0.5
			)
			polygon.append(corner)
			direction = (direction + 3) % 4
		else:
			# 继续前进
			current = front_cell

	# 如果回到起点附近则完成
	return polygon
