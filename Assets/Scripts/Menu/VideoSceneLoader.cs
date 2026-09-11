using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class VideoSceneLoader : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Имя сцены для загрузки")]
    public string targetSceneName;

    [Tooltip("Видеоплеер на сцене")]
    public VideoPlayer videoPlayer;

    [Tooltip("Автоматически начать загрузку при старте")]
    public bool loadOnStart = true;

    [Header("Настройки курсора")]
    [Tooltip("Скрыть курсор на этой сцене")]
    public bool hideCursor = true;

    private AsyncOperation sceneLoadOperation;
    private bool isSceneReady = false;

    private void Start()
    {
        // Управление курсором
        if (hideCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (videoPlayer == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            return;
        }

        if (loadOnStart)
        {
            StartLoading();
        }
    }

    public void StartLoading()
    {
        StartCoroutine(LoadSceneAndWaitForVideo());
    }

    private IEnumerator LoadSceneAndWaitForVideo()
    {
        // Начинаем асинхронную загрузку сцены
        sceneLoadOperation = SceneManager.LoadSceneAsync(targetSceneName);

        // Не позволяем сцене активироваться автоматически
        sceneLoadOperation.allowSceneActivation = false;


        // Ждём завершения загрузки сцены (progress достигнет 0.9)
        while (sceneLoadOperation.progress < 0.9f)
        {
            yield return null;
        }

        isSceneReady = true;

        // Ждём, пока видео не начнёт проигрываться
        while (!videoPlayer.isPlaying)
        {
            yield return null;
        }


        // Ждём окончания видео
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }


        // Активируем загруженную сцену
        sceneLoadOperation.allowSceneActivation = true;
    }

    // Опциональный метод для отображения прогресса
    public float GetLoadProgress()
    {
        if (sceneLoadOperation != null)
        {
            return sceneLoadOperation.progress / 0.9f; // Нормализуем до 0-1
        }
        return 0f;
    }

    public bool IsSceneReady()
    {
        return isSceneReady;
    }
}
