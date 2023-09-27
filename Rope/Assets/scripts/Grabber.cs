using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiCollider))]
public class Grabber : MonoBehaviour
{
	public bool canGrab = true;
	ObiSolver solver;
	ObiCollider obiCollider;
	public ObiRope rope;
	ObiSolver.ObiCollisionEventArgs collisionEvent;
	ObiPinConstraintsBatch newBatch;
	ObiConstraints<ObiPinConstraintsBatch> pinConstraints;

	void Awake()
	{
		solver = FindObjectOfType<ObiSolver>();
		obiCollider = GetComponent<ObiCollider>();
	}

	void Start()
	{
		pinConstraints = rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
	}

	private void OnEnable()
	{
		solver = rope.solver;
		if (solver != null)
		{
			solver.OnCollision += Solver_OnCollision;
		}

	}

	private void OnDisable()
	{
		if (solver != null)
		{
			solver.OnCollision -= Solver_OnCollision;
		}

	}

	private void Solver_OnCollision(object sender, Obi.ObiSolver.ObiCollisionEventArgs e)
	{
		collisionEvent = e;
	}

	public void Grab()
	{
		var world = ObiColliderWorld.GetInstance();
		Debug.Log(pinConstraints);

		if (solver != null && collisionEvent != null)
		{
			Debug.Log("Collision");
			foreach (Oni.Contact contact in collisionEvent.contacts)
			{
				if (contact.distance < 0.01f)
				{
					var contactCollider = world.colliderHandles[contact.bodyB].owner;
					ObiSolver.ParticleInActor pa = solver.particleToActor[contact.bodyA];

					Debug.Log(pa + " hit " + contactCollider);
					if (canGrab)
					{
						if (contactCollider == obiCollider)
						{
							Debug.Log("Hand Collision");
							var batch = new ObiPinConstraintsBatch();
							int solverIndex = contact.bodyA;
							Vector3 positionWS = solver.transform.TransformPoint(solver.positions[solverIndex]);
							Vector3 positionCS = obiCollider.transform.InverseTransformPoint(positionWS);
							batch.AddConstraint(solverIndex, obiCollider, positionCS, Quaternion.identity, 0, 0, float.PositiveInfinity);
							batch.activeConstraintCount = 1;
							newBatch = batch;
							pinConstraints.AddBatch(newBatch);

							canGrab = false;

							rope.SetConstraintsDirty(Oni.ConstraintType.Pin);

						}
					}
				}
			}
		}
	}

	public void Release()
	{
		if (!canGrab)
		{
			Debug.Log("Release");
			pinConstraints.RemoveBatch(newBatch);
			rope.SetConstraintsDirty(Oni.ConstraintType.Pin);
			canGrab = true;
		}
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.G)) Grab();
		if (Input.GetKeyDown(KeyCode.R)) Release();
	}
}
