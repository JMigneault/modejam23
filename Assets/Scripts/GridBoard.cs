using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBoard : MonoBehaviour
{
  // Singleton
  public static GridBoard instance = null;
  void Awake() {
    instance = this;
  }

  public GridEntity[,] entities;

  public void InitBoard(int width, int height) {
    entities = new GridEntity[width, height];
  }

  public void InitTile(GridCoords coords, TILE t) {
    // TODO
    Debug.Log("initting tile: x,y,t " + coords.i + ", " + coords.j + ", " + t);

  }
}
