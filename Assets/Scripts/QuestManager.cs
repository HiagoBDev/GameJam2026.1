using UnityEngine;
using TMPro;
using System.Collections;
using Unity.Cinemachine;

public class QuestManager : MonoBehaviour
{
  public static QuestManager Instance;

  public enum QuestStage { Area1, Area2, Area3, BossFight, Completed }

  [Header("Estado Atual")]
  public QuestStage currentStage = QuestStage.Area1;
  public int currentSouls = 0;
  private bool _hasStarted = false;

  [Header("Metas")]
  public int soulsNeededArea1 = 3;
  public int soulsNeededArea2 = 5;

  [Header("Referências de Cena")]
  [SerializeField] private GameObject fogArea2;
  [SerializeField] private GameObject fogArea3;
  [SerializeField] private CinemachineCamera vCam;
  [SerializeField] private Transform playerTransform;

  [Header("Spawners")]
  [SerializeField] private EnemySpawner spawnerArea1;
  [SerializeField] private EnemySpawner spawnerArea2;

  [Header("Boss & NPC Setup")]
  [SerializeField] private TextMeshProUGUI questCounterText;
  [SerializeField] private GameObject bossObject;
  [SerializeField] private GameObject npcObject;
  [SerializeField] private Transform npcSpotArea3;
  [SerializeField] private GameObject transformationEffect;

  // --- NOVO: IDENTIDADE DO BOSS PARA O DIÁLOGO ---
  [Header("Identidade do Boss")]
  [SerializeField] private string bossName = "Rei das Sombras"; // Nome que vai aparecer
  [SerializeField] private Sprite bossPortrait; // A foto dele
                                                // -----------------------------------------------

  [Header("Diálogo da Armadilha")]
  [TextArea(2, 5)][SerializeField] private string[] trapDialogues;

  void Awake()
  {
    if (Instance == null) Instance = this;
    else Destroy(gameObject);
  }

  void Start()
  {
    UpdateUI();
    if (spawnerArea1 != null) spawnerArea1.StopSpawning();
    if (bossObject != null) bossObject.SetActive(false);
    if (transformationEffect != null) transformationEffect.SetActive(false);
    if (fogArea2 != null) fogArea2.SetActive(true);
    if (fogArea3 != null) fogArea3.SetActive(true);
  }

  public void StartFirstQuest()
  {
    if (_hasStarted) return;
    _hasStarted = true;
    if (spawnerArea1 != null) spawnerArea1.StartSpawning();
    UpdateUI();
  }

  public void CollectSoul()
  {
    if (!_hasStarted) return;
    if (currentStage == QuestStage.BossFight || currentStage == QuestStage.Completed) return;

    currentSouls++;
    UpdateUI();

    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(AudioManager.Instance.specialSound);
  }

  public bool IsQuestReadyToTurnIn()
  {
    int target = GetTargetSouls();
    return currentSouls >= target;
  }

  public void AdvanceQuest()
  {
    currentSouls = 0;

    switch (currentStage)
    {
      case QuestStage.Area1:
        currentStage = QuestStage.Area2;
        if (spawnerArea1 != null) spawnerArea1.StopSpawning();
        DestroyAllActiveEnemies();

        if (spawnerArea2 != null) spawnerArea2.StartSpawning();
        StartCoroutine(UnlockSequence(fogArea2, false));
        break;

      case QuestStage.Area2:
        currentStage = QuestStage.Area3;
        if (spawnerArea2 != null) spawnerArea2.StopSpawning();
        DestroyAllActiveEnemies();

        StartCoroutine(UnlockSequence(fogArea3, true));
        break;
    }
    UpdateUI();
  }

  public void StartBossTrapSequence()
  {
    if (currentStage != QuestStage.Area3) return;
    currentStage = QuestStage.BossFight;
    StartCoroutine(BossTrapRoutine());
  }

  IEnumerator BossTrapRoutine()
  {
    PlayerController p = playerTransform.GetComponent<PlayerController>();
    if (p) { p.LockMovement(true); p.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; }

    // --- AQUI ESTAVA O PROBLEMA ("???", null) ---
    // Agora usamos as variáveis que criamos lá em cima:
    DialogueManager.Instance.StartDialogue(bossName, bossPortrait, trapDialogues);

    while (DialogueManager.Instance.IsDialogueActive) yield return null;

    if (npcObject != null) npcObject.SetActive(false);
    if (transformationEffect != null) transformationEffect.SetActive(true);

    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(AudioManager.Instance.specialSound);

    yield return new WaitForSeconds(1.0f);
    if (transformationEffect != null) transformationEffect.SetActive(false);

    if (bossObject != null) bossObject.SetActive(true);
    TriggerBossFight();

    if (p) p.LockMovement(false);
  }

  private void DestroyAllActiveEnemies()
  {
    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    foreach (GameObject enemy in enemies) Destroy(enemy);
  }

  IEnumerator UnlockSequence(GameObject fogBarrier, bool moveNPC)
  {
    PlayerController p = playerTransform.GetComponent<PlayerController>();
    if (p) p.LockMovement(true);

    Transform originalTarget = vCam.Follow;
    vCam.Follow = fogBarrier.transform;

    if (moveNPC && npcObject != null && npcSpotArea3 != null)
    {
      yield return new WaitForSeconds(0.5f);
      npcObject.transform.position = npcSpotArea3.position;
    }
    else
    {
      yield return new WaitForSeconds(2.0f);
    }

    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(AudioManager.Instance.specialSound);

    fogBarrier.SetActive(false);
    yield return new WaitForSeconds(1.5f);

    vCam.Follow = originalTarget;
    yield return new WaitForSeconds(1.5f);

    if (p) p.LockMovement(false);
  }

  public int GetTargetSouls()
  {
    switch (currentStage)
    {
      case QuestStage.Area1: return soulsNeededArea1;
      case QuestStage.Area2: return soulsNeededArea2;
      default: return 999;
    }
  }

  public void TriggerBossFight() { UpdateUI(); }

  private void UpdateUI()
  {
    if (questCounterText == null) return;
    if (!_hasStarted) { questCounterText.text = ""; return; }

    if (currentStage == QuestStage.BossFight)
    {
      questCounterText.text = "DERROTE O REI!";
      questCounterText.color = Color.red;
    }
    else if (currentStage == QuestStage.Area3)
    {
      questCounterText.text = "ENCONTRE O HOMEM MISTERIOSO NA FLORESTA!";
      questCounterText.color = Color.yellow;
    }
    else
    {
      int target = GetTargetSouls();
      if (currentSouls >= target)
      {
        questCounterText.text = "FALE COM O NPC!";
        questCounterText.color = Color.green;
      }
      else
      {
        questCounterText.text = $"Almas: {currentSouls} / {target}";
        questCounterText.color = Color.white;
      }
    }
  }

  public bool HasQuestStarted() => _hasStarted;
}