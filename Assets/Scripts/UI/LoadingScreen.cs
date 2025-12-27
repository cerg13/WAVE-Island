using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace WaveIsland.UI
{
    /// <summary>
    /// Loading screen with progress bar and tips
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        public static LoadingScreen Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI tipText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image logoImage;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float minLoadingTime = 1f;
        [SerializeField] private float tipChangeInterval = 3f;
        [SerializeField] private GameObject loadingSpinner;

        [Header("Tips")]
        [SerializeField] private string[] loadingTips = new string[]
        {
            "Полив растений ускоряет их рост!",
            "Собирайте урожай вовремя, иначе он завянет.",
            "Открывайте новые рецепты, экспериментируя с ингредиентами.",
            "Духи-помощники дают бонусы к вашим действиям.",
            "Выполняйте ежедневные задания для дополнительных наград.",
            "Посещайте WAVE - получайте бонусы в игре!",
            "В магазине каждый день новые предложения.",
            "Идеальный урожай даёт больше ингредиентов.",
            "Соберите всех духов для бонуса коллекции!",
            "Достижения награждают монетами и опытом."
        };

        [Header("Backgrounds")]
        [SerializeField] private Sprite[] backgrounds;

        // State
        private bool isLoading = false;
        private int currentTipIndex = 0;
        private Coroutine tipCoroutine;
        private Action onLoadComplete;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }

        #region Public API

        /// <summary>
        /// Show loading screen
        /// </summary>
        public void Show(string status = "Загрузка...")
        {
            if (isLoading) return;

            isLoading = true;
            loadingPanel.SetActive(true);

            // Set initial state
            if (progressBar != null)
            {
                progressBar.value = 0;
            }

            if (statusText != null)
            {
                statusText.text = status;
            }

            SetProgress(0);
            ShowRandomTip();
            SetRandomBackground();

            // Start tip rotation
            tipCoroutine = StartCoroutine(TipRotationCoroutine());

            // Fade in
            StartCoroutine(FadeIn());

            // Enable spinner
            if (loadingSpinner != null)
            {
                loadingSpinner.SetActive(true);
            }
        }

        /// <summary>
        /// Hide loading screen
        /// </summary>
        public void Hide()
        {
            if (!isLoading) return;

            if (tipCoroutine != null)
            {
                StopCoroutine(tipCoroutine);
                tipCoroutine = null;
            }

            StartCoroutine(HideCoroutine());
        }

        /// <summary>
        /// Set loading progress (0-1)
        /// </summary>
        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (progressBar != null)
            {
                progressBar.value = progress;
            }

            if (progressText != null)
            {
                progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
        }

        /// <summary>
        /// Set status text
        /// </summary>
        public void SetStatus(string status)
        {
            if (statusText != null)
            {
                statusText.text = status;
            }
        }

        /// <summary>
        /// Load scene with loading screen
        /// </summary>
        public void LoadScene(string sceneName, Action onComplete = null)
        {
            onLoadComplete = onComplete;
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        /// <summary>
        /// Load scene by index
        /// </summary>
        public void LoadScene(int sceneIndex, Action onComplete = null)
        {
            onLoadComplete = onComplete;
            StartCoroutine(LoadSceneCoroutine(sceneIndex));
        }

        /// <summary>
        /// Execute async operation with loading screen
        /// </summary>
        public void ExecuteWithLoading(IEnumerator operation, string status = "Загрузка...")
        {
            StartCoroutine(ExecuteWithLoadingCoroutine(operation, status));
        }

        #endregion

        #region Coroutines

        private IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;

            canvasGroup.alpha = 0;
            float timer = 0;

            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                canvasGroup.alpha = timer / fadeDuration;
                yield return null;
            }

            canvasGroup.alpha = 1;
        }

        private IEnumerator HideCoroutine()
        {
            // Ensure minimum loading time
            yield return new WaitForSecondsRealtime(0.5f);

            // Fade out
            if (canvasGroup != null)
            {
                float timer = 0;
                while (timer < fadeDuration)
                {
                    timer += Time.unscaledDeltaTime;
                    canvasGroup.alpha = 1 - (timer / fadeDuration);
                    yield return null;
                }
                canvasGroup.alpha = 0;
            }

            loadingPanel.SetActive(false);
            isLoading = false;

            if (loadingSpinner != null)
            {
                loadingSpinner.SetActive(false);
            }

            onLoadComplete?.Invoke();
            onLoadComplete = null;
        }

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            Show("Загрузка сцены...");

            float startTime = Time.unscaledTime;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                SetProgress(progress);

                if (operation.progress >= 0.9f)
                {
                    // Ensure minimum loading time
                    float elapsed = Time.unscaledTime - startTime;
                    if (elapsed >= minLoadingTime)
                    {
                        SetProgress(1f);
                        SetStatus("Готово!");
                        yield return new WaitForSecondsRealtime(0.3f);
                        operation.allowSceneActivation = true;
                    }
                }

                yield return null;
            }

            Hide();
        }

        private IEnumerator LoadSceneCoroutine(int sceneIndex)
        {
            Show("Загрузка сцены...");

            float startTime = Time.unscaledTime;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                SetProgress(progress);

                if (operation.progress >= 0.9f)
                {
                    float elapsed = Time.unscaledTime - startTime;
                    if (elapsed >= minLoadingTime)
                    {
                        SetProgress(1f);
                        SetStatus("Готово!");
                        yield return new WaitForSecondsRealtime(0.3f);
                        operation.allowSceneActivation = true;
                    }
                }

                yield return null;
            }

            Hide();
        }

        private IEnumerator ExecuteWithLoadingCoroutine(IEnumerator operation, string status)
        {
            Show(status);

            float startTime = Time.unscaledTime;

            yield return operation;

            // Ensure minimum loading time
            float elapsed = Time.unscaledTime - startTime;
            if (elapsed < minLoadingTime)
            {
                yield return new WaitForSecondsRealtime(minLoadingTime - elapsed);
            }

            SetProgress(1f);
            Hide();
        }

        private IEnumerator TipRotationCoroutine()
        {
            while (isLoading)
            {
                yield return new WaitForSecondsRealtime(tipChangeInterval);
                ShowNextTip();
            }
        }

        #endregion

        #region Tips

        private void ShowRandomTip()
        {
            if (loadingTips == null || loadingTips.Length == 0) return;

            currentTipIndex = UnityEngine.Random.Range(0, loadingTips.Length);
            UpdateTipText();
        }

        private void ShowNextTip()
        {
            if (loadingTips == null || loadingTips.Length == 0) return;

            currentTipIndex = (currentTipIndex + 1) % loadingTips.Length;
            StartCoroutine(FadeTip());
        }

        private void UpdateTipText()
        {
            if (tipText != null && loadingTips != null && loadingTips.Length > 0)
            {
                tipText.text = loadingTips[currentTipIndex];
            }
        }

        private IEnumerator FadeTip()
        {
            if (tipText == null) yield break;

            // Fade out
            float timer = 0;
            Color startColor = tipText.color;

            while (timer < 0.25f)
            {
                timer += Time.unscaledDeltaTime;
                tipText.color = new Color(startColor.r, startColor.g, startColor.b, 1 - timer / 0.25f);
                yield return null;
            }

            UpdateTipText();

            // Fade in
            timer = 0;
            while (timer < 0.25f)
            {
                timer += Time.unscaledDeltaTime;
                tipText.color = new Color(startColor.r, startColor.g, startColor.b, timer / 0.25f);
                yield return null;
            }

            tipText.color = startColor;
        }

        #endregion

        #region Background

        private void SetRandomBackground()
        {
            if (backgroundImage == null || backgrounds == null || backgrounds.Length == 0)
                return;

            backgroundImage.sprite = backgrounds[UnityEngine.Random.Range(0, backgrounds.Length)];
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Static helper to load scene
        /// </summary>
        public static void Load(string sceneName)
        {
            if (Instance != null)
            {
                Instance.LoadScene(sceneName);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        #endregion
    }
}
