namespace Squiggles.Fairy.Menu;

using Godot;
using Squiggles.Core.Error;
using Squiggles.Core.Extension;
using Squiggles.Core.Scenes.Utility;
using Squiggles.Fairy.Multiplayer;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class MainMenu : Control {

  [ExportGroup("Content Tools Creation", "_contentTools")]
  [Export(PropertyHint.File, "*.tscn")] private string _contentToolsScene = "";
  [Export] private Color _contentToolsColour;

  [ExportGroup("Manage Mods", "_manageMods")]
  [Export(PropertyHint.File, "*.tscn")] private string _manageModsScene = "";
  [Export] private Color _manageModsColour;

  [ExportGroup("Node Refs")]
  [Export] private Control _multiplayerPanel;
  [Export] private LineEdit _mpAddress;
  [Export] private LineEdit _mpName;
  [Export] private CheckButton _mpIsHost;

  private GameLobby _lobby;

  public override void _Ready() {
    _lobby = GameLobby.Get(this);
    if (OS.GetCmdlineUserArgs().Contains("--server")) {
      _lobby.Host();
    }
    // _multiplayerPanel.Visible = false;
    if (OS.GetCmdlineArgs().Contains("host")) {
      _mpIsHost.ButtonPressed = true;
      OnJoinHost();
    }
    else if (OS.GetCmdlineArgs().Contains("join")) {
      _mpIsHost.ButtonPressed = true;
      OnJoinHost();
    }
  }

  private void CreateHeadlessServer() {
    if (OS.GetCmdlineUserArgs().Contains("--headless")) {
      Print.Info("Headless server detected. Attempting to load from configurations");
    }
    else {
      Print.Warn("Notice! This server is not running in '--headless' mode! This means that the graphics and windowing processing will occur despite being in server mode! For optimized performance, use '--headless' as part of the engine arguments!");
    }
    this.RemoveAllChildren(); // removes all child nodes so we can focus on server logic
    var server = new GameServer(this);
    server.Create();
  }

  private void OnPlayLocal() { }

  private void OnPlayOnline() => _multiplayerPanel.Visible = true;

  private void OnOptions() { }

  private void OnContentTools() => SceneTransitions.LoadSceneAsync(_contentToolsScene, colour: _contentToolsColour.ToHtml());

  private void OnManageMods() => SceneTransitions.LoadSceneAsync(_manageModsScene, colour: _manageModsColour.ToHtml());

  private void OnQuit() => GetTree().Quit();

  public void OnJoinHost() {
    var address = _mpAddress.Text;
    var hosting = _mpIsHost.ButtonPressed;

    _lobby.LocalClientData["username"] = _mpName.Text.Length > 0
      ? _mpName.Text
      : (hosting
        ? "Host"
        : "Client" + OS.GetProcessId());

    if (hosting) {
      _lobby.Host();
    }
    else {
      _lobby.Join(address);
    }
    SceneTransitions.LoadSceneAsync("res://Game/Menus/Play/multiplayer_lobby_menu.tscn");
  }

}
