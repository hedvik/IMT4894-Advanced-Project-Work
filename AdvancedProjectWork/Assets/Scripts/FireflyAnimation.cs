using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyAnimation : MonoBehaviour
{
    public float _animationSpeed = 5f;
    public float _animationHeight = 5f;

    private float _animationTimer = 0f;
    private Vector3 _basePosition;
    private Vector3 _offsetBasePosition;

    private void Start()
    {
        _basePosition = transform.localPosition;
        _offsetBasePosition = _basePosition + Vector3.up * _animationHeight;
    }

    private void Update()
    {
        _animationTimer += Time.deltaTime * _animationSpeed;

        var newPosition = transform.localPosition;
        newPosition.y =  _basePosition.y * Mathf.Pow(Mathf.Cos(_animationTimer), 2) + _offsetBasePosition.y * Mathf.Pow(Mathf.Sin(_animationTimer), 2);

        transform.localPosition = newPosition;
    }
}
