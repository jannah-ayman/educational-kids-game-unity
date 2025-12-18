using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform piecesContainer;
    [SerializeField] private Image referenceImage;

    private List<PuzzlePiece> pieces = new List<PuzzlePiece>();
    private PuzzlePiece selectedPiece = null;
    private PuzzleImageData currentImageData;
    private Animator characterAnimator;

    void Start()
    {
        characterAnimator = FindObjectOfType<Animator>();
    }

    public void StartPuzzle(PuzzleImageData imageData)
    {
        currentImageData = imageData;
        if (referenceImage != null)
            referenceImage.sprite = imageData.completeImage;
        SetupExistingPieces();
        ShufflePieces();
    }

    void SetupExistingPieces()
    {
        pieces.Clear();
        PuzzlePiece[] existingPieces = piecesContainer.GetComponentsInChildren<PuzzlePiece>(true);
        for (int i = 0; i < existingPieces.Length && i < 16; i++)
        {
            PuzzlePiece piece = existingPieces[i];
            if (i < currentImageData.puzzlePieces.Length)
            {
                Image pieceImage = piece.GetComponent<Image>();
                if (pieceImage != null)
                    pieceImage.sprite = currentImageData.puzzlePieces[i];
                piece.correctIndex = i;
                piece.currentPosition = i;
                piece.manager = this;
                pieceImage.color = Color.white;
                pieces.Add(piece);
            }
        }
    }

    void ShufflePieces()
    {
        List<int> positions = new List<int>();
        for (int i = 0; i < pieces.Count; i++)
            positions.Add(i);
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = positions[i];
            positions[i] = positions[randomIndex];
            positions[randomIndex] = temp;
        }
        for (int i = 0; i < pieces.Count; i++)
        {
            pieces[i].currentPosition = positions[i];
            pieces[i].transform.SetSiblingIndex(positions[i]);
        }
    }

    public void OnPieceClicked(PuzzlePiece piece)
    {
        if (selectedPiece == null)
        {
            selectedPiece = piece;
            piece.pieceImage.color = Color.yellow;
        }
        else
        {
            SwapPieces(selectedPiece, piece);
            selectedPiece.pieceImage.color = Color.white;
            selectedPiece = null;
            CheckWin();
        }
    }

    void SwapPieces(PuzzlePiece piece1, PuzzlePiece piece2)
    {
        int siblingIndex1 = piece1.transform.GetSiblingIndex();
        int siblingIndex2 = piece2.transform.GetSiblingIndex();
        piece1.transform.SetSiblingIndex(siblingIndex2);
        piece2.transform.SetSiblingIndex(siblingIndex1);
        int tempPos = piece1.currentPosition;
        piece1.currentPosition = piece2.currentPosition;
        piece2.currentPosition = tempPos;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    void CheckWin()
    {
        bool allCorrect = true;
        foreach (PuzzlePiece piece in pieces)
        {
            if (!piece.IsInCorrectPosition())
            {
                allCorrect = false;
                break;
            }
        }
        if (allCorrect)
            StartCoroutine(OnPuzzleComplete());
    }

    IEnumerator OnPuzzleComplete()
    {
        if (characterAnimator != null)
            characterAnimator.SetTrigger("Happy");
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayWinFanfare();
        yield return new WaitForSeconds(0.5f);
        if (StarPopupManager.Instance != null)
            StarPopupManager.Instance.ShowStars(5, "Puzzle Complete!");
    }

    public void RestartPuzzle()
    {
        selectedPiece = null;
        ShufflePieces();
        if (StarPopupManager.Instance != null)
            StarPopupManager.Instance.PlayAgain();
    }

    public void BackToSelection()
    {
        ImageSelector selector = FindObjectOfType<ImageSelector>();
        if (selector != null)
            selector.BackToSelection();
    }
}