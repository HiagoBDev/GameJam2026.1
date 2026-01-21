using UnityEngine;
using System.Collections;

public class ShadowBossController : MonoBehaviour
{
  [Header("Status")]
  [SerializeField] float moveSpeed = 2f;
  [SerializeField] int maxHealth = 30;
  [SerializeField] float attackRange = 3f; // Distância para ataque físico (Soco)

  [Header("Ataques & Habilidades")]
  [SerializeField] GameObject groundFirePrefab;
  [SerializeField] float attackGlobalCooldown = 2f; // Tempo de descanso entre qualquer ação
  [SerializeField] float fireSkillCooldown = 20f;   // --- ALTERADO: 20 segundos de Cooldown ---
  [SerializeField] float spawnOffset = 1.5f;        // --- NOVO: Distância para spawnar atrás do player ---

  // Hashes do Animator
  private readonly int IsMovingHash = Animator.StringToHash("IsMoving");
  private readonly int AttackNormalHash = Animator.StringToHash("AttackNormal");
  private readonly int AttackCastHash = Animator.StringToHash("AttackCast");
  private readonly int GotHitHash = Animator.StringToHash("GotHit");
  private readonly int IsDeadHash = Animator.StringToHash("IsDead");

  // Componentes
  Rigidbody2D rb;
  Animator anim;
  Transform playerTarget;

  // Estado Interno
  int currentHealth;
  bool isDead = false;
  bool canAct = true;      // Controla o cooldown global (cansaço)
  bool isAttacking = false; // Controla se está no meio de uma animação
  float _fireTimer = 0f;    // Cronômetro interno do fogo

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    anim = GetComponentInChildren<Animator>();
    currentHealth = maxHealth;
    playerTarget = GameObject.FindGameObjectWithTag("Player").transform;

    // O Boss já começa com o fogo pronto para usar
    _fireTimer = fireSkillCooldown;
  }

  void Update()
  {
    if (isDead || playerTarget == null) return;

    // Atualiza o relógio do fogo
    _fireTimer += Time.deltaTime;

    float distance = Vector2.Distance(transform.position, playerTarget.position);
    FacePlayer();

    // --- MÁQUINA DE DECISÃO ---

    if (isAttacking)
    {
      // Se está atacando, fica parado e não faz mais nada
      rb.linearVelocity = Vector2.zero;
      anim.SetBool(IsMovingHash, false);
      return;
    }

    if (!canAct)
    {
      // Se está recuperando fôlego (Global Cooldown), fica parado
      rb.linearVelocity = Vector2.zero;
      anim.SetBool(IsMovingHash, false);
      return;
    }

    // PRIORIDADE 1: Se o Fogo carregou (20s), usa imediatamente (independente da distância)
    if (_fireTimer >= fireSkillCooldown)
    {
      StartCoroutine(PerformFireCast());
    }
    // PRIORIDADE 2: Se está perto o suficiente, dá soco
    else if (distance <= attackRange)
    {
      StartCoroutine(PerformMeleeAttack());
    }
    // PRIORIDADE 3: Se não tem fogo e está longe, persegue
    else
    {
      ChasePlayer();
    }
  }

  void ChasePlayer()
  {
    Vector2 dir = (playerTarget.position - transform.position).normalized;
    rb.linearVelocity = dir * moveSpeed;
    anim.SetBool(IsMovingHash, true);
  }

  void FacePlayer()
  {
    // Vira o sprite para olhar pro player
    if (playerTarget.position.x > transform.position.x)
      transform.localScale = new Vector3(-1, 1, 1);
    else
      transform.localScale = new Vector3(1, 1, 1);
  }

  // --- ROTINA DO FOGO (CAST ATRÁS DO PLAYER) ---
  IEnumerator PerformFireCast()
  {
    isAttacking = true;
    canAct = false;
    rb.linearVelocity = Vector2.zero;
    anim.SetBool(IsMovingHash, false);

    // Zera o timer do fogo para ter que esperar 20s de novo
    _fireTimer = 0f;

    anim.SetTrigger(AttackCastHash);

    // --- AUDIO: CAST ---
    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(AudioManager.Instance.bossCastSound);

    yield return new WaitForSeconds(0.5f); // Tempo da animação de levantar a mão

    if (groundFirePrefab != null && playerTarget != null)
    {
      // --- CÁLCULO DA POSIÇÃO "ATRÁS" ---
      // 1. Pega a direção que o player está olhando (scale X: 1 ou -1)
      float playerFacingDir = playerTarget.localScale.x;

      // 2. Calcula a posição atrás: Posição Atual - (Direção * Offset)
      // Ex: Se olha pra direita (1), subtraímos no X (vai pra esquerda)
      Vector3 spawnPos = playerTarget.position - new Vector3(playerFacingDir * spawnOffset, 0, 0);

      Instantiate(groundFirePrefab, spawnPos, Quaternion.identity);
    }

    yield return new WaitForSeconds(1f); // Termina a animação visual
    isAttacking = false;

    // Pequeno delay global antes de voltar a andar/bater
    yield return new WaitForSeconds(1.0f);
    canAct = true;
  }

  // --- ROTINA DO SOCO (MELEE) ---
  IEnumerator PerformMeleeAttack()
  {
    isAttacking = true;
    canAct = false;
    rb.linearVelocity = Vector2.zero;
    anim.SetBool(IsMovingHash, false);

    anim.SetTrigger(AttackNormalHash);

    // --- AUDIO: ATAQUE FÍSICO ---
    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(AudioManager.Instance.bossAttackSound);

    yield return new WaitForSeconds(0.5f); // Momento do impacto

    // Verifica distância novamente para dar dano
    if (playerTarget != null && Vector2.Distance(transform.position, playerTarget.position) <= attackRange)
    {
      playerTarget.GetComponent<PlayerController>().TakeDamage(2);
    }

    yield return new WaitForSeconds(0.5f); // Termina animação
    isAttacking = false;

    yield return new WaitForSeconds(attackGlobalCooldown); // Espera o cooldown do soco
    canAct = true;
  }

  public void TakeDamage(int dmg)
  {
    if (isDead) return;
    currentHealth -= dmg;
    anim.SetTrigger(GotHitHash);

    // --- AUDIO: DANO ---
    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(AudioManager.Instance.bossHitSound);

    if (currentHealth <= 0)
    {
      Die();
    }
  }

  void Die()
  {
    isDead = true;
    rb.linearVelocity = Vector2.zero;
    anim.SetBool(IsDeadHash, true);
    GetComponent<Collider2D>().enabled = false;

    // --- AUDIO: MORTE ---
    if (AudioManager.Instance != null)
      AudioManager.Instance.PlaySFX(AudioManager.Instance.bossDieSound);

    Debug.Log("BOSS DERROTADO!");

    // Chama a vitória na UI
    if (UIManager.Instance != null)
    {
      UIManager.Instance.ShowVictoryScreen();
    }
  }
}