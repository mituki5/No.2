using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlashCalcGameManager : MonoBehaviour
{
    [Header("UI Groups")]
    public GameObject titleGroup;
    public GameObject gameGroup;
    public GameObject resultGroup;
    public GameObject feedbackGroup;

    [Header("UI Elements")]
    public Text countdownText;
    public Text questionText;
    public InputField answerInput;
    public Slider timerSlider;
    public Image correctImage;
    public Image wrongImage;
    public Text resultText;

    [Header("Settings")]
    public float initialDisplayTime = 5f;
    public int maxQuestions = 15;

    private int currentQuestionIndex = 0;
    private int correctCount = 0;
    private float displayTime;
    private bool isPlaying = false;

    void Start()
    {
        ShowTitle();
    }

    //==================== 表示切替 ====================//
    public void ShowTitle()
    {
        HideAll();
        titleGroup.SetActive(true);
        StartCoroutine(WaitForEnter());
    }

    IEnumerator WaitForEnter()
    {
        while (!Input.GetKeyDown(KeyCode.Return))
        {
            yield return null;
        }
        ShowGame();
    }

    public void ShowGame()
    {
        HideAll();
        gameGroup.SetActive(true);

        currentQuestionIndex = 0;
        correctCount = 0;
        displayTime = initialDisplayTime;
        isPlaying = true;

        StartCoroutine(GameRoutine());
    }

    public void ShowResult()
    {
        HideAll();
        resultGroup.SetActive(true);

        string rank = GetRank(correctCount);
        resultText.text = $"ランク: {rank}";

        StartCoroutine(ReturnToTitleAfterDelay(3f));
    }

    private void HideAll()
    {
        titleGroup.SetActive(false);
        gameGroup.SetActive(false);
        resultGroup.SetActive(false);
        feedbackGroup.SetActive(false);
    }

    IEnumerator ReturnToTitleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowTitle();
    }

    //==================== ゲームメイン処理 ====================//
    IEnumerator GameRoutine()
    {
        // カウントダウン
        countdownText.gameObject.SetActive(true);
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        countdownText.gameObject.SetActive(false);

        while (isPlaying && currentQuestionIndex < maxQuestions)
        {
            // 問題生成
            int a, b;
            string op;
            GenerateQuestion(currentQuestionIndex, out a, out b, out op);

            questionText.text = $"{a} {op} {b} = ?";
            answerInput.text = "";
            answerInput.ActivateInputField(); // キーボード入力自動フォーカス
            timerSlider.value = 1f;

            float timer = displayTime;
            bool answered = false;
            bool correct = false;

            while (timer > 0f && !answered)
            {
                timer -= Time.deltaTime;
                timerSlider.value = timer / displayTime;

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    correct = CheckAnswer(a, b, op, answerInput.text);
                    answered = true;
                }

                yield return null;
            }

            if (!answered)
            {
                correct = false;
            }

            // フィードバック表示
            yield return StartCoroutine(ShowFeedbackAndClearInput(correct, a, b, op));

            if (!correct)
            {
                EndGame();
                yield break;
            }

            // 速度調整（3問ごとに速くなる）
            correctCount++;
            if ((currentQuestionIndex + 1) % 3 == 0)
            {
                displayTime = Mathf.Max(1f, displayTime - 0.5f);
            }

            // 難易度調整 10問目以降
            currentQuestionIndex++;
            if (currentQuestionIndex == 10)
            {
                displayTime = Mathf.Max(1f, displayTime - 0.5f);
            }
            if (currentQuestionIndex == 11)
            {
                displayTime = initialDisplayTime; // 二桁計算に戻す
            }
        }

        EndGame();
    }

    //==================== 問題生成 ====================//
    void GenerateQuestion(int index, out int a, out int b, out string op)
    {
        // 10問目まで一桁、11問目以降二桁
        int maxVal = (index < 11) ? 9 : 99;
        a = Random.Range(0, maxVal + 1);
        b = Random.Range(1, maxVal + 1); // 割り算用に0を避ける

        // 演算子ランダム（たまに引き算・掛け算・割り算）
        int r = Random.Range(0, 10);
        if (r < 6) op = "+";       // 60%
        else if (r < 8) op = "-";  // 20%
        else if (r < 9) op = "*";  // 10%
        else op = "/";             // 10%
    }

    bool CheckAnswer(int a, int b, string op, string input)
    {
        int correctAnswer = 0;
        switch (op)
        {
            case "+": correctAnswer = a + b; break;
            case "-": correctAnswer = a - b; break;
            case "*": correctAnswer = a * b; break;
            case "/": correctAnswer = a / b; break;
        }

        int playerAnswer;
        if (!int.TryParse(input, out playerAnswer)) return false;
        return playerAnswer == correctAnswer;
    }

    IEnumerator ShowFeedbackAndClearInput(bool correct, int a, int b, string op)
    {
        feedbackGroup.SetActive(true);
        correctImage.gameObject.SetActive(correct);
        wrongImage.gameObject.SetActive(!correct);

        if (!correct)
        {
            // 不正解なら正解も表示
            questionText.text = $"{a} {op} {b} = {GetCorrectAnswer(a, b, op)}";
        }

        yield return new WaitForSeconds(0.5f);

        feedbackGroup.SetActive(false);
        answerInput.text = "";
    }

    int GetCorrectAnswer(int a, int b, string op)
    {
        switch (op)
        {
            case "+": return a + b;
            case "-": return a - b;
            case "×": return a * b;
            case "÷": return a / b;
        }
        return 0;
    }

    void EndGame()
    {
        isPlaying = false;
        ShowResult();
    }

    //==================== ランク ====================//
    string GetRank(int score)
    {
        if (score >= 25) return "SS";
        if (score >= 20) return "S";
        if (score >= 16) return "A";
        if (score >= 13) return "B";
        if (score >= 9) return "C";
        if (score >= 6) return "D";
        return "E";
    }
}
