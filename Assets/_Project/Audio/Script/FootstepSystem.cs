using UnityEngine;

public class FootstepSystem : MonoBehaviour
{
    [System.Serializable]
    public class TerrainSound
    {
        public int textureIndex;
        public AudioClip[] sounds;

        [HideInInspector]
        public int currentIndex;
    }

    [Header("References")]
    public AudioSource footstepSource;
    public Terrain terrain;
    public FirstPersonController controller;
    public PlayerSwimmingSystem swimmingSystem;

    [Header("Settings")]
    public float stepDelay = 0.5f;
    private float stepTimer;

    [Header("Terrain Sounds")]
    public TerrainSound[] terrainSounds;

    void Start()
    {
        if (swimmingSystem == null)
            swimmingSystem = GetComponent<PlayerSwimmingSystem>();
    }

    void FixedUpdate()
    {
        if (terrain == null || footstepSource == null)
            return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        bool isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

        bool inWater = swimmingSystem != null && swimmingSystem.IsInWater();

        // 🔥 ГЛАВНОЕ УСЛОВИЕ
        if (isMoving && !inWater)
        {
            stepTimer -= Time.fixedDeltaTime;

            if (stepTimer <= 0f)
            {
                AudioClip clip = GetTerrainFootstep();

                if (clip != null)
                {
                    footstepSource.pitch = Random.Range(0.95f, 1.05f);
                    footstepSource.PlayOneShot(clip, 0.6f);
                }

                stepTimer = (controller != null && controller.isSprinting)
                    ? stepDelay * 0.6f
                    : stepDelay;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    AudioClip GetTerrainFootstep()
    {
        int index = GetMainTextureIndex(transform.position);

        foreach (var t in terrainSounds)
        {
            if (t.textureIndex == index && t.sounds.Length > 0)
            {
                AudioClip clip = t.sounds[t.currentIndex];

                t.currentIndex++;
                if (t.currentIndex >= t.sounds.Length)
                    t.currentIndex = 0;

                return clip;
            }
        }

        return null;
    }

    int GetMainTextureIndex(Vector3 worldPos)
    {
        TerrainData data = terrain.terrainData;
        Vector3 pos = worldPos - terrain.transform.position;

        int x = (int)((pos.x / data.size.x) * data.alphamapWidth);
        int z = (int)((pos.z / data.size.z) * data.alphamapHeight);

        float[,,] map = data.GetAlphamaps(x, z, 1, 1);

        int index = 0;
        float max = 0;

        for (int i = 0; i < map.GetLength(2); i++)
        {
            if (map[0, 0, i] > max)
            {
                max = map[0, 0, i];
                index = i;
            }
        }

        return index;
    }
}