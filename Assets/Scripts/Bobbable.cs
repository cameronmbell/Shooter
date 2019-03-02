using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bobbable : MonoBehaviour {
    public float period = 0.1f;
    public float displacment = 0.1f;

    [HideInInspector]
    public float amplitude = 0.0f;

    Vector3 initalOffset;

    void Start() {
        initalOffset = transform.localPosition;
    }

    void Update() {
        transform.localPosition = initalOffset + new Vector3(0.0f, Mathf.Sin(Time.time / period) * displacment * amplitude, 0.0f);
    }

    public Transform GetTransform() {
        return transform;
    }
}
