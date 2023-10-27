namespace Squiggles.Fairy.Camera;

using Godot;
using Squiggles.Core.Error;
using System.Collections.Generic;
using System.Linq;

public partial class GroupingCamera : Camera2D {

  [Export] private string _targetGroup;
  [Export] private float _marginPercent = 0.25f;
  [Export] private float _lerpSpeed = 1.0f;
  [Export] private float _reevalTime = 1.5f;
  [Export] private float _maxZoom = 2f;
  [Export] private float _minZoom = .2f;
  private float _lerpFactor;
  private float _marginFactor;

  private List<Node2D> _group = new();
  private Timer _timer;

  public override void _Ready() {
    _lerpFactor = 1.0f / _lerpSpeed;
    _marginFactor = 1.0f + _marginPercent;
    _group.AddRange(GetTree().GetNodesInGroup(_targetGroup).Cast<Node2D>());
    _timer = new();
    AddChild(_timer);
    _timer.WaitTime = _reevalTime;
    _timer.Start();
    _timer.Timeout += () => _group = GetTree().GetNodesInGroup(_targetGroup).Cast<Node2D>().ToList();

  }

  public override void _Process(double delta) {
    var max = new Vector2(float.MinValue, float.MinValue);
    var min = new Vector2(float.MaxValue, float.MaxValue);
    foreach (var g in _group) {
      max.X = Mathf.Max(max.X, g.GlobalPosition.X);
      max.Y = Mathf.Max(max.Y, g.GlobalPosition.Y);
      min.X = Mathf.Min(min.X, g.GlobalPosition.X);
      min.Y = Mathf.Min(min.Y, g.GlobalPosition.Y);
    }
    var center = (max + min) / 2f;
    var width = max.X - center.X;
    var height = max.Y - center.Y;

    LerpToCenter(min, max, (float)delta);

    if (_group.Count > 1) {
      // only manage zoom for multiple users
      LerpToZoom(min, max, (float)delta);
    }
  }

  private void LerpToCenter(Vector2 min, Vector2 max, float delta) {
    var center = (max + min) / 2f;
    GlobalPosition = GlobalPosition.Lerp(center, _lerpFactor * delta);
  }

  private void LerpToZoom(Vector2 min, Vector2 max, float delta) {
    var center = (max + min) / 2f;
    // mul by 2 to account for full width instead of half width
    var size = max - min;

    var pixelSize = GetViewportRect().Size;
    var targetZoom = 0f;
    if (size.Y > size.X) {
      // need more vertical
      targetZoom = size.Y / pixelSize.Y;
    }
    else {
      targetZoom = size.X / pixelSize.X;
    }
    targetZoom *= _marginFactor; // account for margin before the flip
    targetZoom = 1.0f / targetZoom;
    targetZoom = Mathf.Clamp(targetZoom, _minZoom, _maxZoom);

    var nZoom = new Vector2(targetZoom, targetZoom);
    Zoom = Zoom.Lerp(nZoom, _lerpFactor * (float)delta);
  }
}
