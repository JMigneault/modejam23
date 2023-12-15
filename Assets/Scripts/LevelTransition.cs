using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTransition : MonoBehaviour
{

  public static LevelTransition instance;

  void Awake() {
    instance = this;
  }

  public GameObject boltPrefab = null;
  public Transform owner;

  public float yTop;
  public float height;
  public float width;

  private int numBolts;
  public int boltDensity = 1;
  private GameObject[] fleet = null;

  public float speed = 2f;

  public float blackTime = 1.5f;

  public GameObject blackScreen = null;

  void Start() {
    numBolts = boltDensity * 16 * 9 * boltDensity; // 16:9 aspect ratio
    owner = transform.GetChild(0).transform;
    fleet = new GameObject[numBolts];

    float aspectToUnits = 8.0f / 9.0f;
    float aspectPerBolt = 1.0f / boltDensity;
    int index = 0;
    for ( int i = 0; i < 16; i++ ) {
      for ( int j = 0; j < 9; j++ ) {
        for ( int k = 0; k < boltDensity; k++ ) {
          GameObject go = GameObject.Instantiate(boltPrefab, owner.transform);
          Vector3 aspectCenter = new Vector3((i - 8.0f) + k * aspectPerBolt, (j - 4.5f) + k * aspectPerBolt);
          go.GetComponent<BoltWiggle>().Init(aspectCenter * aspectToUnits);
          fleet[index++] = go;
        }
      }
    }

    owner.gameObject.SetActive(false);
  }

  IEnumerator RunBoltWipe(FUNCTION function) {
    owner.gameObject.SetActive(true);
    GameManager.instance.currentLvl.animating = true;
    Vector3 start = new Vector3(0, yTop + height / 2.0f);
    owner.transform.position = start;
    float dist = height * 2.0f;
    float d = 0;
    bool transitioned = false;
    while (d < dist) {
      float dd = Time.deltaTime * speed;
      owner.position += Vector3.down * dd;
      d += dd;
      if (!transitioned && d >= height) {
        TransitionLevel(function);
        transitioned = true;
      }
      yield return null;
    }
    GameManager.instance.currentLvl.animating = false;
    owner.gameObject.SetActive(false);
    yield return null;
  }

  IEnumerator RunBlackWipe(FUNCTION function) {
    blackScreen.SetActive(true);
    yield return new WaitForSeconds(blackTime);
    TransitionLevel(function);
    blackScreen.SetActive(false);
  }

  public void WipeWithBolts(FUNCTION function) {
    StartCoroutine(RunBoltWipe(function));
  }

  public void WipeWithBlack(FUNCTION function) {
    StartCoroutine(RunBlackWipe(function));
  }

  void TransitionLevel(FUNCTION function) {
    switch (function) {
      case FUNCTION.RELOAD:
        GameManager.instance.ReloadLevel();
        break;
      case FUNCTION.WIN:
        GameManager.instance.LoadNextLevel();
        break;
      case FUNCTION.SKIP:
        GameManager.instance.SkipLevel();
        break;
      case FUNCTION.PREVIOUS:
        GameManager.instance.LoadPrevLevel();
        break;
    }
  }

  public void DoFunction(FUNCTION function) {
    switch (function) {
      case FUNCTION.WIN:
        WipeWithBolts(function);
        break;
      case FUNCTION.RELOAD:
      case FUNCTION.SKIP:
      case FUNCTION.PREVIOUS:
        WipeWithBlack(function);
        break;
    }

  }
}
