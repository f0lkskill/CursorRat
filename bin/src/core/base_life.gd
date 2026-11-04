extends base_obj

class_name base_life

const FLOATING_TEXT := preload("res://bin/scene/floating_text.tscn")

@export var life_system:LifeSystem

func initLifeSystem():
    # 初始化生命系统
    life_system.current_hp = life_system.max_hp

func hit(damage:float):
    # 扣血并限制最低为 0
    life_system.current_hp = max(life_system.current_hp - damage, 0.0)
    print("Hit! %s" % damage)
    show_hit_feedback(damage)

func show_hit_feedback(damage:float):
    # 弹出红色伤害文本，旋转掉落并渐出
    if get_tree().current_scene == null:
        return
    var label:floating_text = FLOATING_TEXT.instantiate()
    get_tree().current_scene.add_child(label)
    label.global_position = global_position + Vector2(-50, -80)
    label.setup(str(int(damage)))

func _ready() -> void:
    initLifeSystem()