using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LoadingController : MonoBehaviour {

    public GameObject LoadingScreen;
    
    private enum LoadState {
        Disabled,
        RunningPreloads,
        WaitingForSceneChange,
        RunningLoads
    }

    private class LoadProfile {
        public string Name;
        public Func<IEnumerator> LoadAction;
        IEnumerator e = null;
        bool StillGoing = true;
        public void RunOnce() {
            if (e == null)
                e = LoadAction.Invoke();
            StillGoing = e.MoveNext();
        }
        public bool StillHasIterations() {
            return StillGoing;
        }
    }

    private Queue<LoadProfile> PreloadQueue;
    private Queue<LoadProfile> LoadQueue;
    private string StartingSceneName;
    private string TargetSceneName;
    private int TargetSceneIndex;
    private LoadState state;

    private bool inited;

    private UnityEvent OnStartLoading;
    private UnityEvent OnFinishedLoading;

    public LoadingController() {
        OnStartLoading = new UnityEvent();
        OnFinishedLoading = new UnityEvent();
        inited = false;
    }

    void Awake() {
        if (inited)
            throw new Exception("Already inited");

        PreloadQueue = new Queue<LoadProfile>();
        LoadQueue = new Queue<LoadProfile>();
        inited = true;

        ResetAndDisable();

        LoadInitialScene();
    }

    public void EnqueueOnStartLoading(UnityAction action) {
        OnStartLoading.AddListener(action);
    }

    public void EnqueueOnFinishedLoading(UnityAction action) {
        OnFinishedLoading.AddListener(action);
    }

    private void ResetAndDisable() {
        StartingSceneName = null;
        TargetSceneName = null;
        TargetSceneIndex = -2;
        state = LoadState.Disabled;
        PreloadQueue.Clear();
        LoadQueue.Clear();
        enabled = false;
    }

    void Start() {
        DontDestroyOnLoad(gameObject);
    }

    private void EnqueuePreloadJob(string Name, Func<IEnumerator> action) {
        PreloadQueue.Enqueue(new LoadProfile() {
            Name = "Preload task: " + Name,
            LoadAction = action,
        });
    }

    private void EnqueueLoadJob(string Name, Func<IEnumerator> action) {
        LoadQueue.Enqueue(new LoadProfile() {
            Name = "Load task: " + Name,
            LoadAction = action,
        });
    }

    //public void StartLoadQueue(string SceneName) {
    //    if (enabled)
    //        throw new Exception("Cannot start a load queue while already loading");

    //    enabled = true;
    //    TargetSceneName = SceneName;

    //}

    private LoadProfile CurrentTask;

    public bool IsCurrentlyLoadingScene() {
        return state != LoadState.Disabled;
    }

    void Update() {
        if (state == LoadState.Disabled) {
            ResetAndDisable();
            return;
        }

        long start = DateTime.Now.Ticks;
        float diff = 0;
        while (diff < Time.deltaTime / 10) {

            switch (state) {
            case LoadState.RunningPreloads:
                if (ProcessLoadTask(PreloadQueue))
                    LoadNextScene();
                break;
            case LoadState.WaitingForSceneChange:
                if (SceneManager.GetActiveScene().name != StartingSceneName) { 
                    state = LoadState.RunningLoads;
                    LoadTasks();
                }
                break;
            case LoadState.RunningLoads:
                if (ProcessLoadTask(LoadQueue))
                    Finish();
                break;
            case LoadState.Disabled:
                ResetAndDisable();
                return;
            }

            diff = ((float) (DateTime.Now.Ticks - start)) / 10000000;
            break;
        }

    }

    // Returns true if the current task is finished and the queue is empty
    private bool ProcessLoadTask(Queue<LoadProfile> CurrentQueue) {
        if (CurrentTask == null) {
            if (CurrentQueue.Count == 0)
                return true;

            CurrentTask = CurrentQueue.Dequeue();
            CurrentTask.RunOnce();
        }

        CurrentTask.RunOnce();

        if (!CurrentTask.StillHasIterations()) {
            if (CurrentQueue.Count == 0) {
                return true;
            } else {
                CurrentTask = CurrentQueue.Dequeue();
                CurrentTask.RunOnce();
            }
        }

        return false;
    }

    private void LoadNextScene() {

        if (LoadingScreen != null)
            LoadingScreen.gameObject.SetActive(true);

        if (TargetSceneName != null) {
            SceneManager.LoadSceneAsync(TargetSceneName, LoadSceneMode.Single);
        } else if (TargetSceneIndex != -2) {
            SceneManager.LoadSceneAsync(TargetSceneIndex, LoadSceneMode.Single);
        } else
            throw new Exception("No scene name or index set");

        state = LoadState.WaitingForSceneChange;
    }

    private void Finish() {
        ResetAndDisable();

        if (LoadingScreen != null)
            LoadingScreen.gameObject.SetActive(false);

        OnFinishedLoading.Invoke();
        OnFinishedLoading.RemoveAllListeners();

    }

    public void ChangeScene(string SceneName) {
        if (state != LoadState.Disabled)
            throw new Exception("Cannot change scene, already loading a scene");

        if (enabled)
            throw new Exception("Should not be enabled");

        StartingSceneName = SceneManager.GetActiveScene().name;
        TargetSceneName = SceneName;

        state = LoadState.RunningPreloads;

        UnloadTasks();

        OnStartLoading.Invoke();
        OnStartLoading.RemoveAllListeners();
        enabled = true;
    }

    public void LoadInitialScene() {
        if (state != LoadState.Disabled)
            throw new Exception("Cannot load initial scene, already loading a scene");

        if (enabled)
            throw new Exception("Should not be enabled");

        StartingSceneName = SceneManager.GetActiveScene().name;
        TargetSceneIndex = 1;

        state = LoadState.RunningPreloads;

        UnloadTasks();

        OnStartLoading.Invoke();
        OnStartLoading.RemoveAllListeners();
        enabled = true;
    }

    private void LoadTasks() {
        LoadingTask[] loadingTasks = GameObject.FindObjectsOfType<LoadingTask>();

        for (int i = 0; i < loadingTasks.Length; i++) {
            LoadingTask l = loadingTasks[i];
            EnqueueLoadJob(l.GetType().Name, l.Action);
        }

    }

    private void UnloadTasks() {
        UnloadingTask[] unloadingTasks = GameObject.FindObjectsOfType<UnloadingTask>();

        for (int i = 0; i < unloadingTasks.Length; i++) {
            UnloadingTask l = unloadingTasks[i];
            EnqueuePreloadJob(l.GetType().Name, l.Action);
        }

    }

}

