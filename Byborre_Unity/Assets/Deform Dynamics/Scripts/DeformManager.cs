using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using DeformDynamics;

[AddComponentMenu("Deform Dynamics/Deform Manager")]
public class DeformManager : MonoBehaviour
{
    /**
     * The number of solver iterations. Increasing the number of iterations	will increase the accuracy of the solver at the expense of performance.
     **/
    [Range(2, 64)]
    public uint solverIterations = 2;

    /**
     * The number of timesteps that should be performed during one frame.
     **/
    [Range(1, 16)]
    public uint timestepsPerFrame = 8;

	/**
     * The amount of time, in seconds, that the simulation is advanced during one timestep.
     **/
	[Range(0.000001f, 0.05f)]
	public float timestep = 0.002f;

	[Space]

    /**
     * The gravity force in the simulation.
     **/
    public Vector3 gravity = new Vector3(0, -9.82f, 0);

    /**
     * The wind force in the simulation.
     **/
    public Vector3 wind;

    /**
     * The amount of damping created by the air.
     **/
    [Range(0, 1)]
    public float airFriction = 0.0005f;

    public int targetFrameRate = -1;

    [Space]

    /**
     * Whether DeformBody objects should be able to collide with themselves and others or not.
     **/
    public bool selfCollisions = true;

    /**
     * This option is only valid if Self collision is enabled. If this option is enabled, 
     * particles which are overlapping at initialization are discarded from the self-collision mechanism.
     **/
    [Tooltip("Disable self-collision for particles intersecting at initialization")]
    public bool ignoreIntersectingParticles = true;

    [Tooltip("Sample the surfaces of all DeformBody objects for better self collision. Currently only works for flat meshes")]
    public bool sampleSurface;

    [Range(0, 100)]
    public float kineticFriction = 0.01f;

    [Range(0, 100)]
    public float staticFriction = 0.02f;

    [Space]

    [HideInInspector]
    public bool isPaused = false;

	[HideInInspector]
	public float simulationTime = 0.0f;

    private bool isDragging = false;

    private Vector3 pickedPos;
    private int pickedIndex;
    private float pickedDistance;
    private Vector3 pickedDelta;

    private bool hasCrashed = false;

	private Stopwatch sw;

	public delegate void PreSimulationStartAction();
    public static event PreSimulationStartAction PreSimulationStarted;

    public delegate void SimulationStartAction();
    public static event SimulationStartAction OnSimulationStarted;

    public delegate void SimulationPreUpdateAction();
    public static event SimulationPreUpdateAction PreSimulationUpdated;

    public delegate void SimulationUpdateAction();
    public static event SimulationUpdateAction OnSimulationUpdated;

	public delegate void SimulationPostUpdateAction();
	public static event SimulationPostUpdateAction PostSimulationUpdated;

	public delegate void SimulationEndedAction();
    public static event SimulationEndedAction OnSimulationEnded;

    public delegate void AddCollidersAction();
    public static event AddCollidersAction OnAddColliders;

    public delegate void SkinAction();
    public static event SkinAction OnSkin;

    public delegate void RecordingEndedAction();
	public static event RecordingEndedAction OnRecordingEnded;

    private void Start()
    {

		sw = new Stopwatch();
        
		DeformPlugin.InitializePlugin();
		if (PreSimulationStarted != null) PreSimulationStarted();
        
        DeformPlugin.SimulationParameters.SetSelfCollision(selfCollisions, ignoreIntersectingParticles, sampleSurface, kineticFriction, staticFriction);
        
		if (OnAddColliders != null) OnAddColliders();
        
        DeformPlugin.SimulationParameters.SetSolverIterations((int)solverIterations);
		DeformPlugin.SimulationParameters.SetTimestepsPerFrame((int)timestepsPerFrame);
		DeformPlugin.SimulationParameters.SetTimestep(timestep);
		DeformPlugin.SimulationParameters.SetGravity(gravity.x, gravity.y, gravity.z);
        DeformPlugin.SimulationParameters.SetWind(wind.x, wind.y, wind.z);
        DeformPlugin.SimulationParameters.SetAirFriction(airFriction);
        
        if (OnSkin != null) OnSkin();

        DeformPlugin.StartSimulation();

		if (OnSimulationStarted != null) OnSimulationStarted();

		Application.targetFrameRate = targetFrameRate;
    }

    // Update is called once per frame
    void LateUpdate ()
	{
		HandleInput();

		if (isPaused || hasCrashed) return;
        UpdateSimulation();
    }

    public void UpdateSimulation()
    {
        if (PreSimulationUpdated != null) PreSimulationUpdated();

        // TODO: Only call these functions when their values have changed
        DeformPlugin.SimulationParameters.SetSolverIterations((int)solverIterations);
        DeformPlugin.SimulationParameters.SetTimestepsPerFrame((int)timestepsPerFrame);
		DeformPlugin.SimulationParameters.SetTimestep(timestep);
		DeformPlugin.SimulationParameters.SetGravity(gravity.x, gravity.y, gravity.z);
        DeformPlugin.SimulationParameters.SetWind(wind.x, wind.y, wind.z);
		DeformPlugin.SimulationParameters.SetAirFriction(airFriction);

		sw.Reset();
		sw.Start();

		DeformPlugin.UpdateSimulation();

		sw.Stop();

		long microseconds = sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));

		simulationTime = (float)microseconds / 1000;
        
		if (OnSimulationUpdated != null) OnSimulationUpdated();
		if (PostSimulationUpdated != null) PostSimulationUpdated();
	}

    private void HandleInput()
    {
        HandlePicking();

		if (Input.GetKeyDown(KeyCode.R))
		{
			Reset();
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}

        if (Input.GetKeyDown(KeyCode.N))
        {
            gravity = Vector3.down * 9.82f;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePaused();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            if (isPaused)
            {
                UpdateSimulation();
            }
        }
    }

    private void HandlePicking()
    {
        if (!isDragging && Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 rayBegin = ray.origin;
            Vector3 rayEnd = ray.origin + (4096 * ray.direction);
            
            bool pickResult = DeformPlugin.Interaction.PickParticle(rayBegin.x, rayBegin.y, rayBegin.z,
                               rayEnd.x, rayEnd.y, rayEnd.z,
                               out pickedIndex, out pickedDistance,
                               out pickedDelta.x, out pickedDelta.y, out pickedDelta.z);

            if (pickResult)
            {
                if (pickedIndex != -1)
                {
                    isDragging = true;
                    DeformPlugin.Object.FixParticle(pickedIndex);
                    DeformPlugin.Interaction.GetParticlePosition(pickedIndex, out pickedPos.x, out pickedPos.y, out pickedPos.z);
                }
            }
        }

        if (Input.GetMouseButtonUp(1) && isDragging)
        {
			DeformPlugin.Interaction.ReleaseParticle(pickedIndex);
            isDragging = false;
        }

        if (isDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 dragPos = ray.origin + (ray.direction * pickedDistance) + pickedDelta;
            DeformPlugin.Interaction.MoveParticleLimited(pickedIndex, dragPos.x, dragPos.y, dragPos.z, 
                                                         pickedPos.x, pickedPos.y, pickedPos.z);
        }
    }

	private void OnApplicationQuit()
	{
		if (OnRecordingEnded != null) OnRecordingEnded();
	}

	private void OnDestroy()
    {		
		if (OnSimulationEnded != null) OnSimulationEnded();

        DeformPlugin.StopSimulation();
		DeformPlugin.ShutdownPlugin();
    }

    /**
     * Resets the whole scene.
     **/
    public void Reset()
    {
        if (Application.isPlaying)
        {
			if (OnSimulationEnded != null) OnSimulationEnded();
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    /**
     * Toggles the simulation between "running" and "paused".
     **/
    public void TogglePaused()
    {
        isPaused = !isPaused;
    }

    private static void CustomLogger(string msg)
    {
		UnityEngine.Debug.Log("<color=#820b40>[C++]: " + msg + "</color>");
    }

    public void GetNumRenderVertices(out int numVertices)
    {
		numVertices = (int) DeformPlugin.GetNumRenderVertices();
    }

    public void GetNumRenderIndices(out int numIndices)
    {
		numIndices = (int) DeformPlugin.GetNumRenderIndices();
    }

    public void GetNumDistanceConstraints(out int numConstraints)
    {
		numConstraints = (int) DeformPlugin.GetNumDistanceConstraints();
    }

    public void GetNumBendingConstraints(out int numConstraints)
    {
		numConstraints = (int) DeformPlugin.GetNumBendingConstraints();
    }

    public void SetDistanceStiffness(float value)
    {
        foreach (DeformBody body in FindObjectsOfType<DeformBody>())
        {
            body.distanceStiffness = value;
			DeformPlugin.Object.SetDistanceStiffness(body.GetId(), value);
        }
    }

    public void SetBendingStiffness(float value)
    {
        foreach (DeformBody body in FindObjectsOfType<DeformBody>())
        {
            body.bendingStiffness = value;
			DeformPlugin.Object.SetBendingStiffness(body.GetId(), value);
        }
    }
}
