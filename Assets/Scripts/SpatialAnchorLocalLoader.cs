using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Meta.XR.BuildingBlocks;

namespace SpatialAnchor2
{
	public class SpatialAnchorLocalLoader : MonoBehaviour
	{
		[Header("Prefab to instantiate at anchors")]
		[SerializeField] private GameObject anchorPrefab;

		[System.Serializable]
		public class UuidPrefabMapping
		{
			public string uuid;
			public GameObject prefab;
		}

		[Header("Per-UUID Prefab Mappings (persistent)")]
		[SerializeField] private List<UuidPrefabMapping> uuidPrefabMappings = new List<UuidPrefabMapping>();

		[Header("Options")]
		[SerializeField] private bool loadOnStart = true;
		[SerializeField] private bool spawnOnStart = false;
		[SerializeField] private float spawnOnStartDelaySeconds = 1.0f;
		[SerializeField] private bool eraseOnStart = false;
		[SerializeField] private List<string> eraseOnStartUuids = new List<string>();
		[SerializeField] private bool eraseOnRefresh = true;
		[Tooltip("If true, spawn using unbound poses (grabbable). If false, bind to anchors via SDK (not grabbable).")]
		[SerializeField] private bool useUnboundSpawning = true;
		[Tooltip("Ensure Rigidbody/Collider for grabbing on spawned objects.")]
		[SerializeField] private bool ensureGrabbableComponents = false;
		[Tooltip("Rigidbody.useGravity for spawned objects when ensureGrabbableComponents is enabled.")]
		[SerializeField] private bool spawnedUseGravity = false;
		[Tooltip("Rigidbody.isKinematic for spawned objects when ensureGrabbableComponents is enabled.")]
		[SerializeField] private bool spawnedIsKinematic = true;
		[SerializeField] private Transform spawnParent;

		[Header("Keyboard Shortcuts (Play Mode)")]
		[SerializeField] private bool enableKeyboardShortcuts = true;
		[SerializeField] private KeyCode refreshKey = KeyCode.R;
		[SerializeField] private KeyCode spawnKey = KeyCode.S;

		[Header("Loaded UUIDs (read-only during Play)")]
		[SerializeField] private List<string> loadedUuids = new List<string>();

		[Header("Saved UUIDs (persistent in scene)")]
		[SerializeField] private List<string> savedUuids = new List<string>();

		[Header("UUID Database Asset (optional, persistent)")]
		[SerializeField] private SpatialAnchorUuidDatabase uuidDatabase;
		[Tooltip("When true, any newly loaded UUIDs are auto-saved to the assigned database asset and mirrored into Saved UUIDs.")]
		[SerializeField] private bool autoSaveLoadedUuidsToAsset = true;

		private SpatialAnchorCoreBuildingBlock spatialAnchorCore;

		private void Awake()
		{
			spatialAnchorCore = FindAnyObjectByType<SpatialAnchorCoreBuildingBlock>();
			if (spatialAnchorCore == null)
			{
				Debug.LogWarning("[SpatialAnchorLocalLoader] SpatialAnchorCoreBuildingBlock not found in scene.");
			}
		}

		private async void Start()
		{
			if (loadOnStart)
			{
				await TriggerRefreshAsync();
			}
			// 先删除列表中指定的锚（设备端擦除），再生成
			if (eraseOnStart && eraseOnStartUuids != null && eraseOnStartUuids.Count > 0)
			{
				await EraseAnchorsByUuidListAsync(eraseOnStartUuids);
			}
			if (spawnOnStart)
			{
				// 等待一两帧与可选延迟，保证姿态与追踪就绪
				await Task.Yield();
				await Task.Yield();
				if (spawnOnStartDelaySeconds > 0f)
				{
					await Task.Delay(TimeSpan.FromSeconds(spawnOnStartDelaySeconds));
				}
				Debug.Log("[SpatialAnchorLocalLoader] SpawnOnStart executing.");
				SpawnAnchorsFromPrefab();
			}
		}



		private System.Collections.IEnumerator SpawnAfterDelay()
		{
			// Wait end of frame to let PlayerPrefs and XR pose systems settle
			yield return new WaitForEndOfFrame();
			if (spawnOnStartDelaySeconds > 0f)
			{
				yield return new WaitForSeconds(spawnOnStartDelaySeconds);
			}
			Debug.Log("[SpatialAnchorLocalLoader] SpawnOnStart executing.");
			SpawnAnchorsFromPrefab();
		}

		private void Update()
		{
			if (!Application.isPlaying || !enableKeyboardShortcuts) return;
			if (Input.GetKeyDown(refreshKey))
			{
				_ = TriggerRefreshAsync();
			}
			if (Input.GetKeyDown(spawnKey))
			{
				SpawnAnchorsFromPrefab();
			}
		}

		[ContextMenu("Refresh UUIDs From Local Storage")]
		public async void RefreshUuidsFromLocalStorage()
		{
			await TriggerRefreshAsync();
		}

		private async Task TriggerRefreshAsync()
		{
			// Optionally erase anchors and remove from local storage before refresh
			if (eraseOnRefresh && eraseOnStartUuids != null && eraseOnStartUuids.Count > 0)
			{
				await EraseAnchorsByUuidListAsync(eraseOnStartUuids);
				RemoveUuidsFromLocalStorage(eraseOnStartUuids);
			}

			loadedUuids.Clear();

			// Read directly from PlayerPrefs using same keys used by SpatialAnchorLocalStorageManagerBuildingBlock
			int count = PlayerPrefs.GetInt("numUuids", 0);
			for (int i = 0; i < count; i++)
			{
				var key = "uuid" + i;
				if (PlayerPrefs.HasKey(key))
				{
					var uuidStr = PlayerPrefs.GetString(key, string.Empty);
					if (!string.IsNullOrEmpty(uuidStr))
					{
						loadedUuids.Add(uuidStr);
					}
				}
			}

			Debug.Log($"[SpatialAnchorLocalLoader] Loaded {loadedUuids.Count} UUID(s) from local storage.");

			// Mirror to Saved UUIDs and optionally persist to asset
			savedUuids.Clear();
			for (int i = 0; i < loadedUuids.Count; i++)
			{
				var s = loadedUuids[i] == null ? string.Empty : loadedUuids[i].Trim();
				if (!string.IsNullOrEmpty(s)) savedUuids.Add(s);
			}
			#if UNITY_EDITOR
			if (autoSaveLoadedUuidsToAsset && uuidDatabase != null)
			{
				uuidDatabase.uuids.Clear();
				for (int i = 0; i < savedUuids.Count; i++)
				{
					uuidDatabase.uuids.Add(savedUuids[i]);
				}
				UnityEditor.EditorUtility.SetDirty(uuidDatabase);
				UnityEditor.AssetDatabase.SaveAssets();
			}
			if (!Application.isPlaying)
			{
				UnityEditor.EditorUtility.SetDirty(this);
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
			}
			#endif
		}

		[ContextMenu("Spawn Anchors From Prefab")]
		public void SpawnAnchorsFromPrefab()
		{
			if (anchorPrefab == null && (uuidPrefabMappings == null || uuidPrefabMappings.Count == 0))
			{
				Debug.LogWarning("[SpatialAnchorLocalLoader] Please assign an Anchor Prefab or UUID mappings in the Inspector.");
				return;
			}
			if ((loadedUuids == null || loadedUuids.Count == 0) && (savedUuids == null || savedUuids.Count == 0))
			{
				Debug.Log("[SpatialAnchorLocalLoader] No UUIDs available. Use Refresh during Play, or Save Loaded UUIDs to persist.");
				return;
			}

			var uuidSource = loadedUuids != null && loadedUuids.Count > 0 ? loadedUuids : savedUuids;
			// Apply start-time exclusion filter (no device erase; just skip spawning these)
			var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (eraseOnStart && eraseOnStartUuids != null)
			{
				for (int i = 0; i < eraseOnStartUuids.Count; i++)
				{
					var s = eraseOnStartUuids[i] == null ? string.Empty : eraseOnStartUuids[i].Trim();
					if (!string.IsNullOrEmpty(s)) exclude.Add(s);
				}
			}

			var guids = new List<Guid>();
			for (int i = 0; i < uuidSource.Count; i++)
			{
				var raw = uuidSource[i];
				var s = string.IsNullOrEmpty(raw) ? string.Empty : raw.Trim();
				if (exclude.Contains(s))
				{
					Debug.Log($"[SpatialAnchorLocalLoader] Skipping excluded UUID on start: {s}");
					continue;
				}
				if (Guid.TryParse(s, out var guid))
				{
					guids.Add(guid);
				}
				else
				{
					Debug.LogWarning($"[SpatialAnchorLocalLoader] Invalid UUID string skipped: {raw}");
				}
			}

			if (guids.Count == 0)
			{
				Debug.LogWarning("[SpatialAnchorLocalLoader] No valid UUIDs to load.");
				return;
			}

			if (useUnboundSpawning)
			{
				// Load poses and instantiate without binding, to allow grabbing/movement
				LoadAndSpawnAtAnchorsAsync(guids);
			}
			else
			{
				// Fallback to SDK loader (single prefab only)
				if (spatialAnchorCore == null) spatialAnchorCore = FindAnyObjectByType<SpatialAnchorCoreBuildingBlock>();
				if (spatialAnchorCore == null)
				{
					Debug.LogWarning("[SpatialAnchorLocalLoader] Cannot bind to anchors because SpatialAnchorCoreBuildingBlock is missing.");
					return;
				}
				spatialAnchorCore.LoadAndInstantiateAnchors(anchorPrefab, guids);
			}
		}

		private async void LoadAndSpawnAtAnchorsAsync(List<Guid> uuids)
		{
			var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
			var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, unboundAnchors);
			if (!result.Success || unboundAnchors.Count == 0)
			{
				Debug.LogWarning($"[SpatialAnchorLocalLoader] Failed to load anchors: {result.Status}. Falling back to default prefab at origin for visibility.");
				if (anchorPrefab != null)
				{
					Instantiate(anchorPrefab, Vector3.zero, Quaternion.identity);
				}
				return;
			}

			for (int i = 0; i < unboundAnchors.Count; i++)
			{
				var unbound = unboundAnchors[i];
				if (!unbound.Localized)
				{
					var localized = await unbound.LocalizeAsync();
					if (!localized)
					{
						Debug.LogWarning($"[SpatialAnchorLocalLoader] Failed to localize anchor: {unbound.Uuid}");
						continue;
					}
				}

				// Always resolve pose via a temporary bind for reliability
				var tempGo = new GameObject($"TempAnchor_{unbound.Uuid}");
				var tempAnchor = tempGo.AddComponent<OVRSpatialAnchor>();
				unbound.BindTo(tempAnchor);
				// wait a couple frames for tracking to update
				await Task.Yield();
				await Task.Yield();
				var pose = new Pose(tempAnchor.transform.position, tempAnchor.transform.rotation);
				Destroy(tempGo);
				//Debug.Log($"[SpatialAnchorLocalLoader] Pose resolved via temp bind for {unbound.Uuid} at {pose.position}");

				var prefabToUse = ResolvePrefabForUuid(unbound.Uuid.ToString());
				if (prefabToUse == null) prefabToUse = ResolvePrefabFromDatabase(unbound.Uuid.ToString());
				if (prefabToUse == null) prefabToUse = anchorPrefab;
				if (prefabToUse == null) continue;

				var go = Instantiate(prefabToUse, pose.position, pose.rotation, spawnParent);
				if (ensureGrabbableComponents) EnsureBasicGrabbable(go);
			}
		}

		[ContextMenu("Erase Anchors (device) From eraseOnStartUuids")]
		public void ContextEraseAnchorsFromEraseList()
		{
			_ = EraseAnchorsByUuidListAsync(eraseOnStartUuids);
		}

		private async Task EraseAnchorsByUuidListAsync(List<string> uuidStrings)
		{
			if (uuidStrings == null || uuidStrings.Count == 0) return;

			// Parse to Guid list
			var guids = new List<Guid>();
			for (int i = 0; i < uuidStrings.Count; i++)
			{
				var s = uuidStrings[i] == null ? string.Empty : uuidStrings[i].Trim();
				if (Guid.TryParse(s, out var g)) guids.Add(g);
				else Debug.LogWarning($"[SpatialAnchorLocalLoader] Invalid UUID in erase list skipped: {uuidStrings[i]}");
			}
			if (guids.Count == 0) return;

			// Load unbound anchors to erase
			var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
			var load = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(guids, unboundAnchors);
			if (!load.Success || unboundAnchors.Count == 0)
			{
				Debug.LogWarning($"[SpatialAnchorLocalLoader] Erase load failed or none found. Status={load.Status}");
				return;
			}

			// Erase each by binding to a temporary anchor and calling EraseAnchorAsync
			for (int i = 0; i < unboundAnchors.Count; i++)
			{
				var unbound = unboundAnchors[i];
				var tempGo = new GameObject($"TempErase_{unbound.Uuid}");
				var tempAnchor = tempGo.AddComponent<OVRSpatialAnchor>();
				unbound.BindTo(tempAnchor);
				await Task.Yield();
				var result = await tempAnchor.EraseAnchorAsync();
				UnityEngine.Object.Destroy(tempGo);
				if (!result.Success)
				{
					Debug.LogWarning($"[SpatialAnchorLocalLoader] Failed to erase {unbound.Uuid}: {result.Status}");
				}
				else
				{
					Debug.Log($"[SpatialAnchorLocalLoader] Erased {unbound.Uuid}");
				}
			}
		}

		private void RemoveUuidsFromLocalStorage(List<string> uuidStrings)
		{
			if (uuidStrings == null || uuidStrings.Count == 0) return;
			int playerUuidCount = PlayerPrefs.GetInt("numUuids", 0);
			for (int idx = 0; idx < uuidStrings.Count; idx++)
			{
				var target = uuidStrings[idx] == null ? string.Empty : uuidStrings[idx].Trim();
				if (string.IsNullOrEmpty(target)) continue;
				for (int i = 0; i < playerUuidCount; i++)
				{
					var key = "uuid" + i;
					var value = PlayerPrefs.GetString(key, "");
					if (string.Equals(value, target, StringComparison.OrdinalIgnoreCase))
					{
						var lastKey = "uuid" + (playerUuidCount - 1);
						var lastValue = PlayerPrefs.GetString(lastKey, "");
						PlayerPrefs.SetString(key, lastValue);
						PlayerPrefs.DeleteKey(lastKey);
						playerUuidCount--;
						if (playerUuidCount < 0) playerUuidCount = 0;
						PlayerPrefs.SetInt("numUuids", playerUuidCount);
						break;
					}
				}
			}
		}

		private GameObject ResolvePrefabForUuid(string uuid)
		{
			if (uuidPrefabMappings != null)
			{
				for (int i = 0; i < uuidPrefabMappings.Count; i++)
				{
					var key = uuidPrefabMappings[i].uuid == null ? string.Empty : uuidPrefabMappings[i].uuid.Trim();
					if (!string.IsNullOrEmpty(key) &&
						string.Equals(key, uuid.Trim(), StringComparison.OrdinalIgnoreCase) &&
						uuidPrefabMappings[i].prefab != null)
					{
						return uuidPrefabMappings[i].prefab;
					}
				}
			}
			return null;
		}

		private GameObject ResolvePrefabFromDatabase(string uuid)
		{
			if (uuidDatabase != null && uuidDatabase.mappings != null)
			{
				for (int i = 0; i < uuidDatabase.mappings.Count; i++)
				{
					var key = uuidDatabase.mappings[i].uuid == null ? string.Empty : uuidDatabase.mappings[i].uuid.Trim();
					if (!string.IsNullOrEmpty(key) && string.Equals(key, uuid.Trim(), StringComparison.OrdinalIgnoreCase))
					{
						return uuidDatabase.mappings[i].prefab;
					}
				}
			}
			return null;
		}

		[ContextMenu("Import UUIDs Into Mappings (from PlayerPrefs)")]
		public void ImportUuidsIntoMappings()
		{
			int count = PlayerPrefs.GetInt("numUuids", 0);
			var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < uuidPrefabMappings.Count; i++)
			{
				if (!string.IsNullOrEmpty(uuidPrefabMappings[i].uuid)) existing.Add(uuidPrefabMappings[i].uuid.Trim());
			}
			for (int i = 0; i < count; i++)
			{
				var key = "uuid" + i;
				var uuidStr = PlayerPrefs.GetString(key, string.Empty);
				uuidStr = string.IsNullOrEmpty(uuidStr) ? string.Empty : uuidStr.Trim();
				if (!string.IsNullOrEmpty(uuidStr) && !existing.Contains(uuidStr))
				{
					uuidPrefabMappings.Add(new UuidPrefabMapping { uuid = uuidStr, prefab = null });
				}
			}
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				UnityEditor.EditorUtility.SetDirty(this);
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
			}
			#endif
		}

		[ContextMenu("Save Loaded UUIDs To Inspector (persistent)")]
		public void SaveLoadedUuidsToInspector()
		{
			savedUuids.Clear();
			for (int i = 0; i < loadedUuids.Count; i++)
			{
				var s = loadedUuids[i] == null ? string.Empty : loadedUuids[i].Trim();
				if (!string.IsNullOrEmpty(s)) savedUuids.Add(s);
			}
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				UnityEditor.EditorUtility.SetDirty(this);
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
			}
			#endif
		}

		private void OnValidate()
		{
			#if UNITY_EDITOR
			if (!Application.isPlaying && uuidDatabase != null)
			{
				savedUuids.Clear();
				for (int i = 0; i < uuidDatabase.uuids.Count; i++)
				{
					var s = uuidDatabase.uuids[i] == null ? string.Empty : uuidDatabase.uuids[i].Trim();
					if (!string.IsNullOrEmpty(s)) savedUuids.Add(s);
				}
				UnityEditor.EditorUtility.SetDirty(this);
			}
			#endif
		}

		[ContextMenu("Save Loaded UUIDs To Asset")]
		public void SaveLoadedUuidsToAsset()
		{
			if (uuidDatabase == null)
			{
				#if UNITY_EDITOR
				uuidDatabase = CreateOrFindUuidDatabaseAsset();
				#else
				Debug.LogWarning("[SpatialAnchorLocalLoader] No UUID database asset assigned.");
				return;
				#endif
			}
			uuidDatabase.uuids.Clear();
			for (int i = 0; i < loadedUuids.Count; i++)
			{
				var s = loadedUuids[i] == null ? string.Empty : loadedUuids[i].Trim();
				if (!string.IsNullOrEmpty(s)) uuidDatabase.uuids.Add(s);
			}
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				UnityEditor.EditorUtility.SetDirty(uuidDatabase);
				UnityEditor.AssetDatabase.SaveAssets();
			}
			#endif
		}

		[ContextMenu("Load UUIDs From Asset Into Inspector")]
		public void LoadUuidsFromAssetIntoInspector()
		{
			if (uuidDatabase == null)
			{
				Debug.LogWarning("[SpatialAnchorLocalLoader] No UUID database asset assigned.");
				return;
			}
			savedUuids.Clear();
			for (int i = 0; i < uuidDatabase.uuids.Count; i++)
			{
				var s = uuidDatabase.uuids[i] == null ? string.Empty : uuidDatabase.uuids[i].Trim();
				if (!string.IsNullOrEmpty(s)) savedUuids.Add(s);
			}
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				UnityEditor.EditorUtility.SetDirty(this);
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
			}
			#endif
		}

		private void EnsureBasicGrabbable(GameObject go)
		{
			var rb = go.GetComponent<Rigidbody>();
			if (rb == null) rb = go.AddComponent<Rigidbody>();
			rb.isKinematic = spawnedIsKinematic;
			rb.useGravity = spawnedUseGravity;
			rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

			if (go.GetComponent<Collider>() == null)
			{
				go.AddComponent<BoxCollider>();
			}

			Debug.Log($"[SpatialAnchorLocalLoader] Spawned {go.name} with Rigidbody({(rb!=null)}) and Collider({(go.GetComponent<Collider>()!=null)})");
		}

		#if UNITY_EDITOR
		private SpatialAnchorUuidDatabase CreateOrFindUuidDatabaseAsset()
		{
			var path = "Assets/SpatialAnchorUuidDatabase.asset";
			var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<SpatialAnchorUuidDatabase>(path);
			if (asset == null)
			{
				asset = ScriptableObject.CreateInstance<SpatialAnchorUuidDatabase>();
				UnityEditor.AssetDatabase.CreateAsset(asset, path);
				UnityEditor.AssetDatabase.SaveAssets();
			}
			return asset;
		}
		#endif

		public IReadOnlyList<string> GetLoadedUuids()
		{
			return loadedUuids;
		}
	}
}


