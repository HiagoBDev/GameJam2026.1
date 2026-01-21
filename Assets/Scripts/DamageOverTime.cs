using UnityEngine;

public class DamageOverTime : MonoBehaviour
{
  [Header("Balanceamento do Fogo")]
  [SerializeField] private int damage = 1;
  [SerializeField] private float damageInterval = 0.7f;
  [SerializeField] private float lifetime = 4.0f; // Aumentei um pouco pois 1s ele não faz nada

  [Header("Segurança")]
  [SerializeField] private float warmUpTime = 1.0f; // --- NOVO: Tempo seguro inicial (1s) ---

  private float _nextDamageTime = 0f;
  private float _creationTime; // Para saber quando nasceu

  void Start()
  {
    _creationTime = Time.time; // Marca a hora que nasceu
    Destroy(gameObject, lifetime);

    // Opcional: Se quiser, mude a cor ou alpha aqui para indicar que está "carregando"
    // ex: GetComponent<SpriteRenderer>().color = new Color(1,1,1, 0.5f);
  }

  private void OnTriggerStay2D(Collider2D other)
  {
    // --- LÓGICA DO TEMPO SEGURO ---
    // Se ainda não passou 1 segundo desde que nasceu, não faz nada
    if (Time.time < _creationTime + warmUpTime) return;

    // (Opcional: Voltar a cor ao normal aqui se tiver mudado no Start)

    if (other.CompareTag("Player"))
    {
      if (Time.time >= _nextDamageTime)
      {
        if (other.TryGetComponent(out PlayerController player))
        {
          player.TakeDamage(damage);

          if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.fireBurnSound);

          _nextDamageTime = Time.time + damageInterval;
        }
      }
    }
  }
}