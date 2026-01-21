using UnityEngine;

public class BossTrapTrigger : MonoBehaviour
{
  private bool _triggered = false;

  private void OnTriggerEnter2D(Collider2D other)
  {
    // Se já foi ativado, não faz nada (evita chamar 2x)
    if (_triggered) return;

    if (other.CompareTag("Player"))
    {
      _triggered = true;
      Debug.Log("Armadilha ativada!");

      // Chama a sequência de filme no QuestManager
      if (QuestManager.Instance != null)
      {
        QuestManager.Instance.StartBossTrapSequence();
      }

      // Desliga este gatilho para não atrapalhar mais
      gameObject.SetActive(false);
    }
  }
}