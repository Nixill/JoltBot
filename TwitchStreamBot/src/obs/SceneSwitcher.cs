using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.Data;

namespace Nixill.Streaming.JoltBot.OBS;

public static class SceneSwitcher
{
  public static async Task SwitchTo(string sceneName, params string[] withSources)
  {
    bool supportedScene = OBSJson.SceneSwitcher.ContainsKey(sceneName);
    if (!supportedScene)
    {
      OBSRequests.Scenes.SetCurrentProgramScene(sceneName).SendWithoutWaiting();
      return;
    }

    IEnumerable<string> sourcesToHide = OBSJson.SceneSwitcher[sceneName].Except(withSources);
    IEnumerable<string> sourcesToShow = OBSJson.SceneSwitcher[sceneName].Intersect(withSources);

    var sceneItems = await OBSRequests.SceneItems.GetSceneItemList(sceneName).Send();

    IEnumerable<int> idsToHide = sceneItems
      .Join(
        sourcesToHide,
        si => si.SourceName,
        sth => sth,
        (si, sth) => si.SceneItemID
      );
    IEnumerable<int> idsToShow = sceneItems
      .Join(
        sourcesToShow,
        si => si.SourceName,
        sts => sts,
        (si, sts) => si.SceneItemID
      );

    // TODO restore this to the sendwithoutwaiting command.
    await new OBSRequestBatch(idsToHide
      .Select(id => (OBSRequest)OBSRequests.SceneItems.SetSceneItemEnabled(sceneName, id, false))
      .Concat(idsToShow
        .Select(id => (OBSRequest)OBSRequests.SceneItems.SetSceneItemEnabled(sceneName, id, true)))
      .Append(OBSRequests.General.Sleep(millis: 500))
      .Append(OBSRequests.Scenes.SetCurrentProgramScene(sceneName)))
      .Send();
    // .SendWithoutWaiting();
  }
}