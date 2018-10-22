using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleInstrumentAnimation : MonoBehaviour
{
    public float _speed = 5;
    public Vector3Int _scaleAxis;
    public float _maxScale = 1;
    public float _minScale = 0.8f;
    public float _minAngle = -15f;
    public float _maxAngle = 15f;

    private float _timer = 0;

    void Update()
    {
        _timer += Time.deltaTime * _speed;

        var remappedSineTimeScale = Remap(-1f, 1f, _minScale, _maxScale, Mathf.Sin(_timer));
        var remappedSineTimeRotation = Remap(-1f, 1f, 0, 1, Mathf.Sin(_timer * 0.5f));
        transform.localScale = new Vector3(1, remappedSineTimeScale, 1);
        transform.localRotation = Quaternion.Lerp(Quaternion.AngleAxis(_minAngle, transform.forward), Quaternion.AngleAxis(_maxAngle, transform.forward), remappedSineTimeRotation);
    }

    float Remap(float xMin, float xMax, float yMin, float yMax, float value)
    {
        return Mathf.Lerp(yMin, yMax, Mathf.InverseLerp(xMin, xMax, value));
    }
}
