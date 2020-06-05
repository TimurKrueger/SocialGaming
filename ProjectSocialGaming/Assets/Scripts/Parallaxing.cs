using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallaxing : MonoBehaviour
{
    public Transform[] backgrounds;

    public Transform[,] background;

    public float[] parallaxScales;

    // How smooth the parallax is going to be. (greater 0)
    public float parallaxingAmount = 1.0f;
    
    private Transform cam;
    
    private Vector3 previousCamPos;

    public float distanceGrowth = 1.0f;
    public float speedMin = 1.0f;
    public float globalSpeedMult = 1.0f;
    public int bckCount = 2;
    public float bckWidth = 30.0f;
    public float minX = -60.0f;

    public int[] indices;

    void Awake()
    {
        cam = Camera.main.transform;
    }

    /*
    private void Start()
    {
        Activate(Random.Range(0,2));
    }*/

    public bool active = false;
    bool isBuilder = false;

    /// <summary>
    /// 0 = Ice
    /// 1 = Earth
    /// 2 = Fire
    /// </summary>
    /// <param name="bckSet"></param>
    public void Activate(int bckSet, bool isBuilder)
    {        
        if(isBuilder)
        {
            bckCount *= 2;
            minX *= 2f;
            this.isBuilder = true;
        }

        var mainInstance = Instantiate(backgrounds[bckSet]);
        mainInstance.transform.position = new Vector3(-5f, 0f, 0f);
        int mi_c = mainInstance.childCount;

        background = new Transform[bckCount, mi_c];

        for (int i = 0; i < mi_c; i++)
        {
            background[0, i] = mainInstance.GetChild(i);
        }

        for(int i = 1; i < bckCount; i++)
        {
            for(int j = 0; j < mi_c; j++)
            {
                background[i, j] = Instantiate(mainInstance.GetChild(j));
                background[i, j].parent = mainInstance;
                background[i, j].localScale = Vector3.one * ((isBuilder) ? 40f : 20f);
                /*if (isBuilder)
                    background[i, j].localScale = new Vector3(background[i, j].localScale.x * 2, background[i, j].localScale.y * 2, background[i, j].localScale.z);*/

                background[i, j].localScale = background[i, j].localScale - new Vector3(0f, 0f, background[i, j].localScale.z-1);

                background[i, j].localPosition = new Vector3(bckWidth * i * ((isBuilder) ? 4f : 2f), 0f, background[0, j].localPosition.z);
            }
        }
        
        parallaxScales = new float[background.GetLength(1)];

        for(int i = 0; i < parallaxScales.Length; i++)
        {
            parallaxScales[i] = background[0, i].position.z*-1.0f * distanceGrowth;
        }

        indices = new int[mi_c];
        for(int i = 0; i < mi_c; i++)
        {
            indices[i] = bckCount - 1;
        }

        active = true;
    }


    void Update()
    {
        if (!active) return;

        float dTime = Time.deltaTime;
        int bckDepth = background.GetLength(1);

        for (int i = 0; i < bckCount; i++)
        {
            for(int j = 0; j < bckDepth; j++)
            {
                //if (j != bckDepth - 1) continue;

                if(background[i, j].localPosition.x <= minX)
                {
                    Vector3 temp = background[i, j].localPosition;
                    background[i,j].localPosition = background[indices[j], j].localPosition + new Vector3(bckWidth * ((isBuilder) ? 4f : 2f)-5f, 0f, 0f);

                    indices[j]++;
                    indices[j] %= bckCount;

                    Debug.Log("Background[" + i + "," + j + "] (" + background[i, j].name + ") teleported from " + temp + " to " + background[i, j].localPosition);
                    continue;
                }

                float parallax = (MainGameManager.mgm == null) ? parallaxScales[j] * 0.01f : MainGameManager.mgm.worldSpeed * parallaxScales[j];
                parallax *= globalSpeedMult;

                float backgroundTargetPosX = background[i,j].position.x + parallax;
                Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, background[i,j].position.y, background[i,j].position.z);
                                
                background[i, j].position = Vector3.Lerp(background[i, j].position, backgroundTargetPos, parallaxingAmount * dTime);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Debug.DrawLine(new Vector3(minX, 20f, 0f), new Vector3(minX, -20f, 0f), Color.red);
    }

    public void Deactivate()
    {
        active = false;

        foreach (Transform t in background)
        {
            Destroy(t.gameObject);
        }

        parallaxScales = null;
        indices = null;
        background = null;
        isBuilder = false;

    }
}
