using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnloadingTask : MonoBehaviour {

    public abstract IEnumerator Action();

    public void ChangeScene(string SceneName) {
        GameObject.FindObjectOfType<LoadingController>().ChangeScene(SceneName);
    }

}
