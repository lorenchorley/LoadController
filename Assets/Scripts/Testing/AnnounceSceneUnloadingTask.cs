using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class AnnounceSceneUnloadingTask : UnloadingTask {
    
    public override IEnumerator Action() {
        Debug.Log("Now unloading the scene called " + SceneManager.GetActiveScene().name);

        yield return null;
    }

}
