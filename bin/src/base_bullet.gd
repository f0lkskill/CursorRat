extends Area2D

class_name base_bullet

@export var bullet_system:BulletSystem # 子弹配置资源

var direction:Vector2 = Vector2.RIGHT # 飞行方向

func _ready() -> void:
	area_entered.connect(_on_area_entered)
	# 超过存活时间后自动销毁
	if bullet_system and bullet_system.lifetime > 0:
		get_tree().create_timer(bullet_system.lifetime).timeout.connect(queue_free)

func _process(delta: float) -> void:
	if bullet_system == null:
		return
	position += direction * bullet_system.speed * delta

func _on_area_entered(area: Area2D) -> void:
	if bullet_system == null:
		return
	# 命中拥有 hit 方法的目标则造成伤害
	var target := area.get_parent()
	if target and target.has_method("hit"):
		target.hit(bullet_system.damage)
	queue_free()
