using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    Transform cam;
    Vector3 camStartPos;
    float distance;

    Material[] mat;
    float[] backSpeed;

    float farthestBack;

    [Range(0.01f, 0.5f)]
    public float parallaxSpeed;

    void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;

        int backCount = transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];

        for(int i = 0; i < backCount; i++)
        {
            mat[i] = transform.GetChild(i).GetComponent<Renderer>().material;
        }

        BackSpeedCalculate(backCount);
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

        for(int i = 0; i < transform.childCount; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;
            mat[i].SetTextureOffset("_MainTex", new Vector2(distance, 0) * speed);
        }
    }
}
