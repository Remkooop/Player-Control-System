using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform playerTransform;
    public float offsetY;

    private void LateUpdate() {
        if(playerTransform == null) return;
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y + offsetY, transform.position.z);
    }
}
