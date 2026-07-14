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
    public float cooldownRate = 30f;     // << esfria mais rápido
    public float overheatCooldown = 8f;  // << 8 segundos travada

    [Header("UI")]
    public Image heatBarFill;
    public Color normalColor = Color.cyan;
    public Color hotColor = Color.red;

    [Header("Fogo")]
    public GameObject fireIcon;          // imagem do fogo na ponta da barra
    public float fireBlinkInterval = 0.15f;

    private float currentHeat = 0f;
    private bool isOverheated = false;
    private float overheatTimer = 0f;
    private bool facingRight = true;
    private Coroutine blinkCoroutine;

    void Start()
    {
        if (fireIcon != null)
            fireIcon.SetActive(false);
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
        overheatTimer = overheatCooldown;

        // Ativa o fogo piscando
        if (fireIcon != null)
        {
            fireIcon.SetActive(true);
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkFireIcon());
        }
    }

    private IEnumerator BlinkFireIcon()
    {
        while (isOverheated)
        {
            if (fireIcon != null)
                fireIcon.SetActive(!fireIcon.activeSelf);
            yield return new WaitForSeconds(fireBlinkInterval);
        }

        // Quando terminar esconde o fogo
        if (fireIcon != null)
            fireIcon.SetActive(false);
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

        // Atualiza o fill
        heatBarFill.fillAmount = ratio;

        // Muda a cor gradualmente de azul para vermelho
        heatBarFill.color = Color.Lerp(normalColor, hotColor, ratio);
    }

    public void SetFacing(bool right)
    {
        facingRight = right;
    }
}