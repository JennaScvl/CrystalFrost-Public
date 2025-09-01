using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public Color highlightColor = Color.yellow;

    // Hover state
    private Transform _hoveredTransform;
    private Material _hoveredMaterial;
    private Color _originalHoverEmissionColor;
    private bool _isHovering = false;

    // Selection state
    private Transform _selectedTransform;
    private Material _selectedMaterial;
    private Color _originalSelectedEmissionColor;

    void Update()
    {
        HandleHighlighting();
        HandleSelection();
    }

    private void HandleHighlighting()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // We are hitting something. Check if it's a prim.
            var primInfo = hit.transform.GetComponent<PrimInfo>();
            if (primInfo != null)
            {
                // It's a prim. Are we already hovering over it?
                if (_hoveredTransform != hit.transform)
                {
                    // It's a new prim, clear the old hover highlight and apply a new one.
                    ClearHoverHighlight();
                    SetHoverHighlight(hit.transform);
                }
                // If it's the same prim, do nothing, the highlight is already on.
                return;
            }
        }

        // If we reach here, the raycast either hit nothing or hit a non-prim object.
        // In either case, we should clear the current hover highlight.
        ClearHoverHighlight();
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // If we are hovering over an object, select it.
            if (_hoveredTransform != null)
            {
                // Clear the highlight from the previously selected object.
                ClearSelectionHighlight();

                // Set the new selection.
                _selectedTransform = _hoveredTransform;
                _selectedMaterial = _hoveredMaterial;
                _originalSelectedEmissionColor = _originalHoverEmissionColor;

                // The object is already highlighted from hover, so we just need to "lock" it.
                // The ClearHoverHighlight method will now ignore this object.
            }
            else
            {
                // We clicked on empty space, so deselect the current object.
                ClearSelectionHighlight();
            }
        }
    }

    private void SetHoverHighlight(Transform target)
    {
        var renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            _hoveredTransform = target;
            _hoveredMaterial = renderer.material;
            _originalHoverEmissionColor = _hoveredMaterial.GetColor("_EmissionColor");
            _hoveredMaterial.EnableKeyword("_EMISSION");
            _hoveredMaterial.SetColor("_EmissionColor", highlightColor);
            _isHovering = true;
        }
    }

    private void ClearHoverHighlight()
    {
        if (_isHovering && _hoveredMaterial != null)
        {
            // Don't clear the highlight if this object is the currently selected one.
            if (_hoveredTransform == _selectedTransform)
            {
                _hoveredTransform = null;
                return;
            }

            _hoveredMaterial.SetColor("_EmissionColor", _originalHoverEmissionColor);
            if (_originalHoverEmissionColor == Color.black)
            {
                 _hoveredMaterial.DisableKeyword("_EMISSION");
            }
        }
        _hoveredTransform = null;
        _hoveredMaterial = null;
        _isHovering = false;
    }

    private void ClearSelectionHighlight()
    {
        if (_selectedTransform != null && _selectedMaterial != null)
        {
            _selectedMaterial.SetColor("_EmissionColor", _originalSelectedEmissionColor);
            if (_originalSelectedEmissionColor == Color.black)
            {
                _selectedMaterial.DisableKeyword("_EMISSION");
            }
        }
        _selectedTransform = null;
        _selectedMaterial = null;
    }
}
