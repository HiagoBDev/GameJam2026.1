using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
  [Header("Configuração")]
  [SerializeField] private GameObject enemyPrefab;
  [SerializeField] private float spawnInterval = 3f;
  [SerializeField] private bool autoStart = true; // Area 1 marca TRUE, Area 2 marca FALSE

  [Header("Area de Spawn")]
  [SerializeField] private float width = 5f;
  [SerializeField] private float height = 3f;

  private bool _isSpawning = false;

  void Start()
  {
    if (autoStart)
    {
      StartSpawning();
    }
  }

  public void StartSpawning()
  {
    if (!_isSpawning)
    {
      _isSpawning = true;
      StartCoroutine(SpawnRoutine());
    }
  }

  public void StopSpawning()
  {
    _isSpawning = false;
    StopAllCoroutines();

    // Opcional: Se quiser matar todos os monstros da area quando ela completa:
    // DestroyAllEnemies(); 
  }

  IEnumerator SpawnRoutine()
  {
    while (_isSpawning)
    {
      SpawnEnemy();
      yield return new WaitForSeconds(spawnInterval);
    }
  }

  void SpawnEnemy()
  {
    // Gera uma posição aleatória dentro da caixa definida
    float randomX = Random.Range(-width / 2, width / 2);
    float randomY = Random.Range(-height / 2, height / 2);
    Vector3 spawnPos = transform.position + new Vector3(randomX, randomY, 0);

    Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
  }

  // Desenha a caixa de spawn no editor pra facilitar sua vida
  void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.green;
    Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0));
  }
}