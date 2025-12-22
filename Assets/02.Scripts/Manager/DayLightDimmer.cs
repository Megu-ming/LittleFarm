using UnityEngine;

public class DayLightDimmer : MonoBehaviour
{
    [SerializeField] GameTimeManager _timeManager;
    [SerializeField] Light _directionalLight;

    [Header("Directional Light Intensity")]
    [SerializeField] float _maxIntensity = 1.2f;
    [SerializeField] AnimationCurve _intensityCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("Ambient Intensity")]
    [SerializeField] bool _controlAmbient = true;
    [SerializeField] float _maxAmbientIntensity = 1.0f;
    [Tooltip("0~1 시간 진행도(t)에 따른 ambient 비율 (밤을 확 어둡게: 끝값을 0.03~0.06 근처로)")]
    [SerializeField] AnimationCurve _ambientCurve = AnimationCurve.Linear(0, 1, 1, 0.05f);

    [Header("Smoothing")]
    [SerializeField] float _smoothSpeed = 6f; // 클수록 빨리 따라감

    private void Reset()
    {
        _directionalLight = GetComponent<Light>();
    }

    private void Update()
    {
        if (_timeManager == null || _directionalLight == null) return;

        float t = _timeManager.GetDayProgress01(); // 0~1
        float lerpT = 1f - Mathf.Exp(-_smoothSpeed * Time.deltaTime);

        // 1) 태양(Directional) 밝기
        float targetLight = _intensityCurve.Evaluate(t) * _maxIntensity;
        _directionalLight.intensity = Mathf.Lerp(_directionalLight.intensity, targetLight, lerpT);

        // 2) 환경광(Ambient) 밝기
        if (_controlAmbient)
        {
            float targetAmbient = _ambientCurve.Evaluate(t) * _maxAmbientIntensity;
            RenderSettings.ambientIntensity = Mathf.Lerp(RenderSettings.ambientIntensity, targetAmbient, lerpT);
        }
    }
}
