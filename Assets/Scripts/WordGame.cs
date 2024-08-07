using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordPuzzleManager : MonoBehaviour
{
    public GameObject fiveLettersWordPrefab;
    public GameObject sixLettersWordPrefab;
    public TextMeshProUGUI levelText;

    public List<Button> letterButtons; 
    public List<Button> sixLetterButtons; 
    public Button[] jumbledLetterButtons;
    public List<string> words = new List<string> { "HAPPY", "FIELD", "UNITY", "BLOODY" };
    private string completeWord;
    private string currentWord;
    private List<char> jumbledLetters = new List<char>();
    private int nextLetterIndex = 2;
    private int currentLevel = 0;
    private Dictionary<Button, char> buttonToLetterMap = new Dictionary<Button, char>();

    void Start()
    {
        SetLevel(currentLevel);
    }

    void SetLevel(int levelIndex)
    {
        levelText.text = "Level " + (levelIndex + 1);
        jumbledLetters.Clear();

        if (levelIndex < 0 || levelIndex >= words.Count)
        {
            Debug.Log("Level Index is Out of Bounds");
            return;
        }

        completeWord = words[levelIndex];
        Debug.Log(completeWord.Length);

        if (completeWord.Length == 5)
        {
            sixLettersWordPrefab.SetActive(false);
            fiveLettersWordPrefab.SetActive(true);
            currentWord = completeWord.Substring(0, 2) + new string(' ', completeWord.Length - 2);
            nextLetterIndex = 2;
            InitializeWordDisplay(letterButtons);
        }
        else if (completeWord.Length == 6)
        {
            fiveLettersWordPrefab.SetActive(false);
            sixLettersWordPrefab.SetActive(true);
            currentWord = completeWord.Substring(0, 3) + new string(' ', completeWord.Length - 3);
            nextLetterIndex = 3;
            InitializeWordDisplay(sixLetterButtons);
        }

        InitializeJumbledLetters();
    }

    void InitializeWordDisplay(List<Button> buttons)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (i < completeWord.Length)
            {
                buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentWord[i].ToString();
                buttons[i].gameObject.SetActive(true);
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }
    }

    void InitializeJumbledLetters()
    {
        for (int i = nextLetterIndex; i < completeWord.Length; i++)
        {
            jumbledLetters.Add(completeWord[i]);
        }
        ShuffleList(jumbledLetters);

        for (int i = 0; i < jumbledLetterButtons.Length; i++)
        {
            if (i < jumbledLetters.Count)
            {
                char letter = jumbledLetters[i];
                jumbledLetterButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = jumbledLetters[i].ToString();
                buttonToLetterMap[jumbledLetterButtons[i]] = letter;
                jumbledLetterButtons[i].gameObject.SetActive(true);
            }
            else
            {
                jumbledLetterButtons[i].gameObject.SetActive(false);
            }
        }
    }
    IEnumerator nextLevel()
    {
        yield return new WaitForSeconds(1f);
        currentLevel++;
        if (currentLevel < words.Count)
        {
            SetLevel(currentLevel);
        }
        else
        {
            Debug.Log("Finished");
        }
    }

    void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public void OnJumbledLetterClick(Button clickedButton)
    {
        if (buttonToLetterMap.TryGetValue(clickedButton, out char letter))
        {
            Vector3 targetPosition = GetButtonPositionAtIndex(nextLetterIndex);
            if (letter != completeWord[nextLetterIndex])
            {
                StartCoroutine(WrongLetter(letter, clickedButton,targetPosition));
            }
            else if (letter == completeWord[nextLetterIndex])
            {
                StartCoroutine(CorrectLetter(letter, clickedButton,targetPosition));
                currentWord = currentWord.Substring(0, nextLetterIndex) + letter + currentWord.Substring(nextLetterIndex + 1);


                jumbledLetters.Remove(letter);

                if (currentWord == completeWord)
                {
                    StartCoroutine(nextLevel());
                }
            }
        }
    }
    IEnumerator WrongLetter(char letter, Button clickedButton, Vector3 targetPosition)
    {
        Transform clickedButtonTransform = clickedButton.transform;
        Vector3 originalPosition = clickedButtonTransform.position;

        clickedButtonTransform.DOMove(targetPosition, 0.5f).OnComplete(() =>
        {
            clickedButton.image.color = Color.red;
            Handheld.Vibrate();
            clickedButtonTransform.DOMove(originalPosition, 0.5f); });

        yield return new WaitForSeconds(1f);
        clickedButton.image.color = Color.green;

    }

    
    IEnumerator CorrectLetter(char letter, Button clickedButton, Vector3 targetPosition)
    {
        Transform clickedButtonTransform = clickedButton.transform;
        Vector3 originalPosition = clickedButtonTransform.position;

        clickedButtonTransform.DOMove(targetPosition, 0.5f).OnComplete(() =>
        {
            clickedButtonTransform.DOMove(originalPosition, 0.5f);
            clickedButton.gameObject.SetActive(false);
            nextLetterIndex++;

            UpdateWordDisplay();

        });
        yield return new WaitForSeconds(.1f);
  
    }

    void SetLetterButtonsColor(List<Button> buttons, Color color)
    {
        foreach (Button button in buttons)
        {
            button.GetComponentInChildren<TextMeshProUGUI>().color = color;
        }
    }

    void UpdateWordDisplay(char? wrongLetter = ' ')
    {
        List<Button> activeButtons = completeWord.Length == 5 ? letterButtons : sixLetterButtons;

        for (int i = 0; i < completeWord.Length; i++)
        {
            if (i < nextLetterIndex )
            {
                activeButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = completeWord[i].ToString();
            }
            else if ( i == nextLetterIndex)
            {
                activeButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = wrongLetter.ToString();
            }
            else
            {
                activeButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = " ";
            }
            
        }
    }
    Vector3 GetButtonPositionAtIndex(int index)
    {
        List<Button> activeButtons = completeWord.Length == 5 ? letterButtons : sixLetterButtons;
        return activeButtons[index].transform.position;
    }
   
}
