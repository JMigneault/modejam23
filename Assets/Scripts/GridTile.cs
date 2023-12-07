using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
  public Sprite normal;
  public Sprite highlighted;
  public SpriteRenderer sr;

  void Awake() {
    sr = GetComponent<SpriteRenderer>();
    sr.sprite = normal;
  }

  public void Highlight() {
    sr.sprite = highlighted;
  }

  public void Unhighlight() {
    sr.sprite = normal;
  }
}
