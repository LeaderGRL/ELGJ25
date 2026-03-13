using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


[RequireComponent(typeof(Graphic))]
[ExecuteInEditMode]
[DisallowMultipleComponent]
public class InvertedMaskZone : MonoBehaviour, IMaterialModifier
{
    [Tooltip("Stencil ID for this mask. Targets with the same ID will be cut.")]
    [Range(1, 255)]
    [SerializeField] private int _stencilID = 1;

    [Tooltip("Show the mask shape for debugging.")]
    [SerializeField] private bool _showMaskGraphic = false;

    private Material _modifiedMaterial;
    private Graphic _graphic;

    public int StencilID => _stencilID;

    private void OnEnable()
    {
        _graphic = GetComponent<Graphic>();
        UpdateVisibility();
        _graphic.SetMaterialDirty();
    }

    private void OnDisable()
    {
        CleanupMaterial();
        if (_graphic != null)
            _graphic.SetMaterialDirty();
    }

    private void OnValidate()
    {
        if (_graphic != null)
        {
            UpdateVisibility();
            _graphic.SetMaterialDirty();
        }
    }

    private void UpdateVisibility()
    {
        if (_graphic == null) return;

        if (!_showMaskGraphic)
        {
            var c = _graphic.color;
            c.a = 0f;
            _graphic.color = c;
        }
    }

    public Material GetModifiedMaterial(Material baseMaterial)
    {
        if (!isActiveAndEnabled)
            return baseMaterial;

        CleanupMaterial();

        _modifiedMaterial = new Material(baseMaterial);
        _modifiedMaterial.name = $"InvertedMaskZone (ID:{_stencilID})";

        // Write our stencil ID wherever this Image covers
        _modifiedMaterial.SetInt("_Stencil", _stencilID);
        _modifiedMaterial.SetInt("_StencilOp", (int)StencilOp.Replace);
        _modifiedMaterial.SetInt("_StencilComp", (int)CompareFunction.Always);
        _modifiedMaterial.SetInt("_StencilReadMask", 255);
        _modifiedMaterial.SetInt("_StencilWriteMask", 255);

        // Don't render visible pixels — stencil write only
        if (!_showMaskGraphic)
            _modifiedMaterial.SetInt("_ColorMask", 0);

        return _modifiedMaterial;
    }

    private void CleanupMaterial()
    {
        if (_modifiedMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(_modifiedMaterial);
            else
                DestroyImmediate(_modifiedMaterial);
            _modifiedMaterial = null;
        }
    }

    private void OnDestroy()
    {
        CleanupMaterial();
    }
}