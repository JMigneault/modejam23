using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  public LevelLoader lvlLoader;
  public GameLevel currentLvl;

  public Transform levelParent;
  public Transform levelPrefab;

  public string levelSet = "protov1";

  int currentLevel = 0;

  // Singleton
  public static GameManager instance;
  void Awake() {
    instance = this;
  }

  void Start() {
    lvlLoader = new LevelLoader();
    lvlLoader.Init(levelSet);
    ReloadLevel();
  }

  void Update() {
    if (currentLvl != null && currentLvl.readyToDie) {
      if (currentLvl.failed) {
        ReloadLevel();
      } else {
        LoadNextLevel();
      }
    }
  }

  public void ReloadLevel() {
    if (currentLvl != null) {
      GameObject.Destroy(currentLvl.gameObject);
    }
    currentLvl = lvlLoader.LoadLevel(currentLevel, levelPrefab, levelParent);
  }

  public void LoadNextLevel() {
    lvlLoader.Init(levelSet);
    if (currentLevel < lvlLoader.GetNumLevels() - 1) {
      currentLevel++;
    }
    ReloadLevel();
  }

  public void LoadPrevLevel() {
    lvlLoader.Init(levelSet);
    if (currentLevel > 0) {
      currentLevel--;
    }
    ReloadLevel();
  }

  public void SkipLevel() {
    LoadNextLevel();
    TutorialCursor.instance.KillHints(); // if you're skipping around you don't get hints
  }

}
