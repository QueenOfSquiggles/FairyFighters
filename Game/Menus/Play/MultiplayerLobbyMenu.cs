namespace Squiggles.Fairy.Menu;

using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Extension;
using Squiggles.Core.Scenes.Utility;
using Squiggles.Fairy.Multiplayer;
using System;
using System.Linq;

public partial class MultiplayerLobbyMenu : Control {

  [Export] private Container _peerList;
  [Export] private Container _chatLog;
  [Export] private LineEdit _msgLine;
  [Export] private LabelSettings _chatStyle;
  [Export] private Button _btnStartGame;
  private GameLobby _lobby;

  public override void _Ready() {
    _lobby = GameLobby.Get(this);
    _lobby.PlayerConnected += DoPlayerConnect;
    _lobby.PlayerDisconnected += DoPlayerDisconnect;
    _lobby.ServerDisconnected += OnDisconnect;
    RefreshPeerList();
    if (!Multiplayer.IsServer()) {
      _btnStartGame.QueueFree();
    }
  }

  public override void _ExitTree() {
    _lobby.PlayerConnected -= DoPlayerConnect;
    _lobby.PlayerDisconnected -= DoPlayerDisconnect;
    _lobby.ServerDisconnected -= OnDisconnect;
  }

  private void DoPlayerConnect(long id) => RefreshPeerList();
  private void DoPlayerDisconnect(long id) => RefreshPeerList();

  private void RefreshPeerList() {
    if (!Multiplayer.HasMultiplayerPeer()) {
      // if called after the connection closes, prevent the error from trying to get the unique ID.
      return;
    }
    Print.Debug($"[{Multiplayer.GetUniqueId()}] Refreshing Peer List");
    _peerList.RemoveAllChildren();
    var peers = _lobby.Players;
    foreach (var player in peers.Values) {
      _peerList.AddChild(new Label {
        Text = player.Username
      });
    }
    var autoStartCount = 0;
    foreach (var arg in OS.GetCmdlineUserArgs()) {
      if (!arg.StartsWith("start")) { continue; }
      var _ = int.TryParse(arg.Split("=", 1)[1], out autoStartCount);
    }
    if (autoStartCount > 0 && Multiplayer.IsServer()) {
      DoStartGame();
    }
  }

  // button callbacks
  private void OnReady(bool isReady) => Rpc(MethodName.MsgRPC, "is ready");

  private void OnDisconnect() {
    _lobby.Close();
    SceneTransitions.LoadSceneAsync("res://Game/Menus/main_menu.tscn");
  }

  [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
  private void MsgRPC(string message) {
    var id = Multiplayer.GetRemoteSenderId();
    var player = GameLobby.Get(this).Players[id];
    Print.Debug($"[{id} :: {player.Username}]{message}");
    _chatLog.AddChild(new Label {
      Text = $"[{player.Username}] {message}",
      LabelSettings = _chatStyle,
      AutowrapMode = TextServer.AutowrapMode.WordSmart
    });
  }

  private void SendMessage() {
    if (_msgLine.Text.Length <= 0) {
      return; // don't allow sending empty messages
    }
    Rpc(MethodName.MsgRPC, _msgLine.Text);
    _msgLine.Text = "";
    _msgLine.GrabFocus();
  }

  public override void _UnhandledInput(InputEvent @event) {
    if (@event is InputEventKey key && key.Keycode == Key.Enter && key.IsPressed()) {
      SendMessage();
      this.HandleInput();
    }
  }

  public void DoStartGame() {
    Rpc(MethodName.MsgRPC, "Starting game!");
    _lobby.Rpc(GameLobby.MethodName.LoadGame, "res://Game/Levels/Test/test_multiplayer_level.tscn");
  }

}
