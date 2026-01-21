using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
  [Header("Identidade")]
  [SerializeField] private string npcName = "Deus das Sombras";
  [SerializeField] private Sprite npcPortrait;

  [Header("Diálogos")]
  [TextArea(2, 5)][SerializeField] private string[] introDialogues;
  [TextArea(2, 5)][SerializeField] private string[] area2Dialogues;
  [TextArea(2, 5)][SerializeField] private string[] area3Dialogues; // NPC manda ir para Area 3     
  [TextArea(2, 5)][SerializeField] private string[] waitDialogues;
  [TextArea(2, 5)][SerializeField] private string[] returnToMeDialogues; // "Volte quando terminar"

  private bool _playerIsClose;

  void Update()
  {
    if (_playerIsClose && Input.GetKeyDown(KeyCode.E))
    {
      if (!DialogueManager.Instance.IsDialogueActive)
      {
        InteractWithQuest();
      }
    }
  }

  void InteractWithQuest()
  {
    QuestManager qm = QuestManager.Instance;

    string[] textToShow = waitDialogues;

    // Cenario 1: Missão ainda não começou
    if (!qm.HasQuestStarted())
    {
      textToShow = introDialogues;
      qm.StartFirstQuest();
    }
    // Cenario 2: Jogador completou a coleta (tem almas suficientes)
    else if (qm.IsQuestReadyToTurnIn())
    {
      // Verifica qual fase acabou de completar
      if (qm.currentStage == QuestManager.QuestStage.Area1)
      {
        textToShow = area2Dialogues; // "Bom trabalho, agora vá para a Area 2"
      }
      else if (qm.currentStage == QuestManager.QuestStage.Area2)
      {
        textToShow = area3Dialogues; // "Excelente. Vá para a Area 3, te espero lá."
      }

      // AVANÇA A MISSÃO (Libera nevoa, move NPC, etc)
      qm.AdvanceQuest();
    }
    // Cenario 3: Jogador está no meio da missão (não tem almas suficientes)
    else
    {
      textToShow = returnToMeDialogues; // "Ainda não terminou? Traga mais almas."
    }

    DialogueManager.Instance.StartDialogue(npcName, npcPortrait, textToShow);
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("Player"))
    {
      _playerIsClose = true;
      if (other.TryGetComponent(out PlayerController p)) p.SetInteractionState(true);
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (other.CompareTag("Player"))
    {
      _playerIsClose = false;
      if (other.TryGetComponent(out PlayerController p)) p.SetInteractionState(false);
    }
  }
}