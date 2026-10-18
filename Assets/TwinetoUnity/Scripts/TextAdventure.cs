using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace SimpleTwineDialogue
{
    public class TextAdventure : MonoBehaviour
    {
        [Header("UI Components")]
        public TextMeshProUGUI passageText;
        public Button choiceButtonPrefab;
        public Transform choiceButtonContainer;

        int myChoices = 0;
        public TextMeshProUGUI myChoiceCounterUI;

        [Header("Inventory & Map Panel")]
        public InventorySystem inventorySystem;  // Assign your InventorySystem here
        public ItemData mapItem;                  // Assign the map ItemData here
        public GameObject mapPanel;               // Assign panel to show
        public float panelFadeDuration = 0.5f;

        [Header("File Loading")]
        public bool loadFromWeb;

        [Header("Load from Web")]
        public string webFileURL;

        [Header("Load from Local")]
        public string localFileName;

        private TweeParser tweeParser;
        private Dictionary<string, TweeParser.Passage> passages;
        private string currentPassageTitle;

        void Start()
        {
            tweeParser = new TweeParser();

            string filePath;

            if (loadFromWeb)
            {
                filePath = webFileURL;
            }
            else
            {
                // Use StreamingAssets path for local files
                filePath = Path.Combine(Application.streamingAssetsPath, localFileName);
            }

            StartCoroutine(LoadTweeFile(filePath));
        }

        IEnumerator LoadTweeFile(string filePath)
        {
            string path = filePath;

#if UNITY_ANDROID
            if (!loadFromWeb)
                path = filePath; // path is already streamingAssetsPath
#endif

            using var request = UnityWebRequest.Get(path);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Failed to load Twee file: " + request.error);
            }
            else
            {
                string text = request.downloadHandler.text;
                passages = tweeParser.ParseTweeFileFromText(text);
                CheckForStartPassage();
            }
        }

        public void DisplayPassage(string passageTitle)
        {
            if (!passages.TryGetValue(passageTitle, out var passage))
            {
                Debug.LogError("Passage not found: " + passageTitle);
                return;
            }

            ClearChoices();

            currentPassageTitle = passageTitle;
            passageText.text = passage.Body;

            foreach (var choice in passage.Choices)
            {
                var choiceText = choice.Substring(2, choice.IndexOf("]]") - 2);
                var parts = choiceText.Split('|');

                string buttonText = parts[0];
                string choiceDestination = parts.Length > 1 ? parts[1] : parts[0];

                var choiceButton = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                choiceButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
                choiceButton.onClick.AddListener(() => OnChoiceSelected(choiceDestination));
            }

            // If this passage is the map passage, add it to inventory
            if (passageTitle.ToLower().Contains("map"))
            {
                StartCoroutine(AddMapToInventory());
            }
        }

        void OnChoiceSelected(string choiceTitle)
        {
            DisplayPassage(choiceTitle);
            myChoices++;
            if (myChoiceCounterUI != null)
                myChoiceCounterUI.text = "Choices made: " + myChoices;
        }

        void CheckForStartPassage()
        {
            if (passages.ContainsKey("Start"))
            {
                DisplayPassage("Start");
            }
            else
            {
                foreach (var kvp in passages)
                {
                    Debug.LogWarning("Start passage not found, using first passage: " + kvp.Key);
                    DisplayPassage(kvp.Key);
                    break;
                }
            }
        }

        void ClearChoices()
        {
            foreach (Transform child in choiceButtonContainer)
            {
                Destroy(child.gameObject);
            }
        }

        IEnumerator AddMapToInventory()
        {
            if (inventorySystem == null)
            {
                Debug.LogWarning("InventorySystem not assigned! Cannot add map.");
                yield break;
            }

            if (mapItem == null)
            {
                Debug.LogWarning("Map ItemData not assigned!");
                yield break;
            }

            bool added = inventorySystem.AddItem(mapItem);
            if (!added)
            {
                Debug.LogWarning("Map could not be added to inventory!");
                yield break;
            }

            if (mapPanel != null)
            {
                CanvasGroup cg = mapPanel.GetComponent<CanvasGroup>();
                if (cg == null) cg = mapPanel.AddComponent<CanvasGroup>();
                cg.alpha = 0;
                mapPanel.SetActive(true);

                // Fade in
                float t = 0f;
                while (t < panelFadeDuration)
                {
                    t += Time.unscaledDeltaTime;
                    cg.alpha = Mathf.Lerp(0, 1, t / panelFadeDuration);
                    yield return null;
                }
                cg.alpha = 1;

                // Hold for 1.5 seconds
                yield return new WaitForSecondsRealtime(1.5f);

                // Fade out
                t = 0f;
                while (t < panelFadeDuration)
                {
                    t += Time.unscaledDeltaTime;
                    cg.alpha = Mathf.Lerp(1, 0, t / panelFadeDuration);
                    yield return null;
                }
                cg.alpha = 0;
                mapPanel.SetActive(false);
            }
        }
    }
}
