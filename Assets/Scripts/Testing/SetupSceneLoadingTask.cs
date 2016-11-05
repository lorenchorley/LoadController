using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupSceneLoadingTask : LoadingTask {
    
    public override IEnumerator Action() {

        new GameObject("Test Object");

        yield return null;
    }

}
