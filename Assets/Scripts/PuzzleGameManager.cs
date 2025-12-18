using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PuzzleGameManager : MonoBehaviour
{
    [Header("Game Elements")]
    [Range(2, 6)]
    [SerializeField] private int difficulty = 4;
    [SerializeField] private Transform gameHolder;
    [SerializeField] private Transform piecePrefab;

    [Header("UI Elements")]
    [SerializeField] private List<Texture2D> imageTextures;
    [SerializeField] private Transform levelSelectPanel;
    [SerializeField] private Image levelSelectPrefab;

    [Header("New UI Elements")]
    [SerializeField] private Image referenceImage; // Top-left corner reference
    [SerializeField] private TMP_Text titleText; // "Choose Your Puzzle!" text
    [SerializeField] private TMP_Text timerText; // Timer display

    private List<Transform> pieces;
    private Vector2Int dimensions;
    private float width;
    private float height;

    private Transform draggingPiece = null;
    private Vector3 offset;

    private int piecesCorrect;

    // Timer variables
    private float startTime;
    private bool puzzleActive = false;
    private Texture2D currentTexture;

    // Character animator
    private Animator characterAnimator;

    void Start()
    {
        // Find character animator
        StartCoroutine(AssignAnimatorNextFrame());

        // Hide timer, reference image, and game holder initially
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (referenceImage != null) referenceImage.gameObject.SetActive(false);
        if (gameHolder != null) gameHolder.gameObject.SetActive(false);

        // Show title text
        if (titleText != null) titleText.gameObject.SetActive(true);

        // Create the UI
        foreach (Texture2D texture in imageTextures)
        {
            Image image = Instantiate(levelSelectPrefab, levelSelectPanel);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            // Assign button action with click sound
            image.GetComponent<Button>().onClick.AddListener(delegate
            {
                // 🔊 Play click sound when selecting image
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayButtonClick();

                StartGame(texture);
            });
        }
    }

    IEnumerator AssignAnimatorNextFrame()
    {
        yield return null; // wait 1 frame
        characterAnimator = FindActiveCharacterAnimator();

        if (characterAnimator == null)
            Debug.LogWarning("⚠️ No active character Animator found!");
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

    void Update()
    {
        // Update timer if puzzle is active
        if (puzzleActive && timerText != null)
        {
            float elapsedTime = Time.time - startTime;
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }

        // Puzzle piece dragging logic
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit)
            {
                draggingPiece = hit.transform;
                offset = draggingPiece.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                offset += Vector3.back;

                // 🔊 Play snap sound when picking up piece
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayPuzzleSnap();
            }
        }

        if (draggingPiece && Input.GetMouseButtonUp(0))
        {
            SnapAndDisableIfCorrect();
            draggingPiece.position += Vector3.forward;
            draggingPiece = null;
        }

        if (draggingPiece)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            newPosition += offset;
            draggingPiece.position = newPosition;
        }
    }

    public void StartGame(Texture2D jigsawTexture)
    {
        // Store current texture for reference image
        currentTexture = jigsawTexture;

        // Hide title text
        if (titleText != null) titleText.gameObject.SetActive(false);

        // Show game holder (the puzzle board)
        if (gameHolder != null) gameHolder.gameObject.SetActive(true);

        // Show and setup reference image in top-left
        if (referenceImage != null)
        {
            referenceImage.gameObject.SetActive(true);
            referenceImage.sprite = Sprite.Create(jigsawTexture,
                new Rect(0, 0, jigsawTexture.width, jigsawTexture.height),
                Vector2.zero);
        }

        // Show and start timer
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = "Time: 00:00";
        }
        startTime = Time.time;
        puzzleActive = true;

        // Hide the UI
        levelSelectPanel.gameObject.SetActive(false);

        pieces = new List<Transform>();
        dimensions = GetDimensions(jigsawTexture, difficulty);
        CreateJigsawPieces(jigsawTexture);
        Scatter();
        UpdateBorder();
        piecesCorrect = 0;

        Debug.Log("🎮 Puzzle started!");
    }

    Vector2Int GetDimensions(Texture2D jigsawTexture, int difficulty)
    {
        Vector2Int dimensions = Vector2Int.zero;
        if (jigsawTexture.width < jigsawTexture.height)
        {
            dimensions.x = difficulty;
            dimensions.y = (difficulty * jigsawTexture.height) / jigsawTexture.width;
        }
        else
        {
            dimensions.x = (difficulty * jigsawTexture.width) / jigsawTexture.height;
            dimensions.y = difficulty;
        }
        return dimensions;
    }

    void CreateJigsawPieces(Texture2D jigsawTexture)
    {
        height = 1f / dimensions.y;
        float aspect = (float)jigsawTexture.width / jigsawTexture.height;
        width = aspect / dimensions.x;

        for (int row = 0; row < dimensions.y; row++)
        {
            for (int col = 0; col < dimensions.x; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameHolder);
                piece.localPosition = new Vector3(
                  (-width * dimensions.x / 2) + (width * col) + (width / 2),
                  (-height * dimensions.y / 2) + (height * row) + (height / 2),
                  -1);
                piece.localScale = new Vector3(width, height, 1f);
                piece.name = $"Piece {(row * dimensions.x) + col}";
                pieces.Add(piece);

                float width1 = 1f / dimensions.x;
                float height1 = 1f / dimensions.y;
                Vector2[] uv = new Vector2[4];
                uv[0] = new Vector2(width1 * col, height1 * row);
                uv[1] = new Vector2(width1 * (col + 1), height1 * row);
                uv[2] = new Vector2(width1 * col, height1 * (row + 1));
                uv[3] = new Vector2(width1 * (col + 1), height1 * (row + 1));

                Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                mesh.uv = uv;
                piece.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", jigsawTexture);
            }
        }
    }

    private void Scatter()
    {
        float orthoHeight = Camera.main.orthographicSize;
        float screenAspect = (float)Screen.width / Screen.height;
        float orthoWidth = (screenAspect * orthoHeight);

        float pieceWidth = width * gameHolder.localScale.x;
        float pieceHeight = height * gameHolder.localScale.y;

        orthoHeight -= pieceHeight;
        orthoWidth -= pieceWidth;

        foreach (Transform piece in pieces)
        {
            float x = Random.Range(-orthoWidth, orthoWidth);
            float y = Random.Range(-orthoHeight, orthoHeight);
            piece.position = new Vector3(x, y, -1);
        }
    }

    private void UpdateBorder()
    {
        LineRenderer lineRenderer = gameHolder.GetComponent<LineRenderer>();

        float halfWidth = (width * dimensions.x) / 2f;
        float halfHeight = (height * dimensions.y) / 2f;
        float borderZ = 0f;

        lineRenderer.SetPosition(0, new Vector3(-halfWidth, halfHeight, borderZ));
        lineRenderer.SetPosition(1, new Vector3(halfWidth, halfHeight, borderZ));
        lineRenderer.SetPosition(2, new Vector3(halfWidth, -halfHeight, borderZ));
        lineRenderer.SetPosition(3, new Vector3(-halfWidth, -halfHeight, borderZ));

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.enabled = true;
    }

    private void SnapAndDisableIfCorrect()
    {
        int pieceIndex = pieces.IndexOf(draggingPiece);
        int col = pieceIndex % dimensions.x;
        int row = pieceIndex / dimensions.x;

        Vector2 targetPosition = new((-width * dimensions.x / 2) + (width * col) + (width / 2),
                                     (-height * dimensions.y / 2) + (height * row) + (height / 2));

        if (Vector2.Distance(draggingPiece.localPosition, targetPosition) < (width / 2))
        {
            draggingPiece.localPosition = targetPosition;
            draggingPiece.GetComponent<BoxCollider2D>().enabled = false;

            // 🔊 Play correct sound when piece snaps into place
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCorrect();

            // 😊 Trigger happy animation
            if (characterAnimator != null)
                characterAnimator.SetTrigger("Happy");

            piecesCorrect++;
            Debug.Log($"✓ Piece snapped! {piecesCorrect}/{pieces.Count}");

            if (piecesCorrect == pieces.Count)
            {
                OnPuzzleComplete();
            }
        }
    }

    void OnPuzzleComplete()
    {
        puzzleActive = false;

        // ✅ HIDE puzzle UI
        if (gameHolder != null)
            gameHolder.gameObject.SetActive(false);

        if (referenceImage != null)
            referenceImage.gameObject.SetActive(false);

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        // Calculate time taken
        float totalTime = Time.time - startTime;
        int minutes = Mathf.FloorToInt(totalTime / 60f);
        int seconds = Mathf.FloorToInt(totalTime % 60f);

        // Calculate stars based on time
        // Under 2 minutes (120 seconds) = 5 stars
        // 2-4 minutes (120-240 seconds) = 4 stars  
        // Over 4 minutes = 3 stars
        int starsEarned;
        if (totalTime < 120f)
        {
            starsEarned = 5;
        }
        else if (totalTime < 240f)
        {
            starsEarned = 4;
        }
        else
        {
            starsEarned = 3;
        }

        string message = $"Completed in {minutes:00}:{seconds:00}!";

        Debug.Log($"🎉 Puzzle Complete! Time: {minutes}:{seconds}, Stars: {starsEarned}");

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

    // This is called by StarPopupManager's "Play Again" button
    public void RestartGame()
    {
       
        // Stop timer
        puzzleActive = false;

        // Destroy all the puzzle pieces
        foreach (Transform piece in pieces)
        {
            Destroy(piece.gameObject);
        }
        pieces.Clear();

        // Hide the outline
        gameHolder.GetComponent<LineRenderer>().enabled = false;

        // Hide game holder, reference image and timer
        if (gameHolder != null) gameHolder.gameObject.SetActive(false);
        if (referenceImage != null) referenceImage.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);

        // Show title text
        //if (titleText != null) titleText.gameObject.SetActive(true);

        // Show the level select UI
        //levelSelectPanel.gameObject.SetActive(true);
        StarPopupManager.Instance.PlayAgain();
        Debug.Log("✓ Game restarted - back to level select");
    }
    public void BackToMenu()
    {
        StarPopupManager.Instance.GoToMainMenu();
    }
}