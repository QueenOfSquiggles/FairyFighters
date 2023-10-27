namespace Squiggles.Fairy.Level;

using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Extension;
using Squiggles.Fairy.Character;
using Squiggles.Fairy.Multiplayer;
using System;
using System.Linq;

public partial class TestMultiplayerLevel : Node2D {


  [Export] private PackedScene _playerScene;
  [Export] private PathFollow2D _spawnPath;

  private GameLobby _lobby;
  // private MultiplayerSpawner _mpSpawner;

  public override void _Ready() {
    // _mpSpawner = this.GetComponent<MultiplayerSpawner>();
    // _mpSpawner.SetMultiplayerAuthority(1);
    // _mpSpawner.AddSpawnableScene(_playerScene.ResourcePath);

    _lobby = GameLobby.Get(this);
    var f = 0f;
    var playerOrder = _lobby.Players.Keys.ToArray();
    foreach (var id in playerOrder) {
      CreatePlayerFor(id, f / playerOrder.Length);
      f += 1f;
    }
  }


  // [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
  private void CreatePlayerFor(int peerID, float percent) {
    var node = _playerScene.Instantiate<CharacterBody2D>() as PlayerCharacter;
    var playerData = _lobby.Players[peerID];
    var name = $"${peerID}-{playerData.Username.ToPascalCase()}";
    AddChild(node);
    _spawnPath.ProgressRatio = percent;
    var rand = new Random();
    node.Name = name;
    node.SpawnMP(
      peerID,
      _spawnPath.GlobalPosition
    );
    Print.Info($"[{Multiplayer.GetUniqueId()}] Spawning character for {playerData}");
  }

}
