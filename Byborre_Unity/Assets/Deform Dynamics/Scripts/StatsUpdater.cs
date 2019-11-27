using UnityEngine;
using UnityEngine.UI;

public class StatsUpdater : MonoBehaviour {

    public Text simTimeText;
    public Text numVertText;
	public Text numTriText;
	public Text numDistanceConstraintsText;
    public Text numBendingConstraintsText;    

    int frameCount = 0;
    double dt = 0.0;
    double updateRate = 4.0;  // 4 updates per sec.

	float simTime = 0.0f;

    DeformManager manager;

    int numVertices;
    int numFaces;

    int numDistanceConstraints;
    int numBendingConstraints;

    private void Start()
    {
        manager = FindObjectOfType<DeformManager>();   

        if(!manager) enabled = false;
    }

    // Update is called once per frame
    void Update ()
    {
        if(!manager) return;

		frameCount++;
		dt += Time.deltaTime;
		if (dt > 1.0 / updateRate)
		{
			simTime = manager.simulationTime;
			frameCount = 0;
			dt -= 1.0 / updateRate;
		}

		manager.GetNumRenderVertices(out numVertices);
        manager.GetNumRenderIndices(out numFaces);

        manager.GetNumDistanceConstraints(out numDistanceConstraints);
        manager.GetNumBendingConstraints(out numBendingConstraints);

        numFaces /= 3;
		

        simTimeText.text = simTime.ToString("F1") + " ms/frame";

		numVertText.text = "" + numVertices;
		numTriText.text = "" + numFaces;

		numDistanceConstraintsText.text = "" + numDistanceConstraints;
        numBendingConstraintsText.text = "" + numBendingConstraints;
	}
}
