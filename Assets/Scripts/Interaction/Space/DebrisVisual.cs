using System.Collections;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpaceDebris))]
[RequireComponent(typeof(LightningEffect))]
public class DebrisVisual : MonoBehaviour
{
    [SerializeField] SpaceDebris _debris;

    [Header("HealthBar")]
    [SerializeField] SpriteRenderer _fillRenderer; // Health_Fill 오브젝트 연결
    [SerializeField] float _healthBarDampingTime;
    Coroutine _healthBarCoroutine;

    // HealthBar Shader
    float _currentHealthRatio = 1f;
    float _targetHealthRatio = 1f;
    float _currentVelocity = 0f;
    static readonly int _fillAmountID = Shader.PropertyToID("_FillAmount"); // 셰이더 프로퍼티 이름 (그래프 Blackboard에 만든 이름과 똑같아야 함)
    MaterialPropertyBlock _materialPropertyBlock;

    [Header("Lightning Effect (Overloaded)")]
    [SerializeField] LightningEffect _lightningEffect;
    [SerializeField] float _lightningMinInterval;
    [SerializeField] float _lightningMaxInterval;

    [Header("Explosion Effect (Overloaded)")]
    [SerializeField] Transform _explosionVisualRoot;

    float _lightningTimer = 0f;
    float _nextInterval = 0f;

    void Awake()
    {
        if (_debris == null) { GetComponent<SpaceDebris>(); }
        if (_lightningEffect == null) { GetComponent<LightningEffect>(); }

        // 최적화를 위한 프로퍼티 블록 생성
        _materialPropertyBlock = new MaterialPropertyBlock();

        _currentHealthRatio = 1f;
        _explosionVisualRoot.gameObject.SetActive(false);
    }

    void Start()
    {
        _lightningEffect.SetRadius(_debris.Radius);
        _debris.OnHealthChanged += SetHealthBar;
        _debris.OnOverloadDestroyed += SetExplosion;
    }

    void OnDestroy()
    {
        if (_debris != null)
        {
            _debris.OnHealthChanged -= SetHealthBar;
        }
    }

    void Update()
    {
        LightningRoutine(Time.deltaTime);
    }

    IEnumerator HealthBarUpdateRoutine()
    {
        while (Mathf.Abs(_currentHealthRatio - _targetHealthRatio) > 0.001f)
        {
            _currentHealthRatio = Mathf.SmoothDamp(
                _currentHealthRatio,
                _targetHealthRatio,
                ref _currentVelocity,
                _healthBarDampingTime
            );

            UpdateMaterialBlock(_currentHealthRatio);

            yield return null;
        }

        _currentHealthRatio = _targetHealthRatio;
        UpdateMaterialBlock(_currentHealthRatio);
        _currentVelocity = 0f;

        _healthBarCoroutine = null;
    }

    void UpdateMaterialBlock(float value)
    {
        _fillRenderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetFloat(_fillAmountID, value);
        _fillRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    void LightningRoutine(float deltaTime)
    {
        if (_debris.IsOverloaded)
        {
            _lightningTimer += deltaTime;

            if (_lightningTimer >= _nextInterval)
            {
                _lightningEffect.GenerateLightning();
                _lightningTimer = 0f;
                _nextInterval = Random.Range(_lightningMinInterval, _lightningMaxInterval);
            }
        }
    }

    void SetHealthBar(float hpRatio)
    {
        _targetHealthRatio = hpRatio;

        _healthBarCoroutine ??= StartCoroutine(HealthBarUpdateRoutine());
    }

    void SetExplosion(float scale, float time)
    {
        _explosionVisualRoot.gameObject.SetActive(true);
        _explosionVisualRoot.localScale = Vector3.zero;
        _explosionVisualRoot.DOScale(scale, time).SetLink(gameObject);
    }
}
