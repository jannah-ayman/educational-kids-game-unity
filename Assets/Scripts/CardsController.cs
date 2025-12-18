using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardsController : MonoBehaviour
{
    [SerializeField] Card cardPrefab;
    [SerializeField] Transform gridTransform;
    [SerializeField] Sprite[] sprites;
    [SerializeField] TextMeshProUGUI wrongMatchesText; // Add this in Inspector
    [SerializeField] GameObject winPopup; // Add this in Inspector
    [SerializeField] TextMeshProUGUI finalScoreText; // Text inside popup to show final score

    private List<Sprite> spritePairs;
    Card firstSelected;
    Card secondSelected;
    int matchCounts;
    int wrongMatches = 0; // Counter for wrong matches
    private Animator characterAnimator;
    private bool canPlay = false;

    private void Start()
    {
        PrepareSprites();
        CreateCards();
        UpdateWrongMatchesUI();

        // Hide win popup at start
        /*if (winPopup != null)
            winPopup.SetActive(false);*/

        characterAnimator = FindObjectOfType<Animator>();

        if (characterAnimator == null)
        {
            Debug.LogWarning("⚠️ No CharacterAnimator found in scene!");
        }

        // Show all cards at start
        StartCoroutine(ShowAllCardsAtStart());
    }

    private void PrepareSprites()
    {
        spritePairs = new List<Sprite>();
        for (int i = 0; i < sprites.Length; i++)
        {
            // adding sprite 2 times to make it pair
            spritePairs.Add(sprites[i]);
            spritePairs.Add(sprites[i]);
        }
        ShuffleSprites(spritePairs);
    }

    void CreateCards()
    {
        ShuffleSprites(spritePairs); // Ensure sprites are shuffled before creating cards
        for (int i = 0; i < spritePairs.Count; i++)
        {
            // Instantiate the card prefab as a child of the gridTransform
            Card card = Instantiate(cardPrefab, gridTransform);
            // Set the sprite icon for the created card using a sprite from the shuffled list
            card.SetIconSprite(spritePairs[i]);
            card.controller = this;
        }
    }

    IEnumerator ShowAllCardsAtStart()
    {
        canPlay = false;

        // Show all cards
        foreach (Transform child in gridTransform)
        {
            Card card = child.GetComponent<Card>();
            if (card != null)
            {
                card.iconImage.sprite = card.iconSprite;
            }
        }

        // Wait 1 second
        yield return new WaitForSeconds(1f);

        // Hide all cards
        foreach (Transform child in gridTransform)
        {
            Card card = child.GetComponent<Card>();
            if (card != null)
            {
                card.iconImage.sprite = card.hiddenIconSprite;
            }
        }

        canPlay = true;
    }

    public void SetSelected(Card card)
    {
        if (!canPlay) return;

        if (card.isSelected == false)
        {
            card.Show();
            if (firstSelected == null)
            {
                firstSelected = card;
                return;
            }
            if (secondSelected == null)
            {
                secondSelected = card;
                StartCoroutine(CheckMatching(firstSelected, secondSelected));
                firstSelected = null;
                secondSelected = null;
            }
        }
    }


    IEnumerator CheckMatching(Card a, Card b)
    {
        yield return new WaitForSeconds(0.3f);

        if (a.iconSprite == b.iconSprite)
        {
            // Matched 
            matchCounts++;

            // Happy animation & sound
            if (characterAnimator != null)
                characterAnimator.SetTrigger("Happy");

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCorrect();

            if (matchCounts >= spritePairs.Count / 2)
            {
                StartCoroutine(CelebrationAnimation());
                yield return new WaitForSeconds(0.5f);
                ShowResults();
            }
        }
        else
        {
            // Wrong match 
            wrongMatches++;

            // Sad animation & sound
            if (characterAnimator != null)
                characterAnimator.SetTrigger("Sad");

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayWrong();

            // flip them back 
            a.Hide();
            b.Hide();
        }

        firstSelected = null;
        secondSelected = null;
    }

    void UpdateWrongMatchesUI()
    {
        if (wrongMatchesText != null)
        {
            wrongMatchesText.text = "Wrong Matches: " + wrongMatches;
        }
    }

    void ShowResults()
    {
        int starsEarned = 0;
        if (wrongMatches < 5) starsEarned = 5;
        else if (wrongMatches > 5 && wrongMatches < 10) starsEarned = 4;
        else starsEarned = 3;

        // Create message for popup
        string message = $"Complete!";

        // Show star popup
        if (StarPopupManager.Instance != null)
        {
            StarPopupManager.Instance.ShowStars(starsEarned, message);
        }
        else
        {
            Debug.LogError("❌ StarPopupManager not found!");
        }

    }

    public void RestartGame()
    {
        // Reset counters
        wrongMatches = 0;
        matchCounts = 0;
        firstSelected = null;
        secondSelected = null;

        // Hide popup
        StarPopupManager.Instance.PlayAgain();

        // Clear existing cards
        foreach (Transform child in gridTransform)
        {
            Destroy(child.gameObject);
        }

        // Restart game
        PrepareSprites();
        CreateCards();
        UpdateWrongMatchesUI();

        // Show cards again at restart
        StartCoroutine(ShowAllCardsAtStart());
    }
    /*
    public void RestartGame()
    {
        StarPopupManager.Instance.PlayAgain();
        PrepareSprites();
        CreateCards();

        // Show cards again at restart
        StartCoroutine(ShowAllCardsAtStart());
    }*/

    // Button to go back to main menu
    public void BackToMenu()
    {
        StarPopupManager.Instance.GoToMainMenu();
    }

    IEnumerator AnimateWinPopup()
    {
        Transform popupTransform = winPopup.transform;
        popupTransform.localScale = Vector3.zero;

        float duration = 0.5f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Bounce effect
            float overshoot = 1.70158f;
            t = t - 1;
            float easeValue = t * t * ((overshoot + 1) * t + overshoot) + 1;
            popupTransform.localScale = Vector3.one * easeValue;
            yield return null;
        }

        popupTransform.localScale = Vector3.one;
    }

    IEnumerator CelebrationAnimation()
    {
        // Scale up
        float duration = 0.2f;
        float elapsed = 0;
        Vector3 startScale = gridTransform.localScale;
        Vector3 targetScale = Vector3.one * 1.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // OutBack ease simulation
            float overshoot = 1.70158f;
            t = t - 1;
            float easeValue = t * t * ((overshoot + 1) * t + overshoot) + 1;
            gridTransform.localScale = Vector3.Lerp(startScale, targetScale, easeValue);
            yield return null;
        }

        // Scale back to normal
        duration = 0.1f;
        elapsed = 0;
        startScale = gridTransform.localScale;
        targetScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            gridTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        gridTransform.localScale = Vector3.one;
    }

    // Method to shuffle a list of sprites
    void ShuffleSprites(List<Sprite> spriteList)
    {
        for (int i = spriteList.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            // Swap the elements at i and randomIndex
            Sprite temp = spriteList[i];
            spriteList[i] = spriteList[randomIndex];
            spriteList[randomIndex] = temp;
        }
    }
}