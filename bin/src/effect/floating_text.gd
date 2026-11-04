extends Label

class_name floating_text

func setup(value:String, color:Color = Color(1, 0.15, 0.15)) -> void:
	text = value
	add_theme_color_override("font_color", color)
	# 随机旋转并掉落渐出
	rotation = randf_range(-0.25, 0.25)
	var drop:Vector2 = global_position + Vector2(randf_range(-20.0, 20.0), 90.0)
	var tween := create_tween()
	tween.set_parallel(true)
	tween.tween_property(self, "global_position", drop, 0.7)
	tween.tween_property(self, "rotation", rotation + randf_range(-1.2, 1.2), 0.7)
	tween.tween_property(self, "modulate:a", 0.0, 0.7)
	tween.finished.connect(queue_free)
