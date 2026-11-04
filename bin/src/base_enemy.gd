extends base_life

class_name base_enemy

@export var dtarget:base_obj

func _ready() -> void:
	pass

func _process(delta: float) -> void:
	if(not dtarget):return

	var vec:Vector2
	vec = dtarget.position - position
	vec = vec.normalized()
	position += delta * life_system.speed * vec
