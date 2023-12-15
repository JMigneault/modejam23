using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using System.Random;

public class GridEntity : MonoBehaviour
{
  public bool isEnemy;
  public bool isUnit; // 'unit' refers to the four controllable entities
  public bool isTree = false;
  public bool isConductive = true; // must be electrified; can conduct
  public bool isElectrocuted = false;

  public GridCoords coords;

  public Sprite litBulb;

  public float wobbleDegrees = 100.0f;
  public float wobbleSpeed = 80.0f;
  // TODO: push these to local scope
  public bool wobblingLeft = false;
  public float currentWobble;

  public void SetCoords(GridCoords coords, float speed) {
    this.coords = coords;
    StartCoroutine(MoveTo(coords, speed));
  }

  public IEnumerator MoveTo(GridCoords coords, float speed) {
    Vector3 target = GridBoard.instance.GetLocalPos(coords);
    bool shouldWobble = speed < Mathf.Infinity;

    // Start wobbling
    if (shouldWobble) {
      wobblingLeft = Random.value > 0.5f;
      currentWobble = (2 * Random.value - 1) * wobbleDegrees;
      transform.Rotate(0, 0, currentWobble);
    }

    while ((target - transform.localPosition).magnitude > Mathf.Epsilon) {
      transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, speed * Time.deltaTime);
      // Wobble
      if (shouldWobble) {
        // keep wobbling!
        float dir = wobblingLeft ? 1 : -1 ;
        if (dir * currentWobble < wobbleDegrees) {
          float dWobble = dir * wobbleSpeed * Time.deltaTime;
          transform.Rotate(0, 0, dWobble);
          currentWobble += dWobble;
        } else {
          // reverse the wobble!
          wobblingLeft = !wobblingLeft;
        }
      }


      yield return null;
    }

    // Reset wobble
    if (shouldWobble) {
      transform.rotation = Quaternion.identity;
    }
    yield return null;
  }
}
