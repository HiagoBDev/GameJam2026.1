using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
  [Header("Status do Boss")]
  [SerializeField] private string bossName = "Rei das Sombras";
  [SerializeField] private int maxHealth = 20;
  [SerializeField] private float moveSpeed = 1.5f;

  [Header("Combate")]
  [SerializeField] private float attackRange = 2.0f; // Distância para começar a atacar
  [SerializeField] private float attackCooldown = 3.0f; // Tempo entre ataques
  [SerializeField] private int contactDamage = 1; // Dano se o player encostar nele

  [Header("Habilidades")]
  [SerializeField] private GameObject shockwavePrefab; // Aquele ataque em anel que criamos antes
  [SerializeField] private Transform attackPoint; // De onde sai o poder (centro dele)

  [Header("Referências")]
  [SerializeField] private GameObject healthBarUI; // Opcional: Se quiser barra de vida na cabeça

  // Estado Interno
  private int _currentHealth;
  private Transform _player;
  private bool _isDead = false;
  private bool _canAttack = true;
  private bool _isAttacking = false;
  private Animator _anim;
  private Rigidbody2D _rb;

  // Hashes de Animação (Para otimizar)
  private static readonly int WalkHash = Animator.StringToHash("Walk");
  private static readonly int AttackHash = Animator.StringToHash("Attack");
  private static readonly int CastHash = Animator.StringToHash("Cast"); // Para o poder especial
  private static readonly int DieHash = Animator.StringToHash("Die");
  private static readonly int HitHash = Animator.StringToHash("Hit");

  void Awake()
  {
    _rb = GetComponent<Rigidbody2D>();
    _anim = GetComponent<Animator>();
    _currentHealth = maxHealth;
  }

  void Start()
  {
    GameObject p = GameObject.FindGameObjectWithTag("Player");
    if (p != null) _player = p.transform;
  }

  void Update()
  {
    if (_isDead || _player == null) return;

    // Olha para o Player (Flip)
    FlipTowardsPlayer();

    float distance = Vector2.Distance(transform.position, _player.position);

    // MÁQUINA DE ESTADOS SIMPLES
    if (_isAttacking)
    {
      // Se está no meio da animação de ataque, não se move
      _rb.linearVelocity = Vector2.zero;
    }
    else if (distance <= attackRange && _canAttack)
    {
      // Se chegou perto e pode atacar -> ATACA
      StartCoroutine(AttackRoutine());
    }
    else if (distance > attackRange)
    {
      // Se está longe -> PERSEGUE
      MoveTowardsPlayer();
    }
    else
    {
      // Está perto mas em cooldown -> Fica parado esperando
      _rb.linearVelocity = Vector2.zero;
      if (_anim) _anim.SetBool(WalkHash, false);
    }
  }

  void MoveTowardsPlayer()
  {
    Vector2 direction = (_player.position - transform.position).normalized;
    _rb.linearVelocity = direction * moveSpeed;

    if (_anim) _anim.SetBool(WalkHash, true);
  }

  void FlipTowardsPlayer()
  {
    if (_player.position.x > transform.position.x)
      transform.localScale = new Vector3(-1, 1, 1); // Vira pra direita (se o sprite original olha pra esquerda)
    else
      transform.localScale = new Vector3(1, 1, 1);
  }

  IEnumerator AttackRoutine()
  {
    _isAttacking = true;
    _canAttack = false;
    _rb.linearVelocity = Vector2.zero; // Para de andar
    if (_anim) _anim.SetBool(WalkHash, false);

    // DECIDE QUAL ATAQUE USAR (50% de chance para cada)
    int rng = Random.Range(0, 2);

    if (rng == 0 && shockwavePrefab != null)
    {
      // --- ATAQUE ESPECIAL (Onda de Choque) ---
      if (_anim) _anim.SetTrigger(CastHash);

      // Aviso visual antes do ataque (opcional)
      yield return new WaitForSeconds(0.5f);

      Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
      Debug.Log("Boss usou Shockwave!");
    }
    else
    {
      // --- ATAQUE FÍSICO (Bater perto) ---
      if (_anim) _anim.SetTrigger(AttackHash);

      yield return new WaitForSeconds(0.3f); // Delay do impacto

      // Verifica se player ainda está perto para dar dano
      if (Vector2.Distance(transform.position, _player.position) <= attackRange)
      {
        _player.GetComponent<PlayerController>().TakeDamage(contactDamage);
      }
    }

    yield return new WaitForSeconds(1.0f); // Tempo da animação acabar
    _isAttacking = false;

    yield return new WaitForSeconds(attackCooldown); // Tempo esperando para o próximo
    _canAttack = true;
  }

  public void TakeDamage(int damage)
  {
    if (_isDead) return;

    _currentHealth -= damage;
    if (_anim) _anim.SetTrigger(HitHash);

    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(AudioManager.Instance.enemyHitSound);

    Debug.Log($"Boss Vida: {_currentHealth}/{maxHealth}");

    if (_currentHealth <= 0)
    {
      Die();
    }
  }

  void Die()
  {
    _isDead = true;
    _rb.linearVelocity = Vector2.zero;
    StopAllCoroutines();

    if (_anim) _anim.SetTrigger(DieHash);

    // Remove colisor para não bater mais
    GetComponent<Collider2D>().enabled = false;

    // AVISA O QUEST MANAGER QUE O JOGO ACABOU!
    // QuestManager.Instance.BossDefeated(); // <--- Vamos criar isso depois
    Debug.Log("BOSS DERROTADO! VITÓRIA!");
    if (UIManager.Instance != null)
    {
      // Precisamos criar esse método no UIManager rapidinho ou usar o GameOverPanel com texto diferente
      // Vamos improvisar com um log por enquanto ou você cria um public GameObject victoryPanel no UIManager
      Debug.Log("CHAMAR TELA DE VITORIA AQUI");
    }

    // Tela de Vitoria (Exemplo simples)
    // UIManager.Instance.ShowVictoryScreen(); 
  }

  // Dano por contato (se o player encostar no corpo dele sem atacar)
  private void OnCollisionEnter2D(Collision2D other)
  {
    if (other.gameObject.CompareTag("Player"))
    {
      other.gameObject.GetComponent<PlayerController>().TakeDamage(1);
    }
  }
}