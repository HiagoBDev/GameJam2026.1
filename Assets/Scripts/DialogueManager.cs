using UnityEngine;
using UnityEngine.UI; // Necessário para mexer na Imagem
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
  public static DialogueManager Instance;

  [Header("Componentes da UI")]
  [SerializeField] private GameObject dialoguePanel;
  [SerializeField] private Image portraitImage; // Referência para a Imagem do rosto
  [SerializeField] private TextMeshProUGUI nameText;
  [SerializeField] private TextMeshProUGUI dialogueText;

  [Header("Configurações")]
  [SerializeField] private float typingSpeed = 0.04f; // Quanto menor, mais rápido digita

  // Estado Interno
  private Queue<string> _sentences;
  private bool _isDialogueActive = false;
  public bool IsDialogueActive => _isDialogueActive;
  private bool _isTyping = false;
  private string _currentSentence; // Guarda a frase atual caso precisemos pular a digitação
  private Coroutine _typingCoroutine;

  void Awake()
  {
    if (Instance == null) Instance = this;
    else Destroy(gameObject);

    _sentences = new Queue<string>();
    dialoguePanel.SetActive(false); // Garante que começa fechado
  }

  void Update()
  {
    if (!_isDialogueActive) return;

    // Inputs para avançar o diálogo (E ou Espaço)
    if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
    {
      if (_isTyping)
      {
        // Se estiver digitando, completa a frase imediatamente
        FinishTypingImmediately();
      }
      else
      {
        // Se já terminou de digitar, vai para a próxima
        DisplayNextSentence();
      }
    }
  }

  // --- MÉTODOS PÚBLICOS (Chamados pelos NPCs) ---

  // Agora recebe a Imagem (Sprite) também!
  public void StartDialogue(string npcName, Sprite npcPortrait, string[] sentences)
  {
    _isDialogueActive = true;
    dialoguePanel.SetActive(true);

    PlayerController player = FindFirstObjectByType<PlayerController>();
    if (player != null) player.LockMovement(true);

    // Configura a UI
    nameText.text = npcName;
    portraitImage.sprite = npcPortrait; // Troca a foto

    // Limpa e enfileira as novas frases
    _sentences.Clear();
    foreach (string sentence in sentences)
    {
      _sentences.Enqueue(sentence);
    }

    DisplayNextSentence();
  }

  // --- MÉTODOS INTERNOS ---

  private void DisplayNextSentence()
  {
    // Se acabaram as frases, encerra o diálogo
    if (_sentences.Count == 0)
    {
      EndDialogue();
      return;
    }

    // Pega a próxima frase da fila
    _currentSentence = _sentences.Dequeue();

    // Garante que não tem nenhuma digitação rodando antes de começar outra
    if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);

    // Inicia o efeito de digitação
    _typingCoroutine = StartCoroutine(TypeSentence(_currentSentence));
  }

  // A mágica da digitação letra por letra
  IEnumerator TypeSentence(string sentence)
  {
    _isTyping = true;
    dialogueText.text = ""; // Limpa o texto

    // Adiciona letra por letra
    foreach (char letter in sentence.ToCharArray())
    {
      dialogueText.text += letter;
      // Espera um pouquinho entre cada letra (pode adicionar som aqui depois!)
      yield return new WaitForSeconds(typingSpeed);
    }

    _isTyping = false;
  }

  // Pula o efeito de digitação e mostra tudo de uma vez
  private void FinishTypingImmediately()
  {
    if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
    dialogueText.text = _currentSentence;
    _isTyping = false;
  }

  private void EndDialogue()
  {
    _isDialogueActive = false;
    dialoguePanel.SetActive(false);

    // 2. Encontra o Player e manda destravar
    PlayerController player = FindFirstObjectByType<PlayerController>();
    if (player != null) player.LockMovement(false);
  }
}