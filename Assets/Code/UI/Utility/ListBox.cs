using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ListBox : MonoBehaviour
{
    public enum BoxType
    {
        horizontal,
        vertical
    }
    public enum BoxAlign
    {
        Center,
        Start,
        End
    }

    public BoxType boxType;
    public Vector2 padding;
    public Vector2 alignment;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        SetChildTransforms();
    }

    private void OnValidate()
    {
        SetChildTransforms();
    }

    public void SetChildTransforms()
    {
        Vector3 offset = Vector3.zero;
        Vector3 finalOffset = Vector3.zero;
        bool first = true;
        foreach(RectTransform t in transform.GetComponentsInChildren<RectTransform>())
        {
            if (t.parent == transform)
            {
                t.localPosition = offset;
                switch (boxType)
                {
                    case BoxType.horizontal: offset += Vector3.right * (t.sizeDelta.x + padding.x); break;
                    case BoxType.vertical: offset -= Vector3.up * (t.sizeDelta.y + padding.y); break;
                }
                if(!first)
                {
                    switch (boxType)
                    {
                        case BoxType.horizontal: finalOffset += Vector3.right * (t.sizeDelta.x + padding.x); break;
                        case BoxType.vertical: finalOffset -= Vector3.up * (t.sizeDelta.y + padding.y); break;
                    }
                }
                first = false;
            }
        }
        foreach (RectTransform t in transform.GetComponentsInChildren<RectTransform>())
        {
            if (t.parent == transform)
            {
                switch (boxType)
                {
                    case BoxType.horizontal: t.localPosition -= finalOffset * alignment.x; break;
                    case BoxType.vertical: t.localPosition -= finalOffset * alignment.y; break;
                }
            }
        }
    }
}
