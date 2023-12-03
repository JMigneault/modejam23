using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCoords
{
  public int i;
  public int j;

  public GridCoords(int i, int j) {
    this.i = i;
    this.j = j;
  }
}

public enum TILE {EMPTY, ENEMY, UNIT};
