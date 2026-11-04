extends Label

class_name floating_text

func setup(value:String, spawn_position:Vector2, color:Color = Color(1, 0.15, 0.15)) -> void:
	text = value
	add_theme_color_override("font_color", color)
	# 初始位置、角度和坠落偏移都加入随机
	global_position = spawn_position - pivot_offset + Vector2(randf_range(-18.0, 18.0), randf_range(-12.0, 12.0))
	rotation = randf_range(-0.4, 0.4)
	var drop_offset := Vector2(randf_range(-30.0, 30.0), randf_range(70.0, 120.0))
	var spin := randf_range(-1.5, 1.5)
	var duration := randf_range(0.6, 0.9)
	var tween := create_tween()
	tween.set_parallel(true)
	tween.tween_property(self, "global_position", global_position + drop_offset, duration)
	tween.tween_property(self, "rotation", rotation + spin, duration)
	tween.tween_property(self, "modulate:a", 0.0, duration)
	tween.finished.connect(queue_free)
