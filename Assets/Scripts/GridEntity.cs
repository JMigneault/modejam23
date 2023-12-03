using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntity : MonoBehaviour
{
  public bool isEnemy;
  public bool isUnit; // 'unit' refers to the four controllable entities

  public GridBoard board;
  public GridCoords coords;
}
