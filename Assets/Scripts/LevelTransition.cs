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

  public int numBolts = 1000;
  private GameObject[] fleet = null;

  public float speed = 2f;

  void Start() {
    owner = transform.GetChild(0).transform;
    fleet = new GameObject[numBolts];
    // Vector3 dp = new Vector3(width / numBolts, height / numBolts, 0);
    for (int i = 0; i < numBolts; i++) {
      fleet[i] = GameObject.Instantiate(boltPrefab, owner.transform);
      fleet[i].transform.localPosition = new Vector3((Random.value - 0.5f) * width, (Random.value - 0.5f) * height, 0);
    }

    owner.gameObject.SetActive(false);
  }

  IEnumerator RunTransition() {
    owner.gameObject.SetActive(true);
    GameManager.instance.currentLvl.animating = true;
    Vector3 start = new Vector3(0, yTop + height / 2.0f);
    owner.transform.position = start;
    float dist = height * 2.0f;
    float d = 0;
    while (d < dist) {
      float dd = Time.deltaTime * speed;
      owner.position += Vector3.down * dd;
      d += dd;
      yield return null;
      Debug.Log("loop again");
    }
    GameManager.instance.currentLvl.animating = false;
    Debug.Log("hit that");
    owner.gameObject.SetActive(false);
    yield return null;
  }

  public void Unleash() {
    StartCoroutine(RunTransition());
  }

}
