extends Node2D

class_name enemy_manager

@export var player:player

var enemies:Array[Node]

func init_ememy_info():
	for e in enemies:
		e.dtarget = player

func init() -> void:
	enemies = get_children()
	init_ememy_info()