using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  public LevelLoader lvlLoader;
  public GameLevel currentLvl;

  public Transform levelParent;
  public Transform levelPrefab;

  // Singleton
  public static GameManager instance;
  void Awake() {
    instance = this;
  }

  void Start() {
    lvlLoader = new LevelLoader("protov1");
    List<GameLevel> levels = new List<GameLevel>(); // TODO not used

    // TODO temp
    currentLvl = lvlLoader.LoadLevel(0, levelPrefab, levelParent);
  }

  public void ResetLevel() {
    // TODO: TEMP
    GameObject.Destroy(currentLvl.gameObject);
    lvlLoader.Init("protov1");
    currentLvl = lvlLoader.LoadLevel(0, levelPrefab, levelParent);
  }
}
