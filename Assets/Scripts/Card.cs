using System.Collections;
using System.Collections.Generic;
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
        StartCoroutine(ShowAnimation());
    }

    private IEnumerator ShowAnimation()
    {
        isSelected = true;
        float duration = 0.2f;
        float elapsed = 0;
        Quaternion startRot = transform.localRotation;
        Quaternion endRot = Quaternion.Euler(0f, 180f, 0f);

        bool spriteChanged = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localRotation = Quaternion.Lerp(startRot, endRot, t);

            if (t >= 0.5f && !spriteChanged)
            {
                iconImage.sprite = iconSprite;
                spriteChanged = true;
            }

            yield return null;
        }

        transform.localRotation = endRot;
    }

    public void Hide()
    {
        StartCoroutine(HideAnimation());
    }

    private IEnumerator HideAnimation()
    {
        float duration = 0.2f;
        float elapsed = 0;
        Quaternion startRot = transform.localRotation;
        Quaternion endRot = Quaternion.Euler(0f, 0f, 0f);

        bool spriteChanged = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localRotation = Quaternion.Lerp(startRot, endRot, t);

            if (t >= 0.5f && !spriteChanged)
            {
                iconImage.sprite = hiddenIconSprite;
                spriteChanged = true;
            }

            yield return null;
        }

        transform.localRotation = endRot;
        isSelected = false;
    }
}