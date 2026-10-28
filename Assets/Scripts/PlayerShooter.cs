using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerShooter : MonoBehaviour
{
    [Header("Tiro")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 15f;
    public int damage = 1;

    [Header("Teclas de Tiro")]
    public KeyCode shootKey1 = KeyCode.Z;
    public KeyCode shootKey2 = KeyCode.L;

    [Header("Áudio")]
    public AudioSource shootAudioSource;
    public AudioClip shootSound;

    [Header("Áudio de Superaquecimento")]
    [Tooltip("AudioSource separado, só para o som do superaquecimento (Play On Awake desligado).")]
    public AudioSource overheatAudioSource;
    [Tooltip("Toca em loop desde que superaquece até esfriar por completo.")]
    public AudioClip overheatLoopSound;
    [Range(0f, 1f)]
    public float overheatVolume = 0.6f;

    [Header("Superaquecimento")]
    public float maxHeat = 100f;
    public float heatPerShot = 20f;
    public float overheatCooldown = 8f;

    [Header("UI")]
    public Image heatBarFill;
    public Color normalColor = Color.cyan;
    public Color hotColor = Color.red;

    [Header("Fogo")]
    public GameObject fireIcon;
    public float fireBlinkInterval = 0.15f;

    private float currentHeat = 0f;

    private bool isOverheated = false;
    private bool facingRight = true;

    private Coroutine blinkCoroutine;

    private float cooldownTimer = 0f;
    private float cooldownStartHeat = 0f;
    private bool isCooling = false;

    void Start()
    {
        if (fireIcon != null)
            fireIcon.SetActive(true);
    }

    void Update()
    {
        HandleHeat();
        HandleShoot();
        UpdateHeatBar();
    }

    void OnDisable()
    {
        StopOverheatSound();
    }

    private void HandleShoot()
    {
        if (isOverheated)
            return;

        if (Input.GetKeyDown(shootKey1) || Input.GetKeyDown(shootKey2))
            Shoot();
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.identity
        );

        if (shootAudioSource != null && shootSound != null)
        {
            shootAudioSource.PlayOneShot(shootSound);
        }

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.Init(
                facingRight ? 1f : -1f,
                bulletSpeed,
                damage
            );
        }

        currentHeat += heatPerShot;

        currentHeat = Mathf.Clamp(
            currentHeat,
            0f,
            maxHeat
        );

        cooldownStartHeat = currentHeat;
        cooldownTimer = 0f;
        isCooling = true;

        if (currentHeat >= maxHeat)
        {
            currentHeat = maxHeat;
            TriggerOverheat();
        }
    }

    private void TriggerOverheat()
    {
        isOverheated = true;

        StartOverheatSound();

        if (fireIcon != null)
        {
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);

            blinkCoroutine = StartCoroutine(BlinkFireIcon());
        }

        cooldownStartHeat = maxHeat;
        cooldownTimer = 0f;
        isCooling = true;
    }

    // ── ÁUDIO DE SUPERAQUECIMENTO ───────────────────────────────────────────
    private void StartOverheatSound()
    {
        if (overheatAudioSource == null || overheatLoopSound == null)
            return;

        overheatAudioSource.clip = overheatLoopSound;
        overheatAudioSource.loop = true;
        overheatAudioSource.volume = overheatVolume;
        overheatAudioSource.Play();
    }

    private void StopOverheatSound()
    {
        if (overheatAudioSource == null)
            return;

        if (overheatAudioSource.isPlaying)
            overheatAudioSource.Stop();
    }
    // ────────────────────────────────────────────────────────────────────────

    private IEnumerator BlinkFireIcon()
    {
        while (currentHeat > 0f)
        {
            if (fireIcon != null)
                fireIcon.SetActive(!fireIcon.activeSelf);

            yield return new WaitForSeconds(fireBlinkInterval);
        }

        if (fireIcon != null)
            fireIcon.SetActive(true);
    }

    private void HandleHeat()
    {
        if (!isCooling)
            return;

        cooldownTimer += Time.deltaTime;

        float progress = cooldownTimer / overheatCooldown;

        progress = Mathf.Clamp01(progress);

        currentHeat = Mathf.Lerp(
            cooldownStartHeat,
            0f,
            progress
        );

        if (progress >= 1f)
        {
            currentHeat = 0f;
            isCooling = false;

            if (isOverheated)
            {
                isOverheated = false;
                StopOverheatSound();
            }

            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }

            if (fireIcon != null)
                fireIcon.SetActive(true);
        }
    }

    private void UpdateHeatBar()
    {
        if (heatBarFill == null)
            return;

        float ratio = currentHeat / maxHeat;

        heatBarFill.fillAmount = ratio;

        heatBarFill.color = Color.Lerp(
            normalColor,
            hotColor,
            ratio
        );
    }

    public void SetFacing(bool right)
    {
        facingRight = right;
    }
}