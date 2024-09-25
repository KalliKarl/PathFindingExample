using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
	private GridManager gridManager;

	private List<GridController> openList;
	private List<GridController> closedList;

	public GridController startGrid;
	public GridController endGrid;

	public List<GridController> path;

	[ContextMenu("Findpath")]
	public void FindPath()
	{
		FindPath(startGrid, endGrid);
	}

	private void Start()
	{
		gridManager = GridManager.instance;
	}
	public void FindPath(GridController startGrid, GridController endGrid)
	{
		this.startGrid = startGrid;
		this.endGrid = endGrid;
		//Debug.Log(gameObject.name + " findPath", gameObject);
		path = GetPath(startGrid, endGrid);
		if (path != null)
		{
			for (int i = path.Count - 1; i >= 1; i--)
			{
				Debug.DrawLine(path[i].transform.position, path[i - 1].transform.position, Color.red, 10);
			}
		}
	}

	private List<GridController> GetPath(GridController startNode, GridController targetNode)
	{
		//GridController startNode = GetGridFromPosition(startPos);
		//GridController targetNode = GetGridFromPosition(targetPos);

		openList = new List<GridController> { startNode };
		closedList = new List<GridController>();

		while (openList.Count > 0)
		{
			GridController currentNode = openList[0];

			for (int i = 1; i < openList.Count; i++)
			{
				if (openList[i].FCost < currentNode.FCost || openList[i].FCost == currentNode.FCost && openList[i].hCost < currentNode.hCost)
				{
					currentNode = openList[i];
				}
			}

			openList.Remove(currentNode);
			closedList.Add(currentNode);

			if (currentNode == targetNode)
			{
				return RetracePath(startNode, targetNode);
			}

			foreach (GridController neighbour in currentNode.GetWalkableNeighbours())
			{
				if (!neighbour.isWalkable || closedList.Contains(neighbour))
				{
					continue;
				}

				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if (newMovementCostToNeighbour < neighbour.gCost || !openList.Contains(neighbour))
				{
					neighbour.gCost = newMovementCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, targetNode);
					neighbour.parent = currentNode;

					if (!openList.Contains(neighbour))
					{
						openList.Add(neighbour);
					}
				}
			}
		}

		return null;
	}

	private List<GridController> RetracePath(GridController startNode, GridController endNode)
	{
		List<GridController> path = new List<GridController>();
		GridController currentNode = endNode;

		while (currentNode != startNode)
		{
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}

		path.Reverse();
		return path;
	}

	private int GetDistance(GridController nodeA, GridController nodeB)
	{
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		if (dstX > dstY)
			return 14 * dstY + 10 * (dstX - dstY);
		return 14 * dstX + 10 * (dstY - dstX);
	}

	private GridController GetGridFromPosition(Vector3 position)
	{
		// GridManager'ı kullanarak pozisyona en yakın grid'i bulun
		foreach (var grid in gridManager.gridControllers)
		{
			if (Vector3.Distance(grid.transform.position, position) < gridManager.gridSize)
			{
				return grid;
			}
		}
		return null;
	}
}
