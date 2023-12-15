using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltWiggle : MonoBehaviour
{

  public float centerX;
  public float wobbleDist = 0.5f;
  public float wobbleSpeed = 80.0f;
  public float offset = 0;

  public void Init(Vector3 localPos) {
    transform.localPosition = localPos;
    centerX = localPos.x;
    offset = ((2 * Random.value) - 1) * Mathf.PI; // [-pi, pi]
  }

  void Update() {
    float localX = Mathf.Sin(Time.time * wobbleSpeed + offset) * wobbleDist + centerX;
    transform.localPosition = new Vector3(localX, transform.localPosition.y, 0);
  }

}
