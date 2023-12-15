using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suit : MonoBehaviour
{
  public ABILITY ability;
  public Sprite[] sprites = null;

  public float labelOffset = -1.5f;

  public void SetAbility(ABILITY a, Vector3 boxTop, float boxDist, GameObject label) {
    ability = a;
    GetComponent<SpriteRenderer>().sprite = sprites[(int) a];
    transform.position = boxTop + Vector3.down * ((int) a) * boxDist;
    label.transform.position = transform.position + new Vector3(labelOffset, 0, 0);
  }

}
