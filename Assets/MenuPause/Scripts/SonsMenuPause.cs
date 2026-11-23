using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class SonsMenuPause : MonoBehaviour
{
    [SerializeField] private AudioClip somClique;

    private AudioSource fonte;

    private void Awake()
    {
        fonte = GetComponent<AudioSource>();
        fonte.playOnAwake = false;
        fonte.ignoreListenerPause = true;
    }

    private void Start()
    {
        Button[] botoes = GetComponentsInChildren<Button>(true);

        if (botoes.Length == 0)
            Debug.LogWarning("SonsMenuPause: nenhum botão encontrado abaixo deste objeto. Coloque o script na raiz do menu de pause.");

        foreach (Button botao in botoes)
            botao.onClick.AddListener(TocarClique);
    }

    private void TocarClique()
    {
        if (somClique == null) return;
        fonte.PlayOneShot(somClique);
    }
}