using TMPro;
using UnityEngine;

public class TimeUI : MonoBehaviour
{
    [SerializeField] TMP_Text _yearText;
    [SerializeField] TMP_Text _seasonText;
    [SerializeField] TMP_Text _dateText;
    [SerializeField] TMP_Text _timeText;

    GameTimeManager _timeManager;

    public void Initialize(GameTimeManager timeManager)
    {
        _timeManager = timeManager;

        _timeManager.OnTimeChanged += HandleTimeChanged;
        _timeManager.OnDateChanged += HandleDateChanged;
    }

    void HandleTimeChanged(int hour, int minute)
    {
        if (_timeText == null) return;

        _timeText.text = $"{hour:00}:{minute:00}";
    }

    void HandleDateChanged(int year, Season season, int day)
    {
        if (_dateText == null) return;

        string seasonKr = SeasonToKorean(season);
        _dateText.text = $"{day}일";
        _seasonText.text = seasonKr;
        _yearText.text = $"{year}년";
    }

    string SeasonToKorean(Season s)
    {
        switch (s)
        {
            case Season.Spring: return "봄";
            case Season.Summer: return "여름";
            case Season.Fall: return "가을";
            case Season.Winter: return "겨울";
        }
        return s.ToString();
    }
}
