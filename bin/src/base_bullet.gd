extends Area2D

class_name base_bullet

@export var bullet_system:BulletSystem # 子弹配置资源

var direction:Vector2 = Vector2.RIGHT # 飞行方向
var _traveled:float = 0.0 # 已飞行距离
var _fading:bool = false # 是否正在渐出

func _ready() -> void:
	area_entered.connect(_on_area_entered)
	# 超过存活时间后渐出销毁
	if bullet_system and bullet_system.lifetime > 0:
		get_tree().create_timer(bullet_system.lifetime).timeout.connect(fade_out)

func _process(delta: float) -> void:
	if bullet_system == null or _fading:
		return
	var step:Vector2 = direction * bullet_system.speed * delta
	position += step
	_traveled += step.length()
	# 超出射程后渐出消失
	if _traveled >= bullet_system.max_range:
		fade_out()

func fade_out() -> void:
	if _fading:
		return
	_fading = true
	monitoring = false # 渐出期间不再判定命中
	# 透明度渐出后销毁
	var tween := create_tween()
	tween.tween_property(self, "modulate:a", 0.0, 0.25)
	tween.finished.connect(queue_free)

func _on_area_entered(area: Area2D) -> void:
	if bullet_system == null:
		return
	# 命中拥有 hit 方法的目标则造成伤害
	var target := area.get_parent()
	if target and target.has_method("hit"):
		target.hit(bullet_system.damage)
	queue_free()
