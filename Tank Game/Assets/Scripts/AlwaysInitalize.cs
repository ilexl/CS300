using UnityEngine;
using UnityEngine.SceneManagement;

public class AlwaysInitalize : MonoBehaviour
{
    private static bool Initialised = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Initialised)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            // Scene that should always be started on
            // This prevent init errors in editor when we forget to switch back to correct init scene
            SceneManager.LoadScene("Start");
        }
    }

    public static void Initialise()
    {
        Initialised = true;
    }
}
