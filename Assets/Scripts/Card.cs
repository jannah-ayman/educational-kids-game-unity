using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Image iconImage;

    public Sprite hiddenIconSprite;
    public Sprite iconSprite;

    public bool isSelected;
    public CardsController controller;

    public void OnCardClick()
    {
        controller.SetSelected(this);
    }

    public void SetIconSprite(Sprite sp)
    {
        iconSprite = sp;
    }

    public void Show()
    {
        StartCoroutine(FlipCard(iconSprite, true));
    }

    public void Hide()
    {
        StartCoroutine(FlipCard(hiddenIconSprite, false));
    }

    private IEnumerator FlipCard(Sprite newSprite, bool selecting)
    {
        // Rotate 0 → 90
        for (float t = 0; t < 0.2f; t += Time.deltaTime)
        {
            float angle = Mathf.Lerp(0f, 90f, t / 0.2f);
            transform.localEulerAngles = new Vector3(0f, angle, 0f);
            yield return null;
        }
        transform.localEulerAngles = new Vector3(0f, 90f, 0f);

        // Swap the sprite
        iconImage.sprite = newSprite;

        // Rotate 90 → 180 (or back to 0)
        float endAngle = selecting ? 180f : 0f;
        for (float t = 0; t < 0.2f; t += Time.deltaTime)
        {
            float angle = Mathf.Lerp(90f, endAngle, t / 0.2f);
            transform.localEulerAngles = new Vector3(0f, angle, 0f);
            yield return null;
        }
        transform.localEulerAngles = new Vector3(0f, endAngle, 0f);

        isSelected = selecting;
    }
}