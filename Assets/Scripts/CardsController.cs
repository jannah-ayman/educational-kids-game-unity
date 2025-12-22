using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsController : MonoBehaviour
{
    [Header("Card Setup")]
    [SerializeField] Card cardPrefab;
    [SerializeField] Transform gridTransform;
    [SerializeField] Sprite[] sprites;

    private List<Sprite> spritePairs;

    private Card firstSelected;
    private Card secondSelected;

    private int matchCounts;
    private int wrongFlips;
    private bool canSelect = false;

    private Animator characterAnimator;

    void Start()
    {
        StartCoroutine(AssignAnimatorNextFrame());
        PrepareSprites();
        CreateCards();
    }

    IEnumerator AssignAnimatorNextFrame()
    {
        yield return null;
        characterAnimator = FindActiveCharacterAnimator();

        if (characterAnimator == null)
            Debug.LogError("❌ No active character Animator found!");
    }

    Animator FindActiveCharacterAnimator()
    {
        Animator[] allAnimators = FindObjectsOfType<Animator>();
        foreach (Animator anim in allAnimators)
        {
            if (anim.gameObject.activeInHierarchy)
                return anim;
        }
        return null;
    }

    void PrepareSprites()
    {
        spritePairs = new List<Sprite>();

        foreach (Sprite s in sprites)
        {
            spritePairs.Add(s);
            spritePairs.Add(s);
        }

        ShuffleSprites(spritePairs);
    }

    void CreateCards()
    {
        ShuffleSprites(spritePairs);

        for (int i = 0; i < spritePairs.Count; i++)
        {
            Card card = Instantiate(cardPrefab, gridTransform);
            card.SetIconSprite(spritePairs[i]);

            // Show face-up instantly
            card.iconImage.sprite = spritePairs[i];
            card.isSelected = true;

            card.controller = this;
        }

        StartCoroutine(PreviewCards());
    }

    IEnumerator PreviewCards()
    {
        canSelect = false;

        yield return new WaitForSeconds(0.4f);

        // Flip all cards back down
        foreach (Transform child in gridTransform)
        {
            Card card = child.GetComponent<Card>();
            if (card != null)
                card.Hide();
        }

        // Small delay to finish flip animations
        yield return new WaitForSeconds(0.4f);

        canSelect = true;
    }

    public void SetSelected(Card card)
    {
        if (!canSelect) return;
        if (card.isSelected) return;

        card.Show();

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.PlayCardFlip();

        if (firstSelected == null)
        {
            firstSelected = card;
            return;
        }

        secondSelected = card;
        StartCoroutine(CheckMatching(firstSelected, secondSelected));

        firstSelected = null;
        secondSelected = null;
    }


    IEnumerator CheckMatching(Card a, Card b)
    {
        yield return new WaitForSeconds(0.4f);

        if (a.iconSprite == b.iconSprite)
        {
            matchCounts++;

            if (characterAnimator != null)
                characterAnimator.SetTrigger("Happy");

            if (SettingsManager.Instance != null)
                SettingsManager.Instance.PlayCorrect();

            if (matchCounts >= spritePairs.Count / 2)
                ShowResults();
        }
        else
        {
            wrongFlips++;

            if (characterAnimator != null)
                characterAnimator.SetTrigger("Sad");

            if (SettingsManager.Instance != null)
                SettingsManager.Instance.PlayWrong();

            a.Hide();
            b.Hide();
        }
    }

    void ShowResults()
    {
        int starsEarned = CalculateStars();

        string message = $"Wrong flips: {wrongFlips}";

        if (StarPopupManager.Instance != null)
            StarPopupManager.Instance.ShowStars(starsEarned, message);
        else
            Debug.LogError("❌ StarPopupManager not found!");
    }

    int CalculateStars()
    {
        if (wrongFlips >= 10)
            return 3;

        if (wrongFlips >= 5)
            return 4;

        return 5; // perfect or near-perfect
    }

    void ShuffleSprites(List<Sprite> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public void PlayAgain()
    {
        // Reset state
        matchCounts = 0;
        wrongFlips = 0;
        firstSelected = null;
        secondSelected = null;

        // Clear old cards
        foreach (Transform child in gridTransform)
            Destroy(child.gameObject);

        // Reset stars popup & recreate game
        if (StarPopupManager.Instance != null)
            StarPopupManager.Instance.PlayAgain();

        PrepareSprites();
        CreateCards();
    }

    public void BackToMenu()
    {
        if (StarPopupManager.Instance != null)
            StarPopupManager.Instance.GoToMainMenu();
    }

}
