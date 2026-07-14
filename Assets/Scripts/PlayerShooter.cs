using UnityEngine;
using UnityEngine.UI;

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
    public float cooldownRate = 15f;
    public float overheatCooldown = 2f;

    [Header("UI")]
    public Image heatBarFill;
    public Color normalColor = Color.cyan;
    public Color hotColor = Color.red;

    private float currentHeat = 0f;
    private bool isOverheated = false;
    private float overheatTimer = 0f;
    private bool facingRight = true;

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
        if (Input.GetKeyDown(KeyCode.L) || Input.GetMouseButtonDown(0))
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
    }

    private void HandleHeat()
    {
        if (isOverheated)
        {
            overheatTimer -= Time.deltaTime;

            if (overheatTimer <= 0f)
            {
                isOverheated = false;
                currentHeat = 0f;
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