using UnityEngine;
using StarterAssets;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Cameras")]
    public GameObject tpCamera;
    public GameObject fpCamera;
    public bool isFirstPerson;
    private StarterAssetsInputs _input;
    private ThirdPersonController _movement;
    private PlayerControlState _state;
    [SerializeField] private Renderer[] _firstPersonHiddenRenderers;
    private UnityEngine.Rendering.ShadowCastingMode[] _shadowModes;
    private void Awake()
    {
        _input = GetComponent<StarterAssetsInputs>();
        _movement = GetComponent<ThirdPersonController>();
        _state = GetComponent<PlayerControlState>();
        _shadowModes = new UnityEngine.Rendering.ShadowCastingMode[_firstPersonHiddenRenderers.Length];
        for (int i = 0; i < _shadowModes.Length; i++) if (_firstPersonHiddenRenderers[i] != null) _shadowModes[i] = _firstPersonHiddenRenderers[i].shadowCastingMode;
    }
    private void Start() => SetCameraMode(isFirstPerson);
    private void Update()
    {
        if (_input == null || !_input.toggleCamera) return;
        _input.toggleCamera = false;
        if (_state != null && !_state.GameplayAllowed) return;
        SetCameraMode(!isFirstPerson);
    }
    public void SetCameraMode(bool firstPerson)
    {
        if (tpCamera == null || fpCamera == null) return;
        isFirstPerson = firstPerson;
        tpCamera.SetActive(!firstPerson);
        fpCamera.SetActive(firstPerson);
        if (_movement != null) _movement.FirstPerson = firstPerson;
        for (int i = 0; i < _shadowModes.Length; i++)
            if (_firstPersonHiddenRenderers[i] != null) _firstPersonHiddenRenderers[i].shadowCastingMode = firstPerson ? UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly : _shadowModes[i];
    }
    private void OnDisable()
    {
        if (_shadowModes == null) return;
        for (int i = 0; i < _shadowModes.Length; i++) if (_firstPersonHiddenRenderers[i] != null) _firstPersonHiddenRenderers[i].shadowCastingMode = _shadowModes[i];
    }
}
