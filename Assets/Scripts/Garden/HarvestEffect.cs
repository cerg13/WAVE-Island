using UnityEngine;
using System.Collections;
using WaveIsland.Data;

namespace WaveIsland.Garden
{
    /// <summary>
    /// Visual effect for harvesting plants
    /// Shows floating items and particle effects
    /// </summary>
    public class HarvestEffect : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject floatingItemPrefab;
        [SerializeField] private ParticleSystem harvestParticles;
        [SerializeField] private ParticleSystem sparkleParticles;

        [Header("Animation Settings")]
        [SerializeField] private float floatHeight = 2f;
        [SerializeField] private float floatDuration = 1f;
        [SerializeField] private float itemSpacing = 0.3f;
        [SerializeField] private AnimationCurve floatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.5f, 1, 1);

        [Header("Audio")]
        [SerializeField] private AudioClip harvestSound;
        [SerializeField] private AudioClip bonusSeedSound;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        /// <summary>
        /// Play harvest effect at position
        /// </summary>
        public void PlayHarvestEffect(Vector3 position, IngredientData ingredient, int amount, bool bonusSeed)
        {
            transform.position = position;

            // Play particles
            if (harvestParticles != null)
            {
                harvestParticles.transform.position = position;
                harvestParticles.Play();
            }

            // Play sound
            if (harvestSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(harvestSound);
            }

            // Spawn floating items
            StartCoroutine(SpawnFloatingItems(position, ingredient, amount, bonusSeed));
        }

        private IEnumerator SpawnFloatingItems(Vector3 position, IngredientData ingredient, int amount, bool bonusSeed)
        {
            // Spawn ingredient items
            for (int i = 0; i < Mathf.Min(amount, 5); i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-itemSpacing, itemSpacing),
                    0,
                    Random.Range(-itemSpacing, itemSpacing)
                );

                SpawnFloatingItem(position + offset, ingredient.Icon, false);

                yield return new WaitForSeconds(0.1f);
            }

            // Spawn bonus seed
            if (bonusSeed)
            {
                yield return new WaitForSeconds(0.2f);

                if (sparkleParticles != null)
                {
                    sparkleParticles.transform.position = position;
                    sparkleParticles.Play();
                }

                if (bonusSeedSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(bonusSeedSound);
                }

                SpawnFloatingItem(position, null, true); // Seed icon
            }
        }

        private void SpawnFloatingItem(Vector3 position, Sprite icon, bool isSeed)
        {
            GameObject item;

            if (floatingItemPrefab != null)
            {
                item = Instantiate(floatingItemPrefab, position, Quaternion.identity, transform);
            }
            else
            {
                // Create simple placeholder
                item = new GameObject("FloatingItem");
                item.transform.position = position;

                var sr = item.AddComponent<SpriteRenderer>();
                sr.sprite = icon;
                sr.sortingOrder = 100;
            }

            // Set icon
            var spriteRenderer = item.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && icon != null)
            {
                spriteRenderer.sprite = icon;
            }

            // Color for seed
            if (isSeed && spriteRenderer != null)
            {
                spriteRenderer.color = Color.yellow;
            }

            // Animate
            StartCoroutine(AnimateFloatingItem(item, position));
        }

        private IEnumerator AnimateFloatingItem(GameObject item, Vector3 startPos)
        {
            float elapsed = 0f;
            Vector3 endPos = startPos + Vector3.up * floatHeight;

            while (elapsed < floatDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / floatDuration;

                // Position
                float curveT = floatCurve.Evaluate(t);
                item.transform.position = Vector3.Lerp(startPos, endPos, curveT);

                // Scale
                float scale = scaleCurve.Evaluate(t);
                item.transform.localScale = Vector3.one * scale;

                // Fade out near end
                var sr = item.GetComponent<SpriteRenderer>();
                if (sr != null && t > 0.7f)
                {
                    Color c = sr.color;
                    c.a = 1f - ((t - 0.7f) / 0.3f);
                    sr.color = c;
                }

                yield return null;
            }

            Destroy(item);
        }

        /// <summary>
        /// Play simple pop effect (for watering, etc.)
        /// </summary>
        public void PlayPopEffect(Vector3 position, Color color)
        {
            if (sparkleParticles != null)
            {
                var main = sparkleParticles.main;
                main.startColor = color;

                sparkleParticles.transform.position = position;
                sparkleParticles.Play();
            }
        }
    }
}
