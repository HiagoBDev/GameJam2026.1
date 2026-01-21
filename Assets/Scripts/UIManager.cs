using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
  public static UIManager Instance;

  [Header("Health UI")]
  [SerializeField] private List<Image> hearts;
  [SerializeField] private Sprite fullHeart;
  [SerializeField] private Sprite emptyHeart;

  [Header("Special Bar UI")]
  [SerializeField] private Image ultBarImage; // A imagem da barra na tela
  [SerializeField] private Sprite[] ultBarFrames; // A lista dos pedaços fatiados (0 a 10)

  [Header("Menus & Panels")]
  [SerializeField] private GameObject gameOverPanel;
  [SerializeField] private GameObject victoryPanel; // --- NOVO: Painel de Vitória ---

  void Awake()
  {
    if (Instance == null) Instance = this;
    else Destroy(gameObject);
  }

  // --- SAÚDE (CORAÇÕES) ---
  // --- ATUALIZAÇÃO DE SAÚDE (2 HP = 1 CORAÇÃO) ---
  // --- ATUALIZAÇÃO DE SAÚDE (2 HP = 1 CORAÇÃO) ---  
  public void UpdateHealthUI(int currentHealth)
  {
    Debug.Log($"Vida Atual: {currentHealth} | Corações na Lista: {hearts.Count}");
    for (int i = 0; i < hearts.Count; i++)
    {
      // LÓGICA:
      // Coração 0 (i=0): Representa HP 1 e 2. Deve acender se HP >= 1.
      // Coração 1 (i=1): Representa HP 3 e 4. Deve acender se HP >= 3.
      // Coração 2 (i=2): Representa HP 5 e 6. Deve acender se HP >= 5.
      // Fórmula: (i * 2) + 1

      if (currentHealth >= (i * 2) + 1)
      {
        hearts[i].sprite = fullHeart;
      }
      else
      {
        hearts[i].sprite = emptyHeart;
      }
    }
  }

  // --- BARRA DE ESPECIAL (FRAME A FRAME) ---
  public void UpdateUltBar(float percentage)
  {
    if (ultBarImage == null || ultBarFrames.Length == 0) return;

    // Garante que a porcentagem está entre 0 e 1
    percentage = Mathf.Clamp01(percentage);

    // Matemática mágica: Transforma 0.5 (50%) em índice do array
    // Ex: Se tem 10 frames, 50% vira o frame 5.
    int index = Mathf.FloorToInt(percentage * (ultBarFrames.Length - 1));

    // Troca o sprite na tela
    ultBarImage.sprite = ultBarFrames[index];
  }

  // --- GAME OVER ---
  public void ShowGameOver()
  {
    if (gameOverPanel != null)
    {
      gameOverPanel.SetActive(true);
      // Opcional: Pausar o jogo no game over também, se quiser
      // Time.timeScale = 0f; 
    }
  }

  // --- VITÓRIA (Chamado pelo Boss) ---
  public void ShowVictoryScreen()
  {
    if (victoryPanel != null)
    {
      victoryPanel.SetActive(true);

      // PAUSA O JOGO (Congela tudo)
      Time.timeScale = 0f;
    }
  }

  // --- BOTÕES (Linkar no OnClick da Unity) ---
  public void RestartGame()
  {
    // MUITO IMPORTANTE: Despausar o tempo antes de reiniciar
    // Se não fizer isso, o jogo recomeça congelado!
    Time.timeScale = 1f;

    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }

  public void QuitGame()
  {
    Debug.Log("Saindo do Jogo...");
    Application.Quit();
  }
}