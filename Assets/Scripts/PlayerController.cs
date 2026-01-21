using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
  [Header("Movement Settings")]
  [SerializeField] private float speed = 3f;
  [SerializeField] private float footstepInterval = 0.4f;
  private float _footstepTimer;

  [Header("Combat Settings")]
  [SerializeField] private float attackDuration = 0.4f;
  [SerializeField] private float hitStunDuration = 0.3f;

  [Header("Invincibility Settings")]
  [SerializeField] private float invincibilityDuration = 1.0f;
  [SerializeField] private float flashSpeed = 0.1f;
  private bool _isInvincible = false;

  [Header("Health Settings")]
  [SerializeField] private int maxHealth = 10; // --- ALTERADO PARA 10 ---
  private int _currentHealth;

  [Header("Special Settings")]
  [SerializeField] private float specialCooldown = 10f;
  private float _currentSpecialCharge = 0f;

  [Header("Attack Stats")]
  [SerializeField] private int attackDamage = 1;
  [SerializeField] private float attackRange = 0.8f;
  [SerializeField] private Transform attackPoint;
  [SerializeField] private LayerMask enemyLayers;

  [Header("Dependencies")]
  [SerializeField] private FlashEffect flashEffect;

  // Cache components
  private Animator _anim;
  private Rigidbody2D _rb;
  private SpriteRenderer _spriteRenderer;
  private AudioSource _playerSource; // Adicionado para sons exclusivos se necessário

  // Internal State
  private Vector2 _moveInput;
  private bool _isDead;
  private bool _isLocked;
  private bool _canInteract;

  // Hashes
  private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
  private static readonly int AttackHash = Animator.StringToHash("Attack");
  private static readonly int SpecialHash = Animator.StringToHash("Special");
  private static readonly int HitHash = Animator.StringToHash("Hit");
  private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

  void Awake()
  {
    _anim = GetComponent<Animator>();
    _rb = GetComponent<Rigidbody2D>();

    _spriteRenderer = GetComponent<SpriteRenderer>();
    if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

    // Configuração de AudioSource próprio para pitch se necessário
    _playerSource = GetComponent<AudioSource>();
    if (_playerSource == null) _playerSource = gameObject.AddComponent<AudioSource>();

    _currentHealth = maxHealth;

    PlayerPrefs.SetInt("GameSaved", 1);
    PlayerPrefs.Save();
  }

  void Start()
  {
    _currentSpecialCharge = specialCooldown;
    UpdateSpecialUI();

    // Garante que a UI comece cheia
    if (UIManager.Instance != null) UIManager.Instance.UpdateHealthUI(_currentHealth);
  }

  void Update()
  {
    if (_isDead) return;

    RechargeSpecialRoutine();

    if (_isLocked)
    {
      _moveInput = Vector2.zero;
      return;
    }

    HandleMovementInput();
    HandleFootsteps();
    HandleCombatInput();
  }

  void FixedUpdate()
  {
    if (_isDead) return;

    if (_isLocked)
    {
      _rb.linearVelocity = Vector2.zero;
      return;
    }

    Move();
  }

  private void RechargeSpecialRoutine()
  {
    if (_currentSpecialCharge < specialCooldown)
    {
      _currentSpecialCharge += Time.deltaTime;
      if (_currentSpecialCharge > specialCooldown)
        _currentSpecialCharge = specialCooldown;

      UpdateSpecialUI();
    }
  }

  private void UpdateSpecialUI()
  {
    if (UIManager.Instance != null)
    {
      float percent = _currentSpecialCharge / specialCooldown;
      UIManager.Instance.UpdateUltBar(percent);
    }
  }

  private void HandleMovementInput()
  {
    float moveX = Input.GetAxisRaw("Horizontal");
    float moveY = Input.GetAxisRaw("Vertical");
    _moveInput = new Vector2(moveX, moveY).normalized;

    bool isMoving = _moveInput.sqrMagnitude > 0.01f;
    _anim.SetBool(IsMovingHash, isMoving);

    if (_moveInput.x != 0)
    {
      float direction = Mathf.Sign(_moveInput.x);
      transform.localScale = new Vector3(direction, 1, 1);
    }
  }

  private void HandleFootsteps()
  {
    if (_moveInput.sqrMagnitude > 0.01f && !_isLocked)
    {
      _footstepTimer -= Time.deltaTime;
      if (_footstepTimer <= 0)
      {
        if (AudioManager.Instance != null && AudioManager.Instance.stepSound != null)
        {
          // Pequena variação de pitch para soar natural
          _playerSource.pitch = Random.Range(1.0f, 1.2f);
          _playerSource.PlayOneShot(AudioManager.Instance.stepSound, 0.6f);
        }
        _footstepTimer = footstepInterval;
      }
    }
    else
    {
      _footstepTimer = 0;
    }
  }

  private void HandleCombatInput()
  {
    if (Input.GetKeyDown(KeyCode.K)) TakeDamage(1);
    if (Input.GetKeyDown(KeyCode.L)) Die();

    // ATAQUE BÁSICO -> Botão Esquerdo
    if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
    {
      StartCoroutine(PerformAttack());
    }

    // ESPECIAL -> Botão Direito
    if (Input.GetMouseButtonDown(1))
    {
      if (_currentSpecialCharge >= specialCooldown)
      {
        StartCoroutine(PerformSpecial());
      }
      else
      {
        Debug.Log("Especial carregando...");
      }
    }
  }

  private void Move()
  {
    _rb.linearVelocity = _moveInput * speed;
  }

  private IEnumerator PerformAttack()
  {
    _isLocked = true;
    _anim.SetTrigger(AttackHash);

    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(AudioManager.Instance.attackSound);

    yield return new WaitForSeconds(0.2f);

    if (attackPoint != null)
    {
      Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
      foreach (Collider2D enemy in hitEnemies)
      {
        if (enemy.TryGetComponent(out EnemyController enemyScript))
        {
          enemyScript.TakeDamage(attackDamage);
        }
        else if (enemy.TryGetComponent(out ShadowBossController bossScript))
        {
          bossScript.TakeDamage(attackDamage);
        }
      }
    }

    yield return new WaitForSeconds(attackDuration - 0.2f);
    _isLocked = false;
  }

  private IEnumerator PerformSpecial()
  {
    _currentSpecialCharge = 0;
    UpdateSpecialUI();

    _isLocked = true;
    _anim.SetTrigger(SpecialHash);

    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(AudioManager.Instance.specialSound);

    yield return new WaitForSeconds(0.2f);

    if (flashEffect != null)
    {
      yield return StartCoroutine(flashEffect.Flash());
    }

    KillEnemiesOnScreen();

    yield return new WaitForSeconds(0.5f);
    _isLocked = false;
  }

  public void Heal(int amount)
  {
    if (_isDead) return;
    _currentHealth += amount;
    if (_currentHealth > maxHealth) _currentHealth = maxHealth;
    if (UIManager.Instance != null) UIManager.Instance.UpdateHealthUI(_currentHealth);
  }

  public void TakeDamage(int damage)
  {
    if (_isDead) return;
    if (_isInvincible) return;

    _currentHealth -= damage;
    if (UIManager.Instance != null) UIManager.Instance.UpdateHealthUI(_currentHealth);

    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(AudioManager.Instance.hurtSound);

    if (_currentHealth <= 0)
    {
      Die();
    }
    else
    {
      StopAllCoroutines();
      _isLocked = true;
      _anim.SetTrigger(HitHash);

      StartCoroutine(InvincibilityRoutine());
      StartCoroutine(UnlockRoutine(hitStunDuration));
    }
  }

  private IEnumerator InvincibilityRoutine()
  {
    _isInvincible = true;
    if (_spriteRenderer != null)
    {
      float timer = 0;
      while (timer < invincibilityDuration)
      {
        _spriteRenderer.enabled = !_spriteRenderer.enabled;
        yield return new WaitForSeconds(flashSpeed);
        timer += flashSpeed;
      }
      _spriteRenderer.enabled = true;
    }
    else
    {
      yield return new WaitForSeconds(invincibilityDuration);
    }
    _isInvincible = false;
  }

  private IEnumerator UnlockRoutine(float delay)
  {
    yield return new WaitForSeconds(delay);
    _isLocked = false;
  }

  void KillEnemiesOnScreen()
  {
    var enemies = GameObject.FindGameObjectsWithTag("Enemy");
    foreach (var enemy in enemies)
    {
      if (enemy.TryGetComponent(out EnemyController ec)) ec.TakeDamage(999);
      else if (enemy.TryGetComponent(out ShadowBossController boss)) boss.TakeDamage(10);
      else Destroy(enemy);
    }
  }

  void Die()
  {
    _isDead = true;
    _isLocked = true;
    _anim.SetBool(IsDeadHash, true);
    _rb.linearVelocity = Vector2.zero;

    if (TryGetComponent(out Collider2D col)) col.enabled = false;

    EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
    if (spawner != null) spawner.StopSpawning();

    if (UIManager.Instance != null) UIManager.Instance.ShowGameOver();
  }

  public void LockMovement(bool state)
  {
    _isLocked = state;
    if (_isLocked)
    {
      _moveInput = Vector2.zero;
      _rb.linearVelocity = Vector2.zero;
      _anim.SetBool(IsMovingHash, false);
    }
  }

  public void SetInteractionState(bool canInteract)
  {
    _canInteract = canInteract;
  }

  void OnDrawGizmosSelected()
  {
    if (attackPoint == null) return;
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(attackPoint.position, attackRange);
  }
}