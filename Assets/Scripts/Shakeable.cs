using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shakeable : MonoBehaviour {

    private bool shaking = false;

    public float duration = 0.25f;
    public float speed = 5f;
    public float shakeAmt = 5f;

	// Update is called once per frame
	void Update () {
        if (shaking) {
            Vector3 newPos = Random.insideUnitSphere;
            transform.position = Vector3.Lerp(Random.insideUnitSphere * shakeAmt, transform.position, speed * Time.deltaTime);
        }
	}

    public void shake() {
        StartCoroutine("startShaking");
    }

    IEnumerator startShaking() {

        Vector3 originalPos = transform.position;

        if (!shaking) { shaking = true; }

        yield return new WaitForSeconds(duration);

        shaking = false;
        transform.position = originalPos;
    }
}
