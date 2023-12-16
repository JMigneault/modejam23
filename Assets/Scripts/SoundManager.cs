using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
  public static SoundManager instance;

  void Awake() {
    instance = this;
  }

  public AudioSource musicSource = null;
  public AudioSource soundSource = null;

  public AudioClip rotate = null;
  public AudioClip spawn = null;
  public AudioClip magnetize = null;
  public AudioClip electrocute = null;

  public AudioClip win = null;
  public AudioClip reload = null;
  public AudioClip previous = null;
  public AudioClip skip = null;

  public void PlayAbility(ABILITY a) {
    switch (a) {
      case ABILITY.MAGNETIZE:
        soundSource.clip = magnetize;
        break;
      case ABILITY.ROTATE:
        soundSource.clip = rotate;
        break;
      case ABILITY.SPAWN:
        soundSource.clip = spawn;
        break;
      case ABILITY.ELECTROCUTE:
        soundSource.clip = electrocute;
        break;
    }
    soundSource.Play();
  }

  public void PlayFunction(FUNCTION f) {
    switch (f) {
      case FUNCTION.WIN:
        soundSource.clip = win;
        break;
      case FUNCTION.RELOAD:
        soundSource.clip = reload;
        break;
      case FUNCTION.PREVIOUS:
        soundSource.clip = previous;
        break;
      case FUNCTION.SKIP:
        soundSource.clip = skip;
        break;
    }
    soundSource.Play();
  }

}
