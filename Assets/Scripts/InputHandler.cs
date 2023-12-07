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

  public void PressMagnetize() {
    GameManager.instance.currentLvl.DoAbility(ABILITY.MAGNETIZE);
  }

  public void PressSpawn() {
    GameManager.instance.currentLvl.DoAbility(ABILITY.SPAWN);
  }

  public void PressRotate() {
    GameManager.instance.currentLvl.DoAbility(ABILITY.ROTATE);
  }

  public void PressElectrocute() {
    GameManager.instance.currentLvl.DoAbility(ABILITY.ELECTROCUTE);
  }

  public void PressUndoMovement() {
    GameManager.instance.currentLvl.UndoLastMovement();
  }

  // The player needs to use an ability. Play a flash effect to make them notice.
  public void FlashButtons() {
    Debug.Log("TODO: flash buttons");
  }

  public void FlashInvalidSpawn() {
    Debug.Log("TODO: signal bad spawn");
  }

}
