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
    abilityImages = abilityButtonParent.GetComponentsInChildren<Image>();
  }

  // TODO: improve button code (maybe after we have assets)
  public GameObject abilityButtonParent;
  public Sprite unusedButton;
  public Sprite usedButton;
  public Sprite invalidButton;
  public Sprite flashButton;
  public Image[] abilityImages;
  public Image spawnImage;
  public float flashTime = 0.5f;

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

  // TODO Button code is really hacky right now :(

  public void ResetUI() {
    // beware of running coroutines.
    foreach (Image im in abilityImages) {
      im.sprite = unusedButton;
    }
  }

  public void InitButtons(AbilityUsage usage) {
    for (int i = 0; i < abilityImages.Length; i++) {
      bool available = (i == abilityImages.Length-1) || usage.IsAvailable((ABILITY) i);
      abilityImages[i].sprite = available ? unusedButton : usedButton ;
    }
  }

  // The player needs to use an ability. Play a flash effect to make them notice.
  public void FlashButtons() {
    StartCoroutine(DoFlashButtons());
  }

  IEnumerator DoFlashButtons() {
    for (int i = 0; i < abilityImages.Length; i++) {
      abilityImages[i].sprite = flashButton;
    }
    yield return new WaitForSeconds(flashTime);

    for (int i = 0; i < abilityImages.Length; i++) {
      bool available = (i == abilityImages.Length-1) || GameManager.instance.currentLvl.abilities.IsAvailable((ABILITY) i);
      abilityImages[i].sprite = available ? unusedButton : usedButton ;
    }
  }

  public void Failed(ABILITY a) {
    StartCoroutine(DoFlashFailed(a));
  }

  IEnumerator DoFlashFailed(ABILITY a) {
    Image im = abilityImages[(int)a];
    Sprite old = im.sprite;
    im.sprite = invalidButton;
    yield return new WaitForSeconds(flashTime);
    bool available = GameManager.instance.currentLvl.abilities.IsAvailable(a);
    im.sprite = available ? unusedButton : usedButton ;
  }

  public void Use(ABILITY a) {
    abilityImages[(int)a].sprite = usedButton;
  }

}
