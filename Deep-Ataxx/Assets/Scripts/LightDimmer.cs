using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightDimmer : MonoBehaviour {

    Light2D light;

    private void Awake() {
        light = this.GetComponent<Light2D>();

    }
    private void LateUpdate() {

        if(light.intensity > 0.1f) {
            light.intensity -= (0.05f * Time.deltaTime);
        }
    }
}
