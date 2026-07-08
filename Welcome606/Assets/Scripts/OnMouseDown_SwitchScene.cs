using UnityEngine;
using UnityEngine.SceneManagement;

public class OnMouseDown_SwitchScene : MonoBehaviour
{
    public string sceneName;

    void OnMouseDown()
	{
        SwitchScene();
    }

    public void SwitchScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
