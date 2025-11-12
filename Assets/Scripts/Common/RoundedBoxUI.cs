using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoundedBoxUI : UIBehaviour, IMeshModifier
{
    private Image _image;
    public Vector4 borderRadius;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (_image == null)
        {
            if (!TryGetComponent(out _image)) return;
        }
        _image.SetAllDirty();
    }
#endif

    protected override void OnEnable()
    {
        _image = GetComponent<Image>();
    }

    protected override void OnDisable()
    {
        _image = null;
    }

    protected override void Start()
    {
        StartCoroutine(DelayVertexGeneration());
    }

    IEnumerator DelayVertexGeneration()
    {
        yield return new WaitForSeconds(0.1f);
        if (_image == null)
        {
            if (!TryGetComponent(out _image)) yield break;
        }
        _image.SetAllDirty();
    }

    public void ModifyMesh(Mesh mesh)
    {
    }

    public void ModifyMesh(VertexHelper verts)
    {
        if (_image == null)
        {
            if (!TryGetComponent(out _image)) return;
        }

        var rectTransform = (RectTransform)transform;
        var rect = rectTransform.rect;
        var offset = new Vector4(rect.x, rect.y, Mathf.Abs(rect.width), Mathf.Abs(rect.height));
        UIVertex vert = new();

        for (int i = 0; i < verts.currentVertCount; i++)
        {
            verts.PopulateUIVertex(ref vert, i);
            var uv0 = vert.uv0;
            uv0.z = offset.z;
            uv0.w = offset.w;
            vert.uv0 = uv0;
            vert.uv1 = borderRadius * 0.5f;
            verts.SetUIVertex(vert, i);
        }
    }
}
