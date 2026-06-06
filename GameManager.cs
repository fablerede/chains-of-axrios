using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public ICharacterDataService CharacterDataService { get; private set; }
    public CharacterData SelectedCharacter { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CharacterDataService = new LocalCharacterDataService();
    }

    public void SetSelectedCharacter(CharacterData character)
    {
        SelectedCharacter = character;
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex, LoadSceneMode.Single);
    }

    public void LoadTitleScreen() => LoadScene(0);
    public void LoadCharacterSelect() => LoadScene(1);
    public void LoadGameWorld() => LoadScene(2);

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}