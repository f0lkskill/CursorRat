extends base_obj

class_name base_life

@export var life_system:LifeSystem

func initLifeSystem():
    # 初始化生命系统
    life_system.current_hp = life_system.max_hp

func _ready() -> void:
    initLifeSystem()