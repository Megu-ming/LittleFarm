using System;
using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    // ===== 설정값 =====

    [Header("시작 날짜")]
    [SerializeField] int _startYear = 1;
    [SerializeField] Season _startSeason = Season.Spring;
    [SerializeField] int _startDay = 1;   // 1 ~ 28

    [Header("하루 시작/종료 시간")]
    [SerializeField] int _dayStartHour = 6; // 06:00 시작
    [SerializeField] int _dayEndHour = 2; // 새벽 02:00에 강제 종료

    [Header("시간 흐름")]
    [Tooltip("실제 1초 동안 흐를 게임 시간(분 단위). 1이면 1초에 1분 흐름 = 10초에 10분")]
    [SerializeField] float _gameMinutesPerRealSecond = 1f;

    public const int DaysPerSeason = 28;

    // ===== 현재 시간 상태 =====

    int _year;
    Season _season;
    int _day;    // 1 ~ 28
    int _hour;   // 0 ~ 23
    int _minute; // 0 ~ 59

    float _minuteAccumulator;

    // ===== 프로퍼티 =====
    public int Year => _year;
    public Season Season => _season;
    public int Day => _day;
    public int Hour => _hour;
    public int Minute => _minute;
    public int DayStartHour => _dayStartHour;
    public int DayEndHour => _dayEndHour;


    /// <summary>오늘이 진행 가능한 시간대인지 (06:00 ~ 다음날 02:00)</summary>
    public bool IsPlayableTime
    {
        get
        {
            // 06:00 ~ 23:59
            if (_hour >= _dayStartHour || _hour < _dayEndHour)
                return true;
            return false;
        }
    }

    // 나중에 UI, 성장 시스템 등이 구독할 수 있는 이벤트들 (원하면 사용)
    public event Action<int, int> OnTimeChanged;        // (hour, minute)
    public event Action<int, Season, int> OnDateChanged;// (year, season, day)

    public void Initialize()
    {
        _year = _startYear;
        _season = _startSeason;
        _day = Mathf.Clamp(_startDay, 1, DaysPerSeason);

        _hour = _dayStartHour;
        _minute = 0;

        _minuteAccumulator = 0f;

        OnTimeChanged?.Invoke(_hour, _minute);
        OnDateChanged?.Invoke(_year, _season, _day);
    }

    void Update()
    {
        // 에디터에서 테스트할 때 시간을 멈추고 싶으면 조건 추가 가능
        Tick(Time.deltaTime);
    }

    void Tick(float deltaTime)
    {
        if (_gameMinutesPerRealSecond <= 0f)
            return;

        _minuteAccumulator += deltaTime * _gameMinutesPerRealSecond;

        while (_minuteAccumulator >= 1f)
        {
            _minuteAccumulator -= 1f;
            AdvanceMinutes(1);
        }
    }

    void AdvanceMinutes(int minutes)
    {
        _minute += minutes;
        while (_minute >= 60)
        {
            _minute -= 60;
            _hour++;
            if (_hour >= 24)
                _hour -= 24; // 날짜는 여기서는 안 바꿈 (침대/02:00에서만 바꿈)
        }
        // 10분마다 UI 변경
        if (_minute % 10 == 0)
            OnTimeChanged?.Invoke(_hour, _minute);

        // 새벽 02:00 이후면 강제 다음날로 넘김
        if (_hour == _dayEndHour && _minute == 0)
        {
            ForceNextDay();
        }
    }

    /// <summary>
    /// 플레이어가 침대에서 잠을 잘 때 호출해줄 함수.
    /// 몇 시든 무조건 다음날 06:00으로 점프.
    /// </summary>
    public void SleepToNextDay()
    {
        ForceNextDay();
    }

    void ForceNextDay()
    {
        // 날짜 1일 증가
        AdvanceDate();

        // 시간은 항상 06:00으로
        _hour = _dayStartHour;
        _minute = 0;
        _minuteAccumulator = 0f;

        OnTimeChanged?.Invoke(_hour, _minute);
    }

    void AdvanceDate()
    {
        _day++;
        if (_day > DaysPerSeason)
        {
            _day = 1;
            _season++;

            if ((int)_season > (int)Season.Winter)
            {
                _season = Season.Spring;
                _year++;
            }
        }

        OnDateChanged?.Invoke(_year, _season, _day);
    }

    /// <summary>
    /// 06:00(0) ~ 다음날 02:00(1) 기준으로 현재 시간 진행도를 0~1로 반환
    /// </summary>
    public float GetDayProgress01()
    {
        int startMinutes = _dayStartHour * 60;
        int endMinutes = (_dayEndHour + 24) * 60; // 다음날로 취급

        int h = _hour;
        if (h < _dayStartHour) h += 24; // 0~5시는 다음날로 취급

        int currentMinutes = (h * 60) + _minute;

        return Mathf.Clamp01(Mathf.InverseLerp(startMinutes, endMinutes, currentMinutes));
    }

    // 디버그용: 지금 시간을 문자열로
    public string GetTimeText()
    {
        return $"{_year}년 {SeasonToKorean(_season)} {_day}일 {Hour:00}:{Minute:00}";
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

    #region Testing
    public void MoveToNextDayTest()
    {
        ForceNextDay();
    }
    #endregion
}
