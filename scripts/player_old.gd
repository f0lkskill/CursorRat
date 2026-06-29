extends CharacterBody2D

func _ready() -> void:

	# 加载贴图并通过Image缩小
	var image = Image.load_from_file("res://assets/cursor.png")
	if image:
		image.resize(60, 75, Image.INTERPOLATE_BILINEAR)
		var cursor_tex = ImageTexture.create_from_image(image)
		Input.set_custom_mouse_cursor(cursor_tex)

func _input(event: InputEvent) -> void:
	position = get_global_mouse_position()
