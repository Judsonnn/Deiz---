using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    [Header("Fontes de áudio")]
    [Tooltip("Se ficar vazio, usa o AudioSource do próprio inimigo (ou cria um).")]
    public AudioSource alertSource;
    [Tooltip("Se ficar vazio, usa o mesmo AudioSource do alerta.")]
    public AudioSource stepSource;

    [Header("Alerta (viu o player e começou a correr)")]
    public AudioClip alertSound;
    [Range(0f, 1f)] public float alertVolume = 1f;

    [Header("Passos")]
    public AudioClip[] footstepSounds;
    [Range(0f, 1f)] public float footstepVolume = 0.6f;
    [Tooltip("Tempo entre passos andando")]
    public float walkStepInterval = 0.5f;
    [Tooltip("Tempo entre passos correndo (perseguindo o player)")]
    public float runStepInterval = 0.28f;
    [Tooltip("Velocidade horizontal mínima para contar como andando")]
    public float minMoveSpeed = 0.1f;
    [Tooltip("Acima dessa velocidade vertical (pulo/queda) não toca passo")]
    public float maxVerticalSpeed = 1f;
    [Tooltip("Variação aleatória de tom para os passos não soarem repetidos")]
    public float pitchVariation = 0.1f;

    [Header("Dano")]
    [Tooltip("Pode ser um clip só ou vários (um é sorteado a cada dano)")]
    public AudioClip[] hurtSounds;
    [Range(0f, 1f)] public float hurtVolume = 1f;
    public float hurtPitchVariation = 0.1f;

    private bool isChasing = false;
    private float stepTimer = 0f;
    private Vector3 lastPosition;
    private int lastClipIndex = -1;

    void Awake()
    {
        if (alertSource == null)
        {
            alertSource = GetComponent<AudioSource>();
            if (alertSource == null)
                alertSource = gameObject.AddComponent<AudioSource>();
        }

        if (stepSource == null)
            stepSource = alertSource;

        alertSource.playOnAwake = false;
        stepSource.playOnAwake = false;
    }

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (Time.deltaTime <= 0f) return;

        Vector3 delta = transform.position - lastPosition;
        float speedX = Mathf.Abs(delta.x) / Time.deltaTime;
        float speedY = Mathf.Abs(delta.y) / Time.deltaTime;
        lastPosition = transform.position;

        bool isMoving = speedX > minMoveSpeed && speedY < maxVerticalSpeed;

        if (!isMoving)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer += Time.deltaTime;
        float interval = isChasing ? runStepInterval : walkStepInterval;

        if (stepTimer >= interval)
        {
            stepTimer = 0f;
            PlayFootstep();
        }
    }

    public void StartChase()
    {
        if (isChasing) return;

        isChasing = true;
        PlayAlert();
    }

    public void StopChase()
    {
        isChasing = false;
    }

    public void PlayAlert()
    {
        if (alertSound == null) return;

        alertSource.pitch = 1f;
        alertSource.PlayOneShot(alertSound, alertVolume);
    }

    public void PlayHurt(bool willDie = false)
    {
        if (hurtSounds == null || hurtSounds.Length == 0) return;

        AudioClip clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
        if (clip == null) return;

        if (willDie)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, hurtVolume);
            return;
        }

        alertSource.pitch = 1f + Random.Range(-hurtPitchVariation, hurtPitchVariation);
        alertSource.PlayOneShot(clip, hurtVolume);
    }

    private void PlayFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;

        int index = Random.Range(0, footstepSounds.Length);
        if (footstepSounds.Length > 1 && index == lastClipIndex)
            index = (index + 1) % footstepSounds.Length;
        lastClipIndex = index;

        stepSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        stepSource.PlayOneShot(footstepSounds[index], footstepVolume);
    }
}