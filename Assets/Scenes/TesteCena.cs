using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TesteCena : MonoBehaviour
{
    [SerializeField] private string proximaCena = "CenaB";

    private void Update()
    {
        transform.Rotate(0f, 0f, 90f * Time.deltaTime);

        if (PauseUniversal.Pausado) return;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            SceneManager.LoadScene(proximaCena);
    }
}