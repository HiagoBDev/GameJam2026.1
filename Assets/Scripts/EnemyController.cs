using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
  [Header("Stats")]
  [SerializeField] private float speed = 2f;
  [SerializeField] private float stopDistance = 1.2f;
  [SerializeField] private float attackCooldown = 2f;
  [SerializeField] private int maxHealth = 3;

  [Header("Combat Stats")]
  [SerializeField] private int attackDamage = 1;
  [SerializeField] private float attackRange = 0.6f;
  [SerializeField] private Transform attackPoint;
  [SerializeField] private LayerMask playerLayer;

  [Header("Loot Settings (Health)")]
  [SerializeField] private GameObject healthDropPrefab;
  [SerializeField][Range(0, 100)] private int dropChance = 30; // Chance do Coração

  [Header("Loot Settings (Quest)")] // --- NOVO ---
  [SerializeField] private GameObject soulPrefab; // Arraste o item Alma aqui
  [SerializeField][Range(0, 100)] private int soulDropChance = 20; // Chance da Alma (Balanceie aqui!)

  // Components
  private Animator _anim;
  private Rigidbody2D _rb;
  private Transform _playerTarget;

  // States
  private bool _isDead = false;
  private bool _canAttack = true;
  private bool _isSpawning = true;
  private int _currentHealth;
  private Vector2 _movementDirection;

  // Hashes
  private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
  private static readonly int AttackHash = Animator.StringToHash("Attack");
  private static readonly int HitHash = Animator.StringToHash("Hit");
  private static readonly int DieHash = Animator.StringToHash("Die");

  void Awake()
  {
    _anim = GetComponent<Animator>();
    _rb = GetComponent<Rigidbody2D>();
    _currentHealth = maxHealth;
  }

  void Start()
  {
    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
    if (playerObj != null) _playerTarget = playerObj.transform;

    StartCoroutine(SpawnRoutine());
  }

  IEnumerator SpawnRoutine()
  {
    yield return new WaitForSeconds(2.0f);
    _isSpawning = false;
  }

  void Update()
  {
    if (_isDead || _isSpawning || _playerTarget == null)
    {
      _movementDirection = Vector2.zero;
      return;
    }

    float distanceToPlayer = Vector2.Distance(transform.position, _playerTarget.position);

    // --- CORREÇÃO DO FLIP ---
    if (_playerTarget.position.x > transform.position.x)
    {
      transform.localScale = new Vector3(-1, 1, 1);
    }
    else
    {
      transform.localScale = new Vector3(1, 1, 1);
    }
    // ------------------------

    if (distanceToPlayer > stopDistance)
    {
      PrepareChase();
    }
    else
    {
      StopAndAttack();
    }
  }

  void FixedUpdate()
  {
    if (_isDead || _isSpawning)
    {
      _rb.linearVelocity = Vector2.zero;
      return;
    }
    _rb.linearVelocity = _movementDirection * speed;
  }

  void PrepareChase()
  {
    _movementDirection = (_playerTarget.position - transform.position).normalized;
    _anim.SetBool(IsMovingHash, true);
  }

  void StopAndAttack()
  {
    _movementDirection = Vector2.zero;
    _anim.SetBool(IsMovingHash, false);

    if (_canAttack)
    {
      StartCoroutine(AttackRoutine());
    }
  }

  IEnumerator AttackRoutine()
  {
    _canAttack = false;
    _anim.SetTrigger(AttackHash);

    yield return new WaitForSeconds(0.2f);

    if (!_isDead)
    {
      CheckPlayerHit();
    }

    yield return new WaitForSeconds(attackCooldown);
    _canAttack = true;
  }

  void CheckPlayerHit()
  {
    if (attackPoint == null) return;

    Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);

    if (hitPlayer != null)
    {
      if (hitPlayer.TryGetComponent(out PlayerController playerScript))
      {
        playerScript.TakeDamage(attackDamage);
      }
    }
  }

  public void TakeDamage(int damage)
  {
    if (_isDead) return;

    _currentHealth -= damage;

    // Som de Dano no Inimigo
    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(AudioManager.Instance.enemyHitSound);

    if (_currentHealth <= 0)
    {
      Die();
    }
    else
    {
      _anim.SetTrigger(HitHash);
      StartCoroutine(KnockbackRoutine());
    }
  }

  IEnumerator KnockbackRoutine()
  {
    _isSpawning = true;
    _movementDirection = Vector2.zero;
    _rb.linearVelocity = Vector2.zero;
    yield return new WaitForSeconds(0.2f);
    _isSpawning = false;
  }

  void Die()
  {
    if (_isDead) return;

    _isDead = true;
    _movementDirection = Vector2.zero;
    _rb.linearVelocity = Vector2.zero;

    StopAllCoroutines();

    _anim.SetTrigger(DieHash);

    // Som de Morte
    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(AudioManager.Instance.enemyDieSound);

    TryDropLoot();

    var colliders = GetComponents<Collider2D>();
    foreach (var col in colliders) col.enabled = false;

    Destroy(gameObject, 2f);
  }

  // --- SISTEMA DE LOOT ATUALIZADO ---
  void TryDropLoot()
  {
    // 1. Tenta dropar Vida (Coração)
    if (healthDropPrefab != null)
    {
      int roll = Random.Range(0, 100);
      if (roll < dropChance)
      {
        Instantiate(healthDropPrefab, transform.position, Quaternion.identity);
        Debug.Log("Inimigo dropou item de cura!");
      }
    }

    // 2. Tenta dropar Alma (Quest) --- NOVO ---
    if (soulPrefab != null)
    {
      int roll = Random.Range(0, 100);
      if (roll < soulDropChance)
      {
        // Adicionei um pequeno offset (desvio) para não ficar exatamente em cima do coração se dropar os dois
        Vector3 offset = new Vector3(0.2f, 0.2f, 0);
        Instantiate(soulPrefab, transform.position + offset, Quaternion.identity);
        Debug.Log("Inimigo dropou ALMA!");
      }
    }
  }
  // ----------------------------------

  void OnDrawGizmosSelected()
  {
    if (attackPoint != null)
    {
      Gizmos.color = Color.yellow;
      Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
  }
}