using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Necessário para Corrotinas

public class CutsceneController : MonoBehaviour
{
  [Header("Configuração")]
  [SerializeField] private GameObject[] slideObjects;
  [SerializeField] private string gameSceneName = "MainScene";
  [SerializeField] private float timePerSlide = 5f;
  [SerializeField] private float fadeDuration = 1.0f; // Tempo para aparecer (Fade In)

  private int _currentIndex = 0;
  private float _timer;

  void Start()
  {
    InitializeSlides();
  }

  void Update()
  {
    _timer += Time.deltaTime;

    // Avança com Espaço, Clique ou Tempo
    if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || _timer >= timePerSlide)
    {
      NextSlide();
    }
  }

  void InitializeSlides()
  {
    for (int i = 0; i < slideObjects.Length; i++)
    {
      if (slideObjects[i] != null)
      {
        // Garante que só o primeiro está ativo
        bool isActive = (i == 0);
        slideObjects[i].SetActive(isActive);

        // Se for o primeiro, faz o Fade In nele
        if (isActive)
        {
          StartCoroutine(DoFadeIn(slideObjects[i]));
        }
      }
    }
  }

  void NextSlide()
  {
    // Desliga o atual
    if (_currentIndex < slideObjects.Length)
    {
      slideObjects[_currentIndex].SetActive(false);
    }

    _currentIndex++;
    _timer = 0;

    // Se ainda tem slide
    if (_currentIndex < slideObjects.Length)
    {
      slideObjects[_currentIndex].SetActive(true);
      // MÁGICA AQUI: Chama o Fade In para o novo slide
      StartCoroutine(DoFadeIn(slideObjects[_currentIndex]));
    }
    else
    {
      EndCutscene();
    }
  }

  // --- A MÁGICA DO FADE IN ---
  IEnumerator DoFadeIn(GameObject slide)
  {
    // 1. Tenta pegar o CanvasGroup. Se não tiver, adiciona um na hora.
    CanvasGroup group = slide.GetComponent<CanvasGroup>();
    if (group == null)
    {
      group = slide.AddComponent<CanvasGroup>();
    }

    // 2. Começa invisível
    group.alpha = 0f;
    float timer = 0f;

    // 3. Loop para aumentar a opacidade gradualmente
    while (timer < fadeDuration)
    {
      timer += Time.deltaTime;
      // Regra de 3 para ir de 0 a 1 suavemente
      group.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
      yield return null; // Espera o próximo frame
    }

    // 4. Garante que terminou 100% visível
    group.alpha = 1f;
  }

  void EndCutscene()
  {
    SceneManager.LoadScene(gameSceneName);
  }
}