using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class PuzzlePiece : MonoBehaviour
{
    [HideInInspector] public int correctIndex; // The correct position (0-15)
    [HideInInspector] public int currentPosition; // Current position (0-15)
    [HideInInspector] public Image pieceImage; // Reference to Image component
    [HideInInspector] public PuzzleManager manager; // Reference to manager

    private Button button;

    void Awake()
    {
        pieceImage = GetComponent<Image>();
        button = GetComponent<Button>();

        // Make sure button exists
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }

        // Add click listener
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (manager != null)
        {
            manager.OnPieceClicked(this);
        }
    }

    public bool IsInCorrectPosition()
    {
        return currentPosition == correctIndex;
    }
}