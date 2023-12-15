using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteButton : MonoBehaviour
{

  public FUNCTION function;

  void OnMouseDown() {
    InputHandler.instance.Button(function);
  }

}

