using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// should eventually use an object pooling system
public class BulletInstance : MonoBehaviour {
    public float lifetime = 1.0f;
    public float speed = 15.0f;

    IEnumerator WaitToDestroy() {
        yield return new WaitForSeconds(lifetime);

        Destroy(gameObject);

        yield return null;
    }

    void Start() {
        StartCoroutine(WaitToDestroy());
    }

    void Update() {
        transform.position += transform.forward * Time.deltaTime * speed;
    }
}
