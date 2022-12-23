using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.Mathf;
using UnityEngine.UI;
using TMPro;

public class KMeansClustering : MonoBehaviour
{

    [SerializeField] GameObject pointPrefab;
    [SerializeField] GameObject massCenterPrefab;
    List<GameObject> massCenters = new List<GameObject>(); 
    [SerializeField] int num = 100;
    bool isStepEnd = false;
    int iteration = 0;

    [SerializeField] Vector3[] originalMean;
    Vector3[] mean;

    [SerializeField] Material  defaultMat;
    List<Material> mat = new List<Material>();

    List<GameObject> points = new List<GameObject>();
    [SerializeField,Range(2,10)] int k = 3;


    [SerializeField] Button button, reset, randomBtn, stepBtn;
    [SerializeField] Slider k_slider, pointsSlider;
    [SerializeField] TextMeshProUGUI kText, numText, endText, iterText;
    [SerializeField] Transform mcParent;
    [SerializeField] GameObject mcPosItem;
    [SerializeField] List<GameObject> textMeshPros = new List<GameObject>();


    private void Awake()
    {
        mean = new Vector3[originalMean.Length];
       

        for (int i = 0; i < originalMean.Length; i++)
        {
            mean[i] = originalMean[i];

            GameObject mcp = Instantiate(mcPosItem, mcParent);
            mcp.GetComponent<TextMeshProUGUI>().text = "mc[" + i + "] = " + mean[i];
            textMeshPros.Add(mcp);

            GameObject temp = Instantiate(massCenterPrefab, originalMean[i], Quaternion.identity, transform);
            Material tempMat = new Material(Shader.Find("Unlit/Color"));
            tempMat.color = UnityEngine.Random.ColorHSV(0f,1f,1f,1f,0.5f,1f);
            massCenters.Add(temp);
            mat.Add(tempMat);
        }

        /////////////////////////////////////////////////////////////////////////
        bool done = false;
        int count = 0;
        while (!done)
        {
             count++;
            float W = UnityEngine.Random.Range(0f, 10f);
            float H = UnityEngine.Random.Range(0f, 10f);
            Vector3 pos = new Vector3(W, H, 0f);
            GameObject temp = Instantiate(pointPrefab, pos,Quaternion.identity,transform);
            temp.GetComponent<MeshRenderer>().material = defaultMat;
            points.Add(temp);
            if (count == num)
                done = true;
        }
        //////////////////////////////////////////////////////////////////////
        k_slider.value = originalMean.Length;
        pointsSlider.value = num;
        endText.enabled = false;
        iterText.text = "Iteration : " ;
        numText.text = "Points =" + num.ToString();
        kText.text = "K =" + originalMean.Length.ToString();
    }
    // Start is called before the first frame update
    void Start()
    {
        

        pointsSlider.onValueChanged.AddListener(delegate
        {
            endText.enabled = false;
            isStepEnd = false;
            iteration = 0;
            num =FloorToInt (pointsSlider.value);
            numText.text = "Points =" + num.ToString();
            foreach (var o in points)
            {
                GameObject.DestroyImmediate(o);
            }
            points.Clear();

            bool done = false;
            int count = 0;
            while (!done)
            {
                count++;
                float W = UnityEngine.Random.Range(0f, 10f);
                float H = UnityEngine.Random.Range(0f, 10f);
                Vector3 pos = new Vector3(W, H, 0f);
                GameObject temp = Instantiate(pointPrefab, pos, Quaternion.identity, transform);
                temp.GetComponent<MeshRenderer>().material = defaultMat;
                points.Add(temp);
                if (count == num)
                    done = true;
            }

            iterText.text = "Iteration : ";

        });

        k_slider.onValueChanged.AddListener(delegate
        {
            endText.enabled = false;
            isStepEnd = false;
            iteration = 0;

            int k =FloorToInt( k_slider.value);
            kText.text = "K =" + k.ToString();

            


            originalMean = new Vector3[k];
            mean = new Vector3[originalMean.Length];

            for (int i = 0; i < k; i++)
            {
                float X = UnityEngine.Random.Range(1f, 9f);
                float Y = UnityEngine.Random.Range(1f, 9f);
                originalMean[i] = new Vector3(X, Y, 0);
            }

            foreach (var o in massCenters)
            {
                GameObject.DestroyImmediate(o);
            }
            massCenters.Clear();

            foreach (var o in textMeshPros)
            {
                GameObject.DestroyImmediate(o);
            }
            textMeshPros.Clear();


            for (int i = 0; i < originalMean.Length; i++)
            {
                mean[i] = originalMean[i];

                GameObject mcp = Instantiate(mcPosItem, mcParent);
                mcp.GetComponent<TextMeshProUGUI>().text = "mc[" + i + "] = " + mean[i];
                textMeshPros.Add(mcp);

                GameObject temp = Instantiate(massCenterPrefab, originalMean[i], Quaternion.identity, transform);
                Material tempMat = new Material(Shader.Find("Unlit/Color"));
                tempMat.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.6f, 1f);
                massCenters.Add(temp);
                mat.Add(tempMat);
            }

            iterText.text = "Iteration : ";

        });

        randomBtn.onClick.AddListener(delegate
        {
            endText.enabled = false;
            isStepEnd = false;
            iteration = 0;

            foreach (var o in points)
            {
                GameObject.DestroyImmediate(o);
            }
            points.Clear();

            bool done = false;
            int count = 0;
            while (!done)
            {
                count++;
                float W = UnityEngine.Random.Range(0f, 10f);
                float H = UnityEngine.Random.Range(0f, 10f);
                Vector3 pos = new Vector3(W, H, 0f);
                GameObject temp = Instantiate(pointPrefab, pos, Quaternion.identity, transform);
                temp.GetComponent<MeshRenderer>().material = defaultMat;
                points.Add(temp);
                if (count == num)
                    done = true;
            }

            for (int i = 0; i < mean.Length; i++)
            {
                mean[i] = originalMean[i];
                massCenters[i].transform.position = originalMean[i];
            }

            iterText.text = "Iteration : ";

        });



        reset.onClick.AddListener(delegate
        {
            endText.enabled = false; ;
            isStepEnd = false;
            iterText.text = "Iteration : ";
            iteration = 0;

            foreach (var p in points)
            {
                p.GetComponent<MeshRenderer>().material = defaultMat;
            }

            for (int i = 0; i < mean.Length; i++)
            {
                mean[i] = originalMean[i];
                massCenters[i].transform.position = originalMean[i];
                textMeshPros[i].GetComponent<TextMeshProUGUI>().text = "mc[" + i + "] = " + originalMean[i];
            }

        });
        button.onClick.AddListener(delegate
        {
            endText.enabled = false; ;
            isStepEnd = false;

            int iteration=0;
            iteration =K_Mean_Clustering(points, mean, mat.ToArray());
            for (int i = 0; i < mean.Length; i++)
            {
                massCenters[i].transform.position = mean[i];
                textMeshPros[i].GetComponent<TextMeshProUGUI>().text = "mc[" + i + "] = " + mean[i];
            }

            iterText.text = "Iteration : " + iteration.ToString();

        });
        stepBtn.onClick.AddListener(delegate
        {
            
            int[] identification = new int[points.Count];
            Vector3[] lastMean = new Vector3[mean.Length];

            if (!isStepEnd)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    identification[i] = -1;
                }
                iteration = K_Mean_Clustering_Step(points, mean, lastMean, identification, mat.ToArray(), iteration,out isStepEnd);
                //Debug.Log("iteration ==" + iteration);
                iterText.text = "Iteration : " + iteration.ToString();

                for (int i = 0; i < mean.Length; i++)
                {
                    massCenters[i].transform.position = mean[i];
                    textMeshPros[i].GetComponent<TextMeshProUGUI>().text = "mc[" + i + "] = " + mean[i];
                }
            }
            else
            {
                endText.enabled =true;
                endText.text = "K_Mean_Clustering is Over";
            }

            

        });
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3(5f, 5f, 0f), new Vector3(10f, 10f, 0));
    }


    int K_Mean_Clustering(List<GameObject> spheres,Vector3[] mean, Material[] mat)
    {

        int iteration = 0;
        int[] identification = new int[points.Count];
        Vector3[] lastMean = new Vector3[mean.Length];
       

        for (int i = 0; i < points.Count; i++)
        {
            identification[i] = -1;
        }

        bool stop = false;

        while (!stop)
        {
            iteration++;
           

            for (int i = 0; i < mean.Length; i++)
            {
                lastMean[i] = mean[i];
            }

            for (int i = 0; i < spheres.Count; i++)
            {
                float[] dist = new float[mean.Length];

                for (int j = 0; j < mean.Length; j++)
                {
                    dist[j] = Pow(Vector3.Magnitude(spheres[i].transform.position - mean[j]), 2);
                }

                float minDis = Min(dist);

                for (int j = 0; j < mean.Length; j++)
                {
                    if (minDis == dist[j])
                    {
                        identification[i] = j;
                    }
                }
            }


            for (int i = 0; i < spheres.Count; i++)
            {
                for (int j = 0; j < mean.Length; j++)
                {
                    if (identification[i] == j)
                        spheres[i].GetComponent<MeshRenderer>().material = mat[j];
                }
            }




            for (int i = 0; i < mean.Length; i++)
            {
                mean[i] = Vector3.zero;
            }


            for (int j = 0; j < mean.Length; j++)
            {
                int sum = 0;
                for (int i = 0; i < spheres.Count; i++)
                {
                    if (identification[i] == j)
                        sum++;
                }

                for (int i = 0; i < spheres.Count; i++)
                {
                    if (identification[i] == j)
                        mean[j] += spheres[i].transform.position / sum;
                }
            }


            bool[] over = new bool[mean.Length];
            for (int i = 0; i < mean.Length; i++)
            {
                over[i] = (Abs(Vector3.Magnitude(mean[i] - lastMean[i])) < 0.01f) ? true : false;

            }

            if (!Array.Exists(over, (a) => a.Equals(false)))
            {
                stop = true;
                Debug.Log("K_Mean_Clustering is Over");
            }

        }

        //Debug.Log("iteration ==" + iteration);
        return iteration;
    }
    int K_Mean_Clustering_Step(List<GameObject> spheres, Vector3[] mean,Vector3[] lastMean, int[] identification, Material[] mat, int iteration,out bool isEnd)
    {
       
        iteration++;
        isEnd = false;

        for (int i = 0; i < mean.Length; i++)
        {
            lastMean[i] = mean[i];
        }

        for (int i = 0; i < spheres.Count; i++)
        {
            float[] dist = new float[mean.Length];

            for (int j = 0; j < mean.Length; j++)
            {
                dist[j] = Pow(Vector3.Magnitude(spheres[i].transform.position - mean[j]), 2);
            }

            float minDis = Min(dist);

            for (int j = 0; j < mean.Length; j++)
            {
                if (minDis == dist[j])
                {
                    identification[i] = j;
                }
            }
        }

        for (int i = 0; i < spheres.Count; i++)
        {
            for (int j = 0; j < mean.Length; j++)
            {
                if (identification[i] == j)
                    spheres[i].GetComponent<MeshRenderer>().material = mat[j];
            }
        }

        for (int i = 0; i < mean.Length; i++)
        {
            mean[i] = Vector3.zero;
        }


        for (int j = 0; j < mean.Length; j++)
        {
            int sum = 0;
            for (int i = 0; i < spheres.Count; i++)
            {
                if (identification[i] == j)
                    sum++;
            }

            for (int i = 0; i < spheres.Count; i++)
            {
                if (identification[i] == j)
                    mean[j] += spheres[i].transform.position / sum;
            }
        }

        bool[] over = new bool[mean.Length];
        for (int i = 0; i < mean.Length; i++)
        {
            over[i] = (Abs(Vector3.Magnitude(mean[i] - lastMean[i])) < 0.01f) ? true : false;

        }

        if (!Array.Exists(over, (a) => a.Equals(false)))
        {
            Debug.Log("K_Mean_Clustering is Over");
            isEnd = true;
        }
        return iteration;
    }



}
