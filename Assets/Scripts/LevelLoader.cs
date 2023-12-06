using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: does this class need to exist? it doesn't even do the loading bit?!?!?
public class LevelLoader
{
  public List<TextAsset> levelTexts;

  public LevelLoader() { }

  public int GetNumLevels() { return levelTexts.Count; }

  public void Init(string levelSetName) {
    // TODO: figure out long term level loading strategy; eg what happens if you add/rm levels text file??
    string path = "Levels/" + levelSetName;

    // Load all level files and put them in alpha order.
    Object[] res = Resources.LoadAll(path, typeof(TextAsset));
    levelTexts = new List<TextAsset>(res.Length);
    for (int i = 0; i < res.Length; i++) {
      levelTexts.Add((TextAsset) res[i]);
    }
    levelTexts.Sort(delegate(TextAsset x, TextAsset y) { return x.name.CompareTo(y.name); });
  }

  public GameLevel LoadLevel(int lvlNumber, Transform levelPrefab, Transform levelParent) {
    Debug.Log("Loading Level: " + levelTexts[lvlNumber].name);
    GameLevel lvl = GameObject.Instantiate(levelPrefab, levelParent).GetComponent<GameLevel>();
    lvl.InitFrom(levelTexts[lvlNumber].bytes, levelTexts[lvlNumber].name);
    return lvl;
  }
}
