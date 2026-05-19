using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    Transform cam;
    Vector3 camStartPos;
    float distance;

    Material[] mat;
    float[] backSpeed;
    int _backCount;

    float farthestBack;

    [Range(0.01f, 0.5f)]
    public float parallaxSpeed;

    private static readonly int MainTexID = Shader.PropertyToID("_MainTex");

    void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;

        _backCount = transform.childCount;
        mat = new Material[_backCount];
        backSpeed = new float[_backCount];

        for(int i = 0; i < _backCount; i++)
        {
            mat[i] = transform.GetChild(i).GetComponent<Renderer>().material;
        }

        BackSpeedCalculate(_backCount);
    }

    void BackSpeedCalculate(int backCount)
    {
        for(int i = 0; i < backCount; i++)
        {
            if ((transform.GetChild(i).position.z - cam.position.z) > farthestBack)
            {
                farthestBack = transform.GetChild(i).position.z - cam.position.z;
            }
        }

        for(int i = 0; i < backCount; i++)
        {
            backSpeed[i] = 1 - (transform.GetChild(i).position.z - cam.position.z) / farthestBack;
        }
    }

    private void LateUpdate()
    {
        distance = cam.position.x - camStartPos.x;

        for(int i = 0; i < _backCount; i++)
        {
            mat[i].SetTextureOffset(MainTexID, Vector2.right * (distance * backSpeed[i] * parallaxSpeed));
        }
    }
}
