using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InputHandler : MonoBehaviour
{
  // Singleton
  public static InputHandler instance;
  void Awake() {
    instance = this;
  }

  void Update() {
    if (GameManager.instance.currentLvl != null && GameManager.instance.currentLvl.animating) {
      return;
    }

    if (Input.GetMouseButtonDown(0)) { // click
      GameManager.instance.currentLvl.Click(WorldPos(Input.mousePosition));
    }

    if (Input.GetMouseButton(0)) {
      GameManager.instance.currentLvl.MousePosition(WorldPos(Input.mousePosition));
    }

    if (Input.GetMouseButtonUp(0)) {
      GameManager.instance.currentLvl.Release();
    }
  }

  Vector3 WorldPos(Vector3 mp) {
    Vector3 wp = Camera.main.ScreenToWorldPoint(mp);
    return new Vector3(wp.x, wp.y, 0);
  }

  public void Button(FUNCTION f) {
    if (GameManager.instance.currentLvl != null && GameManager.instance.currentLvl.animating) {
      return;
    }
    LevelTransition.instance.DoFunction(f);
  }

}
