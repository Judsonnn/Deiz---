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

    [Header("Superaquecimento")]
    public float maxHeat = 100f;
    public float heatPerShot = 20f;

    // Tempo que a barra leva para ir do calor atual até 0
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

    // Controle do resfriamento
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

    private void HandleShoot()
    {
        if (isOverheated)
            return;

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown(0))
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

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.Init(
                facingRight ? 1f : -1f,
                bulletSpeed,
                damage
            );
        }

        // Aumenta o calor
        currentHeat += heatPerShot;

        currentHeat = Mathf.Clamp(currentHeat, 0f, maxHeat);

        // Começa um novo ciclo de resfriamento
        cooldownStartHeat = currentHeat;
        cooldownTimer = 0f;
        isCooling = true;

        // Chegou ao máximo
        if (currentHeat >= maxHeat)
        {
            currentHeat = maxHeat;
            TriggerOverheat();
        }
    }

    private void TriggerOverheat()
    {
        isOverheated = true;

        // Começa a piscar
        if (fireIcon != null)
        {
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);

            blinkCoroutine = StartCoroutine(BlinkFireIcon());
        }

        // Reinicia o resfriamento
        cooldownStartHeat = maxHeat;
        cooldownTimer = 0f;
        isCooling = true;
    }

    private IEnumerator BlinkFireIcon()
    {
        while (currentHeat > 0f)
        {
            if (fireIcon != null)
                fireIcon.SetActive(!fireIcon.activeSelf);

            yield return new WaitForSeconds(fireBlinkInterval);
        }

        // Quando terminar de esfriar
        if (fireIcon != null)
            fireIcon.SetActive(true);
    }

    private void HandleHeat()
    {
        if (!isCooling)
            return;

        // Avança o tempo do resfriamento
        cooldownTimer += Time.deltaTime;

        // Quanto do tempo total já passou
        float progress = cooldownTimer / overheatCooldown;

        progress = Mathf.Clamp01(progress);

        // Faz a barra ir do calor inicial até 0
        currentHeat = Mathf.Lerp(
            cooldownStartHeat,
            0f,
            progress
        );

        // Terminou de esfriar
        if (progress >= 1f)
        {
            currentHeat = 0f;
            isCooling = false;

            if (isOverheated)
            {
                isOverheated = false;
            }

            // Para o pisca-pisca
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
