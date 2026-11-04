extends Node2D

@export var player:player
@export var enemies:enemy_manager


func _ready() -> void:
	enemies.player = player
	enemies.init()

