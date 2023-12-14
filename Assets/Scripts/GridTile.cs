using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
  public Sprite normal;
  public Sprite highlighted;
  public Sprite darkened;
  public SpriteRenderer sr;

  bool isHighlighted;

  void Awake() {
    sr = GetComponent<SpriteRenderer>();
    sr.sprite = normal;
  }

  public void SetDarkened(bool d) {
    if (d) {
      sr.sprite = darkened;
    } else {
      sr.sprite = isHighlighted ? highlighted : normal;
    }
  }

  public void Highlight() {
    isHighlighted = true;
    sr.sprite = highlighted;
  }

  public void Unhighlight() {
    isHighlighted = false;
    sr.sprite = normal;
  }
}
