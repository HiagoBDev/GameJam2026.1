using UnityEngine;

public class AudioManager : MonoBehaviour
{
  public static AudioManager Instance;

  [Header("Fontes de Áudio")]
  [SerializeField] private AudioSource sfxSource;   // Efeitos rápidos (PlayOneShot)
  [SerializeField] private AudioSource musicSource; // Música de fundo (Loop)

  [Header("Clips do Player")]
  public AudioClip attackSound;
  public AudioClip hurtSound;
  public AudioClip specialSound;
  public AudioClip stepSound;

  [Header("Clips do Inimigo Comum")]
  public AudioClip enemyHitSound;
  public AudioClip enemyDieSound;

  [Header("Clips do Inimigo Área 2 (Opcional)")] // --- NOVO ---
  public AudioClip enemy2HitSound;
  public AudioClip enemy2DieSound;

  [Header("Clips do Boss (Rei das Sombras)")] // --- NOVO ---
  public AudioClip bossIntroSound;  // Transformação/Risada
  public AudioClip bossAttackSound; // Soco/Golpe físico
  public AudioClip bossCastSound;   // Invocando o fogo
  public AudioClip bossHitSound;    // Levando dano
  public AudioClip bossDieSound;    // Morrendo

  [Header("Clips de Ambiente/Poderes")] // --- NOVO ---
  public AudioClip fireBurnSound;   // Som de queimadura (quando o player pisa no fogo)
  public AudioClip trapClickSound;  // Som do gatilho da armadilha (opcional)

  void Awake()
  {
    if (Instance == null) Instance = this;
    else Destroy(gameObject);
  }

  public void PlaySFX(AudioClip clip)
  {
    if (clip != null)
    {
      // Varia levemente o pitch (tom) para o som não ficar robótico repetitivo
      sfxSource.pitch = Random.Range(0.9f, 1.1f);
      sfxSource.PlayOneShot(clip);
      sfxSource.pitch = 1f; // Reseta o pitch
    }
  }

  public void PlayMusic(AudioClip music)
  {
    if (musicSource.clip == music) return; // Não reinicia se for a mesma música

    musicSource.clip = music;
    musicSource.loop = true;
    musicSource.Play();
  }
}