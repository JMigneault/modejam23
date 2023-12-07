using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntity : MonoBehaviour
{
  public bool isEnemy;
  public bool isUnit; // 'unit' refers to the four controllable entities
  public bool isTree = false;
  public bool isConductive = true; // must be electrified; can conduct
  public bool isElectrocuted = false;

  public GridCoords coords;

  public void SetCoords(GridCoords coords, float speed) {
    this.coords = coords;
    StartCoroutine(MoveTo(coords, speed));
  }

  public IEnumerator MoveTo(GridCoords coords, float speed) {
    Vector3 target = GridBoard.instance.GetLocalPos(coords);

    while ((target - transform.localPosition).magnitude > Mathf.Epsilon) {
      transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, speed * Time.deltaTime);
      yield return null;
    }
  }
}
