using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  public LevelLoader lvlLoader;
  public GameLevel currentLvl;

  public Transform levelParent;
  public Transform levelPrefab;

  void Start() {
    lvlLoader = new LevelLoader("protov1");
    List<GameLevel> levels = new List<GameLevel>();

    // TODO temp
    currentLvl = lvlLoader.LoadLevel(0, levelPrefab, levelParent);
  }
}
