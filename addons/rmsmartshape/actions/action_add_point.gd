extends SS2D_Action

## ActionAddPoint

const ActionCloseShape := preload("res://addons/rmsmartshape/actions/action_close_shape.gd")
var _close_shape: ActionCloseShape

const ActionInvertOrientation := preload("res://addons/rmsmartshape/actions/action_invert_orientation.gd")
var _invert_orientation: ActionInvertOrientation

var _shape: SS2D_Shape_Base
var _key: int
var _position: Vector2
var _idx: int


func _init(shape: SS2D_Shape_Base, position: Vector2, idx: int = -1) -> void:
	_shape = shape
	_position = position
	_idx = _shape.adjust_add_point_index(idx)
	_key = _shape.reserve_key()
	_invert_orientation = ActionInvertOrientation.new(shape)
	_close_shape = ActionCloseShape.new(shape)


func get_name() -> String:
	return "Add Point at (%d, %d)" % [_position.x, _position.y]


func do() -> void:
	_shape.begin_update()
	_key = _shape.add_point(_position, _idx, _key)
	_close_shape.do()
	_invert_orientation.do()
	_shape.end_update()


func undo() -> void:
	_shape.begin_update()
	_invert_orientation.undo()
	_close_shape.undo()
	_shape.remove_point(_key)
	_shape.end_update()


func get_key() -> int:
	return _key
