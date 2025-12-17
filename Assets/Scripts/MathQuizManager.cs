using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MathQuizManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text questionText;
    public Button[] answerButtons; // 4 buttons
    //public TMP_Text scoreText;
    public TMP_Text questionNumberText;

    [Header("Game Settings")]
    public int totalQuestions = 10;
    public int maxNumber = 20;

    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;
    private int correctAnswer;
    private List<Question> questions = new List<Question>();
    private Animator characterAnimator;

    void Start()
    {
        GenerateQuestions();
        DisplayQuestion();
        //UpdateScoreUI();
        characterAnimator = FindObjectOfType<Animator>();

        if (characterAnimator == null)
        {
            Debug.LogWarning("⚠️ No CharacterAnimator found in scene!");
        }
    }

    void GenerateQuestions()
    {
        questions.Clear();

        for (int i = 0; i < totalQuestions; i++)
        {
            int num1 = Random.Range(1, maxNumber + 1);
            int num2 = Random.Range(1, maxNumber + 1);

            // Randomly choose operation: 0=add, 1=subtract, 2=multiply
            int operation = Random.Range(0, 3);

            Question q = new Question();

            switch (operation)
            {
                case 0: // Addition
                    q.questionText = $"{num1} + {num2} = ?";
                    q.correctAnswer = num1 + num2;
                    break;

                case 1: // Subtraction (keep result positive)
                    if (num1 < num2)
                    {
                        int temp = num1;
                        num1 = num2;
                        num2 = temp;
                    }
                    q.questionText = $"{num1} - {num2} = ?";
                    q.correctAnswer = num1 - num2;
                    break;

                case 2: // Multiplication (smaller numbers)
                    num1 = Random.Range(2, 11);
                    num2 = Random.Range(2, 11);
                    q.questionText = $"{num1} × {num2} = ?";
                    q.correctAnswer = num1 * num2;
                    break;
            }

            questions.Add(q);
        }

        Debug.Log($"✅ Generated {totalQuestions} questions");
    }

    void DisplayQuestion()
    {
        if (currentQuestionIndex >= totalQuestions)
        {
            ShowResults();
            return;
        }

        Question q = questions[currentQuestionIndex];
        questionText.text = q.questionText;
        correctAnswer = q.correctAnswer;

        // Update UI counters
        questionNumberText.text = $"Question {currentQuestionIndex + 1}/{totalQuestions}";

        // Generate 4 answer options (1 correct + 3 wrong)
        List<int> options = GenerateAnswerOptions(correctAnswer);

        // Assign options to buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int answer = options[i];
            answerButtons[i].GetComponentInChildren<TMP_Text>().text = answer.ToString();

            // Remove old listeners to avoid duplicates
            answerButtons[i].onClick.RemoveAllListeners();

            // Capture the answer value in local variable
            int capturedAnswer = answer;
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(capturedAnswer));

            // Enable button
            answerButtons[i].interactable = true;
        }
    }

    List<int> GenerateAnswerOptions(int correct)
    {
        List<int> options = new List<int> { correct };

        // Generate 3 wrong answers
        while (options.Count < 4)
        {
            // Create wrong answer near the correct one
            int offset = Random.Range(-10, 11);
            if (offset == 0) offset = Random.Range(1, 6); // Avoid generating correct answer

            int wrongAnswer = correct + offset;

            // Make sure it's positive and not duplicate
            if (wrongAnswer > 0 && !options.Contains(wrongAnswer))
            {
                options.Add(wrongAnswer);
            }
        }

        // Shuffle the options so correct answer isn't always first
        ShuffleList(options);

        return options;
    }

    void ShuffleList(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    void OnAnswerSelected(int selectedAnswer)
    {
        // Disable all buttons to prevent multiple clicks
        foreach (Button btn in answerButtons)
            btn.interactable = false;

        // Check if answer is correct
        if (selectedAnswer == correctAnswer)
        {
            correctAnswers++;
            characterAnimator.SetTrigger("Happy");
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCorrect();
        }
        else
        {
            characterAnimator.SetTrigger("Sad");
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayWrong();
        }

        //UpdateScoreUI();

        // Move to next question after short delay
        Invoke(nameof(NextQuestion), 0.5f);
    }

    void NextQuestion()
    {
        currentQuestionIndex++;
        DisplayQuestion();
    }

    //void UpdateScoreUI()
    //{
    //    scoreText.text = $"Score: {correctAnswers}/{totalQuestions}";
    //}

    void ShowResults()
    {
        // Calculate percentage
        float percentage = (float)correctAnswers / totalQuestions * 100f;

        // Calculate stars (0-5 scale)
        // 5 stars: 90-100%
        // 4 stars: 80-89%
        // 3 stars: 60-79%
        // 2 stars: 40-59%
        // 1 star: 20-39%
        // 0 stars: below 20%

        int starsEarned = 0;
        if (percentage >= 90) starsEarned = 5;
        else if (percentage >= 80) starsEarned = 4;
        else if (percentage >= 60) starsEarned = 3;
        else if (percentage >= 40) starsEarned = 2;
        else if (percentage >= 20) starsEarned = 1;

        // Create message for popup
        string message = $"You got {correctAnswers} out of {totalQuestions} correct!";

        // Show star popup
        if (StarPopupManager.Instance != null)
        {
            StarPopupManager.Instance.ShowStars(starsEarned, message);
        }
        else
        {
            Debug.LogError("❌ StarPopupManager not found!");
        }

        Debug.Log($"🎉 Quiz completed! Stars earned: {starsEarned}");
    }

    public void RestartQuiz()
    {
        currentQuestionIndex = 0;
        correctAnswers = 0;
        StarPopupManager.Instance.PlayAgain();
        GenerateQuestions();
        DisplayQuestion();
        //UpdateScoreUI();
    }

    // Button to go back to main menu
    public void BackToMenu()
    {
        StarPopupManager.Instance.GoToMainMenu();
    }
}

// Helper class to store question data
[System.Serializable]
public class Question
{
    public string questionText;
    public int correctAnswer;
}