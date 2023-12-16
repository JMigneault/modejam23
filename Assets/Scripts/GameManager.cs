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
  int majorLevelNumber = 0;
  int minorLevelNumber = 1;

  public int[] levelSectionLengths = null; // TODO: hardcoded level numbering

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
      currentLvl.readyToDie = false;
      LevelTransition.instance.DoFunction( currentLvl.failed ? FUNCTION.FAIL : FUNCTION.WIN );
    }
  }

  public void ReloadLevel() {
    Debug.Log("current " + currentLevel);
    Debug.Log("minor " + majorLevelNumber);
    Debug.Log("major " + minorLevelNumber);
    if (currentLvl != null) {
      GameObject.Destroy(currentLvl.gameObject);
    }
    currentLvl = lvlLoader.LoadLevel(currentLevel, levelPrefab, levelParent);
    currentLvl.SetLevelNumberDisplay(majorLevelNumber, minorLevelNumber);
  }

  public void LoadNextLevel() {
    lvlLoader.Init(levelSet);
    if (currentLevel < lvlLoader.GetNumLevels() - 1) {
      currentLevel++;
      minorLevelNumber++;
      if (minorLevelNumber > levelSectionLengths[majorLevelNumber]) {
        // Next section
        majorLevelNumber++;
        minorLevelNumber = 1;
      }
    }
    ReloadLevel();
  }

  public void LoadPrevLevel() {
    lvlLoader.Init(levelSet);
    if (currentLevel > 0) {
      currentLevel--;
      minorLevelNumber--;
      if (minorLevelNumber == 0) {
        // Next section
        majorLevelNumber--;
        minorLevelNumber = levelSectionLengths[majorLevelNumber];
      }
    }
    ReloadLevel();
  }

  public void SkipLevel() {
    LoadNextLevel();
    TutorialCursor.instance.KillHints(); // if you're skipping around you don't get hints
  }

}
