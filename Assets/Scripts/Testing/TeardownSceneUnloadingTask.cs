using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TeardownSceneUnloadingTask : UnloadingTask {
    
    public override IEnumerator Action() {

        GameObject obj = GameObject.Find("Test Object");
        Assert.IsNotNull(obj);

        Destroy(obj);

        yield return null;
    }

}
