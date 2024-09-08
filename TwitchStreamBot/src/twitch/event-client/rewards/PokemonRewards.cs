namespace Nixill.Streaming.JoltBot.Twitch.Events.Rewards;

[RewardContainer]
public static class PokemonRewards
{
  [ChannelPointsReward("CatchSomethingRandom")]
  [AllowedWithGame("Pokémon")]
  [AllowedWithGame("Ranger", Invert = true)]
  [ModifyWhenGameIs("Arceus", Price = 100, StopIfApplicable = true)]
  [DefaultRewardModifier(Price = 250)]
  public static Task CatchSomethingRandomReward(BaseContext ctx) => Task.CompletedTask;
}