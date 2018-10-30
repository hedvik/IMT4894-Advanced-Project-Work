using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float _rotationSpeed = 3;
    public float _movementSpeed = 1;
    public MeshRenderer _mainMeshRenderer;

    [HideInInspector] public float _damageValue = 0f;
    [HideInInspector] public float _chargeValue = 10f;
    [HideInInspector] public Transform _targetTransform;
    private Vector3 _rotationAxis;

    // Use this for initialization
    void Start()
    {
        _rotationAxis = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)).normalized;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(_rotationAxis, Time.deltaTime * _rotationSpeed);
        transform.position += (_targetTransform.position - transform.position).normalized * Time.deltaTime * _movementSpeed;
    }
}
