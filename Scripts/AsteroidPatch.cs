using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Voxel;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace FixOreSensorLenses.Scripts
{
	[HarmonyPatch(typeof(Asteroid))]
	public static class ThingPatch
    {
		public static Dictionary<Vector3, Mineables> GetAllMineables(Asteroid asteroid)
        {
			Dictionary<Vector3, Mineables> mineables = null;
			try
            {
				mineables = (Dictionary<Vector3, Mineables>)typeof(Asteroid).GetField("_allMineables", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(asteroid);
            }
			catch (Exception ex)
            {
				Debug.LogError(ModReference.Name + ": Cannot get _allMineables from Asteroid via reflection: " + ex.Message);
            }

			return mineables;
        }

		[HarmonyPatch("PopulateMineable")]
		[HarmonyPrefix]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static bool PopulateMineablePrefix(Asteroid __instance, Vector3 voxelPosition, MineableType voxelType)
		{
			bool flag = voxelType <= MineableType.Stone || voxelType > (MineableType)200;
			if (!flag)
			{
				Mineables mineable = __instance.GetMineable(voxelPosition);
				bool flag2 = mineable != null;
				if (flag2)
				{
					bool flag3 = mineable.VoxelType == voxelType;
					if (flag3)
					{
						return false;
					}
					GetAllMineables(__instance).Remove(mineable.Position);
				}
				Mineables mineables = new Mineables(MiningManager.FindMineableType(voxelType), voxelPosition * ChunkObject.VoxelSize + this.Position, this)
				{
					LocalPosition = BitConverter.GetBytes(__instance.ChunkController.Vector2Offset[(int)voxelPosition.x, (int)voxelPosition.y, (int)voxelPosition.z])
				};
				GetAllMineables(__instance)[voxelPosition] = mineables;
				MeshRenderer meshRenderer;
				MiningManager.MineableGoggleVisualizers.TryGetValue(mineables, out meshRenderer);
				Mesh mesh = null;
				Material material = null;
				bool flag4 = meshRenderer != null;
				if (flag4)
				{
					MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
					mesh = ((component != null) ? component.sharedMesh : null);
					material = meshRenderer.sharedMaterial;
				}
				bool flag5 = mesh == null || material == null;
				InstancedIndirectDrawCall instancedIndirectDrawCall;
				if (flag5)
				{
					instancedIndirectDrawCall = InstancedIndirectDrawCall.FindOrAddDrawCall(__instance.MineableVisualizerDrawCalls, MiningManager.Instance.defaultMineableVisualizerMaterial, MiningManager.Instance.defaultMineableVisualizerMesh, 0, 0);
				}
				else
				{
					instancedIndirectDrawCall = InstancedIndirectDrawCall.FindOrAddDrawCall(__instance.MineableVisualizerDrawCalls, material, mesh, 0, 0);
				}
				Matrix4x4 objectToWorld = Matrix4x4.TRS(mineables.Position, Quaternion.identity, new Vector3(1f, 1f, 1f));
				instancedIndirectDrawCall.AddInstance(objectToWorld);

			}

			return false; // skip original method
		}

	}
}
