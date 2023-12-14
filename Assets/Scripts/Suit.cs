using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suit : MonoBehaviour
{
  public ABILITY ability;
  public Sprite[] sprites = null;

  public void SetAbility(ABILITY a, Vector3 boxTop, float boxDist) {
    ability = a;
    GetComponent<SpriteRenderer>().sprite = sprites[(int) a];
    transform.position = boxTop + Vector3.down * ((int) a) * boxDist;
  }

}
