using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
  // Singleton
  public static InputHandler instance;
  void Awake() {
    instance = this;
  }

  void Update() {
    if (Input.GetMouseButtonDown(0)) { // click
      Vector3 clickWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      GridCoords coords = GridBoard.instance.WorldToGrid(clickWorldPos);
      if (GridBoard.instance.IsCoordValid(coords)) {
        GameManager.instance.currentLvl.ClickTile(coords);
      }
    }

    // EXTREMELY TEMP CONTROLS TODO
    if (Input.GetKeyDown(KeyCode.H)) {
      GameManager.instance.currentLvl.DoAbility(ABILITY.SPAWN);
    } else if (Input.GetKeyDown(KeyCode.V)) {
      GameManager.instance.currentLvl.DoAbility(ABILITY.VSPAWN);
    } else if (Input.GetKeyDown(KeyCode.D)) {
      GameManager.instance.currentLvl.DoAbility(ABILITY.ROTATE);
    } else if (Input.GetKeyDown(KeyCode.M)) {
      GameManager.instance.currentLvl.DoAbility(ABILITY.MAGNETIZE);
    } else if (Input.GetKeyDown(KeyCode.E)) {
      Debug.Log("im e");
      GameManager.instance.currentLvl.DoAbility(ABILITY.ELECTROCUTE);
    }

    if (Input.GetKeyDown(KeyCode.Space)) {
      GameManager.instance.ReloadLevel();
    }

    if (Input.GetKeyDown(KeyCode.LeftArrow)) {
      GameManager.instance.LoadPrevLevel();
    } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
      GameManager.instance.LoadNextLevel();
    }
  }
}
