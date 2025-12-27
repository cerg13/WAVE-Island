using UnityEngine;
using System.Collections;
using WaveIsland.Data;

namespace WaveIsland.Alchemy
{
    /// <summary>
    /// Visual and audio effects for crafting
    /// </summary>
    public class CraftingEffects : MonoBehaviour
    {
        [Header("Particle Systems")]
        [SerializeField] private ParticleSystem mixingParticles;
        [SerializeField] private ParticleSystem successParticles;
        [SerializeField] private ParticleSystem failParticles;
        [SerializeField] private ParticleSystem discoveryParticles;
        [SerializeField] private ParticleSystem ingredientAddParticles;

        [Header("Result Display")]
        [SerializeField] private Transform resultSpawnPoint;
        [SerializeField] private GameObject resultItemPrefab;
        [SerializeField] private float resultFloatHeight = 1f;
        [SerializeField] private float resultAnimDuration = 1f;

        [Header("Shake Effect")]
        [SerializeField] private Transform tableTransform;
        [SerializeField] private float shakeIntensity = 0.1f;
        [SerializeField] private float shakeDuration = 0.3f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip mixingLoop;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip failSound;
        [SerializeField] private AudioClip discoveryFanfare;
        [SerializeField] private AudioClip ingredientDropSound;

        private Vector3 originalTablePosition;
        private Coroutine mixingCoroutine;

        private void Awake()
        {
            if (tableTransform != null)
                originalTablePosition = tableTransform.localPosition;

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Play effect when ingredient is added
        /// </summary>
        public void PlayIngredientAdd(Vector3 position)
        {
            if (ingredientAddParticles != null)
            {
                ingredientAddParticles.transform.position = position;
                ingredientAddParticles.Play();
            }

            PlaySound(ingredientDropSound);
        }

        /// <summary>
        /// Start mixing animation and sound
        /// </summary>
        public void StartMixing()
        {
            if (mixingParticles != null)
                mixingParticles.Play();

            if (mixingLoop != null && audioSource != null)
            {
                audioSource.clip = mixingLoop;
                audioSource.loop = true;
                audioSource.Play();
            }

            mixingCoroutine = StartCoroutine(MixingAnimation());
        }

        /// <summary>
        /// Stop mixing and show result
        /// </summary>
        public void StopMixing(bool success, bool isDiscovery)
        {
            if (mixingParticles != null)
                mixingParticles.Stop();

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }

            if (mixingCoroutine != null)
            {
                StopCoroutine(mixingCoroutine);
                mixingCoroutine = null;
            }

            // Reset table position
            if (tableTransform != null)
                tableTransform.localPosition = originalTablePosition;

            // Play result effects
            if (success)
            {
                if (isDiscovery)
                {
                    PlayDiscoveryEffect();
                }
                else
                {
                    PlaySuccessEffect();
                }
            }
            else
            {
                PlayFailEffect();
            }
        }

        private void PlaySuccessEffect()
        {
            if (successParticles != null)
                successParticles.Play();

            PlaySound(successSound);
            StartCoroutine(ShakeEffect());
        }

        private void PlayFailEffect()
        {
            if (failParticles != null)
                failParticles.Play();

            PlaySound(failSound);
        }

        private void PlayDiscoveryEffect()
        {
            if (discoveryParticles != null)
                discoveryParticles.Play();

            if (successParticles != null)
                successParticles.Play();

            PlaySound(discoveryFanfare);
            StartCoroutine(ShakeEffect());
        }

        /// <summary>
        /// Show the result item floating up
        /// </summary>
        public void ShowResultItem(RecipeData recipe)
        {
            if (resultItemPrefab == null || resultSpawnPoint == null)
                return;

            StartCoroutine(ResultItemAnimation(recipe));
        }

        private IEnumerator ResultItemAnimation(RecipeData recipe)
        {
            GameObject item = Instantiate(resultItemPrefab, resultSpawnPoint.position, Quaternion.identity);

            // Set icon
            var spriteRenderer = item.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && recipe.Icon != null)
            {
                spriteRenderer.sprite = recipe.Icon;
            }

            var image = item.GetComponent<UnityEngine.UI.Image>();
            if (image != null && recipe.Icon != null)
            {
                image.sprite = recipe.Icon;
            }

            // Animate
            Vector3 startPos = resultSpawnPoint.position;
            Vector3 endPos = startPos + Vector3.up * resultFloatHeight;

            float elapsed = 0f;
            while (elapsed < resultAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / resultAnimDuration;

                // Ease out
                float easeT = 1f - Mathf.Pow(1f - t, 3f);

                item.transform.position = Vector3.Lerp(startPos, endPos, easeT);

                // Scale pop
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f;
                item.transform.localScale = Vector3.one * scale;

                // Fade at end
                if (t > 0.7f)
                {
                    float alpha = 1f - ((t - 0.7f) / 0.3f);
                    if (spriteRenderer != null)
                    {
                        var c = spriteRenderer.color;
                        c.a = alpha;
                        spriteRenderer.color = c;
                    }
                }

                yield return null;
            }

            Destroy(item);
        }

        private IEnumerator MixingAnimation()
        {
            while (true)
            {
                // Subtle shake while mixing
                if (tableTransform != null)
                {
                    Vector3 shake = new Vector3(
                        Random.Range(-1f, 1f) * shakeIntensity * 0.3f,
                        Random.Range(-1f, 1f) * shakeIntensity * 0.3f,
                        0
                    );
                    tableTransform.localPosition = originalTablePosition + shake;
                }

                yield return new WaitForSeconds(0.05f);
            }
        }

        private IEnumerator ShakeEffect()
        {
            if (tableTransform == null)
                yield break;

            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float intensity = shakeIntensity * (1f - elapsed / shakeDuration);

                Vector3 shake = new Vector3(
                    Random.Range(-1f, 1f) * intensity,
                    Random.Range(-1f, 1f) * intensity,
                    0
                );

                tableTransform.localPosition = originalTablePosition + shake;
                yield return null;
            }

            tableTransform.localPosition = originalTablePosition;
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
