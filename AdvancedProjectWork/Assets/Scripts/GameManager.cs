using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class AttackType
    {
        [Header("Visuals and Animation")]
        public Material _attackMaterial;
        public float _attackSpeed;
        public string _telegraphAnimationTrigger;

        [Header("Combat Values")]
        public float _attackDamage;
        public float _attackChargeAmount;
    }

    [Header("MetaInformation")]
    public GameObject _playerObject;
    public GameObject _bossObject;
    public GameObject _projectilePrefab;
    public Transform _bossRotationPivot;
    public GameObject _eyesObject;

    [Header("GameSettings")]
    public List<AttackType> _attackTypes = new List<AttackType>();
    public float _attackCooldown = 2f;
    public float _bossMovementSpeed = 5f;
    public float _offsetFromPivot = 10f;
    public AnimationCurve _stunnedAnimationVelocity;

    [Header("Audio")]
    public AudioClip _gameStartAudio;
    public AudioClip _gameSoundtrack;
    private AudioSource _audioSource;


    private bool _gameActive = false;
    private float _attackTimer = 0f;
    private float _currentOrbitAngle = 0f;
    private bool _cooldownPeriod = true;
    private const float _ATTACK_TO_MOVEMENT_COOLDOWN = 0.5f;
    private bool _stunned = false;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _eyesObject.SetActive(false);
    }

    private void Update()
    {
        if(_stunned)
        {
            return;
        }
        _bossObject.transform.LookAt(_playerObject.transform, Vector3.up);
        if (!_gameActive)
        {
            return;
        }

        if (_cooldownPeriod)
        {
            _attackTimer += Time.deltaTime;
            if (_attackTimer >= _attackCooldown)
            {
                _attackTimer -= _attackCooldown;
                _cooldownPeriod = false;
                TelegraphAttack();
            }
        }

    }

    public void TakeDamage(float damage)
    {
        _stunned = true;
        _cooldownPeriod = false;
        StopAllCoroutines();
        StartCoroutine(TakeDamage());
    }

    private void StartGame()
    {
        // Start Animations
        _audioSource.PlayOneShot(_gameStartAudio);
        _eyesObject.SetActive(true);

        StartCoroutine(StartGameAnimation());
    }

    private void TelegraphAttack()
    {
        var attackIndex = Random.Range(0, _attackTypes.Count);
        var projectileSettings = _attackTypes[attackIndex];

        if (projectileSettings._telegraphAnimationTrigger != "")
        {
            StartCoroutine(projectileSettings._telegraphAnimationTrigger, attackIndex);
        }
        else
        {
            _cooldownPeriod = true;
            AttackPlayer(attackIndex);
        }
    }

    private void AttackPlayer(int attackIndex)
    {
        var projectileSettings = _attackTypes[attackIndex];

        // The projectile spawns with a small offset in forward direction so it does not spawn inside the boss
        var projectileObject = Instantiate(_projectilePrefab, _bossObject.transform.position + _bossObject.transform.forward, Quaternion.identity);
        var projectileComponent = projectileObject.GetComponent<Projectile>();

        projectileComponent._targetTransform = _playerObject.transform;
        projectileComponent._movementSpeed = projectileSettings._attackSpeed;
        projectileComponent._mainMeshRenderer.material = projectileSettings._attackMaterial;

        var newAngle = Random.Range(0f, 360f);
        StartCoroutine(OrbitLerpRoutine(newAngle));
    }

    #region Animations/Interpolations
    private IEnumerator OrbitLerpRoutine(float newAngle)
    {
        yield return new WaitForSeconds(_ATTACK_TO_MOVEMENT_COOLDOWN);
        var startAngle = _currentOrbitAngle;
        var lerpTimer = 0f;
        while (_currentOrbitAngle > newAngle + 0.005 || _currentOrbitAngle < newAngle - 0.005)
        {
            _currentOrbitAngle = Mathf.Lerp(startAngle, newAngle, lerpTimer);
            _bossRotationPivot.transform.rotation = Quaternion.AngleAxis(_currentOrbitAngle, Vector3.up);
            lerpTimer += Time.deltaTime * _bossMovementSpeed;
            yield return null;
        }
    }

    private IEnumerator DoubleJumpAnimation(int attackIndex)
    {
        var lerpTimer = 0f;
        var basePosition = _bossObject.transform.position;

        // Rather hacky, but it coincides with two jumps at a speed of 5 :P
        while (lerpTimer < 4f)
        {
            lerpTimer += Time.deltaTime * 5;
            var pingPongLerp = Mathf.PingPong(lerpTimer, 1);
            var newPosition = _bossObject.transform.position;
            newPosition.y = Mathf.Lerp(basePosition.y, basePosition.y + 1, pingPongLerp);
            _bossObject.transform.position = newPosition; 
            yield return null;
        }
        _bossObject.transform.position = basePosition;

        _cooldownPeriod = true;
        AttackPlayer(attackIndex);
    }

    private IEnumerator SpinAnimation(int attackIndex)
    {
        var lerpTimer = 0f;
        var currentAngle = 0f;
        var forwardAxis = _bossObject.transform.forward;
        while (currentAngle > 360 + 0.005 || currentAngle < 360 - 0.005)
        {
            currentAngle = Mathf.Lerp(0, 360, lerpTimer);
            _bossObject.transform.localRotation = Quaternion.AngleAxis(currentAngle, forwardAxis);
            lerpTimer += Time.deltaTime;
            yield return null;
        }
        _bossObject.transform.localRotation = Quaternion.AngleAxis(0, forwardAxis);

        _cooldownPeriod = true;
        AttackPlayer(attackIndex);
    }

    private IEnumerator StartGameAnimation()
    {
        // Letting the initial audioclip finish before moving on
        yield return new WaitForSeconds(2);
        var targetPosition = _bossRotationPivot.position - (Vector3.forward * _offsetFromPivot);
        var startPosition = _bossObject.transform.position;
        var lerpTimer = 0f;

        while (lerpTimer < 1f)
        {
            lerpTimer += Time.deltaTime * 5;
            _bossObject.transform.position = Vector3.Lerp(startPosition, targetPosition, lerpTimer);
            yield return null;
        }

        _bossObject.transform.position = targetPosition;
        _bossObject.transform.SetParent(_bossRotationPivot);
        _bossObject.GetComponent<TrailRenderer>().enabled = true;
        _gameActive = true;
        _audioSource.clip = _gameSoundtrack;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    private IEnumerator TakeDamage()
    {
        _bossObject.transform.LookAt(Vector3.up * -1000, Vector3.up);

        yield return new WaitForSeconds(3);


        if(!_gameActive)
        {
            StartGame();
        }
        else
        {
            var newPosition = _bossObject.transform.position;
            newPosition.y = _bossRotationPivot.position.y;
            _bossObject.transform.position = newPosition;
        }

        _stunned = false;
        _cooldownPeriod = true;
    }
    #endregion
}
