using Nixill.OBSWS;

namespace Nixill.Streaming.JoltBot.OBS;

internal static class OBSUtils
{
  readonly static Dictionary<SceneItemRecord, int> IndexCache = [];

  public static async Task<int> GetSceneItemIndex(string sceneName, string inputName, int searchOffset = 0)
  {
    var record = new SceneItemRecord(sceneName, inputName, searchOffset);

    if (!IndexCache.TryGetValue(record, out int value))
    {
      int index = await OBSRequests.SceneItems.GetSceneItemId(sceneName, inputName, searchOffset).Send();
      value = index;
      IndexCache[record] = value;
      return index;
    }

    return value;
  }

  public static async Task<TResult> WithSceneItemIndex<TResult>(Func<ID, int, TResult> func, string sceneName,
    string inputName, int searchOffset = 0) where TResult : OBSRequest
  => func(sceneName, await GetSceneItemIndex(sceneName, inputName, searchOffset));

  public static async Task<TResult> WithSceneItemIndex<T3, TResult>(Func<ID, int, T3, TResult> func, string sceneName,
    string inputName, T3 otherArg, int searchOffset = 0) where TResult : OBSRequest
  => func(sceneName, await GetSceneItemIndex(sceneName, inputName, searchOffset), otherArg);

  public static async Task CopyImages(string[] fromInputs, string[] toInputs)
  {
    if (fromInputs.Length != toInputs.Length)
      throw new InvalidOperationException("From and to arrays must be equal in length.");

    string[] existingImageNames = [
      .. (await new OBSRequestBatch(
        fromInputs.Select(s => OBSRequests.Inputs.GetInputSettings(s))
      ).Send()).Select(r => (string)(r.RequestResult as InputSettings).Settings["file"])
    ];

    await new OBSRequestBatch(
      toInputs.Zip(existingImageNames).Select(t => OBSExtraRequests.Inputs.Image
        .SetInputImage(t.First, t.Second))
    ).Send();
  }
}

internal record struct SceneItemRecord(string SceneName, string InputName, int SearchOffset);