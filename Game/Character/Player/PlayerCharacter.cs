namespace Squiggles.Fairy.Character;

using Godot;
using Squiggles.Core.Extension;
using System;

public partial class PlayerCharacter : CharacterBody2D {

  public const float SPEED = 300.0f;
  public const float JUMP_VELOCITY = -400.0f;

  // Get the gravity from the project settings to be synced with RigidBody nodes.
  private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

  [Export] private float _mpSyncSpeed = 1000f;
  [ExportGroup("Multplayer Sync")]
  [Export] public int HostClient;
  [Export] private Vector2 _mpPosition;

  private MultiplayerSynchronizer _mpSync;
  public override void _Ready() {
    if (!Multiplayer.HasMultiplayerPeer()) { return; }
    _mpSync = this.GetComponent<MultiplayerSynchronizer>();
  }

  public void SpawnMP(int host, Vector2 position) {
    Position = _mpPosition = position;
    HostClient = host;
    _mpSync.SetMultiplayerAuthority(HostClient);
  }

  public override void _PhysicsProcess(double delta) {
    if (Multiplayer.HasMultiplayerPeer() && Multiplayer.GetUniqueId() != HostClient) {
      // lerp target post
      Position = Position.Lerp(_mpPosition, _mpSyncSpeed * (float)delta); // lerps to new position to smooth out networking lag
    }
    else {
      // client-side physics logic
      var velocity = Velocity;

      // Add the gravity.
      if (!IsOnFloor()) {
        velocity.Y += _gravity * (float)delta;
      }

      // Handle Jump.
      if (Input.IsActionJustPressed("ui_accept") && IsOnFloor()) {
        velocity.Y = JUMP_VELOCITY;
      }

      // Get the input direction and handle the movement/deceleration.
      // As good practice, you should replace UI actions with custom gameplay actions.
      var direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
      velocity.X = direction != Vector2.Zero
        ? direction.X * SPEED
        : Mathf.MoveToward(Velocity.X, 0, SPEED);

      Velocity = velocity;
      MoveAndSlide();
      _mpPosition = Position;
    }
  }
}
