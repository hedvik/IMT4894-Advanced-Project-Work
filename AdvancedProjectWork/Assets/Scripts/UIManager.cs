using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public Text _scoreText;
    public Text _experimentText;
    public float _scoreDisplaySpeed = 2f;

    private void Start()
    {
        _scoreText.rectTransform.parent.localScale = new Vector3(1, 0, 1);
        _experimentText.rectTransform.parent.localScale = new Vector3(1, 0, 1);
    }

    public void DisplayScore(int timeTaken, int numberOfHits, int totalScore, int participantID, int experimentID, int scorePlacement, int totalParticipants)
    {
        var scoreText = "Time Taken:\n~" + timeTaken + "s\n" + "Hits Taken:\n" + numberOfHits + "\n" + "Score:\n" + totalScore;
        _scoreText.text = scoreText;
        var experimentText = "Participant ID:\n" + participantID + "\n" + "Experiment ID:\n" + experimentID + "\n" + "Score Placement:\n" + scorePlacement + "/" + totalParticipants;
        _experimentText.text = experimentText;
        StartCoroutine(DisplayScoreAnimation());
    }

    private IEnumerator DisplayScoreAnimation()
    {
        var lerpTimer = 0f;
        var newScale = _scoreText.rectTransform.parent.localScale;
        while (lerpTimer <= 1f)
        {
            lerpTimer += Time.deltaTime * _scoreDisplaySpeed;
            newScale.y = Mathf.Lerp(0, 1, lerpTimer);
            _scoreText.rectTransform.parent.localScale = newScale;
            _experimentText.rectTransform.parent.localScale = newScale;
            yield return null;
        }

        _scoreText.rectTransform.parent.localScale = Vector3.one;
    }
}
