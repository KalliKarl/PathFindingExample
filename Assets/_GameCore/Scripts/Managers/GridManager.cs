using DG.Tweening;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GridManager : MonoBehaviour
{
	[Range(1, 8)]
	public int XSize;
	[Range(1, 12)]
	public int YSize;
	public GridController gridPrefab;
	public Camera cam;
	public float gridSize;
	public float offset;
	public List<GridController> gridControllers;

	public List<GridController> cornerTiles;
	public List<GridController> exitTiles;
	public GameObject cameraLookAt;
	public static GridManager instance;
	public StickmanController stickmanPrefab;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
	}
	private void OnEnable()
	{
		//EventManager.GenerateNextLevelEvent += StartLevel;
	}
	private void OnDisable()
	{
		//EventManager.GenerateNextLevelEvent -= StartLevel;
	}
	private void Start()
	{
		//CreateTiles();
		if (gridControllers.Count > 0)
		{
			GetEachGridNeighbours();
		}
		AdjustCamera();
	}

	[ContextMenu("AdjustCamera")]
	public void AdjustCamera()
	{
		cam = Camera.main;
		Vector3 cameraPos = Vector3.zero;
		cameraPos.x = gridSize * XSize * 0.5f;
		cameraPos.z = gridSize * (YSize - 1) * 0.5f;
		cam.transform.parent.position = cameraPos;
		cam.transform.localPosition = Vector3.zero;
		var dir = -cam.transform.forward;
		dir.Normalize();
		cam.transform.localPosition = 14 * XSize * dir;

	}

	public void StartLevel(/*SoLevelData soLevelData*/)
	{
		//XSize = soLevelData.gridXYSize.x;
		//YSize = soLevelData.gridXYSize.y;
		//levelData = soLevelData;
		CreateTiles();
		AdjustCamera();
	}

	[ContextMenu("Create")]
	public void CreateTiles()
	{
		ClearGridControllers();
		CreateGridControllers();

		DOVirtual.DelayedCall(0.1f, () =>
		{
			GetEachGridNeighbours();
		}, false);
	}

	private void CreateGridControllers()
	{
		for (int y = 0; y < YSize; y++)
		{
			for (int x = 0; x < XSize; x++)
			{
				GridType gridType = GridType.normal;
				//if (levelData.obstacles.Any(item => item.y == y && item.x == x))
				//{
				//	gridType = GridType.wall;
				//}

				Vector3 worldPosition = new(x * (gridSize + offset), 0, y * (gridSize + offset));
				worldPosition += transform.position;

				GridController spawnedGrid = PrefabUtility.InstantiatePrefab(gridPrefab.gameObject as GameObject).GameObject().GetComponent<GridController>();
				spawnedGrid.transform.position = worldPosition;
				spawnedGrid.transform.rotation = Quaternion.identity;
				spawnedGrid.transform.SetParent(transform, true);

				spawnedGrid.gameObject.name = "Y: " + y + " X: " + x;
				spawnedGrid.gridX = x;
				spawnedGrid.gridY = y;
				gridControllers.Add(spawnedGrid);
				spawnedGrid.Init(gridSize, offset, this, gridType);
			}
		}
	}

	private void GetEachGridNeighbours()
	{
		foreach (GridController item in gridControllers)
		{
			item.GetNeighbour();
			item.SetHighlight(false, Color.red);
		}
	}

	private void ClearGridControllers()
	{
		if (gridControllers.Count > 0)
		{
			foreach (var item in gridControllers)
			{
				if (item != null)
					DestroyImmediate(item.gameObject);
			}
			gridControllers.Clear();
		}
		cornerTiles.Clear();
		exitTiles.Clear();
	}
}
