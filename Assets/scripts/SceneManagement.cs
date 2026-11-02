using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public static SceneManagement Instance { get; private set; }

    [Header("シーン名（シーン名と完全一致させる）")]
    [SerializeField] private string titleSceneName = "Title";
    [SerializeField] private string gameSceneName;
    [SerializeField] private string gameOverSceneName;
    [SerializeField] private string gameClearSceneName;

    [Header("操作キー")]
    [SerializeField] private Key startKey = Key.Space;
    [SerializeField] private Key endingKey = Key.Enter;

    private bool isLoading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // GameObject上の他のスクリプトには触れず、このコンポーネントだけ削除
            enabled = false;
            Destroy(this);
            return;
        }

        Instance = this;

        // DontDestroyOnLoadは使わず、各シーンのオブジェクトを通常どおり破棄する
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (isLoading || Keyboard.current == null)
            return;

        string activeSceneName = SceneManager.GetActiveScene().name;

        if (activeSceneName == titleSceneName &&
            IsKeyPressed(startKey))
        {
            LoadScene(gameSceneName);
        }
        else if ((activeSceneName == gameOverSceneName ||
                  activeSceneName == gameClearSceneName) &&
                 IsKeyPressed(endingKey))
        {
            LoadScene(titleSceneName);
        }
    }

    private bool IsKeyPressed(Key key)
    {
        return key != Key.None && Keyboard.current[key].wasPressedThisFrame;
    }

    public static void LoadGameOver()
    {
        if (Instance != null)
            Instance.LoadScene(Instance.gameOverSceneName);
        else
            Debug.LogError("現在のシーンにSceneManagementがありません。");
    }

    public static void LoadGameClear()
    {
        if (Instance != null)
            Instance.LoadScene(Instance.gameClearSceneName);
        else
            Debug.LogError("現在のシーンにSceneManagementがありません。");
    }

    private void LoadScene(string sceneName)
    {
        if (isLoading)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("遷移先のシーン名が設定されていません。", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"シーン「{sceneName}」を読み込めません。Build ProfilesのScene Listを確認してください。",
                this
            );
            return;
        }

        isLoading = true;
        SceneManager.LoadScene(sceneName);
    }
}