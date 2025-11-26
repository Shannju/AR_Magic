using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialAnchor2
{
	[CreateAssetMenu(fileName = "SpatialAnchorUuidDatabase", menuName = "SpatialAnchors/Uuid Database", order = 0)]
	public class SpatialAnchorUuidDatabase : ScriptableObject
	{
		[Serializable]
		public class UuidPrefab
		{
			public string uuid;
			public GameObject prefab;
		}

		public List<string> uuids = new List<string>();
		public List<UuidPrefab> mappings = new List<UuidPrefab>();
	}
}


