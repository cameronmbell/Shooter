using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Collidable : MonoBehaviour {
    public abstract bool IsColliding(LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore);
}