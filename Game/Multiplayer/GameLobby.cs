namespace Squiggles.Fairy.Multiplayer;

using System;
using System.Collections.Generic;
using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Scenes.Utility;

public partial class GameLobby : Node {

  [Signal] public delegate void PlayerDisconnectedEventHandler(long mpID);
  [Signal] public delegate void PlayerConnectedEventHandler(long mpID);
  [Signal] public delegate void ServerDisconnectedEventHandler();

  private const string DEFAULT_SERVER_IP = "127.0.0.1";

  private const int PORT = 7000;

  // public Action<long> PlayerDisconnected;
  // public Action<long> PlayerConnected;
  // public Action ServerDisconnected;

  public readonly Dictionary<int, PlayerClient> Players = new();

  public readonly Godot.Collections.Dictionary<string, string> LocalClientData = new() {
    {"username", "John Mastodon"}
  };

  public static GameLobby Get(Node context) => context?.GetTree().Root.GetNode<GameLobby>("GameLobby");

  public override void _Ready() {
    // internal connections
    Multiplayer.PeerConnected += OnPlayerConnected;
    Multiplayer.PeerDisconnected += OnPlayerDisconnected;
    Multiplayer.ServerDisconnected += OnServerDisconnected;
    Multiplayer.ConnectedToServer += OnConnectedSuccessful;
    Multiplayer.ConnectionFailed += OnConnectionFail;
  }

  public void Join(string address = "") {
    if (address == "") {
      address = DEFAULT_SERVER_IP;
    }
    var peer = new ENetMultiplayerPeer();
    var err = peer.CreateClient(address, PORT);
    if (err != Error.Ok) {
      Print.Error($"Failed to join multiplayer game @{address}:{PORT}. Error code: {err}", this);
      return;
    }
    GetTree().GetMultiplayer().MultiplayerPeer = peer;
    Multiplayer.MultiplayerPeer = peer;
  }

  // 64 is pretty extreme. But probably on a LAN connnection the speed wouldn't be too terrible??
  public void Host(int maxConnections = 32) {
    var peer = new ENetMultiplayerPeer();
    var err = peer.CreateServer(PORT, maxConnections);
    if (err != Error.Ok) {
      Print.Error($"Failed to host multiplayer game @localhost:{PORT}. Error code: {err}", this);
      return;
    }
    GetTree().GetMultiplayer().MultiplayerPeer = peer;
    Multiplayer.MultiplayerPeer = peer;
    Players[1] = GetClientFrom(LocalClientData, 1);
  }

  public void Close() => Multiplayer.MultiplayerPeer = null;

  [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
  public void LoadGame(string path) => SceneTransitions.LoadSceneAsync(path);

  [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
  public void RegisterPlayerRPC(Godot.Collections.Dictionary<string, string> clientData) {
    var id = Multiplayer.GetRemoteSenderId();
    // if (!Players.ContainsKey(id) && id != Multiplayer.GetUniqueId()) {
    //   // two way exchange of client information
    //   RpcId(id, MethodName.RegisterPlayerRPC, LocalClientData);
    // }
    var nPlayer = GetClientFrom(clientData, id);
    Players[id] = nPlayer;
    Print.Info($"[{Multiplayer.GetUniqueId()}] Peer connnected! {nPlayer.Username}#{id}");
    EmitSignal(SignalName.PlayerConnected, id);
  }

  // when a new peer is found (called locally)
  private void OnPlayerConnected(long id) => Rpc(MethodName.RegisterPlayerRPC, LocalClientData);

  private void OnPlayerDisconnected(long id) {
    Players.Remove((int)id);
    EmitSignal(SignalName.PlayerDisconnected, id);
  }

  private void OnConnectedSuccessful() => Rpc(MethodName.RegisterPlayerRPC, LocalClientData);

  private void OnConnectionFail() => Close();

  private void OnServerDisconnected() {
    Close();
    Players.Clear();
    EmitSignal(SignalName.ServerDisconnected);
  }

  // use this to handle new data for clients such as cosmetics, upgrades, etc...
  private static PlayerClient GetClientFrom(Godot.Collections.Dictionary<string, string> data, int id) => new() {
    Username = data["username"],
    ID = id
  };
}
