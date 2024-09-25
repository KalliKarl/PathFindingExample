using DG.Tweening;
using System;
using UnityEngine;

public enum AnimationState { idle, walk, run, sleep }

public class StickmanController : MonoBehaviour
{
	private const float walkSpeed = 13f;

	public SkinnedMeshRenderer meshRenderer;
	public GameObject visual;
	public GridController gridController;
	public Pathfinding pathfinding;
	public bool isLevelDesing;
	public bool isMoving;
	public bool isDebug;
	public AnimationState currentAnimationState;
	[SerializeField] public Animator animator;
	public Action stickmanArrived;

	private void OnValidate()
	{
		if (!isLevelDesing) return;
	}
	private void Start()
	{
		DOVirtual.DelayedCall(1f, StartMovement, false);
	}
	[ContextMenu("StartMovement")]
	public void StartMovement()
	{
		if (isMoving || gridController == null) return;
		isMoving = true;
		gridController = pathfinding.startGrid;
		pathfinding.FindPath(pathfinding.startGrid, pathfinding.endGrid);
		MoveAlongPath(pathfinding.path[^1], 0);

	}

	public bool TryMoveToTarget(GridController start, GridController end)
	{
		if (isMoving) return false;
		gridController = start;
		pathfinding.FindPath(start, end);
		bool canYouMoveToTarget = (pathfinding.path != null);
		if (canYouMoveToTarget)
		{
			MoveAlongPath(end, 0);
			isMoving = true;
		}
		return canYouMoveToTarget;
	}
	private void MoveAlongPath(GridController targetGrid, int pathIndex)
	{

		animator.SetBool("isRunning", true);
		animator.SetBool("isIdle", false);
		animator.SetBool("isSleeping", false);
		animator.SetBool("isWalk", false);
		currentAnimationState = AnimationState.run;

		GridController nextGrid = pathfinding.path[pathIndex];
		GridController nextNextGrid = null;
		if (pathIndex < pathfinding.path.Count - 1)
		{
			nextNextGrid = pathfinding.path[pathIndex + 1];
		}
		Vector3 lookDirection;
		if (nextNextGrid != null)
		{
			lookDirection = ((nextGrid.gridVisual.transform.position + nextNextGrid.gridVisual.transform.position) / 2f) - transform.position;
		}
		else
		{
			lookDirection = nextGrid.gridVisual.transform.position - transform.position;
		}

		lookDirection.y = 0; // Ensure we're only rotating on the Y-axis
		float angle = Vector3.SignedAngle(transform.forward, lookDirection, Vector3.up);
		if (Mathf.Abs(angle) > 10) // Adjust threshold as needed
		{
			float lookDuration = Mathf.Abs(angle) / 360f; // Adjust rotation speed as needed
			transform.DORotate(new Vector3(0, transform.eulerAngles.y + angle, 0), lookDuration)
				.SetEase(Ease.OutQuad)
				.SetId("Rotate_" + gameObject.GetInstanceID());

		}


		Vector3 targetPosition;
		if (nextNextGrid != null)
		{
			targetPosition = (nextGrid.gridVisual.transform.position + nextNextGrid.gridVisual.transform.position) / 2f;
		}
		else
		{
			targetPosition = nextGrid.gridVisual.transform.position;
		}


		transform.DOMove(targetPosition, walkSpeed)
			.SetSpeedBased(true)
			.SetEase(Ease.Linear)
			.SetId("Move_" + gameObject.GetInstanceID())
			.OnComplete(
		() =>
		{
			if (pathIndex < pathfinding.path.Count - 1) // next gridin hareket kodunu çalıştır
			{
				isMoving = false;
				MoveAlongPath(targetGrid, pathIndex + 1);
			}
			else
			{
				animator.SetBool("isRunning", false);
				animator.SetBool("isIdle", true);
				animator.SetBool("isWalk", false);
				animator.SetBool("isSleeping", false);
				currentAnimationState = AnimationState.sleep;
				DOTween.Complete("Move_" + gameObject.GetInstanceID(), true);
				DOTween.Complete("Rotate_" + gameObject.GetInstanceID(), true);
				stickmanArrived?.Invoke();
			}
		});
	}

}
