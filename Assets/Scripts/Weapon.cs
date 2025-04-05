using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    public static Weapon Instance { get; private set; }

    //shooting
    public bool isShooting, readyToShoot;
    bool allowReset = true;
    public float shootingDelay = 0.2f;

    //burst
    public int bulletPerBurst = 1;
    public int burstBulletsLeft;

    //spread
    public float spreadIntensity = 30f;

    //bullet
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 500f;
    public float bulletPrefabLifeTime = 2.5f; //seconds

    //loading
    public float reloadTime =1f;
    public int magazineSize=20;
    public int bulletsLeft;
    public bool isReloading;
    bool isADS;

    // Reload UI
    public Slider reloadSlider; // UI Slider for reload bar

    private Animator animator;
    public GameObject muzzleEffect;

    private Camera mainCamera;
    public float normalFOV = 60f;  // Default FOV
    public float adsFOV = 40f;     // Zoomed-in FOV
    public float adsSpeed = 10f;   // How fast FOV changes

    //for shoving
    public float shoveForce = 10f;
    public float shoveRange = 5f; // Max range of shove
    public float shoveRadius = 1f; // Detection radius
    public LayerMask shoveMask;

    public enum ShootingMode
    {
        Single,
        Burst,
        Auto
    }

    public ShootingMode currentShootingMode;

    PlayerMovement runSpeed; //for adjusting player speed when ADS
    public GameObject defaultReticle;
    public GameObject ADSReticle;
  

    private void Awake()
    {
        
        readyToShoot = true;
        burstBulletsLeft = bulletPerBurst;

        bulletsLeft = magazineSize;

        if (reloadSlider != null)
        {
            reloadSlider.value = 0;
        }

        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        runSpeed = FindFirstObjectByType<PlayerMovement>();
    }



    void Update()
    {
        if (currentShootingMode == ShootingMode.Auto)
        {
            //holding down left mouse button
            isShooting = Input.GetKey(KeyCode.Mouse0);
        }
        else if (currentShootingMode == ShootingMode.Single || currentShootingMode == ShootingMode.Burst)
        {
            //clicking left mouse button once
            isShooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        //ADS
        if(Input.GetMouseButtonDown(1))
        {
            if (runSpeed != null) runSpeed.speed = 6f; //adjusting player speed
            defaultReticle.SetActive(false);
            ADSReticle.SetActive(true);
            animator.SetTrigger("enterADS");
            isADS = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (runSpeed != null) runSpeed.speed = 12f;
            defaultReticle.SetActive(true);
            ADSReticle.SetActive(false);
            animator.SetTrigger("exitADS");
            isADS = false;
        }

        if (Input.GetMouseButtonDown(2))
        {
            animator.SetTrigger("shove");
            Shove();
        }

        float targetFOV = isADS ? adsFOV : normalFOV;
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * adsSpeed);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && isReloading == false)
        {
            Reload();
        }

        
        if (readyToShoot && isShooting && bulletsLeft > 0)
        {
            burstBulletsLeft = bulletPerBurst;
            FireWeapon();
        }

        if (AmmoManager.Instance.ammoDisplay != null)
        {
            AmmoManager.Instance.ammoDisplay.text = $"{bulletsLeft/bulletPerBurst}/{magazineSize/bulletPerBurst}";
        }
    }

    void Shove()
    {
        Vector3 shoveOrigin = transform.position + transform.forward * 1f; // Offset in front of player
        Vector3 shoveDirection = transform.forward; // Push forward direction

        Debug.Log("Shove Attempted");

        // Detect enemies in a small sphere around the shove area
        Collider[] hitColliders = Physics.OverlapSphere(shoveOrigin, shoveRadius, shoveMask);

        if (hitColliders.Length == 0)
        {
            Debug.Log("No enemies in range to shove");
            return;
        }

        foreach (Collider hit in hitColliders)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            Debug.Log($"Detected: {hit.gameObject.name} at distance {distance}");

            // Ensure it's within the valid shove range
            if (distance > shoveRange)
            {
                Debug.Log($"{hit.gameObject.name} is too far (Max: {shoveRange}m)");
                continue;
            }

            // Check if enemy is in front (dot product method)
            Vector3 toEnemy = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Dot(toEnemy, transform.forward);

            if (angle < 0.5f) // Adjust threshold if necessary
            {
                Debug.Log($"{hit.gameObject.name} is not directly in front.");
                continue;
            }
        }
    }

    private void FireWeapon()
    {
        muzzleEffect.GetComponent<ParticleSystem>().Play(); 
        SoundManager.Instance.shootingPistolSound.Play();


        if (isADS)
        {
            animator.SetTrigger("shootADSRecoil");
        }
        else
        {
            animator.SetTrigger("shootRecoil");
        }

            bulletsLeft--;

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;
        //instantiate the bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

        //pointing the bullte to face the shooting direction
        bullet.transform.forward = shootingDirection;

        //shoot the bullet
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward.normalized * bulletVelocity, ForceMode.Impulse);

        //destroy the bullet after some time
        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));

        //check if we are done shooting
        if(allowReset)
        {
            Invoke("ResetShot", shootingDelay);
            allowReset = false;
        }

        //burst mode
        if (currentShootingMode == ShootingMode.Burst && burstBulletsLeft > 1) // we already shot once before this check
        {
            burstBulletsLeft--;
            Invoke("FireWeapon", shootingDelay);
        }
    }

    private IEnumerator ReloadProgress()
    {
        
        CanvasGroup reloadSlidercanvas = reloadSlider.GetComponent<CanvasGroup>();
        reloadSlidercanvas.alpha = 1;
        float elapsedTime = 0f;
        while (elapsedTime < reloadTime)
        {
            elapsedTime += Time.deltaTime;
            if (reloadSlider != null)
            {
                reloadSlider.value = Mathf.Lerp(1, 0, elapsedTime / reloadTime);
            }
            yield return null;
        }

        if (reloadSlider != null)
        {
            reloadSlider.value = 0; // Ensure it's empty after reloading
            reloadSlidercanvas.alpha = 0; //make invisble again
        }
        readyToShoot = true;
    }

    private void Reload()
    {
        isReloading = true;
        SoundManager.Instance.reloadSound.Play();
        readyToShoot = false;

        if (reloadSlider != null)
        {
            reloadSlider.value = 1;
        }

        StartCoroutine(ReloadProgress());

        Invoke("ReloadCompleted", reloadTime);
    }

    private void ReloadCompleted()
    {
        int neededAmmo = magazineSize - bulletsLeft; // How much ammo is needed

        if (AmmoManager.Instance.totalAmmo >= neededAmmo)
        {
            bulletsLeft += neededAmmo;
            AmmoManager.Instance.UseAmmo(neededAmmo); // Deduct from total ammo
        }
        else
        {
            bulletsLeft += AmmoManager.Instance.totalAmmo;  // Take whatever is left
            AmmoManager.Instance.UseAmmo(AmmoManager.Instance.totalAmmo);  // Set to 0
        }

        isReloading = false;
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    private Vector3 CalculateDirectionAndSpread()
    {
      
        // Base shooting direction from the center of the screen
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // Adjust spread based on ADS state
        float currentSpread = isADS ? spreadIntensity * 0.2f : spreadIntensity;

        // Generate random spread offsets
        float spreadX = UnityEngine.Random.Range(-currentSpread, currentSpread);
        float spreadY = UnityEngine.Random.Range(-currentSpread, currentSpread);

        Vector3 spreadDirection = ray.direction + (Camera.main.transform.right * spreadX) + (Camera.main.transform.up * spreadY);

        return spreadDirection.normalized;
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }

    
}
