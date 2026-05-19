using UnityEngine;

public class Parallax : MonoBehaviour
{
    Material mat;
    float distance;

    [Range(0.0f, 0.5f)]
    public float speed = 0.2f;

    private static readonly int MainTexID = Shader.PropertyToID("_MainTex");

    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        distance += Time.deltaTime * speed;
        mat.SetTextureOffset(MainTexID, Vector2.right * distance);
    }
}
