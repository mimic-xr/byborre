using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DeformDynamics
{
    public static class DeformPlugin
    {
		private static bool initialized = false;

        #region DLL
        [DllImport("deform")] private static extern void CreateVivaceWrapped();
        [DllImport("deform")] private static extern void DestroyVivaceWrapped();

        [DllImport("deform")] private static extern void Reset();
        [DllImport("deform")] private static extern void Load(string path);
        [DllImport("deform")] private static extern void Init();
        [DllImport("deform")] private static extern void Update();

        /// <summary>
		/// Advances the simulation one frame using curves
		/// </summary>
        [DllImport("deform")] public static extern void UpdateUsingCurve(int curveId, float t);

        [DllImport("deform")] private static extern void Shutdown();

        /// <summary>
		/// Retrieves the total number of render vertices
		/// </summary>
        [DllImport("deform")] public static extern uint GetNumRenderVertices();

        /// <summary>
		/// Retrieves the total number of render indices
		/// </summary>
        [DllImport("deform")] public static extern uint GetNumRenderIndices();

        /// <summary>
		/// Retrieves the total number of distance constraints
		/// </summary>
        [DllImport("deform")] public static extern uint GetNumDistanceConstraints();

        /// <summary>
		/// Retrieves the total number of bending constraints
		/// </summary>
        [DllImport("deform")] public static extern uint GetNumBendingConstraints();
        #endregion

        /// <summary>
        /// Initializes the plugin. 
        /// </summary>
        public static void InitializePlugin()
        {
			if (!initialized)
            {
                CreateVivaceWrapped();
				Reset();
#if UNITY_EDITOR
                Load(Application.dataPath + "/Deform Dynamics/Native/Plugins/deform_config.xml");
#else
                Load(Application.dataPath + "/Plugins/deform_config.xml");
#endif
                initialized = true;
			}
        }

        /// <summary>
		/// Shuts down the physics engine and deallocates all internal data
		/// </summary>
        public static void ShutdownPlugin()
        {
			if (initialized)
			{
				DestroyVivaceWrapped();
				initialized = false;
            }
        }

        /// <summary>
        /// Initializes the physics engine. This should be called after adding objects to the simulation.
        /// </summary>
        public static void StartSimulation()
        {
            Init();
        }

        /// <summary>
		/// Advances the simulation one frame
		/// </summary>
        public static void UpdateSimulation()
        {
            Update();
        }
        
        public static void StopSimulation()
        {
            Shutdown();
        }

        public static class Interaction
        {
#region DLL
            [DllImport("deform")]
            public static extern bool SetPickingEnabled(int objectIndex, bool enabled);

            /// <summary>
            /// Selects the closest particle, on the triangle intersected by the ray, and returns information
            /// needed to generate a proper picking behaviour.</summary>
            /// <param name="rayBeginX">Start point of ray</param>
            /// <param name="rayBeginY">Start point of ray</param>
            /// <param name="rayBeginZ">Start point of ray</param>
            /// <param name="rayEndX">End point of ray</param>
            /// <param name="rayEndY">End point of ray</param>
            /// <param name="rayEndZ">End point of ray</param>
            /// <param name="particleIndex">the index of the particle closest to the ray</param>
            /// <param name="distanceToRayStart">the distance between the start of the ray and the particle</param>
            /// <param name="deltaX">the vector between the start of the ray and the particle</param>
            /// <param name="deltaY">the vector between the start of the ray and the particle</param>
            /// <param name="deltaZ">the vector between the start of the ray and the particle</param>
            /// <returns>
            /// The function returns a bool that indicates if a particle was found.
            /// </returns>
            [DllImport("deform")]
            public static extern bool PickParticle(float rayBeginX, float rayBeginY, float rayBeginZ,
                                                    float rayEndX, float rayEndY, float rayEndZ,
                                                    out int particleIndex, out float distanceToRayStart,
                                                    out float deltaX, out float deltaY, out float deltaZ);

            [DllImport("deform")]
            public static extern bool SetMaximumPullDistance(int objectIndex, float maxDistance);

            [DllImport("deform")]
            public static extern void GetParticlePosition(int particleIndex, out float x, out float y, out float z);

            /// <summary>
            /// Moves the specified particle to a position in world space. The invmass of the particle should be 
            /// 0 in order for the picking to behave properly. This has to be done in a separate step using FixObjectParticleSM.
            /// </summary>
            /// <param name="particleIndex">The global index of the particle that will be moved</param>
            /// <param name="position">The new position of the particle</param>
            /// <returns>
            /// The function returns a bool that indicates if the particle was successfully moved.
            /// </returns>
            [DllImport("deform")]
            public static extern void MoveParticle(int particleIndex, float x, float y, float z);

            [DllImport("deform")]
            public static extern void MoveParticleLimited(int particleIndex, float x, float y, float z,
                                                           float origin_x, float origin_y, float origin_z);

            /// <summary>
            /// Moves the specified particle to a position in world space. The invmass of the particle should be 
            /// 0 in order for the picking to behave properly. This has to be done in a separate step using FixObjectParticleSM.
            /// </summary>
            /// <param name="objectId">The object index</param>
            /// <param name="particleIndex">The local index of the particle that will be moved</param>
            /// <param name="posX">The new position of the particle</param>
            /// <param name="posY">The new position of the particle</param>
            /// <param name="posZ">The new position of the particle</param>
            /// <returns>
            /// The function returns a bool that indicates if the particle was successfully moved.
            /// </returns>
            [DllImport("deform")]
            public static extern void MoveObjectParticle(
                                                         int objectId,
                                                         int particleIndex,
                                                         float posX,
                                                         float posY,
                                                         float posZ);

            [DllImport("deform")]
            public static extern void ReleaseParticle(int particleIndex);

            /// <summary>
            /// Function that finds the closest particles within a radius of a point. Also finds the
            /// delta vector and distances from the point to the particles
            /// </summary>
            /// <param name="posX">The point. The particles closest to this point will be found</param>
            /// <param name="posY">The point. The particles closest to this point will be found</param>
            /// <param name="posZ">The point. The particles closest to this point will be found</param>
            /// <param name="radius">The radius. Only the particles within this radius of the point will be considered</param>
            /// <param name="numPicked">Number of particles within the radius</param>
            /// <param name="objectIds">Object indices of the particles within the radius</param>
            /// <param name="indices">Particle indices of the particles within the radius</param>
            /// <param name="deltas">The delta vectors, for each particle within the radius, to the point</param>
            /// <param name="distances">The distances, for each particle within the radius, to the point</param>
            [DllImport("deform")]
            public static extern bool GetClosestParticles(float posX,
                                                           float posY,
                                                           float posZ,
                                                           float radius,
                                                           out int numPicked,
                                                           IntPtr objectIds,   //int
                                                           IntPtr indices,     //int
                                                           IntPtr deltas,      //vector3
                                                           IntPtr distances);  //float
#endregion
        }

        public static class SimulationParameters
        {
#region DLL
            /// <summary>
            /// Sets the number of iterations the solver make in one timestep.
            /// </summary>
            /// <param name="solverIterations">The number of iterations</param>
            [DllImport("deform")]
            public static extern void SetSolverIterations(int solverIterations);

            /// <summary>
            /// Sets the number of timesteps, i.e. number of times that the physics simulation
            /// should be advanced in one frame
            /// </summary>
            /// <param name="timesteps">The number of timesteps</param>
            [DllImport("deform")]
            public static extern void SetTimestepsPerFrame(int timesteps);

            /// <summary>
			/// Sets the amount of time, in seconds, that should be simulated during one timestep
			/// </summary>
			/// <param name="timestep">Amount of time in seconds</param>
            [DllImport("deform")]
            public static extern void SetTimestep(float timestep);
            
            /// <summary>
            /// Sets the global gravity coefficient of the simulation
            /// </summary>
            /// <param name="gravityX"></param>
            /// <param name="gravityY"></param>
            /// <param name="gravityZ"></param>
            [DllImport("deform")]
            public static extern void SetGravity(float gravityX, float gravityY, float gravityZ);
            
            /// <summary>
            /// Sets the global wind force of the simulation
            /// </summary>
            /// <param name="windX"></param>
            /// <param name="windY"></param>
            /// <param name="windZ"></param>
            [DllImport("deform")]
            public static extern void SetWind(float windX, float windY, float windZ);

            /// <summary>
            /// Sets the air friction of the simulation. Must be a value between 0-1.
            /// </summary>
            /// <param name="airFriction"></param>
            [DllImport("deform")]
            public static extern void SetAirFriction(float airFriction);

            /// <summary>
            /// Sets the self collisions and its parameters
            /// </summary>
            /// <param name="selfCollision">Whether or not the self collisions should be enabled</param>
            /// <param name="ignoreIntersectingParticles">Removes particles which are intersecting at initialization</param>
            /// <param name="sampleSurface">Enables surface sampling</param>
            /// <param name="kineticFrictionCoef">Kinetic friction coefficient</param>
            /// <param name="staticFrictionCoef">Static friction coefficient</param>
            [DllImport("deform")]
            public static extern void SetSelfCollision(
                                                       bool selfCollision,
                                                       bool ignoreIntersectingParticles,
                                                       bool sampleSurface,
                                                       float kineticFrictionCoef,
                                                       float staticFrictionCoef);

#endregion
        }

        public static class Object
        {
#region DLL
            /// <summary>
            /// Sets the stiffness of the distance constraints of the object specified.
            /// </summary>
            /// <param name="objectId">The index of the object</param>
            /// <param name="distanceStiffness">The stiffness coefficient. Must be a value between 0 and 1.</param>            
            [DllImport("deform")]
            public static extern void SetDistanceStiffness(int objectId, float distanceStiffness);

            /// <summary>
            /// Sets the stiffness of the bending constraints of the object specified.
            /// </summary>
            /// <param name="objectId">The index of the object</param>
            /// <param name="bendingStiffness">The stiffness coefficient. Must be a value between 0 and 1.</param> 
            [DllImport("deform")]
            public static extern void SetBendingStiffness(int objectId, float bendingStiffness);

            /// <summary>
            /// Creates a deformable object from the mesh specified and returns the object index of the created object.
            /// </summary>
            /// <param name="vertices">Array of vertices of the mesh</param>
            /// <param name="numVertices">Number of vertices</param>
            /// <param name="indices">Array of indices of the mesh</param>
            /// <param name="numIndices">Number of indices</param>
            /// <param name="objectId">[output] The object index of the created object</param>          
            [DllImport("deform")]
            public static extern void CreateDeformableObject(Vector3[] vertices,
                                                              int numVertices,
                                                              int[] indices,
                                                              int numIndices,
                                                              out int objectId);

            /// <summary>
            /// Deletes the deformable object specified from the simulation.
            /// </summary>
            /// <param name="objectId">The object that should be deleted</param>
            [DllImport("deform")]
            public static extern void DeleteDeformableObject(int objectId);

            [DllImport("deform")]
            public static extern void AddRenderMesh(int objectId, Vector3[] vertices, int numVertices, int[] triangles, int numTriangles);
            
            /// <summary>
            /// Moves the deformable object specified.
            /// </summary>
            /// <param name="objectId">The object that should be moved</param>
            /// <param name="posX">The world position</param>
            /// <param name="posY">The world position</param>
            /// <param name="posZ">The world position</param> 
            [DllImport("deform")]
            public static extern void MoveObject(int objectId, float posX, float posY, float posZ);

            /// <summary>
            /// Rotates the deformable object specified.
            /// </summary>
            /// <param name="objectId">The object that should be moved</param>
            /// <param name="angle">The amount of rotation in radians</param>
            /// <param name="axisX">The angle of rotation</param>
            /// <param name="axisY">The angle of rotation</param>
            /// <param name="axisZ">The angle of rotation</param>  
            [DllImport("deform")]
            public static extern void RotateObject(int objectId, float angle, float axisX, float axisY, float axisZ);

            /// <summary>
            /// Scales the deformable object specified.
            /// </summary>
            /// <param name="objectId">The object that should be moved</param>
            /// <param name="scaleX">The scale factor</param>
            /// <param name="scaleY">The scale factor</param>
            /// <param name="scaleZ">The scale factor</param>
            [DllImport("deform")]
            public static extern void ScaleObject(int objectId, float scaleX, float scaleY, float scaleZ);

            /// <summary>
            /// Function to retrieve vertex positions of the deformable object specified.
            /// </summary>
            /// <param name="objectId">The object index</param>
            /// <param name="renderVertices">The vertex position data</param>  
            [DllImport("deform")]
            public static extern void GetObjectRenderVertices(int objectId, IntPtr renderVertices);

            /// <summary>
            /// Function to retrieve vertex positions in local space of the deformable object specified.
            /// </summary>
            /// <param name="objectId">The object index</param>
            /// <param name="renderVertices">The vertex position data</param>  
            [DllImport("deform")]
            public static extern void GetObjectRenderVerticesLocal(int objectId, IntPtr renderVertices);

            /// <summary>
            /// Function to retrieve vertex normals of the deformable object specified.
            /// </summary>
            /// <param name="objectId">The object index</param>
            /// <param name="renderNormals">The vertex normal data</param>   
            [DllImport("deform")]
            public static extern void GetObjectRenderNormals(int objectId, IntPtr renderNormals);
            
            /// <summary>
            /// Function to retrieve the bounding of the deformable object specified.
            /// </summary>
            /// <param name="objectId">The object index</param>
            /// <param name="minX">The min point of the AABB</param>
            /// <param name="minY">The min point of the AABB</param>
            /// <param name="minZ">The min point of the AABB</param>
            /// <param name="maxX">The max point of the AABB</param>
            /// <param name="maxY">The max point of the AABB</param>
            /// <param name="maxZ">The max point of the AABB</param> 
            [DllImport("deform")]
            public static extern void GetObjectBoundingBox(int objectId,
                                                            out float minX,
                                                            out float minY,
                                                            out float minZ,
                                                            out float maxX,
                                                            out float maxY,
                                                            out float maxZ);

            /// <summary>
            /// Function to retrieve the physics vertex positions (particle positions) of the deformable object specified.
            /// </summary>
            /// <param name="objectId">The object index</param>
            /// <param name="physicsVertices">The vertex position data</param>  
            [DllImport("deform")]
            public static extern void GetObjectPhysicsVertices(int objectId, IntPtr physicsVertices);

            /// <summary>
            /// Retrieves the number of physics vertices of the specified object
            /// </summary>
            /// <param name="objectId">The object index</param>
            /// <param name="numVertices">The number of physics vertices</param>        
            [DllImport("deform")]
            public static extern void GetNumPhysicsVerticesOfObject(int objectId, out int numVertices);

            /// <summary>
            /// Sets the constraint rest values of the object specified.
            /// </summary>
            /// <param name="objectId">The object index</param>
            /// <param name="distanceRestValues">The list of distance constraint rest values</param>
            /// <param name="numDistanceConstraints">The number of distance constraint rest values</param>
            /// <param name="bendingRestValues">The list of bending constraint rest values</param>
            /// <param name="numBendingConstraints">The number of bending constraint rest values</param>  
            [DllImport("deform")]
            public static extern void SetObjectRestValues(int objectId,
                                                           float[] distanceRestValues,
                                                           uint numDistanceConstraints,
                                                           float[] bendingRestValues,
                                                           uint numBendingConstraints);

            /// <summary>
            /// Anchors a deformable object to a collider. The particles of the deformable object that are
            /// inside the collider, will be anchored.
            /// </summary>
            /// <param name="objectId">The object index</param>
            /// <param name="colliderId">The collider index</param>      
            [DllImport("deform")]
            public static extern void AnchorObjectToCollider(int objectId, int colliderId);

            /// <summary>
            /// Reads a tetrahedra mesh (.mesh), and returns the number of vertices and indices.
            /// </summary>
            /// <param name="path">The path to the .mesh file</param>
            /// <param name="numVertices">The number of vertices</param>
            /// <param name="numIndices">The number of indices of the surface triangles</param>  
            [DllImport("deform")]
            public static extern void ReadTetramesh(string path, out int numVertices, out int numIndices);

            /// <summary>
            /// Retrieves the vertex and index arrays of the tetrahedra mesh read by <see cref="ReadTetramesh"/>.
            /// </summary>
            /// <param name="vertices">Array containing all the vertices</param>
            /// <param name="indices">Array containing the index list of the surface triangles</param>      
            [DllImport("deform")]
            public static extern void RetrieveTetramesh(IntPtr vertices, IntPtr indices);

            /// <summary>
            /// Adds a volumetric deformable object to the simulation.
            /// </summary>
            /// <param name="path">Path to the tetrahedra .mesh file</param>
            /// <param name="objectId">The index of this object</param>       
            [DllImport("deform")]
            public static extern void CreateVolumetricDeformableObject(string path, out int objectId);

            /// <summary>
            /// Sets the volume stiffness of a volumetric deformable object.
            /// </summary>
            /// <param name="objectId"></param>
            /// <param name="volumeStiffness"></param>  
            [DllImport("deform")]
            public static extern void SetVolumeStiffness(int objectId, float volumeStiffness);

            /// <summary>
            /// Sets the invmass of the specified particle to 0.
            /// </summary>
            /// <param name="particleIndex">The global index of the particle</param> 
            [DllImport("deform")]
            private static extern void SetInvmass(uint particle_id, float value);

            [DllImport("deform")]
            private static extern void SetObjectParticleInvmass(uint object_id, uint particle_id, float value);

            /// <summary>
            /// Sets the invmass of all particles in the specified object to value.
            /// </summary>
            /// <param name="objectId">The index of the object</param>
            /// <param name="value">The invmass value
            [DllImport("deform")]
            public static extern void SetObjectInvmass(uint object_id, float value);

            /// <summary>
            /// Sets the kinetic friction coefficient of the specified particle
            /// </summary>
            /// <param name="objectId">The index of the object</param>
            /// <param name="particleIndex">The local index of the particle</param>
            /// <param name="kineticFrictionCoef">The kinetic friction coefficient. Must be a value between 0 and 1.</param> 
            [DllImport("deform")]
            public static extern void SetParticleFrictionKinetic(int objectId, int particleIndex, float kineticFrictionCoef);

            /// <summary>
            /// Sets the static friction coefficient of the specified particle
            /// </summary>
            /// <param name="objectId">The index of the object</param>
            /// <param name="particleIndex">The local index of the particle</param>
            /// <param name="staticFrictionCoef">The static friction coefficient. Must be a value between 0 and 5.</param>   
            [DllImport("deform")]
            public static extern void SetParticleFrictionStatic(int objectId, int particleIndex, float staticFrictionCoef);
#endregion
           
            public static void FixParticle(int particleIndex)
            {
                SetInvmass((uint)particleIndex, 0);
            }

            /// <summary>
            /// Sets the invmass of the specified particle to 0.
            /// </summary>
            /// <param name="objectId">The index of the object</param>
            /// <param name="particleIndex">The local index of the particle</param>
            public static void FixObjectParticle(uint object_id, uint particle_id)
            {
                SetObjectParticleInvmass(object_id, particle_id, 0);
            }

            /// <summary>
            /// Sets the invmass of the specified particle to 1.
            /// </summary>
            /// <param name="objectId">The index of the object</param>
            /// <param name="particleIndex">The local index of the particle</param>
            public static void ReleaseObjectParticle(uint object_id, uint particle_id)
            {
                SetObjectParticleInvmass(object_id, particle_id, 1);
            }
        }

        public static class Collider
        {
#region DLL

            /// <summary>
            /// Translates a collider in space.
            /// </summary>
            /// <param name="id">The index of the collider</param>
            /// <param name="posX">x-position</param>
            /// <param name="posY">y-position</param>
            /// <param name="posZ">z-position</param>   
            [DllImport("deform")]
            public static extern void MoveCollider(int id, float posX, float posY, float posZ);

            /// <summary>
            /// Rotates a collider in space.
            /// </summary>
            /// <param name="colliderId">The index of the collider</param>
            /// <param name="angle">The angle in radians</param>
            /// <param name="axisX">Rotation axis</param>
            /// <param name="axisY">Rotation axis</param>
            /// <param name="axisZ">Rotation axis</param>
            /// <param name="centerX">Center of rotation</param>
            /// <param name="centerY">Center of rotation</param>
            /// <param name="centerZ">Center of rotation</param> 
            [DllImport("deform")]
            public static extern void RotateCollider(int colliderId,
                                                     float angle,
                                                     float axisX,
                                                     float axisY,
                                                     float axisZ,
                                                     float centerX,
                                                     float centerY,
                                                     float centerZ);

            /// <summary>
            /// Adds a plane collider to the simulation.
            /// </summary>
            /// <param name="normalX">The normal of the plane</param>
            /// <param name="normalY">The normal of the plane</param>
            /// <param name="normalZ">The normal of the plane</param>
            /// <param name="kineticFrictionCoef">Kinetic friction coefficient</param>
            /// <param name="staticFrictionCoef">Static friction coefficient</param>
            /// <param name="colliderId">The index of the collider</param>   
            [DllImport("deform")]
            public static extern void CreatePlaneCollider(float normalX, float normalY, float normalZ,
                                                          float kineticFrictionCoef,
                                                          float staticFrictionCoef,
                                                          out int colliderId);

            /// <summary>
            /// Adds a sphere collider to the simulation.
            /// </summary>
            /// <param name="radius">The radius of the sphere</param>
            /// <param name="kineticFrictionCoef">Kinetic friction coefficient</param>
            /// <param name="staticFrictionCoef">Static friction coefficient</param>
            /// <param name="colliderId">The index of the collider</param> 
            [DllImport("deform")]
            public static extern void CreateSphereCollider(float radius,
                                                           float kineticFrictionCoef,
                                                           float staticFrictionCoef,
                                                           out int colliderId);

            /// <summary>
            /// Adds a box collider to the simulation.
            /// </summary>
            /// <param name="length">Box dimensions</param>
            /// <param name="height">Box dimensions</param>
            /// <param name="depth">Box dimensions</param>
            /// <param name="kineticFrictionCoef">Kinetic friction coefficient</param>
            /// <param name="staticFrictionCoef">Static friction coefficient</param>
            /// <param name="colliderId">The index of the collider</param>       
            [DllImport("deform")]
            public static extern void CreateObjectOrientedBoxCollider(float length,
                                                                      float height,
                                                                      float depth,
                                                                      float kineticFrictionCoef,
                                                                      float staticFrictionCoef,
                                                                      out int colliderId);

            /// <summary>
            /// Adds a capsule collider to the simulation.
            /// </summary>
            /// <param name="radiusA">The radius of the first end of the capsule</param>
            /// <param name="radiusB">The radius of the second end of the capsule</param>
            /// <param name="xA">Position of the first end point of the capsule</param>
            /// <param name="yA">Position of the first end point of the capsule</param>
            /// <param name="zA">Position of the first end point of the capsule</param>
            /// <param name="xB">Position of the second end point of the capsule</param>
            /// <param name="yB">Position of the second end point of the capsule</param>
            /// <param name="zB">Position of the second end point of the capsule</param>
            /// <param name="kineticFrictionCoef">Kinetic friction coefficient</param>
            /// <param name="staticFrictionCoef">Static friction coefficient</param>
            /// <param name="colliderId">The index of the collider</param>   
            [DllImport("deform")]
            public static extern void CreateCapsuleCollider(float radiusA, float radiusB,
                                                            float xA, float yA, float zA,
                                                            float xB, float yB, float zB,
                                                            float kineticFrictionCoef,
                                                            float staticFrictionCoef,
                                                            out int colliderId);

            /// <summary>
            /// Updates a capsule collider.
            /// </summary>
            /// <param name="radiusA">The radius of the first end of the capsule</param>
            /// <param name="radiusB">The radius of the second end of the capsule</param>
            /// <param name="xA">Position of the first end point of the capsule</param>
            /// <param name="yA">Position of the first end point of the capsule</param>
            /// <param name="zA">Position of the first end point of the capsule</param>
            /// <param name="xB">Position of the second end point of the capsule</param>
            /// <param name="yB">Position of the second end point of the capsule</param>
            /// <param name="zB">Position of the second end point of the capsule</param> 
            [DllImport("deform")]
            public static extern void UpdateCapsuleCollider(int colliderId,
                                                            float radiusA, float radiusB,
                                                            float xA, float yA, float zA,
                                                            float xB, float yB, float zB);

            /// <summary>
            /// Adds a mesh collider to the simulation.
            /// </summary>
            /// <param name="vertices">Array of vertices</param>
            /// <param name="normals">Array of normals</param>
            /// <param name="numVertices">Number of vertices</param>
            /// <param name="indices">Array of indices</param>
            /// <param name="numIndices">Number of indices</param>
            /// <param name="bias">Amount of displacement in the direction of the normal. Use this to prevent z-fighting.</param>
            /// <param name="kineticFrictionCoef">Kinetic friction coefficient</param>
            /// <param name="staticFrictionCoef">Static friction coefficient</param>
            /// <param name="colliderId">The index of the collider</param>   
            [DllImport("deform")]
            public static extern void CreateMeshCollider(Vector3[] vertices,
                                                         Vector3[] normals,
                                                         int numVertices,
                                                         int[] indices,
                                                         int numIndices,
                                                         float bias,
                                                         float kineticFrictionCoef,
                                                         float staticFrictionCoef,
                                                         out int colliderId);

            /// <summary>
            /// Adds a mesh collider where the mesh has been optimized to provide the best performance.
            /// The remeshed mesh will have approximately the same number of vertices specified in the parameters
            /// a long with triangles of approximately the same size. This can be used to achieve better
            /// performance when having a complex mesh with many triangles or triangles of varying size.
            /// </summary>
            /// <param name="vertices">Array of vertices</param>
            /// <param name="normals">Array of normals</param>
            /// <param name="numVertices">Number of vertices</param>
            /// <param name="indices">Array of indices</param>
            /// <param name="numIndices">Number of indices</param>
            /// <param name="numRemeshedVertices">The desired number of vertices</param>
            /// <param name="bias">Amount of displacement in the direction of the normal. Use this to prevent z-fighting.</param>
            /// <param name="kineticFrictionCoef">Kinetic friction coefficient</param>
            /// <param name="staticFrictionCoef">Static friction coefficient</param>
            /// <param name="colliderId">The index of the collider</param> 
            [DllImport("deform")]
            public static extern void CreateMeshColliderRemeshed(Vector3[] vertices,
                                                                 Vector3[] normals,
                                                                 int numVertices,
                                                                 int[] indices,
                                                                 int numIndices,
                                                                 int numRemeshedVertices,
                                                                 float bias,
                                                                 float kineticFrictionCoef,
                                                                 float staticFrictionCoef,
                                                                 out int colliderId);

            /// <summary>
            /// Updates the vertices of the mesh collider. Can be used for dynamically changing meshes, such as 
            /// characters.
            /// </summary>
            /// <param name="colliderId">Index of the mesh collider</param>
            /// <param name="vertices">Array of vertices</param>
            /// <param name="normals">Array of normals</param>
            /// <param name="numVertices">Number of vertices</param>
            [DllImport("deform")]
            public static extern void UpdateMeshCollider(int colliderId, IntPtr vertices, IntPtr normals, float bias);

            [DllImport("deform")]
            public static extern void SetMeshColliderTransform(int colliderId, float t_x, float t_y, float t_z,
                                                               float r_x, float r_y, float r_z, float r_w,
                                                               float r_center_x, float r_center_y, float r_center_z,
                                                               float s_x, float s_y, float s_z);

            /// <summary>
            /// Makes a collider sticky. All cloth vertices touching with the collider will be glued to it.
            /// </summary>
            /// <param name="colliderId">The index of the collider</param>
            /// <param name="sticky">Whether or not the collider should be sticky</param>   
            [DllImport("deform")]
            public static extern void SetColliderSticky(int colliderId, bool sticky);

            /// <summary>
            /// Enables/disables the collider specified.
            /// </summary>
            [DllImport("deform")]
            public static extern void SetColliderEnabled(int colliderId, bool enabled);

            [DllImport("deform")]
            public static extern void SkinToMeshCollider(int objectId, int[] skinnedVertices, int numVertices, int colliderId);

            [DllImport("deform")]
            public static extern void SetGlobalSkinningEnabled(int objectId, bool enabled);
            
            [DllImport("deform")]
            public static extern void SetSelfCollisionMask(int objectId, int[] mask);

            [DllImport("deform")]
            public static extern void SetExternalCollisionMask(int objectId, int[] mask);

            [DllImport("deform")]
            public static extern void SetMeshColliderMask(int colliderId, int[] mask);
#endregion

            /// <summary>
            /// Updates the transform of the mesh collider. Can be used for dynamically changing meshes, such as 
            /// characters.
            /// </summary>           
            public static void SetMeshColliderTransform(int colliderId, Vector3 translation, 
                Quaternion rotation, Vector3 rotationCenter,
                Vector3 scale)
            {
                SetMeshColliderTransform(colliderId, translation.x, translation.y, translation.z,
                    rotation.x, rotation.y, rotation.z, rotation.w, rotationCenter.x, rotationCenter.y, rotationCenter.z,
                    scale.x, scale.y, scale.z);
            }
        }

        public static class Seam
        {
#region DLL

            /// <summary>
            /// Creates a seam between two deformable objects.
            /// </summary>
            /// <param name="objectIdA">The index of the first object</param>
            /// <param name="objectIdB">The index of the second object</param>
            /// <param name="indices">The indices of the vertices that should be sewed together. The input must be
            ///						  a list of pairs such as:
            ///						  [A<sub>1</sub>, B<sub>1</sub>, A<sub>2</sub>, B<sub>2</sub> ... A<sub>i</sub>, B<sub>i</sub>]</param>
            /// <param name="numIndices">The total number of indices</param>
            /// <param name="id">The index of the created seam</param>
            [DllImport("deform")]
            public static extern void CreateSeam(int object_id_0, int object_id_1, int[] indices, int numIndices, out int id);

            /// <summary>
			/// Sets the distance stiffness of the seam constraints
			/// </summary>
			/// <param name="seamId">The index of the seam</param>
			/// <param name="stiffness">The distance stiffness parameter</param>  
            [DllImport("deform")]
            public static extern void SetSeamDistanceStiffness(int seam_id, float stiffness);

            /// <summary>
			/// Sets the bending stiffness of the seam constraints
			/// </summary>
			/// <param name="seamId">The index of the seam</param>
			/// <param name="stiffness">The bending stiffness parameter</param>  
            [DllImport("deform")]
            public static extern void SetSeamBendingStiffness(int seam_id, float stiffness);

            /// <summary>
			/// Decides whether or not damping should be enabled when sewing
			/// </summary>
			/// <param name="seamId">The index of the seam</param>
			/// <param name="enabled">Indicate whether or not damping should be enabled</param>  
            [DllImport("deform")]
            public static extern void SetSeamDamping(int seam_id, bool enabled);

            /// <summary>
			/// Sets the priority of a seam. Seams with low values will be sewed before seams with high values.
			/// </summary>
			/// <param name="seamId">The index of the seam</param>
			/// <param name="priority">The initialization priority of this seam</param>  
            [DllImport("deform")]
            public static extern void SetSeamPriority(int seam_id, int priority);

            [DllImport("deform")]
            public static extern void SewCloseVertices(int object_id);
#endregion
        }

        public static class IO
        {
#region DLL
            [DllImport("deform")]
            public static extern void SnapshotFBX(int exporterId, int objectId, string path);

            /// <summary>
            /// Builds the initial FBX configuration. This function has to be called before
            /// <see cref="RecordFrameFBXFromDataSM"/> and <see cref="ExportFBXFromDataSM"/>.
            /// </summary>
            /// <param name="vertices">The vertex array of the mesh</param>
            /// <param name="indices">The index array of the mesh</param>
            /// <param name="numVertices">The number of vertices of the mesh</param>
            /// <param name="numIndices">The number of indices of the mesh</param>
            /// <param name="exporterId">The exporter id</param>  
            [DllImport("deform")]
            public static extern void BuildFBXFromData(string name,
                                                        Vector3[] vertices,
                                                        int[] indices,
                                                        int numVertices,
                                                        int numIndices,
                                                        out int exporterId);

            /// <summary>
            /// Records one frame to the FBX.
            /// </summary>
            /// <param name="exporterId">The index of the exporter that the frame should be recorded to.</param>
            /// <param name="vertices">The list of vertices that should be recorded.</param>
            /// <param name="normals">The list of normals that should be recorded.</param>
            /// <param name="numVertices">The number of vertices that should be recorded.</param>
            /// <param name="translationX">The translation of the recorded mesh</param>
            /// <param name="translationY">The translation of the recorded mesh</param>
            /// <param name="translationZ">The translation of the recorded mesh</param>
            /// <param name="rotationX">The quaternion rotation of the recorded mesh</param>
            /// <param name="rotationY">The quaternion rotation of the recorded mesh</param>
            /// <param name="rotationZ">The quaternion rotation of the recorded mesh</param>
            /// <param name="rotationW">The quaternion rotation of the recorded mesh</param>
            /// <param name="scaleX">The scale of the recorded mesh</param>
            /// <param name="scaleY">The scale of the recorded mesh</param>
            /// <param name="scaleZ">The scale of the recorded mesh</param>
            /// <param name="flipX">If the recorded mesh should be flipped or not</param>
            /// <param name="flipY">If the recorded mesh should be flipped or not</param>
            /// <param name="flipZ">If the recorded mesh should be flipped or not</param> 
            [DllImport("deform")]
            public static extern void RecordFrameFBXFromData(
                                                              int exporterId,
                                                              Vector3[] vertices,
                                                              Vector3[] normals,
                                                              int numVertices,
                                                              float translationX, float translationY, float translationZ,
                                                              float rotationX, float rotationY, float rotationZ, float rotationW,
                                                              float scaleX, float scaleY, float scaleZ,
                                                              bool flipX, bool flipY, bool flipZ);

            /// <summary>
            /// Exports a recorded simulation to an FBX-file. An associated point cache file will also be generated.
            /// </summary>
            /// <param name="exporterId">The index of the exporter</param>
            /// <param name="path">Path to where the FBX-file should be saved</param> 
            [DllImport("deform")]
            public static extern void ExportFBXFromData(int exporterId, string path);
#endregion
        }

        public static class DXF
        {
#region DLL

            /// <summary>
            /// Given a closed 2D polygon as a list of 2D vertices, produces a new 2D polygon with vertices with equal distances
            /// from each other.
            /// </summary>
            /// <param name="inputVertices">The vertices of the 2D shape</param>
            /// <param name="numInputPoints">The number of vertices of the 2D shape</param>
            /// <param name="distance">The desired distance between the points</param>
            /// <param name="numOutputPoints">The resulting number of points</param> 
            [DllImport("deform")]
            public static extern int SimplifyShape(Vector2[] inputPoints,
                                                   int numInputPoints,
                                                   float distance,
                                                   out int numOutputPoints);

            /// <summary>
            /// Retrieves the shape data produced by <see cref="SimplifyShape"/>.
            /// </summary>
            /// <param name="points">The points of the simplified shape</param>   
            [DllImport("deform")]
            public static extern void RetrieveSimplifiedShape(IntPtr points);

            /// <summary>
            /// Given a 2D shape consisting of vertices, produces a triangulated mesh.
            /// </summary>
            /// <param name="inputVertices">The vertices of the 2D shape</param>
            /// <param name="numInputVertices">The number of vertices in the shape</param>
            /// <param name="numSubShapes">The number of subshapes of this shape</param>
            /// <param name="subShapePointCounts">The number of points for each subshape</param>
            /// <param name="triangleArea">Desired triangle area</param>
            /// <param name="angleConstraint">Desired smallest angle of triangles</param>
            /// <param name="numOutputVertices">The number of vertices in the triangulated mesh</param>
            /// <param name="numOutputTriangles">The number of indices in the triangulated mesh</param> 
            [DllImport("deform")]
            public static extern void TriangulateShape(
                                                       Vector2[] inputVertices,
                                                       int numInputVertices,
                                                       int numSubShapes,
                                                       int[] subShapePointCounts,
                                                       float triangleArea,
                                                       float angleConstraint,
                                                       out int numOutputVertices,
                                                       out int numOutputTriangles);

            /// <summary>
            /// Retrieves the mesh data produced by <see cref="TriangulateShape"/>.
            /// </summary>
            /// <param name="vertices">The vertices of the triangulated shape</param>
            /// <param name="triangles">The triangles of the triangulated shape</param> 
            [DllImport("deform")]
            public static extern void RetrieveTriangulatedShape( IntPtr vertices, IntPtr triangles);

            /// <summary>
            /// Reads a DXF-file and parses its data.
            /// </summary>
            /// <param name="path">Path to the DXF-file</param>
            /// <param name="numObjects">Number of DXF shapes</param>
            /// <param name="numPoints">Total number of points</param>
            /// <param name="bbMin">The minimal value of the bounding box</param>
            /// <param name="bbMax">The maximal value of the bounding box</param>  
            [DllImport("deform")]
            public static extern void ProcessDXF(
                                                 string path,
                                                 out int numObjects,
                                                 out int numPoints,
                                                 out Vector2 bbMin,
                                                 out Vector2 bbMax);

            /// <summary>
            /// Retrieves the shape data produced by <see cref="ProcessDXF"/>.
            /// </summary>
            /// <param name="points">The points of the DXF</param>
            /// <param name="pointCounts">The number of points of each shape</param>
            /// <param name="layers">The layer of each shape</param>
            /// <param name="holes">A list of integers defining if each shape is a hole or not</param>
            /// <param name="holeOwners">The "owner" shape of each hole</param>
            /// <param name="positions">The positions in world space of each shape</param>  
            [DllImport("deform")]
            public static extern void RetrieveDXFData(IntPtr points,
                                                      IntPtr pointCounts,
                                                      IntPtr layers,
                                                      IntPtr holes,
                                                      IntPtr holeOwners,
                                                      IntPtr positions);
#endregion
        }

        public static class Utils
        {
#region DLL

            /// <summary>
            /// Finds and returns information of the contours of a mesh.
            /// </summary>
            /// <param name="vertices">A list of vertices</param>
            /// <param name="vertexCount">The number of vertices</param>
            /// <param name="triangles">A list of triangles</param>
            /// <param name="triangleCount">The number of triangles</param>
            /// <param name="bbMin">The min point of the bounding box encapsulating the mesh</param>
            /// <param name="bbMin">The max point of the bounding box encapsulating the mesh</param>
            /// <param name="numContourPoints">The number of vertices on the contours</param>
            /// <remarks>Use <see cref="RetrieveMeshContours"/> to retrieve the vertex data of the contour.</remarks>
            [DllImport("deform")]
            public static extern int FindMeshContours(Vector3[] vertices,
                                                      int vertexCount,
                                                      int[] triangles,
                                                      int triangleCount,
                                                      float bbMinX,
                                                      float bbMinY,
                                                      float bbMinZ,
                                                      float bbMaxX,
                                                      float bbMaxY,
                                                      float bbMaxZ,
                                                      out int numContours,
                                                      out int numContourPoints);

            [DllImport("deform")]
            public static extern int RetrieveMeshContours(  IntPtr vertexIndices, IntPtr vertexCounts);

			/// <summary>
			/// Creates a cloth patch optimized for simulation with the DeformPlugin.
			/// </summary>
			/// <param name="sizeX">The size of the patch</param>
			/// <param name="sizeY">The size of the patch</param>
			/// <param name="resolution">The number of vertices of the longest side of the patch</param>
			/// <param name="vertices">The list of vertices</param>
			/// <param name="indices">The list of indices</param>       
			[DllImport("deform")]
            public static extern void CreatePatch(float sizeX, float sizeY, uint resolution, IntPtr vertices, IntPtr indices);
#endregion
		}

        public static class Serialization
        {
#region DLL

            /// <summary>
            /// Saves the current state of the simulation to the path specified
            /// </summary>    
            [DllImport("deform")]
            public static extern int SaveSerializedData( string path);

            /// <summary>
            /// Loads the state of the simulation from the file generated by <see cref="SaveSerializedDataSM"/>.
            /// </summary>
            /// <param name="path">The path to the file that should be loaded</param>
            /// <param name="numObjects">The number of deformable objects</param>
            /// <param name="numSeams">The number of seams</param>   
            [DllImport("deform")]
            public static extern int LoadSerializedData( string path, out int numObjects, out int numSeams);

            /// <summary>
            /// Retrieves the essential object information from the serialization loaded by <see cref="LoadSerializedDataSM"/>.
            /// </summary>
            /// <param name="objectId">The index of the object</param>
            /// <param name="numVertices">The number of vertices of the object</param>
            /// <param name="numIndices">The number of indices of the object</param>
            /// <param name="numDistanceConstraints">The number of distance constraints</param>
            /// <param name="numBendingConstraints">The number of bending constraints</param>
            [DllImport("deform")]
            public static extern int RetrieveSerializedObjectInfo(int objectId,
                                                                   out int numVertices,
                                                                   out int numIndices,
                                                                   out int numDistanceConstraints,
                                                                   out int numBendingConstraints);

            /// <summary>
            /// Retrieves the essential object data from the serialization loaded by <see cref="LoadSerializedDataSM"/>.
            /// </summary>
            /// <param name="objectId">The index of the object</param>
            /// <param name="vertices">The array of vertices</param>
            /// <param name="indices">The array of indices</param>
            /// <param name="distanceRestValues">The array of distance rest values</param>
            /// <param name="bendingRestValues">The array of bending rest values</param>
            /// <param name="kineticFrictionCoefs">The array of kinetic friction coefficients (one per vertex)</param>
            /// <param name="staticFrictionCoefs">The array of static friction coefficients (one per vertex)</param>
            /// <param name="invmass">The array of inverse mass (one per vertex)</param>
            /// <param name="distanceStiffness">The distance stiffness of the object</param>
            /// <param name="bendingStiffness">The bending stiffness of the object</param>  
            [DllImport("deform")]
            public static extern int RetrieveSerializedObjectData(int objectId,
                                                                   IntPtr vertices,
                                                                   IntPtr indices,
                                                                   IntPtr distanceRestValues,
                                                                   IntPtr bendingRestValues,
                                                                   IntPtr kineticFrictionCoefs,
                                                                   IntPtr staticFrictionCoefs,
                                                                   IntPtr invmass,
                                                                   out float distanceStiffness,
                                                                   out float bendingStiffness);

            /// <summary>
            /// Retrieves the essential seam information from the serialization loaded by <see cref="LoadSerializedDataSM"/>.
            /// </summary>
            /// <param name="seamId">The index of the seam</param>
            /// <param name="numIndices">The number of seam indices</param>
            [DllImport("deform")]
            public static extern int RetrieveSerializedSeamInfo( int seamId, out int numIndices);

            /// <summary>
            /// Retrieves the essential seam information from the serialization loaded by <see cref="LoadSerializedDataSM"/>.
            /// </summary>
            /// <param name="seamId">The index of the seam</param>
            /// <param name="objectIdA">The index of the first object this seam is sewed with</param>
            /// <param name="objectIdB">The index of the second object this seam is sewed with</param>
            /// <param name="indices">The seam indices</param>
            /// <param name="distanceStiffness">The distance stiffness of the seam constraints</param>
            /// <param name="bendingStiffness">The bending stiffness of the seam constraints</param>
            /// <param name="priority">The initialization priority of this seam</param>    
            [DllImport("deform")]
            public static extern int RetrieveSerializedSeamData(int seamId,
                                                                 out int objectIdA,
                                                                 out int objectIdB,
                                                                 IntPtr indices,
                                                                 out float distanceStiffness,
                                                                 out float bendingStiffness,
                                                                 out int priority);
#endregion
        }

        public static class BezierPath
        {
#region DLL
            
            [DllImport("deform")]
            private static extern void get_anchor_points(int path_id, int curve_id, out float startX, out float startY, out float startZ, out float endX, out float endY, out float endZ);

            [DllImport("deform")]      
            private static extern void set_control_point_tangential( int path_id, int curve_id, bool start, float x, float y, float z);

            [DllImport("deform")]      
            private static extern void set_control_points_individual_gpu( int[] path_ids, int num_path_ids, int curve_id, bool start, float delta_x, float delta_y, float delta_z);

            [DllImport("deform")]      
            private static extern void set_control_points_tangential_gpu( int[] path_ids, int num_path_ids, int curve_id, bool start, float delta_x, float delta_y, float delta_z);

            [DllImport("deform")]      
            private static extern void set_control_points_symmetrical_gpu( int[] path_ids, int num_path_ids, int curve_id, bool start, float delta_x, float delta_y, float delta_z);

            [DllImport("deform")]      
            private static extern void get_all_points_curve_sampling_gpu(IntPtr points, int num_points);

            [DllImport("deform")]      
            private static extern void get_all_points_path_sampling(IntPtr points, int num_points);

            [DllImport("deform")]      
            private static extern void get_points_at_curve_time(IntPtr points, int num_points, int curve, float t);

            [DllImport("deform")]      
            private static extern void get_all_points_specified_paths(IntPtr points, int num_points, int[] path_ids, int num_path_ids);

            [DllImport("deform")]
            private static extern int add_bezier_path();

            [DllImport("deform")]
            private static extern void add_bezier_curve( int path_id, float a_x, float a_y, float a_z, float b_x, float b_y, float b_z);
            
            [DllImport("deform")]
            private static extern void get_point_on_path_time( int path_id, float t, out float p_x, out float p_y, out float p_z);

            [DllImport("deform")]
            private static extern void get_point_on_path_dist( int path_id, float d, out float p_x, out float p_y, out float p_z);

            [DllImport("deform")]
            private static extern void get_point_on_path_udist( int path_id, float ud, out float p_x, out float p_y, out float p_z);

            [DllImport("deform")]
            private static extern void get_point_on_curve_udist( int path_id, int curve_id, float ud, out float p_x, out float p_y, out float p_z);

            [DllImport("deform")]
            private static extern int get_num_curves( int path_id);

            [DllImport("deform")]
            private static extern int get_num_curves_total();

            [DllImport("deform")]
            private static extern int get_num_paths();

            [DllImport("deform")]
            private static extern void create_bezier();

            [DllImport("deform")]
            private static extern void destroy_bezier();

            [DllImport("deform")]
            private static extern void initialize_bezier( int num_samples_per_curve, int num_samples_per_path);

            [DllImport("deform")]
            private static extern void get_end_points(
                                int path_id,
                                int curve_id,
                                out float start_x,
                                out float start_y,
                                out float start_z,
                                out float end_x,
                                out float end_y,
                                out float end_z);

            [DllImport("deform")]
            private static extern void get_control_points(
                                    int path_id,
                                    int curve_id,
                                    out float start_ctrl_x,
                                    out float start_ctrl_y,
                                    out float start_ctrl_z,
                                    out float end_ctrl_x,
                                    out float end_ctrl_y,
                                    out float end_ctrl_z);

            [DllImport("deform")]
            private static extern void set_control_point_individual(
                                int path_id,
                                int curve_id,
                                bool start,
                                float x,
                                float y,
                                float z);

            [DllImport("deform")]
            private static extern void set_control_point_tangent(
                                int path_id,
                                int curve_id,
                                bool start,
                                float x,
                                float y,
                                float z);

            [DllImport("deform")]
            private static extern void set_control_point_symmetrical(
                                int path_id,
                                int curve_id,
                                bool start,
                                float x,
                                float y,
                                float z);
#endregion

            public static int AddBezierPath()
            {
                return add_bezier_path();
            }
            
            public static void AddBezierCurve(int path_id, Vector3 a, Vector3 b)
            {
                add_bezier_curve(path_id, a.x, a.y, a.z, b.x, b.y, b.z);
            }

            public static void GetPointOnPathUniformDistance(int path_id, float ud, out float pX, out float pY, out float pZ)
            {
                get_point_on_path_udist(path_id, ud, out pX, out pY, out pZ);
            }

            public static int GetNumCurves(int path_id)
            {
                return get_num_curves(path_id);
            }

            public static int GetNumCurvesTotal()
            {
                return get_num_curves_total();
            }

            public static int GetNumPaths()
            {
                return get_num_paths();
            }

            public static void CreateBezier()
            {
                create_bezier();
            }

            public static void DestroyBezier()
            {
                destroy_bezier();
            }

            public static void InitializeBezier(int num_samples_per_curve, int num_samples_per_path)
            {
                initialize_bezier(num_samples_per_curve, num_samples_per_path);
            }

            public static void GetAnchorPoints(int path_id,
                                               int curve_id,
                                               out float startX,
                                               out float startY,
                                               out float startZ,
                                               out float endX,
                                               out float endY,
                                               out float endZ)
            {
                get_anchor_points(path_id, curve_id, out startX, out startY, out startZ, out endX, out endY, out endZ);
            }

            public static void GetControlPoints(int path_id,
                                                int curve_id,
                                                out float startCtrlX,
                                                out float startCtrlY,
                                                out float startCtrlZ,
                                                out float endCtrlX,
                                                out float endCtrlY,
                                                out float endCtrlZ)
            {
                get_control_points(path_id, curve_id, out startCtrlX, out startCtrlY, out startCtrlZ, out endCtrlX, out endCtrlY, out endCtrlZ);
            }

            public static void SetControlPointIndividual(int path_id, int curve_id, bool start, float x, float y, float z)
            {
                set_control_point_individual(path_id, curve_id, start, x, y, z);
            }

            public static void SetControlPointTangential(int path_id, int curve_id, bool start, float x, float y, float z)
            {
                set_control_point_tangential(path_id, curve_id, start, x, y, z);
            }

            public static void SetControlPointSymmetrical(int path_id, int curve_id, bool start, float x, float y, float z)
            {
                set_control_point_symmetrical(path_id, curve_id, start, x, y, z);
            }

            public static void SetControlPointsIndividualGPU(int[] path_ids, int curve_id, bool start, float delta_x, float delta_y, float delta_z)
            {
                set_control_points_individual_gpu(path_ids, path_ids.Length, curve_id, start, delta_x, delta_y, delta_z);
            }

            public static void SetControlPointsTangentialGPU(int[] path_ids, int curve_id, bool start, float delta_x, float delta_y, float delta_z)
            {
                set_control_points_tangential_gpu(path_ids, path_ids.Length, curve_id, start, delta_x, delta_y, delta_z);
            }

            public static void SetControlPointsSymmetricalGPU(int[] path_ids, int curve_id, bool start, float delta_x, float delta_y, float delta_z)
            {
                set_control_points_symmetrical_gpu(path_ids, path_ids.Length, curve_id, start, delta_x, delta_y, delta_z);
            }

            public static void GetAllPointsCurveSamplingGPU(IntPtr points, int num_points)
            {
                get_all_points_curve_sampling_gpu(points, num_points);
            }

            public static void GetAllPointsPathSamplingGPU(IntPtr points, int num_points)
            {
                get_all_points_path_sampling(points, num_points);
            }

            public static void GetPointsAtCurveTime(IntPtr points, int num_points, int curve, int t)
            {
                get_points_at_curve_time(points, num_points, curve, t);
            }

            public static void GetAllPointsSpecifiedPaths(IntPtr points, int num_points, int[] path_ids)
            {
                get_all_points_specified_paths(points, num_points, path_ids, path_ids.Length);
            }
        }
    }
}
