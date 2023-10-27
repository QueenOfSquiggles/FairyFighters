namespace Squiggles.Fairy.Multiplayer;

public sealed record PlayerClient {

  // multiplayer data
  public int ID { get; init; }

  // identity
  public string Username { get; set; }
  public bool IsReady { get; set; }
}
