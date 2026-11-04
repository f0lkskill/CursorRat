extends base_obj

class_name base_life

@export var life_system:LifeSystem

func initLifeSystem():
    # 初始化生命系统
    life_system.current_hp = life_system.max_hp

func hit(damage:float):
    # 扣血并限制最低为 0
    life_system.current_hp = max(life_system.current_hp - damage, 0.0)
    print("Hit! %s" % damage)

func _ready() -> void:
    initLifeSystem()