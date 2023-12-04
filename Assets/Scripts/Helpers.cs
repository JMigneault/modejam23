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

  public bool Equals(GridCoords that) {
    return this.i == that.i && this.j == that.j;
  }

  public GridCoords Go(DIR d) {
    switch (d) {
      case DIR.NONE: return this;
      case DIR.LEFT: return Left();
      case DIR.RIGHT: return Right();
      case DIR.UP: return Up();
      case DIR.DOWN: return Down();
      case DIR.DIAGUL: return Up().Left();
      case DIR.DIAGUR: return Up().Right();
      case DIR.DIAGDL: return Down().Left();
      case DIR.DIAGDR: return Down().Right();
      default:
        Debug.LogError("Got unexpected DIR in Go()");
        return this;
    }
  }

  public GridCoords Up() {
    return new GridCoords(i, j-1);
  }

  public GridCoords Down() {
    return new GridCoords(i, j+1);
  }

  public GridCoords Left() {
    return new GridCoords(i-1, j);
  }

  public GridCoords Right() {
    return new GridCoords(i+1, j);
  }

  public int DistanceTo(GridCoords that) {
    return Mathf.Abs(this.i - that.i) + Mathf.Abs(this.j - that.j);
  }

  public DIR DirectionTo(GridCoords that) {
    if (i == that.i && j == that.j) {
      return DIR.NONE;
    }

    if (i == that.i || j == that.j) {
      // orthogonal
      if (i == that.i) { // same col
        return (that.j > j) ? DIR.DOWN : DIR.UP ;
      }

      if (j == that.j) { // same row
        return (that.i > i) ? DIR.RIGHT : DIR.LEFT ;
      }
    }

    // diagonal
    if (that.i > i && that.j > j) {
      return DIR.DIAGDR;
    }

    if (that.i < i && that.j < j) {
      return DIR.DIAGUL;
    }

    if (that.i > i && that.j < j) {
      return DIR.DIAGUR;
    }

    if (that.i < i && that.j > j) {
      return DIR.DIAGDL;
    }

    Debug.LogError("Unexpectedly hit the end of DirectionTo()");
    return DIR.NONE;
  }

  public bool IsOrthogonal(GridCoords that) {
    switch (DirectionTo(that)) {
      case DIR.UP:
      case DIR.DOWN:
      case DIR.LEFT:
      case DIR.RIGHT:
        return true;
      default:
        return false;
    }
  }
}

public enum DIR {NONE, LEFT, RIGHT, UP, DOWN, DIAGUR, DIAGUL, DIAGDR, DIAGDL}
public enum TILE {EMPTY, ENEMY, UNIT};
public enum ABILITY {ROTATE, HSPAWN, VSPAWN, MAGNETIZE, ELECTROCUTE, NONE /* TODO: is NONE too hacky? */ };
