using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class CornerToCornerGradientUI : BaseMeshEffect
{
    [Header("Gradient Colors")]
    public Color colorA = Color.white;
    public Color colorB = Color.blue;

    [Header("Animation")]
    [Min(0.01f)] public float speed = 1f;
    public bool pingPong = true;
    public bool playOnStart = true;

    [Header("Direction")]
    public GradientDirection startDirection = GradientDirection.BottomLeftToTopRight;

    private Graphic targetGraphic;
    private float progress;
    private bool isPlaying;

    public enum GradientDirection
    {
        BottomLeftToTopRight,
        TopLeftToBottomRight,
        TopRightToBottomLeft,
        BottomRightToTopLeft
    }

    protected override void Awake()
    {
        base.Awake();
        targetGraphic = GetComponent<Graphic>();
        isPlaying = playOnStart;
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        if (pingPong)
        {
            progress = Mathf.PingPong(Time.unscaledTime * speed, 1f);
        }
        else
        {
            progress = (Time.unscaledTime * speed) % 1f;
        }

        if (targetGraphic != null)
            targetGraphic.SetVerticesDirty();
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0)
            return;

        UIVertex vertex = new UIVertex();

        Vector2 min = Vector2.zero;
        Vector2 max = Vector2.zero;
        bool initialized = false;

        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            Vector2 pos = vertex.position;

            if (!initialized)
            {
                min = pos;
                max = pos;
                initialized = true;
            }
            else
            {
                min = Vector2.Min(min, pos);
                max = Vector2.Max(max, pos);
            }
        }

        Vector2 size = max - min;

        if (Mathf.Approximately(size.x, 0f) || Mathf.Approximately(size.y, 0f))
            return;

        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            Vector2 normalized = new Vector2(
                Mathf.InverseLerp(min.x, max.x, vertex.position.x),
                Mathf.InverseLerp(min.y, max.y, vertex.position.y)
            );

            float t = GetCornerLerp(normalized);

            t = Mathf.Repeat(t + progress, 1f);

            Color gradientColor = Color.Lerp(colorA, colorB, t);
            vertex.color *= gradientColor;

            vh.SetUIVertex(vertex, i);
        }
    }

    private float GetCornerLerp(Vector2 uv)
    {
        switch (startDirection)
        {
            case GradientDirection.BottomLeftToTopRight:
                return (uv.x + uv.y) * 0.5f;

            case GradientDirection.TopLeftToBottomRight:
                return (uv.x + (1f - uv.y)) * 0.5f;

            case GradientDirection.TopRightToBottomLeft:
                return ((1f - uv.x) + (1f - uv.y)) * 0.5f;

            case GradientDirection.BottomRightToTopLeft:
                return ((1f - uv.x) + uv.y) * 0.5f;

            default:
                return (uv.x + uv.y) * 0.5f;
        }
    }

    public void Play()
    {
        isPlaying = true;
    }

    public void Stop()
    {
        isPlaying = false;
    }
}