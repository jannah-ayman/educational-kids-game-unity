using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSelector : MonoBehaviour
{

    [Header("Panels")]
    [SerializeField] private GameObject imageSelectionPanel;
    [SerializeField] private GameObject puzzleGamePanel;

    [Header("Image Data")]
    [SerializeField] private PuzzleImageData[] puzzleImages; 

    [Header("Buttons")]
    [SerializeField] private Button imageButton1;
    [SerializeField] private Button imageButton2;
    [SerializeField] private Button imageButton3;

    private PuzzleManager puzzleManager;

    void Start()
    {
        puzzleManager = FindObjectOfType<PuzzleManager>();

        
        imageButton1.onClick.AddListener(() => SelectImage(0));
        imageButton2.onClick.AddListener(() => SelectImage(1));
        imageButton3.onClick.AddListener(() => SelectImage(2));

        
        imageSelectionPanel.SetActive(true);
        puzzleGamePanel.SetActive(false);
    }

    void SelectImage(int imageIndex)
    {
        if (imageIndex >= 0 && imageIndex < puzzleImages.Length)
        {
            
            imageSelectionPanel.SetActive(false);

            
            puzzleGamePanel.SetActive(true);

            
            puzzleManager.StartPuzzle(puzzleImages[imageIndex]);
        }
    }

    public void BackToSelection()
    {
        imageSelectionPanel.SetActive(true);
        puzzleGamePanel.SetActive(false);
    }
}


[System.Serializable]
public class PuzzleImageData
{
    public string imageName;
    public Sprite completeImage; 
    public Sprite[] puzzlePieces; 
}

