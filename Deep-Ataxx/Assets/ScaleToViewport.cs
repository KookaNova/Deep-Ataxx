using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleToViewport : MonoBehaviour
{
    void Awake()
    {
        transform.localScale = new Vector3(Camera.main.rect.width, Camera.main.rect.height, 1);
    }
}
