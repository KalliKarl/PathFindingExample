using System.Collections.Generic;
using UnityEngine;


public enum GridPos
{
	Top, Right, Bottom, Left, TopRight, TopLeft, BottomLeft, BottomRight, Center
}
public enum GridType
{
	normal, exit, wall, enter
}

public class GridController : MonoBehaviour
{
	public BoxCollider boxCollider;
	public GameObject gridVisual;
	public GameObject highlightGreen;
	public GameObject highlightRed;

	public GameObject wall;
	public GameObject exit;

	public GridController frontGrid;
	public GridController backGrid;
	public GridController leftGrid;
	public GridController rightGrid;

	private float gridSize;
	private float gridOffset;

	public Vector3 forwardPosition;
	public Vector3 backPosition;
	public Vector3 leftPosition;
	public Vector3 rightPosition;
	public GridType gridType;
	public GridPos gridPos;
	public bool isDrawGismosActive;
	public GridManager gridManager;
	public StickmanController stickmanController;


	public int gridX; // Yeni eklenen özellik
	public int gridY; // Yeni eklenen özellik
	public bool isWalkable; // Yeni değişken
	public int gCost;
	public int hCost;
	public GridController parent;

	public int FCost
	{
		get
		{
			return gCost + hCost;
		}
	}

	public void Init(float gridSize, float gridOffset, GridManager gridManager, GridType gridType = GridType.normal)
	{
		this.gridSize = gridSize;
		this.gridOffset = gridOffset;
		this.gridManager = gridManager;
		this.gridType = gridType;
		boxCollider.size = new Vector3(gridSize, gridSize * 0.1f, gridSize);
		//boxCollider.center = new Vector3(gridSize * 0.5f, gridSize * 0.1f, gridSize * 0.5f);
		//gridVisual.transform.localScale = new Vector3(gridSize, 0.2f, gridSize);


		forwardPosition = new Vector3(0, 0, gridSize + gridOffset);
		backPosition = new Vector3(0, 0, -(gridSize + gridOffset));
		leftPosition = new Vector3(-(gridSize + gridOffset), 0, 0);
		rightPosition = new Vector3((gridSize + gridOffset), 0, 0);

		if (gridType == GridType.wall)
		{
			//wall.SetActive(true);
			gridVisual.SetActive(false);
			exit.SetActive(false);
		}
	}

	[ContextMenu("CheckNeighbour")]
	public void GetNeighbour()
	{
		frontGrid = CheckNeighbour(forwardPosition);
		backGrid = CheckNeighbour(backPosition);
		leftGrid = CheckNeighbour(leftPosition);
		rightGrid = CheckNeighbour(rightPosition);


		if (frontGrid == null || backGrid == null || leftGrid == null || rightGrid == null)
		{
			gridManager.cornerTiles.Add(this);
		}
		SetGridPos();

		isWalkable = (gridType != GridType.wall && stickmanController == null);
	}

	private void SetGridPos()
	{
		if (frontGrid != null && backGrid != null && leftGrid != null && rightGrid != null)
		{
			gridPos = GridPos.Center;
		}
		else if (frontGrid == null && backGrid != null && leftGrid != null && rightGrid != null)
		{
			gridPos = GridPos.Top;
		}
		else if (frontGrid != null && backGrid == null && leftGrid != null && rightGrid != null)
		{
			gridPos = GridPos.Bottom;
		}
		else if (frontGrid != null && backGrid != null && leftGrid == null && rightGrid != null)
		{
			gridPos = GridPos.Left;
		}
		else if (frontGrid != null && backGrid != null && leftGrid != null && rightGrid == null)
		{
			gridPos = GridPos.Right;
		}
		else if (frontGrid == null && backGrid != null && leftGrid == null && rightGrid != null)
		{
			gridPos = GridPos.TopLeft;
		}
		else if (frontGrid == null && backGrid != null && leftGrid != null && rightGrid == null)
		{
			gridPos = GridPos.TopRight;
		}
		else if (frontGrid != null && backGrid == null && leftGrid == null && rightGrid != null)
		{
			gridPos = GridPos.BottomLeft;
		}
		else if (frontGrid != null && backGrid == null && leftGrid != null && rightGrid == null)
		{
			gridPos = GridPos.BottomRight;
		}
	}
	public List<GridController> GetNeighbours()
	{
		List<GridController> neighbours = new();

		if (frontGrid != null) neighbours.Add(frontGrid);
		if (backGrid != null) neighbours.Add(backGrid);
		if (leftGrid != null) neighbours.Add(leftGrid);
		if (rightGrid != null) neighbours.Add(rightGrid);

		return neighbours;
	}
	public List<GridController> GetWalkableNeighbours()
	{
		List<GridController> neighbours = new();

		if (frontGrid != null && frontGrid.isWalkable) neighbours.Add(frontGrid);
		if (backGrid != null && backGrid.isWalkable) neighbours.Add(backGrid);
		if (leftGrid != null && leftGrid.isWalkable) neighbours.Add(leftGrid);
		if (rightGrid != null && rightGrid.isWalkable) neighbours.Add(rightGrid);

		return neighbours;
	}
	private GridController CheckNeighbour(Vector3 position)
	{
		Vector3 halfExtents = new(boxCollider.size.x * 0.4f, boxCollider.size.y * 0.2f, boxCollider.size.z * 0.4f);
		var gridColliders = Physics.OverlapBox(gridVisual.transform.position + position, halfExtents, Quaternion.identity);
		foreach (Collider collider in gridColliders)
		{
			if (collider.TryGetComponent(out GridController gridController))
			{
				if (gridController != this)
				{
					return gridController;
				}
			}
		}
		return null;

	}
	public void SetHighlight(bool enable, Color color)
	{
		if (enable)
		{
			if (color == Color.green)
			{
				highlightGreen.SetActive(enable);
				highlightRed.SetActive(false);

			}
			else if (color == Color.red)
			{
				highlightRed.SetActive(enable);
				highlightGreen.SetActive(false);

			}
		}
		else
		{
			highlightGreen.SetActive(false);
			highlightRed.SetActive(false);
		}

	}
	private void OnDrawGizmos()
	{
		if (!isDrawGismosActive)
			return;

		Gizmos.color = Color.red;
		Vector3 halfExtents = new(boxCollider.size.x * 0.4f, boxCollider.size.y * 0.2f, boxCollider.size.z * 0.4f);
		DrawOverlapBoxGizmo(gridVisual.transform.position + backPosition, halfExtents, Quaternion.identity);

		Gizmos.color = Color.yellow;
		DrawOverlapBoxGizmo(gridVisual.transform.position + forwardPosition, halfExtents, Quaternion.identity);

		Gizmos.color = Color.blue;
		DrawOverlapBoxGizmo(gridVisual.transform.position + leftPosition, halfExtents, Quaternion.identity);

		Gizmos.color = Color.green;
		DrawOverlapBoxGizmo(gridVisual.transform.position + rightPosition, halfExtents, Quaternion.identity);

	}

	private void DrawOverlapBoxGizmo(Vector3 position, Vector3 halfExtents, Quaternion orientation)
	{
		Matrix4x4 originalMatrix = Gizmos.matrix;
		Gizmos.matrix = Matrix4x4.TRS(position, orientation, Vector3.one);
		Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);
		Gizmos.matrix = originalMatrix;
	}
}
