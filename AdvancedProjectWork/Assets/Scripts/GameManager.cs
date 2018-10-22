using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Metainformation")]
    public GameObject _playerObject;
    public GameObject _bossObject;
    public List<Material> _projectileMaterials;
    public GameObject _projectilePrefab;
    public Transform _bossRotationPivot;

    [Header("GameSettings")]
    public float _attackCooldown = 2f;
    public float _bossMovementSpeed = 5f;
    public float _offsetFromPivot = 10f;

    private bool _gameActive = false;
    private float _attackTimer = 0f;
    private float _currentAngle = 0f;
    private Coroutine _lerpRoutineReference;

    private void Start()
    {
        _bossObject.transform.position = _bossRotationPivot.position - (Vector3.forward * _offsetFromPivot);
        _bossObject.transform.SetParent(_bossRotationPivot);
        _gameActive = true;
    }

    private void Update()
    {
        if (_gameActive)
        {
            _attackTimer += Time.deltaTime;
            if (_attackTimer >= _attackCooldown)
            {
                _attackTimer -= _attackCooldown;
                AttackPlayer();
            }
        }
        _bossObject.transform.LookAt(_playerObject.transform, Vector3.up);
    }

    private void AttackPlayer()
    {
        var projectile = Instantiate(_projectilePrefab, _bossObject.transform.position, Quaternion.identity);
        projectile.GetComponent<Projectile>()._targetTransform = _playerObject.transform;
        DecideNewAttackPosition();
    }

    private void DecideNewAttackPosition()
    {
        var newAngle = Random.Range(0f, 360f);
        if (_lerpRoutineReference != null)
        {
            StopCoroutine(_lerpRoutineReference);
        }

        _lerpRoutineReference = StartCoroutine(LerpRoutine(newAngle));
    }

    private IEnumerator LerpRoutine(float newAngle)
    {
        var startAngle = _currentAngle;
        var lerpTimer = 0f;
        while (_currentAngle > newAngle + 0.005 || _currentAngle < newAngle - 0.005)
        {
            _currentAngle = Mathf.Lerp(startAngle, newAngle, lerpTimer);
            _bossRotationPivot.transform.rotation = Quaternion.AngleAxis(_currentAngle, Vector3.up);
            lerpTimer += Time.deltaTime * _bossMovementSpeed;
            yield return null;
        }
    }
}
