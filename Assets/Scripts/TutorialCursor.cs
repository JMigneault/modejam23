using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Hint {
  public GameObject startMarker;
  public GameObject endMarker;
}

public class TutorialCursor : MonoBehaviour
{
  public Hint[] hints;
  public float speed = 1.0f;
  public float holdTime = 0.5f;
  float t = 0;
  bool done = false;
  int i = 0;
  bool paused = false;

  public static TutorialCursor instance = null;
  void Awake() {
    instance = this;
  }

  void Start() {
    StartHint();
  }

  void StartHint() {
    transform.position = hints[i].startMarker.transform.position;
    t = holdTime;
  }

  public void NextHint() {
    i++;
    done = (i >= hints.Length);
    if (done) {
      gameObject.SetActive(false);
    } else {
      StartHint();
    }
  }

  bool AtDest() {
    return (transform.position - hints[i].endMarker.transform.position).magnitude < Mathf.Epsilon;
  }

  public void SetHintsPaused(bool pause) {
    paused = pause;
    GetComponent<SpriteRenderer>().enabled = !paused;
  }

  public void KillHints() {
    done = true;
    gameObject.SetActive(false);
  }

  void Update() {
    if (!done && !paused) {
      if (t > 0) {
        t -= Time.deltaTime;
        if (t <= 0 && AtDest()) {
          StartHint();
        }
      } else {
        transform.position = Vector3.MoveTowards(transform.position, 
                                                 hints[i].endMarker.transform.position, speed * Time.deltaTime);
        if (AtDest()) {
          t = holdTime;
        }
      }
    }
  }
}
