using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader
{
  public List<TextAsset> levelTexts;

  public LevelLoader(string levelSetName) {
    Init(levelSetName);
  }

  public int GetNumLevels() { return levelTexts.Count; }

  public void Init(string levelSetName) {
    string path = "Levels/" + levelSetName;

    // Load all level files and put them in alpha order.
    Object[] res = Resources.LoadAll(path, typeof(TextAsset));
    levelTexts = new List<TextAsset>(res.Length);
    for (int i = 0; i < res.Length; i++) {
      levelTexts.Add((TextAsset) res[i]);
    }
    levelTexts.Sort(delegate(TextAsset x, TextAsset y) { return x.name.CompareTo(y.name); });

    string logmsg = "Read level text files:";
    foreach (TextAsset lt in levelTexts) {
      logmsg += "\n  " + lt.name;
    }
    Debug.Log(logmsg);
  }

  public GameLevel LoadLevel(int lvlNumber, Transform levelPrefab, Transform levelParent) {
    GameLevel lvl = GameObject.Instantiate(levelPrefab, levelParent).GetComponent<GameLevel>();
    lvl.InitFrom(levelTexts[lvlNumber].bytes, levelTexts[lvlNumber].name);
    return lvl;
  }
}
