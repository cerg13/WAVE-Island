using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using WaveIsland.Core;

namespace WaveIsland.Tutorial
{
    /// <summary>
    /// Manages the tutorial flow for new players
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        public event Action<TutorialStep> OnStepStarted;
        public event Action<TutorialStep> OnStepCompleted;
        public event Action OnTutorialCompleted;

        [Header("Tutorial Steps")]
        [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();

        [Header("Settings")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private float stepDelay = 0.5f;

        private int currentStepIndex = -1;
        private TutorialStep currentStep;
        private bool isTutorialActive = false;
        private bool isTutorialCompleted = false;

        private const string TUTORIAL_SAVE_KEY = "tutorial_progress";
        private const string TUTORIAL_COMPLETE_KEY = "tutorial_completed";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            LoadProgress();

            if (autoStart && !isTutorialCompleted)
            {
                StartCoroutine(DelayedStart());
            }
        }

        private IEnumerator DelayedStart()
        {
            // Wait for game to initialize
            yield return new WaitForSeconds(1f);
            StartTutorial();
        }

        /// <summary>
        /// Start the tutorial from the beginning or resume
        /// </summary>
        public void StartTutorial()
        {
            if (isTutorialCompleted)
            {
                Debug.Log("[Tutorial] Tutorial already completed");
                return;
            }

            isTutorialActive = true;

            if (currentStepIndex < 0)
            {
                currentStepIndex = 0;
            }

            if (currentStepIndex < tutorialSteps.Count)
            {
                StartStep(tutorialSteps[currentStepIndex]);
            }
            else
            {
                CompleteTutorial();
            }
        }

        /// <summary>
        /// Start a specific tutorial step
        /// </summary>
        private void StartStep(TutorialStep step)
        {
            currentStep = step;

            Debug.Log($"[Tutorial] Starting step: {step.StepId} - {step.Title}");
            OnStepStarted?.Invoke(step);

            // Auto-complete timed steps
            if (step.AutoCompleteDelay > 0)
            {
                StartCoroutine(AutoCompleteStep(step.AutoCompleteDelay));
            }
        }

        private IEnumerator AutoCompleteStep(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (currentStep != null && currentStep.AutoCompleteDelay > 0)
            {
                CompleteCurrentStep();
            }
        }

        /// <summary>
        /// Complete the current step and move to the next
        /// </summary>
        public void CompleteCurrentStep()
        {
            if (currentStep == null) return;

            Debug.Log($"[Tutorial] Completed step: {currentStep.StepId}");
            OnStepCompleted?.Invoke(currentStep);

            currentStepIndex++;
            SaveProgress();

            if (currentStepIndex < tutorialSteps.Count)
            {
                StartCoroutine(NextStepWithDelay());
            }
            else
            {
                CompleteTutorial();
            }
        }

        private IEnumerator NextStepWithDelay()
        {
            yield return new WaitForSeconds(stepDelay);

            if (currentStepIndex < tutorialSteps.Count)
            {
                StartStep(tutorialSteps[currentStepIndex]);
            }
        }

        /// <summary>
        /// Complete the entire tutorial
        /// </summary>
        private void CompleteTutorial()
        {
            isTutorialActive = false;
            isTutorialCompleted = true;
            currentStep = null;

            PlayerPrefs.SetInt(TUTORIAL_COMPLETE_KEY, 1);
            PlayerPrefs.Save();

            Debug.Log("[Tutorial] Tutorial completed!");
            OnTutorialCompleted?.Invoke();

            // Give completion reward
            GiveTutorialReward();
        }

        private void GiveTutorialReward()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData != null)
            {
                playerData.Coins += 100;
                playerData.AddSeeds("mint", 5);
                playerData.AddSeeds("lime", 3);
                SaveSystem.SavePlayerData(playerData);

                Debug.Log("[Tutorial] Tutorial completion rewards granted!");
            }
        }

        /// <summary>
        /// Skip the tutorial entirely
        /// </summary>
        public void SkipTutorial()
        {
            StopAllCoroutines();
            CompleteTutorial();
        }

        /// <summary>
        /// Check if a specific step's action has been completed
        /// </summary>
        public void NotifyAction(TutorialAction action, string context = "")
        {
            if (!isTutorialActive || currentStep == null) return;

            if (currentStep.RequiredAction == action)
            {
                // Check context if required
                if (!string.IsNullOrEmpty(currentStep.RequiredContext) &&
                    !string.IsNullOrEmpty(context) &&
                    currentStep.RequiredContext != context)
                {
                    return;
                }

                CompleteCurrentStep();
            }
        }

        /// <summary>
        /// Get the current tutorial step
        /// </summary>
        public TutorialStep GetCurrentStep() => currentStep;

        /// <summary>
        /// Check if tutorial is active
        /// </summary>
        public bool IsTutorialActive() => isTutorialActive;

        /// <summary>
        /// Check if tutorial has been completed
        /// </summary>
        public bool IsTutorialCompleted() => isTutorialCompleted;

        /// <summary>
        /// Get progress percentage
        /// </summary>
        public float GetProgress()
        {
            if (tutorialSteps.Count == 0) return 1f;
            return (float)currentStepIndex / tutorialSteps.Count;
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetInt(TUTORIAL_SAVE_KEY, currentStepIndex);
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            currentStepIndex = PlayerPrefs.GetInt(TUTORIAL_SAVE_KEY, 0);
            isTutorialCompleted = PlayerPrefs.GetInt(TUTORIAL_COMPLETE_KEY, 0) == 1;
        }

        /// <summary>
        /// Reset tutorial progress (for testing)
        /// </summary>
        public void ResetTutorial()
        {
            currentStepIndex = 0;
            isTutorialCompleted = false;
            currentStep = null;

            PlayerPrefs.DeleteKey(TUTORIAL_SAVE_KEY);
            PlayerPrefs.DeleteKey(TUTORIAL_COMPLETE_KEY);
            PlayerPrefs.Save();

            Debug.Log("[Tutorial] Tutorial progress reset");
        }

        /// <summary>
        /// Initialize default tutorial steps
        /// </summary>
        [ContextMenu("Initialize Default Steps")]
        public void InitializeDefaultSteps()
        {
            tutorialSteps = new List<TutorialStep>
            {
                // Welcome
                new TutorialStep
                {
                    StepId = "welcome",
                    Title = "Welcome to WAVE Island!",
                    Description = "Welcome to your tropical paradise! Let's learn how to grow ingredients and craft delicious drinks.",
                    HighlightTarget = TutorialHighlight.None,
                    RequiredAction = TutorialAction.TapToContinue,
                    AutoCompleteDelay = 0
                },

                // Garden intro
                new TutorialStep
                {
                    StepId = "garden_intro",
                    Title = "Your Garden",
                    Description = "This is your garden! Here you can grow fresh ingredients for your cocktails.",
                    HighlightTarget = TutorialHighlight.GardenButton,
                    RequiredAction = TutorialAction.TapToContinue,
                    AutoCompleteDelay = 0
                },

                // Plant seed
                new TutorialStep
                {
                    StepId = "plant_seed",
                    Title = "Plant a Seed",
                    Description = "Tap on an empty plot to plant a seed. Let's plant some mint!",
                    HighlightTarget = TutorialHighlight.GardenPlot,
                    RequiredAction = TutorialAction.PlantSeed,
                    RequiredContext = "mint",
                    AutoCompleteDelay = 0
                },

                // Water plant
                new TutorialStep
                {
                    StepId = "water_plant",
                    Title = "Water Your Plants",
                    Description = "Plants need water to grow faster. Tap the plant to water it!",
                    HighlightTarget = TutorialHighlight.GardenPlot,
                    RequiredAction = TutorialAction.WaterPlant,
                    AutoCompleteDelay = 0
                },

                // Wait for growth
                new TutorialStep
                {
                    StepId = "wait_growth",
                    Title = "Growing...",
                    Description = "Your plant is growing! In the full game, you'd wait for it to mature. For now, let's continue.",
                    HighlightTarget = TutorialHighlight.None,
                    RequiredAction = TutorialAction.TapToContinue,
                    AutoCompleteDelay = 3f
                },

                // Crafting intro
                new TutorialStep
                {
                    StepId = "crafting_intro",
                    Title = "Crafting Table",
                    Description = "Now let's learn about crafting! Open the Crafting Table.",
                    HighlightTarget = TutorialHighlight.CraftingButton,
                    RequiredAction = TutorialAction.OpenCrafting,
                    AutoCompleteDelay = 0
                },

                // Add ingredients
                new TutorialStep
                {
                    StepId = "add_ingredients",
                    Title = "Add Ingredients",
                    Description = "Drag ingredients into the crafting slots to mix them. Try making a simple drink!",
                    HighlightTarget = TutorialHighlight.CraftingSlot,
                    RequiredAction = TutorialAction.AddIngredient,
                    AutoCompleteDelay = 0
                },

                // Craft
                new TutorialStep
                {
                    StepId = "craft_item",
                    Title = "Mix It!",
                    Description = "Press the Mix button to craft your creation!",
                    HighlightTarget = TutorialHighlight.CraftButton,
                    RequiredAction = TutorialAction.Craft,
                    AutoCompleteDelay = 0
                },

                // Recipe book
                new TutorialStep
                {
                    StepId = "recipe_book",
                    Title = "Recipe Book",
                    Description = "Check the Recipe Book to see all available recipes and track your discoveries!",
                    HighlightTarget = TutorialHighlight.RecipeBookButton,
                    RequiredAction = TutorialAction.OpenRecipeBook,
                    AutoCompleteDelay = 0
                },

                // Shop
                new TutorialStep
                {
                    StepId = "shop_intro",
                    Title = "The Shop",
                    Description = "Visit the Shop to buy ingredients and seeds you can't grow yourself.",
                    HighlightTarget = TutorialHighlight.ShopButton,
                    RequiredAction = TutorialAction.OpenShop,
                    AutoCompleteDelay = 0
                },

                // Complete
                new TutorialStep
                {
                    StepId = "complete",
                    Title = "You're Ready!",
                    Description = "Great job! You've learned the basics. Now explore, discover new recipes, and build your tropical empire!",
                    HighlightTarget = TutorialHighlight.None,
                    RequiredAction = TutorialAction.TapToContinue,
                    AutoCompleteDelay = 0
                }
            };

            Debug.Log($"[Tutorial] Initialized {tutorialSteps.Count} default steps");
        }
    }

    [Serializable]
    public class TutorialStep
    {
        public string StepId;
        public string Title;
        [TextArea(2, 4)]
        public string Description;
        public TutorialHighlight HighlightTarget;
        public TutorialAction RequiredAction;
        public string RequiredContext;
        public float AutoCompleteDelay;
        public Sprite CustomImage;
        public Vector2 HighlightOffset;
        public Vector2 DialoguePosition;
    }

    public enum TutorialHighlight
    {
        None,
        GardenButton,
        GardenPlot,
        CraftingButton,
        CraftingSlot,
        CraftButton,
        RecipeBookButton,
        ShopButton,
        InventoryButton,
        SpiritButton,
        Custom
    }

    public enum TutorialAction
    {
        None,
        TapToContinue,
        PlantSeed,
        WaterPlant,
        HarvestPlant,
        OpenCrafting,
        AddIngredient,
        RemoveIngredient,
        Craft,
        OpenRecipeBook,
        OpenShop,
        BuyItem,
        OpenInventory,
        OpenSpirits
    }
}
