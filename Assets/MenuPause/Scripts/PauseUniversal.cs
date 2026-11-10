using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseUniversal : MonoBehaviour
{
    public static PauseUniversal Instancia { get; private set; }
    public static bool Pausado { get; private set; }
    public static bool PermitirPause { get; set; } = true;

    private const string NomePrefab = "MenuPauseUniversal";
    private const string ChaveVolume = "pause_volume";
    private const string ChaveTelaCheia = "pause_tela_cheia";

    [SerializeField] private GameObject painelPause;
    [SerializeField] private GameObject painelBotoes;
    [SerializeField] private GameObject painelOpcoes;

    [SerializeField] private GameObject primeiroBotao;
    [SerializeField] private GameObject primeiroItemOpcoes;

    [SerializeField] private string nomeCenaMenu = "";

    [SerializeField] private Slider sliderVolume;
    [SerializeField] private Toggle toggleTelaCheia;

    [SerializeField] private string[] cenasSemPause;
    [SerializeField] private bool pausarAudio = false;

    private float escalaAnterior = 1f;
    private CursorLockMode travaCursorAntes;
    private bool cursorVisivelAntes;
    private bool cenaPermitePause = true;
    private EventSystem meuEventSystem;

    private bool PodePausar => PermitirPause && cenaPermitePause;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CriarAutomaticamente()
    {
        if (Instancia != null) return;

        GameObject prefab = Resources.Load<GameObject>(NomePrefab);
        if (prefab == null)
        {
            Debug.LogError("PauseUniversal: prefab '" + NomePrefab + "' não encontrado. " +
                           "Ele precisa estar dentro de uma pasta chamada Resources.");
            return;
        }
        Instantiate(prefab);
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);

        painelPause.SetActive(false);

        float volume = PlayerPrefs.GetFloat(ChaveVolume, 1f);
        sliderVolume.SetValueWithoutNotify(volume);
        AudioListener.volume = volume;

        bool telaCheia = PlayerPrefs.GetInt(ChaveTelaCheia, Screen.fullScreen ? 1 : 0) == 1;
        toggleTelaCheia.SetIsOnWithoutNotify(telaCheia);
        AplicarTelaCheia(telaCheia);

        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void OnDestroy()
    {
        if (Instancia != this) return;

        SceneManager.sceneLoaded -= AoCarregarCena;
        Instancia = null;

        if (Pausado)
        {
            Pausado = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }

    private void Update()
    {
        if (!PausePressionado()) return;

        if (Pausado)
        {
            if (painelOpcoes.activeSelf) VoltarDasOpcoes();
            else Continuar();
        }
        else if (PodePausar)
        {
            Pausar();
        }
    }

    private bool PausePressionado()
    {
        bool teclado = Keyboard.current != null
                       && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool controle = Gamepad.current != null
                        && Gamepad.current.startButton.wasPressedThisFrame;
        return teclado || controle;
    }

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        if (modo == LoadSceneMode.Single)
        {
            cenaPermitePause = !CenaSemPause(cena.name);

            if (!cenaPermitePause && Pausado) Retomar(false);
        }

        GarantirEventSystem();
    }

    private bool CenaSemPause(string nomeCena)
    {
        foreach (string nome in cenasSemPause)
            if (nome == nomeCena) return true;
        return false;
    }

    private void GarantirEventSystem()
    {
        foreach (EventSystem es in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
        {
            if (es != meuEventSystem)
            {
                if (meuEventSystem != null) meuEventSystem.gameObject.SetActive(false);
                return;
            }
        }

        if (meuEventSystem == null) meuEventSystem = CriarEventSystem();
        meuEventSystem.gameObject.SetActive(true);
    }

    private EventSystem CriarEventSystem()
    {
        GameObject obj = new GameObject("EventSystem (Pause)");
        DontDestroyOnLoad(obj);
        EventSystem es = obj.AddComponent<EventSystem>();
        InputSystemUIInputModule modulo = obj.AddComponent<InputSystemUIInputModule>();
        modulo.AssignDefaultActions();
        return es;
    }

    public void Pausar()
    {
        if (Pausado || !PodePausar) return;

        Pausado = true;
        escalaAnterior = Time.timeScale;
        Time.timeScale = 0f;
        if (pausarAudio) AudioListener.pause = true;

        travaCursorAntes = Cursor.lockState;
        cursorVisivelAntes = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        painelPause.SetActive(true);
        MostrarBotoes();
    }

    public void Continuar()
    {
        Retomar(true);
    }

    public void AbrirOpcoes()
    {
        painelBotoes.SetActive(false);
        painelOpcoes.SetActive(true);
        Selecionar(primeiroItemOpcoes);
    }

    public void VoltarDasOpcoes()
    {
        MostrarBotoes();
    }

    public void Sair()
    {
        Retomar(false);

        if (!string.IsNullOrEmpty(nomeCenaMenu) && Application.CanStreamedLevelBeLoaded(nomeCenaMenu))
        {
            SceneManager.LoadScene(nomeCenaMenu);
            return;
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void DefinirVolume(float valor)
    {
        AudioListener.volume = valor;
        PlayerPrefs.SetFloat(ChaveVolume, valor);
    }

    public void DefinirTelaCheia(bool ativa)
    {
        AplicarTelaCheia(ativa);
        PlayerPrefs.SetInt(ChaveTelaCheia, ativa ? 1 : 0);
    }

    private void Retomar(bool continuarJogo)
    {
        if (!Pausado) return;

        Pausado = false;
        painelPause.SetActive(false);
        AudioListener.pause = false;

        if (continuarJogo)
        {
            Cursor.lockState = travaCursorAntes;
            Cursor.visible = cursorVisivelAntes;
        }

        Time.timeScale = (continuarJogo && escalaAnterior > 0f) ? escalaAnterior : 1f;
        PlayerPrefs.Save();
    }

    private void MostrarBotoes()
    {
        painelOpcoes.SetActive(false);
        painelBotoes.SetActive(true);
        Selecionar(primeiroBotao);
    }

    private void AplicarTelaCheia(bool ativa)
    {
        Screen.fullScreenMode = ativa ? FullScreenMode.FullScreenWindow
                                      : FullScreenMode.Windowed;
    }

    private void Selecionar(GameObject alvo)
    {
        if (EventSystem.current == null || alvo == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(alvo);
    }
}