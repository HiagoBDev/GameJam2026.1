using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Necessário para mexer no Botão

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuração da UI")]
    [SerializeField] private Button continueButton; // Arraste o botão Continuar pra cá

    [Header("Nomes das Cenas")]
    // IMPORTANTE: Escreva aqui o nome EXATO das suas cenas
    [SerializeField] private string cutsceneSceneName = "CutsceneScene"; 
    [SerializeField] private string gameSceneName = "MainScene";

    void Start()
    {
        // Lógica do Save:
        // Verifica se existe a chave "GameSaved". Se NÃO existir, desliga o botão Continuar.
        if (!PlayerPrefs.HasKey("GameSaved"))
        {
            continueButton.interactable = false; // Deixa cinza e não clicável
            // Opcional: Se quiser esconder o botão, use: continueButton.gameObject.SetActive(false);
        }
    }

    public void OnNewGameClicked()
    {
        // Novo jogo: Começa pela história (Cutscene)
        // Opcional: PlayerPrefs.DeleteAll(); // Se quiser apagar o save antigo ao iniciar um novo
        SceneManager.LoadScene(cutsceneSceneName);
    }

    public void OnContinueClicked()
    {
        // Continuar: Pula a história e vai direto pro jogo
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnQuitClicked()
    {
        Debug.Log("Saindo do Jogo...");
        Application.Quit();
    }
}