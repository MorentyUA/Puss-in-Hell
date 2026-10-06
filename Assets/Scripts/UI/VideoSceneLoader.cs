using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace PussInHell.UI
{
    public class VideoSceneLoader : MonoBehaviour
    {
        [SerializeField] private string targetSceneName;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private bool hideCursor = true;

        private AsyncOperation loadOperation;

        public bool IsSceneReady { get; private set; }
        public float Progress => loadOperation != null ? Mathf.Clamp01(loadOperation.progress / 0.9f) : 0f;

        private void Start()
        {
            if (hideCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (loadOnStart && videoPlayer != null && !string.IsNullOrEmpty(targetSceneName))
                StartLoading();
        }

        public void StartLoading()
        {
            if (loadOperation == null) StartCoroutine(LoadAfterVideo());
        }

        private IEnumerator LoadAfterVideo()
        {
            loadOperation = SceneManager.LoadSceneAsync(targetSceneName);
            loadOperation.allowSceneActivation = false;

            while (loadOperation.progress < 0.9f) yield return null;
            IsSceneReady = true;

            while (!videoPlayer.isPlaying) yield return null;
            while (videoPlayer.isPlaying) yield return null;

            loadOperation.allowSceneActivation = true;
        }
    }
}
