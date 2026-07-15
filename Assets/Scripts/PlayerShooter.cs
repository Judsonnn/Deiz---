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
    public float cooldownRate = 5f;
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

    void Start()
    {
        if (fireIcon != null)
            fireIcon.SetActive(true); // sempre visível
    }

    void Update()
    {
        HandleHeat();
        HandleShoot();
        UpdateHeatBar();
    }

    private void HandleShoot()
    {
        if (isOverheated) return;

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown(0))
            Shoot();
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
            bulletScript.Init(facingRight ? 1f : -1f, bulletSpeed, damage);

        currentHeat += heatPerShot;

        if (currentHeat >= maxHeat)
        {
            currentHeat = maxHeat;
            TriggerOverheat();
        }
    }

    private void TriggerOverheat()
    {
        isOverheated = true;

        // Começa a piscar ao encher
        if (fireIcon != null)
        {
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkFireIcon());
        }
    }

    private IEnumerator BlinkFireIcon()
    {
        // Pisca enquanto a barra não esvaziar
        while (currentHeat > 0f)
        {
            if (fireIcon != null)
                fireIcon.SetActive(!fireIcon.activeSelf);
            yield return new WaitForSeconds(fireBlinkInterval);
        }

        // Para de piscar quando esvazia — volta visível normal
        if (fireIcon != null)
            fireIcon.SetActive(true);
    }

    private void HandleHeat()
    {
        if (isOverheated)
        {
            // Desce gradualmente durante o overheat
            currentHeat -= (maxHeat / overheatCooldown) * Time.deltaTime;

            if (currentHeat <= 0f)
            {
                currentHeat = 0f;
                isOverheated = false;
            }
        }
        else
        {
            currentHeat -= cooldownRate * Time.deltaTime;
            currentHeat = Mathf.Clamp(currentHeat, 0f, maxHeat);
        }
    }

    private void UpdateHeatBar()
    {
        if (heatBarFill == null) return;

        float ratio = currentHeat / maxHeat;
        heatBarFill.fillAmount = ratio;
        heatBarFill.color = Color.Lerp(normalColor, hotColor, ratio);
    }

    public void SetFacing(bool right)
    {
        facingRight = right;
    }
}