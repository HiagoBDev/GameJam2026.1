using UnityEngine;

public class HealthPickup : MonoBehaviour
{
  [SerializeField] private int healAmount = 1;

  // Efeitinho visual: flutuar para cima e para baixo
  void Update()
  {
    float y = Mathf.Sin(Time.time * 3) * 0.1f;
    transform.localPosition += new Vector3(0, y, 0) * Time.deltaTime;
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    // Só o Player pode pegar
    if (other.CompareTag("Player"))
    {
      // Tenta pegar o script do player
      if (other.TryGetComponent(out PlayerController player))
      {
        player.Heal(healAmount);
        Destroy(gameObject); // O item some depois de pego
      }
    }
  }
}