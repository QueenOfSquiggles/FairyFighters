namespace Squiggles.Fairy.Multiplayer;

using Godot;

public sealed class GameServer {

  private readonly Node _host;

  public GameServer(Node host) {
    _host = host;
  }

  public void Create() {
    var args = OS.GetCmdlineUserArgs();
    var port = 0;
    var maxClients = 0;
    for (var i = 0; i < args.Length - 1; i++) {
      if (args[i].ToLower().Contains("port")) {
        var _ = int.TryParse(args[i + 1], out port);
      }
      if (args[i].ToLower().Contains("client")) {
        var _ = int.TryParse(args[i + 1], out maxClients);
      }
    }
    var peer = new ENetMultiplayerPeer();
    peer.CreateServer(port, maxClients);
    _host.Multiplayer.MultiplayerPeer = peer;
  }

}
