using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongLoadingTask : LoadingTask {

    public float duration = 2;

    private float counter;

    public override IEnumerator Action() {
        counter = duration;

        while (counter > 0) {
            new WaitForSeconds(Time.deltaTime);
            counter -= Time.deltaTime;
            yield return null; // This allows the update cycle to complete in the normal fashion so that a standard framerate can be maintained
        }

        yield return null;
    }

}
