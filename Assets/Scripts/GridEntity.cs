using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntity : MonoBehaviour
{
  public bool isEnemy;
  public bool isUnit; // 'unit' refers to the four controllable entities

  public GridCoords coords;

  public void SetCoords(GridCoords coords) {
    this.coords = coords;
    this.transform.localPosition = GridBoard.instance.GetLocalPos(coords);
  }
}
