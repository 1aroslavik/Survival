using UnityEngine;

public class AxeHit : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip treeHitSound;
    [Range(0f, 1f)] public float volume = 0.8f;
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    [Header("Damage")]
    public float animalDamage = 25f;

    [Header("References")]
    public TreeReplacer terrainChopper;
    public Camera playerCamera;

    [Header("Hit Settings")]
    public float hitDistance = 4f;
    public float sphereRadius = 0.5f;

    [Header("VFX")]
    public GameObject hitVFX;
    public float fallbackDestroyTime = 2f;

    bool hasHit = false;

    void Start()
    {
        if (terrainChopper == null)
            terrainChopper = FindObjectOfType<TreeReplacer>();

        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            hasHit = false;
            Invoke(nameof(DoHit), 0.1f);
        }
    }

    // 🔊 безопасное воспроизведение звука (НЕ двигает руки)
    void PlayTreeHitSound(Vector3 position)
    {
        if (treeHitSound == null) return;

        GameObject temp = new GameObject("TempAudio");
        temp.transform.position = position;

        AudioSource source = temp.AddComponent<AudioSource>();
        source.clip = treeHitSound;
        source.volume = volume;
        source.spatialBlend = 1f;
        source.minDistance = 2f;
        source.maxDistance = 15f;

        // лёгкая вариация
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);

        source.Play();

        Destroy(temp, treeHitSound.length / source.pitch);
    }

    void DoHit()
    {
        if (hasHit) return;
        hasHit = true;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        Debug.DrawRay(ray.origin, ray.direction * hitDistance, Color.red, 2f);

        RaycastHit hit;

        bool hasRayHit = Physics.Raycast(ray, out hit, hitDistance);

        if (!hasRayHit)
        {
            if (!Physics.SphereCast(ray, sphereRadius, out hit, hitDistance))
            {
                Debug.Log("[AxeHit] Nothing hit");
                return;
            }
        }

        Debug.Log($"[AxeHit] Hit: {hit.collider.name}");

        Vector3 hitPoint = hit.point;

        // 🔥 VFX
        SpawnVFX(hitPoint, hit.normal);

        // ❌ Игнор игрока
        if (hit.collider.GetComponentInParent<FirstPersonController>() != null)
            return;

        // 🐇 Животное
        AnimalHealth animal = hit.collider.GetComponentInParent<AnimalHealth>();
        if (animal != null)
        {
            animal.TakeDamage(animalDamage);
            return;
        }

        // 👹 Враг
        EnemyBaseAI enemy = hit.collider.GetComponentInParent<EnemyBaseAI>();
        if (enemy != null)
        {
            enemy.TakeDamage(animalDamage);
            return;
        }

        // 🌲 Дерево
        TreeHealth tree = hit.collider.GetComponentInParent<TreeHealth>();

        if (tree == null && hit.rigidbody != null)
            tree = hit.rigidbody.GetComponentInParent<TreeHealth>();

        if (tree != null)
        {
            tree.Hit(hitPoint);
            PlayTreeHitSound(hitPoint);
            return;
        }

        // 🌳 Terrain дерево
        if (terrainChopper != null)
        {
            GameObject obj = terrainChopper.TryChopClosestTree(hitPoint);

            if (obj != null)
            {
                TreeHealth h = obj.GetComponent<TreeHealth>();

                if (h == null)
                    h = obj.GetComponentInChildren<TreeHealth>();

                if (h != null)
                {
                    h.Hit(hitPoint);
                    PlayTreeHitSound(hitPoint);
                    return;
                }
            }
        }

        // 🔥 fallback
        Collider[] cols = Physics.OverlapSphere(hitPoint, 1.5f);

        foreach (var c in cols)
        {
            TreeHealth t = c.GetComponentInParent<TreeHealth>();
            if (t != null)
            {
                t.Hit(hitPoint);
                PlayTreeHitSound(hitPoint);
                return;
            }
        }
    }

    void SpawnVFX(Vector3 position, Vector3 normal)
    {
        if (hitVFX == null) return;

        Quaternion rot = Quaternion.LookRotation(normal);
        GameObject vfx = Instantiate(hitVFX, position, rot);

        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            var main = ps.main;

            float duration = main.duration;
            float lifetime = main.startLifetime.mode == ParticleSystemCurveMode.Constant
                ? main.startLifetime.constant
                : main.startLifetime.constantMax;

            Destroy(vfx, duration + lifetime);
        }
        else
        {
            Destroy(vfx, fallbackDestroyTime);
        }
    }
}