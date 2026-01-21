using UnityEngine;
using System.Collections;

public class FlashEffect : MonoBehaviour
{
  public CanvasGroup canvasGroup;
  public float fadeInTime = 0.1f;
  public float holdTime = 0.15f;
  public float fadeOutTime = 0.25f;
  public float delayBeforeFlash = 0.4f; // Tempo de atraso antes do clarão

  void Awake()
  {
    canvasGroup.alpha = 0;
  }

  public IEnumerator Flash()
  {
    // Adicionando atraso antes do clarão
    yield return new WaitForSeconds(delayBeforeFlash);

    // Fade in
    float t = 0;
    while (t < fadeInTime)
    {
      t += Time.deltaTime;
      canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeInTime);
      yield return null;
    }

    canvasGroup.alpha = 1;
    yield return new WaitForSeconds(holdTime);

    // Fade out
    t = 0;
    while (t < fadeOutTime)
    {
      t += Time.deltaTime;
      canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeOutTime);
      yield return null;
    }

    canvasGroup.alpha = 0;
  }
}
