using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
[ExecuteInEditMode]
[DisallowMultipleComponent]
public class InvertedMaskTarget : MonoBehaviour, IMaterialModifier
{
    [Tooltip("Must match the Stencil ID of the InvertedMaskZone that cuts this graphic.")]
    [Range(1, 255)]
    [SerializeField] private int _stencilID = 1;

    private Material _modifiedMaterial;

    private void OnEnable()
    {
        var graphic = GetComponent<Graphic>();
        if (graphic != null)
            graphic.SetMaterialDirty();
    }

    private void OnDisable()
    {
        CleanupMaterial();
        var graphic = GetComponent<Graphic>();
        if (graphic != null)
            graphic.SetMaterialDirty();
    }

    private void OnValidate()
    {
        var graphic = GetComponent<Graphic>();
        if (graphic != null)
            graphic.SetMaterialDirty();
    }

    public Material GetModifiedMaterial(Material baseMaterial)
    {
        if (!isActiveAndEnabled)
            return baseMaterial;

        CleanupMaterial();

        _modifiedMaterial = new Material(baseMaterial);
        _modifiedMaterial.name = $"InvertedMaskTarget (ID:{_stencilID})";

        // Only render where stencil does NOT equal our ID
        _modifiedMaterial.SetInt("_Stencil", _stencilID);
        _modifiedMaterial.SetInt("_StencilOp", (int)StencilOp.Keep);
        _modifiedMaterial.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
        _modifiedMaterial.SetInt("_StencilReadMask", 255);
        _modifiedMaterial.SetInt("_StencilWriteMask", 0);

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
