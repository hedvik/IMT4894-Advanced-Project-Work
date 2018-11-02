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
    private IdleInstrumentAnimation _bossIdleAnimation;
    private const float _DEBUG_HEAD_SPEED = 100;
    private ParticleSystem _bossSmokeParticles;
    private TrailRenderer _trainRenderer;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _eyesObject.SetActive(false);
        _bossIdleAnimation = _bossObject.transform.GetChild(0).GetComponent<IdleInstrumentAnimation>();
        _bossSmokeParticles = _bossObject.transform.GetChild(2).GetComponent<ParticleSystem>();
        _trainRenderer = _bossObject.GetComponent<TrailRenderer>();
    }

    private void Update()
    {
        #region Debug
        if (Input.GetKey(KeyCode.A))
        {
            _playerObject.transform.Rotate(Vector3.up, Time.deltaTime * -_DEBUG_HEAD_SPEED);
        }
        if (Input.GetKey(KeyCode.D))
        {
            _playerObject.transform.Rotate(Vector3.up, Time.deltaTime * _DEBUG_HEAD_SPEED);
        }
        if (Input.GetKey(KeyCode.P))
        {
            _playerObject.GetComponent<PlayerManager>().AddCharge(100);
        }
        #endregion

        if (_stunned)
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
        StartCoroutine(TakeDamageAnimation());
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
        while (lerpTimer < 1)
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
        var upAxis = _bossObject.transform.up;
        while (lerpTimer < 1)
        {
            currentAngle = Mathf.Lerp(0, 360, lerpTimer);
            _bossObject.transform.localRotation = Quaternion.AngleAxis(currentAngle, upAxis);
            lerpTimer += Time.deltaTime;
            yield return null;
        }
        _bossObject.transform.localRotation = Quaternion.AngleAxis(0, upAxis);

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
        _trainRenderer.enabled = true;
        _gameActive = true;
        _audioSource.clip = _gameSoundtrack;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    private IEnumerator TakeDamageAnimation()
    {
        // We need to find out what y position we want to fall to
        // Layer 9 contains terrain
        var positionToFallTowards = _bossObject.transform.position;
        var layerBitMask = 1 << 9;
        RaycastHit hit;
        if(Physics.Raycast(_bossObject.transform.position, Vector3.down, out hit, 50, layerBitMask))
        {
            positionToFallTowards.y = hit.point.y + 0.75f;
        }

        // TODO: Tilt the boss slightly upwards, replace eye texture with a hurt one

        // We want a small bounce at the start of the fall animation so we "start" at a later stage in the animation before falling using a sine wave
        _bossIdleAnimation.enabled = false;
        var basePosition = _bossObject.transform.position;
        var offsetBasePosition = basePosition;
        offsetBasePosition.y += 1;
        // inverseLerp calculates the t parameter of the current position
        var positionLerpTimer = Mathf.InverseLerp(positionToFallTowards.y, offsetBasePosition.y, basePosition.y);
        while (Mathf.Sin(positionLerpTimer) > 0.0f)
        {
            _bossObject.transform.position = Vector3.Lerp(positionToFallTowards, offsetBasePosition, Mathf.Sin(positionLerpTimer));
            positionLerpTimer += (Time.deltaTime * 4);
            yield return null;
        }

        // As the boss faces down the position has to go down a bit as well so it looks like it is on the floor
        _bossObject.transform.LookAt(Vector3.up * -1000, Vector3.up);
        _bossObject.transform.position += Vector3.down * 0.5f;

        // Creates a small bit of wiggle after the fall
        yield return new WaitForSeconds(1f);
        _trainRenderer.enabled = false;
        var oldIdleSpeed = _bossIdleAnimation._speed;
        _bossIdleAnimation._speed *= 10;
        _bossIdleAnimation.enabled = true;
        yield return new WaitForSeconds(0.5f);
        _bossIdleAnimation.enabled = false;
        _bossIdleAnimation._speed = oldIdleSpeed;
        yield return new WaitForSeconds(1.5f);
        _bossSmokeParticles.Play();
        // TODO: Play "poof" sound effect?


        if (!_gameActive)
        {
            _bossObject.transform.position = basePosition;
            StartGame();
        }
        else
        {
            // This is mostly to make sure the boss doesn't change height if the damage animation happens right as another height modifying animation was active
            var newPosition = basePosition;
            newPosition.y = _bossRotationPivot.position.y;
            _bossObject.transform.position = newPosition;
        }

        _trainRenderer.enabled = true;
        _stunned = false;
        _cooldownPeriod = true;
        _bossIdleAnimation.enabled = true;
    }
    #endregion
}
