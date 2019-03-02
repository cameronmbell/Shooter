using UnityEngine;
using System.Collections;

public class CollidableSphere : Collidable {
    public float radius = 1.0f;

    public override bool IsColliding(LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore) {
        return Physics.CheckSphere(transform.position, radius, layerMask, queryTriggerInteraction);
    }

    void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
