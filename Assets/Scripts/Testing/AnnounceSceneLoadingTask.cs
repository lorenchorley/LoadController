using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class AnnounceSceneLoadingTask : LoadingTask {
    
    public override IEnumerator Action() {
        Debug.Log("Now loading the scene called " + SceneManager.GetActiveScene().name);

        yield return null;
    }

}
