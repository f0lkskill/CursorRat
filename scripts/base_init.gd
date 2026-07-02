extends Node

@onready var sprite: Node2D = $"../sprite"
@onready var shape: CollisionShape2D = $"../shape"
@onready var light_occluder: LightOccluder2D = $"../light_occluder"

# 透明像素阈值 (0-255)，高于此值视为不透明
@export var alpha_threshold: int = 250

# 网格采样尺寸（像素）：每个网格cell_size × cell_size，越大性能越好
@export var cell_size: int = 21

# 是否启用调试信息
@export var debug_mode: bool = false

# 缓存：上一次的动画和帧，用于检测变化
var _last_animation: String = ""
var _last_frame: int = -1

# 获取精灵的offset（AnimatedSprite2D/Sprite2D都有这个属性）
func _get_sprite_offset() -> Vector2:
	if sprite is AnimatedSprite2D:
		return (sprite as AnimatedSprite2D).offset
	elif sprite is Sprite2D:
		return (sprite as Sprite2D).offset
	return Vector2.ZERO

func _ready() -> void:
	_update_shapes()

func _process(delta: float) -> void:
	# 检测AnimatedSprite2D的帧变化，动态更新碰撞
	if sprite is AnimatedSprite2D:
		var anim_sprite: AnimatedSprite2D = sprite as AnimatedSprite2D
		var cur_anim: String = anim_sprite.animation
		var cur_frame: int = anim_sprite.frame

		if cur_anim != _last_animation or cur_frame != _last_frame:
			_last_animation = cur_anim
			_last_frame = cur_frame
			_update_shapes()

# 核心：根据当前精灵帧，重新计算并更新碰撞形状和光遮挡器
func _update_shapes() -> void:
	var texture: Texture2D = get_current_texture()
	if texture == null:
		return

	var image: Image = texture.get_image()
	if image == null:
		return

	var image_size: Vector2 = Vector2(image.get_size())
	var sprite_scale: Vector2 = sprite.scale
	var sprite_offset: Vector2 = _get_sprite_offset()

	# ⭐ 关键修正：精灵的实际绘制位置 = position + offset * scale
	# 精灵的offset会随scale一起缩放
	var effective_pos: Vector2 = sprite.position + sprite_offset * sprite_scale

	# 第一步：网格采样
	var grid: Dictionary = sample_grid(image, alpha_threshold, cell_size)

	if grid.size() == 0:
		# 没有不透明内容，退回完整矩形
		var full_size: Vector2 = image_size * sprite_scale
		var rect_shape: RectangleShape2D = RectangleShape2D.new()
		rect_shape.size = full_size
		shape.shape = rect_shape
		shape.position = effective_pos

		var half: Vector2 = full_size * 0.5
		var rect_poly: PackedVector2Array = PackedVector2Array()
		rect_poly.append(Vector2(-half.x, -half.y))
		rect_poly.append(Vector2(half.x, -half.y))
		rect_poly.append(Vector2(half.x, half.y))
		rect_poly.append(Vector2(-half.x, half.y))
		light_occluder.occluder.polygon = rect_poly
		light_occluder.position = effective_pos
		return

	# 第二步：从网格提取边界点生成凸包
	var convex_hull_points: PackedVector2Array = build_convex_hull(grid, cell_size)

	if debug_mode:
		print("[base_init] 图像: ", image_size, " 网格: ", grid.size(), " 凸包: ", convex_hull_points.size(), "点 offset: ", sprite_offset)

	# 将像素坐标转换为以图像中心为原点的缩放坐标
	var image_center: Vector2 = image_size * 0.5
	var scaled_hull: PackedVector2Array = PackedVector2Array()
	for i in range(convex_hull_points.size()):
		scaled_hull.append((convex_hull_points[i] - image_center) * sprite_scale)

	# 第三步：设置碰撞形状（凸包多边形）
	setup_collision_convex(scaled_hull, effective_pos)

	# 第四步：设置光遮挡器（凸包多边形）
	light_occluder.occluder.polygon = scaled_hull
	light_occluder.position = effective_pos

# 获取精灵当前显示的纹理
func get_current_texture() -> Texture2D:
	if sprite is AnimatedSprite2D:
		var anim_sprite: AnimatedSprite2D = sprite as AnimatedSprite2D
		if anim_sprite.sprite_frames != null and anim_sprite.sprite_frames.get_frame_count(anim_sprite.animation) > 0:
			return anim_sprite.sprite_frames.get_frame_texture(anim_sprite.animation, anim_sprite.frame)
	elif sprite is Sprite2D:
		return (sprite as Sprite2D).texture
	return null

# 读取单个像素的alpha值（0-255整数）
func get_pixel_alpha(image: Image, x: int, y: int) -> int:
	var width: int = image.get_width()
	var height: int = image.get_height()
	if x < 0 or y < 0 or x >= width or y >= height:
		return 0

	# 先尝试 get_pixel
	var c: Color = image.get_pixel(x, y)
	if c.a > 0.01:
		return int(c.a * 255.0)

	# 失败则直接读字节
	var fmt: int = image.get_format()
	var bytes_per_pixel: int = 4
	var alpha_offset: int = 3

	if fmt == Image.FORMAT_RGB8:
		bytes_per_pixel = 3
		alpha_offset = -1
	elif fmt == Image.FORMAT_RGBA8:
		bytes_per_pixel = 4
		alpha_offset = 3
	elif fmt == Image.FORMAT_RGBA4444:
		bytes_per_pixel = 2
		alpha_offset = 1
	elif fmt == Image.FORMAT_L8:
		bytes_per_pixel = 1
		alpha_offset = -1
	elif fmt == Image.FORMAT_LA8:
		bytes_per_pixel = 2
		alpha_offset = 1

	if alpha_offset < 0:
		return 255

	var data: PackedByteArray = image.get_data()
	if data.is_empty():
		return 0

	var byte_index: int = (y * width + x) * bytes_per_pixel + alpha_offset
	if byte_index < 0 or byte_index >= data.size():
		return 0

	return int(data[byte_index])

# 网格采样：把图像分成 cell_size × cell_size 的网格
# 每个网格采样几个点，若不透明点数超过阈值则标记该格为有内容
# 返回 Dictionary:  key="gx,gy" -> Vector2(网格中心像素坐标)
func sample_grid(image: Image, threshold: int, cs: int) -> Dictionary:
	var width: int = image.get_width()
	var height: int = image.get_height()

	var grid: Dictionary = {}
	var gx: int = 0

	while gx * cs < width:
		var gy: int = 0
		while gy * cs < height:
			var opaque: int = 0
			var total: int = 0

			# 在网格内采样5个点（4角+中心）
			var sx0: int = gx * cs
			var sy0: int = gy * cs
			var sx1: int = min(sx0 + cs, width)
			var sy1: int = min(sy0 + cs, height)

			var samples: Array = [
				Vector2(sx0, sy0),
				Vector2(sx1 - 1, sy0),
				Vector2(sx0, sy1 - 1),
				Vector2(sx1 - 1, sy1 - 1),
				Vector2((sx0 + sx1) / 2, (sy0 + sy1) / 2)
			]

			for s in samples:
				var px: int = int(s.x)
				var py: int = int(s.y)
				total += 1
				if get_pixel_alpha(image, px, py) > threshold:
					opaque += 1

			# 至少有1个采样点不透明，就认为该网格有内容
			if opaque > 0:
				var cx: float = float(sx0 + sx1) * 0.5
				var cy: float = float(sy0 + sy1) * 0.5
				grid[str(gx) + "," + str(gy)] = Vector2(cx, cy)

			gy += 1
		gx += 1

	return grid

# 凸包辅助：叉积（判断三点转向）
func _hull_cross(o: Vector2, a: Vector2, b: Vector2) -> float:
	return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x)

# 凸包辅助：两点比较（用于排序）
func _hull_less(a: Vector2, b: Vector2) -> bool:
	return a.x < b.x or (a.x == b.x and a.y < b.y)

# 凸包辅助：构建边界框多边形
func _hull_bbox(points: Array) -> PackedVector2Array:
	if points.size() == 0:
		var empty: PackedVector2Array = PackedVector2Array()
		empty.append(Vector2(0, 0))
		empty.append(Vector2(100, 0))
		empty.append(Vector2(100, 100))
		empty.append(Vector2(0, 100))
		return empty
	var min_v: Vector2 = points[0]
	var max_v: Vector2 = points[0]
	for p in points:
		if p.x < min_v.x: min_v.x = p.x
		if p.y < min_v.y: min_v.y = p.y
		if p.x > max_v.x: max_v.x = p.x
		if p.y > max_v.y: max_v.y = p.y
	var result: PackedVector2Array = PackedVector2Array()
	result.append(Vector2(min_v.x, min_v.y))
	result.append(Vector2(max_v.x, min_v.y))
	result.append(Vector2(max_v.x, max_v.y))
	result.append(Vector2(min_v.x, max_v.y))
	return result

# 从网格点构建凸包多边形（Andrew's Monotone Chain 算法）
func build_convex_hull(grid: Dictionary, cs: int) -> PackedVector2Array:
	# 把网格中心坐标收集到数组
	var points: Array = []
	for key in grid.keys():
		points.append(grid[key])

	if points.size() < 3:
		return _hull_bbox(points)

	# 按 x 排序（手工排序，避免lambda）
	points.sort_custom(_hull_less)

	# 构建下包（从左到右）
	var lower: Array = []
	for p in points:
		while lower.size() >= 2 and _hull_cross(lower[lower.size() - 2], lower[lower.size() - 1], p) <= 0:
			lower.remove_at(lower.size() - 1)
		lower.append(p)

	# 构建上包（从右到左）
	var upper: Array = []
	for i in range(points.size() - 1, -1, -1):
		var p: Vector2 = points[i]
		while upper.size() >= 2 and _hull_cross(upper[upper.size() - 2], upper[upper.size() - 1], p) <= 0:
			upper.remove_at(upper.size() - 1)
		upper.append(p)

	# 组合：下包 + 上包（去掉首尾重复的点）
	var hull: Array = []
	for i in range(lower.size() - 1):
		hull.append(lower[i])
	for i in range(upper.size() - 1):
		hull.append(upper[i])

	# 转换为 PackedVector2Array
	var result: PackedVector2Array = PackedVector2Array()
	for p in hull:
		result.append(p)

	if result.size() < 3:
		return _hull_bbox(points)

	return result

# 设置碰撞形状：用凸包多边形
func setup_collision_convex(hull: PackedVector2Array, offset: Vector2) -> void:
	if hull.size() < 3:
		return

	var poly_shape: ConvexPolygonShape2D = ConvexPolygonShape2D.new()
	poly_shape.points = hull
	shape.shape = poly_shape
	shape.position = offset

	if debug_mode:
		print("[base_init] 碰撞多边形: ", hull.size(), "个点")
