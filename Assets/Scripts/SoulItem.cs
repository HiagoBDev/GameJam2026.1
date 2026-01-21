using UnityEngine;

public class SoulItem : MonoBehaviour
{
  // Efeito visual de flutuar
  void Update()
  {
    float y = Mathf.Sin(Time.time * 3) * 0.1f;
    transform.localPosition += new Vector3(0, y, 0) * Time.deltaTime;
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("Player"))
    {
      // Avisa o gerente que pegou uma
      QuestManager.Instance.CollectSoul();

      Destroy(gameObject); // Some
    }
  }
}