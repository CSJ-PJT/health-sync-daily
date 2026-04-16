using DeepStake.Characters;
using UnityEngine;

namespace DeepStake.World
{
    public static class WorldPrototypeVisualPass
    {
        private static Texture2D[] cachedTileTextures = System.Array.Empty<Texture2D>();
        private static Texture2D[] cachedWallTextures = System.Array.Empty<Texture2D>();
        private static bool environmentTexturesLoaded;

        private const string GeneratedVisualRootName = "__GeneratedZoneVisuals";
        private const string PropVisualRootName = "__PropVisual";
        private const string PropVisualVersionName = "__PropVisualVersion_42";
        private const string LegacyCuteProxyRootName = "__CutePlayerProxy";
        public static void EnsureCutePlayerProxy(Transform playerTransform)
        {
            if (playerTransform == null)
            {
                return;
            }

            var legacyRoot = playerTransform.Find(LegacyCuteProxyRootName);
            if (legacyRoot != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(legacyRoot.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(legacyRoot.gameObject);
                }
            }

            var view = playerTransform.GetComponent<ArticulatedHumanoidView>();
            if (view == null)
            {
                view = playerTransform.gameObject.AddComponent<ArticulatedHumanoidView>();
            }
            view.Configure(ArticulatedHumanoidRole.Player);
        }

        public static void EnsureArchivistProxy(Transform npcTransform)
        {
            if (npcTransform == null)
            {
                return;
            }

            var view = npcTransform.GetComponent<ArticulatedHumanoidView>();
            if (view == null)
            {
                view = npcTransform.gameObject.AddComponent<ArticulatedHumanoidView>();
            }
            view.Configure(ArticulatedHumanoidRole.Archivist);
        }

        public static void EnsureWorldPropVisual(Transform target, string kind)
        {
            if (target == null)
            {
                return;
            }

            HideLegacyRenderers(target);

            var propRoot = target.Find(PropVisualRootName);
            if (propRoot == null)
            {
                var propObject = new GameObject(PropVisualRootName);
                propObject.transform.SetParent(target, false);
                propRoot = propObject.transform;
            }

            var hasCurrentVersion = propRoot.Find(PropVisualVersionName) != null;
            if (propRoot.childCount > 0 && !hasCurrentVersion)
            {
                for (var index = propRoot.childCount - 1; index >= 0; index--)
                {
                    var child = propRoot.GetChild(index);
                    if (Application.isPlaying)
                    {
                        Object.Destroy(child.gameObject);
                    }
                    else
                    {
                        Object.DestroyImmediate(child.gameObject);
                    }
                }
            }
            else if (propRoot.childCount > 0)
            {
                return;
            }

            var versionMarker = new GameObject(PropVisualVersionName);
            versionMarker.transform.SetParent(propRoot, false);

            switch (kind)
            {
                case "farm-sign":
                    CreateSignProp(propRoot);
                    break;
                case "supply-cache":
                    CreateSupplyProp(propRoot);
                    break;
                case "observer-record":
                    CreateObserverProp(propRoot);
                    break;
                case "recovery-beacon":
                    CreateBeaconPreview(propRoot);
                    break;
                case "supply-relay":
                    CreateRelayPreview(propRoot);
                    break;
            }
        }

        private static void HideLegacyRenderers(Transform target)
        {
            var renderers = target.GetComponents<Renderer>();
            for (var index = 0; index < renderers.Length; index++)
            {
                if (renderers[index] != null)
                {
                    renderers[index].enabled = false;
                }
            }
        }

        public static void RebuildZoneVisuals(
            Transform zoneRoot,
            WorldPrototype3DDefinition definition,
            Material fieldMaterial,
            Material archiveMaterial,
            Material placementMaterial,
            Material roadMaterial,
            Material storageMaterial)
        {
            if (zoneRoot == null || definition == null)
            {
                return;
            }

            var generatedRoot = zoneRoot.Find(GeneratedVisualRootName);
            if (generatedRoot == null)
            {
                var generatedObject = new GameObject(GeneratedVisualRootName);
                generatedObject.transform.SetParent(zoneRoot, false);
                generatedRoot = generatedObject.transform;
            }

            for (var index = generatedRoot.childCount - 1; index >= 0; index--)
            {
                var child = generatedRoot.GetChild(index);
                if (Application.isPlaying)
                {
                    Object.Destroy(child.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            if (definition.landmarks == null)
            {
                return;
            }

            CreateBaseGroundLayers(generatedRoot);

            for (var index = 0; index < definition.landmarks.Length; index++)
            {
                CreateLandmarkVisual(
                    generatedRoot,
                    definition.landmarks[index],
                    fieldMaterial,
                    archiveMaterial,
                    placementMaterial,
                    roadMaterial,
                    storageMaterial);
            }
        }

        private static void CreateBaseGroundLayers(Transform root)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, "SiteBase", new Vector3(-0.5f, -0.05f, 0.2f), new Vector3(7.8f, 0.08f, 5.8f), new Color(0.38f, 0.39f, 0.4f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "FarmApron", new Vector3(-1.8f, 0.0f, 1.6f), new Vector3(4.5f, 0.03f, 3.8f), new Color(0.52f, 0.49f, 0.4f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "RoadBand", new Vector3(2.3f, -0.01f, -3.55f), new Vector3(5.3f, 0.02f, 0.95f), new Color(0.29f, 0.3f, 0.31f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "ArchiveYardPatch", new Vector3(2.7f, 0.01f, 3.8f), new Vector3(2.2f, 0.02f, 1.5f), new Color(0.48f, 0.48f, 0.4f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "SupplyLanePatch", new Vector3(-4.6f, 0.01f, -0.8f), new Vector3(1.9f, 0.02f, 1.6f), new Color(0.52f, 0.48f, 0.36f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "FieldPatchDark", new Vector3(-0.7f, 0.01f, 3.0f), new Vector3(2.4f, 0.02f, 1.2f), new Color(0.47f, 0.45f, 0.35f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "FieldPatchWarm", new Vector3(-3.7f, 0.01f, 1.8f), new Vector3(1.4f, 0.02f, 1.0f), new Color(0.58f, 0.52f, 0.38f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "MudPatchA", new Vector3(0.8f, 0.0f, 2.4f), new Vector3(1.2f, 0.02f, 0.8f), new Color(0.39f, 0.35f, 0.3f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "MudPatchB", new Vector3(-2.9f, 0.0f, -0.2f), new Vector3(0.8f, 0.02f, 0.6f), new Color(0.36f, 0.33f, 0.29f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "ArchiveWalk", new Vector3(3.2f, 0.0f, 1.5f), new Vector3(1.1f, 0.02f, 0.22f), new Color(0.45f, 0.43f, 0.38f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "WorkerLaneWalk", new Vector3(-3.3f, 0.0f, -1.2f), new Vector3(1.2f, 0.02f, 0.18f), new Color(0.44f, 0.41f, 0.34f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "TrampledStripA", new Vector3(-1.1f, 0.0f, 0.2f), new Vector3(1.05f, 0.015f, 0.12f), new Color(0.47f, 0.43f, 0.36f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "TrampledStripB", new Vector3(1.9f, 0.0f, 0.4f), new Vector3(0.95f, 0.015f, 0.1f), new Color(0.44f, 0.41f, 0.35f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "GrassPatchA", new Vector3(-4.8f, 0.0f, 2.7f), new Vector3(0.7f, 0.015f, 0.45f), new Color(0.34f, 0.39f, 0.29f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "GrassPatchB", new Vector3(4.8f, 0.0f, 3.6f), new Vector3(0.8f, 0.015f, 0.5f), new Color(0.35f, 0.4f, 0.3f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "PathEdgeNorth", new Vector3(0.9f, 0.01f, 1.4f), new Vector3(0.9f, 0.015f, 0.08f), new Color(0.48f, 0.44f, 0.37f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "PathEdgeSouth", new Vector3(0.5f, 0.01f, -1.8f), new Vector3(1.2f, 0.015f, 0.08f), new Color(0.43f, 0.4f, 0.34f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "SiteStoneA", new Vector3(5.1f, 0.04f, -2.2f), new Vector3(0.18f, 0.04f, 0.12f), new Color(0.46f, 0.46f, 0.45f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "SiteStoneB", new Vector3(-5.0f, 0.03f, 0.4f), new Vector3(0.16f, 0.03f, 0.1f), new Color(0.43f, 0.43f, 0.42f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "SiteStoneC", new Vector3(2.9f, 0.03f, 4.2f), new Vector3(0.14f, 0.03f, 0.09f), new Color(0.42f, 0.42f, 0.41f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "ArchiveRug", new Vector3(3.95f, 0.0f, 2.35f), new Vector3(0.7f, 0.01f, 0.18f), new Color(0.34f, 0.31f, 0.28f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "SupplyRutsA", new Vector3(-2.5f, 0.0f, -1.1f), new Vector3(0.9f, 0.01f, 0.05f), new Color(0.35f, 0.31f, 0.27f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "SupplyRutsB", new Vector3(-2.5f, 0.0f, -0.75f), new Vector3(0.9f, 0.01f, 0.05f), new Color(0.35f, 0.31f, 0.27f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "FieldPatchCool", new Vector3(1.8f, 0.0f, 3.2f), new Vector3(1.05f, 0.015f, 0.6f), new Color(0.33f, 0.38f, 0.31f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "FieldPatchDust", new Vector3(-0.8f, 0.0f, -2.4f), new Vector3(0.9f, 0.015f, 0.55f), new Color(0.44f, 0.4f, 0.34f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "ArchiveStep", new Vector3(3.55f, 0.04f, 1.95f), new Vector3(0.46f, 0.025f, 0.12f), new Color(0.43f, 0.4f, 0.36f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "WorkerMat", new Vector3(-4.1f, 0.0f, -1.0f), new Vector3(0.5f, 0.01f, 0.16f), new Color(0.31f, 0.28f, 0.24f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "BeaconLanePatch", new Vector3(0.55f, 0.0f, -1.2f), new Vector3(1.25f, 0.015f, 0.13f), new Color(0.46f, 0.42f, 0.34f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "ArchiveLanePatch", new Vector3(3.35f, 0.0f, 2.65f), new Vector3(0.7f, 0.015f, 0.12f), new Color(0.4f, 0.37f, 0.33f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "SupplyLanePatchDeep", new Vector3(-2.9f, 0.0f, -1.45f), new Vector3(1.15f, 0.015f, 0.12f), new Color(0.4f, 0.35f, 0.29f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "FieldMoundA", new Vector3(-4.6f, 0.08f, 3.5f), new Vector3(0.42f, 0.08f, 0.22f), new Color(0.46f, 0.42f, 0.33f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "FieldMoundB", new Vector3(-2.6f, 0.07f, 2.7f), new Vector3(0.38f, 0.07f, 0.2f), new Color(0.44f, 0.4f, 0.31f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "FieldPathEdgeA", new Vector3(-2.1f, 0.01f, 2.05f), new Vector3(0.72f, 0.014f, 0.06f), new Color(0.46f, 0.42f, 0.34f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "FieldPathEdgeB", new Vector3(-3.9f, 0.01f, 0.7f), new Vector3(0.64f, 0.014f, 0.06f), new Color(0.44f, 0.4f, 0.33f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "FieldPathEdgeC", new Vector3(-1.1f, 0.01f, 3.55f), new Vector3(0.54f, 0.014f, 0.06f), new Color(0.45f, 0.41f, 0.34f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "StorageEdgeA", new Vector3(-4.7f, 0.01f, -0.3f), new Vector3(0.52f, 0.014f, 0.06f), new Color(0.39f, 0.35f, 0.29f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "NorthBerm", new Vector3(-1.2f, 0.26f, 5.45f), new Vector3(5.1f, 0.22f, 0.38f), new Color(0.36f, 0.39f, 0.33f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "WestBerm", new Vector3(-6.65f, 0.22f, 0.8f), new Vector3(0.38f, 0.2f, 4.2f), new Color(0.35f, 0.37f, 0.32f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "SouthRoadLip", new Vector3(1.4f, 0.08f, -4.85f), new Vector3(5.8f, 0.05f, 0.22f), new Color(0.31f, 0.32f, 0.34f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "ServiceShoulder", new Vector3(4.9f, 0.0f, -1.7f), new Vector3(2.4f, 0.03f, 2.8f), new Color(0.43f, 0.44f, 0.4f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "WorkerLaneShadow", new Vector3(-3.2f, -0.01f, -1.7f), new Vector3(1.2f, 0.01f, 0.32f), new Color(0.41f, 0.39f, 0.33f));

            CreateTreeMass(root, "NorthTreeMassA", new Vector3(-4.4f, 0f, 5.5f), new Vector3(1.4f, 1.8f, 1.2f));
            CreateTreeMass(root, "NorthTreeMassB", new Vector3(1.8f, 0f, 5.7f), new Vector3(1.2f, 1.7f, 1.1f));
            CreateTreeMass(root, "NorthTreeMassC", new Vector3(5.1f, 0f, 5.5f), new Vector3(1.7f, 2.1f, 1.2f));

            CreateBackdropMass(root, "ArchiveBlockA", new Vector3(5.5f, 0f, 2.8f), new Vector3(1.5f, 2.6f, 1.6f), new Color(0.31f, 0.32f, 0.34f));
            CreateBackdropMass(root, "ArchiveBlockB", new Vector3(3.6f, 0f, 5.05f), new Vector3(2.4f, 2.3f, 1.0f), new Color(0.33f, 0.35f, 0.34f));
            CreateBackdropMass(root, "SupplyBlockA", new Vector3(-7.0f, 0f, -1.5f), new Vector3(1.1f, 1.8f, 1.3f), new Color(0.36f, 0.33f, 0.28f));
            CreateBackdropMass(root, "SupplyBlockB", new Vector3(-6.75f, 0f, 1.0f), new Vector3(1.1f, 1.55f, 2.1f), new Color(0.38f, 0.35f, 0.29f));
            CreateBackdropMass(root, "RoadMassWest", new Vector3(-5.8f, 0f, -4.8f), new Vector3(2.6f, 0.8f, 0.7f), new Color(0.31f, 0.32f, 0.33f));
            CreateBackdropMass(root, "RoadMassEast", new Vector3(7.3f, 0f, -4.7f), new Vector3(2.0f, 0.8f, 0.7f), new Color(0.31f, 0.32f, 0.33f));

            CreateFenceLine(root, "NorthFence", new Vector3(-4.8f, 0f, 4.85f), new Vector3(5.0f, 0f, 4.85f), new Color(0.47f, 0.41f, 0.31f));
            CreateFenceLine(root, "WestFence", new Vector3(-5.95f, 0f, -2.2f), new Vector3(-5.95f, 0f, 3.2f), new Color(0.45f, 0.39f, 0.3f));
            CreateFenceLine(root, "SouthBarrier", new Vector3(-3.0f, 0f, -4.25f), new Vector3(6.5f, 0f, -4.25f), new Color(0.34f, 0.35f, 0.37f));

            CreateSmallPropCluster(root, "WorkerBenchCluster", new Vector3(-3.9f, 0f, -1.5f), new Color(0.43f, 0.38f, 0.29f));
            CreateSmallPropCluster(root, "ArchiveCache", new Vector3(4.4f, 0f, 2.0f), new Color(0.41f, 0.39f, 0.34f));
            CreateSmallPropCluster(root, "SupplyScatter", new Vector3(-1.6f, 0f, -1.4f), new Color(0.42f, 0.36f, 0.28f));
            CreateLooseTarp(root, "WorkerTarp", new Vector3(-4.65f, 0f, -0.95f), new Color(0.24f, 0.3f, 0.32f));
            CreateLooseTarp(root, "ArchiveTarp", new Vector3(3.55f, 0f, 2.55f), new Color(0.26f, 0.29f, 0.31f));
            CreateFenceBreak(root, "FenceBreakWest", new Vector3(-5.95f, 0f, -0.7f), new Color(0.45f, 0.39f, 0.3f));
            CreateFenceBreak(root, "FenceBreakNorth", new Vector3(1.4f, 0f, 4.85f), new Color(0.47f, 0.41f, 0.31f));
            CreateStakeLine(root, "FieldStakeLineA", new Vector3(-4.1f, 0f, 2.3f), 4, new Vector3(0.5f, 0f, 0.1f), new Color(0.46f, 0.4f, 0.3f));
            CreateStakeLine(root, "FieldStakeLineB", new Vector3(-4.4f, 0f, 1.0f), 3, new Vector3(0.45f, 0f, 0.12f), new Color(0.44f, 0.38f, 0.28f));
            CreateCableSpool(root, "ArchiveSpool", new Vector3(2.75f, 0f, 2.05f), new Color(0.35f, 0.31f, 0.24f));
            CreateCableSpool(root, "WorkerSpool", new Vector3(-4.75f, 0f, -1.65f), new Color(0.33f, 0.29f, 0.22f));
            CreateCrateStack(root, "SupplyStackA", new Vector3(-1.1f, 0f, -1.8f), new Color(0.4f, 0.34f, 0.28f));
            CreateCrateStack(root, "ArchiveStackA", new Vector3(4.95f, 0f, 1.35f), new Color(0.38f, 0.34f, 0.29f));
            CreateMarkerFlag(root, "BeaconFlag", new Vector3(0.2f, 0f, -1.05f), new Color(0.63f, 0.56f, 0.36f));
            CreateMarkerFlag(root, "WorkerFlag", new Vector3(-3.0f, 0f, -1.3f), new Color(0.58f, 0.5f, 0.3f));
            CreateToolScatter(root, "WorkerTools", new Vector3(-4.35f, 0f, -1.85f), new Color(0.42f, 0.38f, 0.3f));
            CreatePaperScatter(root, "ArchivePapers", new Vector3(4.2f, 0f, 2.45f), new Color(0.66f, 0.61f, 0.52f));
            CreateSupplyMarker(root, "SupplyMarkerPost", new Vector3(-1.7f, 0f, -1.7f), new Color(0.55f, 0.47f, 0.28f));
            CreateArchiveShelf(root, "ArchiveShelfExt", new Vector3(5.2f, 0f, 2.1f), new Color(0.4f, 0.36f, 0.3f));
            CreateStackedPosts(root, "WorkerPosts", new Vector3(-4.9f, 0f, -0.25f), new Color(0.42f, 0.36f, 0.28f));
            CreateStackedPosts(root, "ArchivePosts", new Vector3(4.8f, 0f, 3.05f), new Color(0.4f, 0.35f, 0.29f));
            CreateGroundDebris(root, "WorkerDebris", new Vector3(-3.25f, 0f, -2.15f), new Color(0.39f, 0.35f, 0.3f));
            CreateGroundDebris(root, "ArchiveDebris", new Vector3(3.2f, 0f, 1.35f), new Color(0.42f, 0.39f, 0.34f));
            CreateGroundDebris(root, "FieldDebris", new Vector3(-2.2f, 0f, 3.8f), new Color(0.41f, 0.37f, 0.31f));
            CreateStakeBundle(root, "WorkerStakeBundle", new Vector3(-5.15f, 0f, -1.05f), new Color(0.44f, 0.38f, 0.28f));
            CreateStakeBundle(root, "FieldStakeBundle", new Vector3(-4.2f, 0f, 2.9f), new Color(0.46f, 0.4f, 0.3f));
            CreateCanCluster(root, "WorkerCans", new Vector3(-4.55f, 0f, -1.9f), new Color(0.46f, 0.48f, 0.46f));
            CreateCanCluster(root, "SupplyCans", new Vector3(-1.05f, 0f, -2.05f), new Color(0.48f, 0.49f, 0.47f));
            CreateClothLine(root, "ArchiveClothLine", new Vector3(3.1f, 0f, 3.55f), new Vector3(4.6f, 0f, 3.72f), new Color(0.43f, 0.44f, 0.42f));
            CreateClothLine(root, "WorkerClothLine", new Vector3(-4.9f, 0f, -0.45f), new Vector3(-3.55f, 0f, -0.25f), new Color(0.41f, 0.38f, 0.34f));
            CreateBackdropMass(root, "NorthHedge", new Vector3(-0.8f, 0f, 6.4f), new Vector3(6.8f, 1.4f, 0.9f), new Color(0.33f, 0.38f, 0.32f));
            CreateBackdropMass(root, "WorkerShackMass", new Vector3(-6.2f, 0f, -0.2f), new Vector3(1.9f, 1.6f, 1.6f), new Color(0.34f, 0.32f, 0.28f));
            CreateBackdropMass(root, "ArchiveStoreMass", new Vector3(5.8f, 0f, 4.15f), new Vector3(2.4f, 1.8f, 1.2f), new Color(0.33f, 0.35f, 0.34f));
            CreateCrateStack(root, "WorkerCratesB", new Vector3(-5.45f, 0f, -0.45f), new Color(0.39f, 0.34f, 0.28f));
            CreateCrateStack(root, "ArchiveCratesB", new Vector3(4.75f, 0f, 3.4f), new Color(0.38f, 0.34f, 0.3f));
            CreatePaperScatter(root, "ArchivePapersB", new Vector3(3.55f, 0f, 3.0f), new Color(0.68f, 0.64f, 0.58f));
            CreateToolScatter(root, "WorkerToolsB", new Vector3(-4.85f, 0f, -0.55f), new Color(0.41f, 0.37f, 0.31f));
            CreatePathPost(root, "BeaconGuideA", new Vector3(-0.2f, 0f, -0.8f), new Color(0.62f, 0.56f, 0.37f));
            CreatePathPost(root, "BeaconGuideB", new Vector3(0.9f, 0f, -1.6f), new Color(0.62f, 0.56f, 0.37f));
            CreatePathPost(root, "WorkerGuide", new Vector3(-2.4f, 0f, -1.6f), new Color(0.59f, 0.52f, 0.34f));
            CreatePathPost(root, "ArchiveGuide", new Vector3(2.95f, 0f, 1.85f), new Color(0.56f, 0.51f, 0.38f));
            CreateGroundDebris(root, "RoadDebrisA", new Vector3(2.1f, 0f, -3.9f), new Color(0.34f, 0.33f, 0.31f));
            CreateGroundDebris(root, "RoadDebrisB", new Vector3(-1.4f, 0f, -3.6f), new Color(0.36f, 0.35f, 0.33f));
            CreateStakeBundle(root, "ArchiveStakeBundle", new Vector3(4.2f, 0f, 2.2f), new Color(0.41f, 0.36f, 0.29f));
            CreateCanCluster(root, "ArchiveCans", new Vector3(5.0f, 0f, 2.7f), new Color(0.46f, 0.47f, 0.45f));
            CreateSmallPropCluster(root, "FieldTools", new Vector3(-2.8f, 0f, 2.6f), new Color(0.4f, 0.35f, 0.28f));
            CreateLooseTarp(root, "FieldTarpB", new Vector3(-1.7f, 0f, 3.4f), new Color(0.27f, 0.31f, 0.29f));
            CreateFenceLine(root, "ArchiveFenceShort", new Vector3(2.6f, 0f, 1.3f), new Vector3(5.2f, 0f, 1.3f), new Color(0.41f, 0.37f, 0.3f));
            CreateFenceLine(root, "WorkerFenceShort", new Vector3(-5.6f, 0f, -2.4f), new Vector3(-3.8f, 0f, -2.4f), new Color(0.39f, 0.35f, 0.29f));
            CreateBackdropMass(root, "FieldBackdropWest", new Vector3(-6.8f, 0f, 3.1f), new Vector3(1.8f, 1.2f, 1.4f), new Color(0.34f, 0.37f, 0.32f));
            CreateBackdropMass(root, "FieldBackdropEast", new Vector3(6.8f, 0f, 1.6f), new Vector3(1.7f, 1.1f, 1.5f), new Color(0.35f, 0.37f, 0.34f));
            CreateGroundDebris(root, "ArchiveEntryDebris", new Vector3(2.8f, 0f, 1.55f), new Color(0.39f, 0.37f, 0.34f));
            CreateGroundDebris(root, "WorkerEntryDebris", new Vector3(-3.6f, 0f, -1.85f), new Color(0.38f, 0.35f, 0.31f));
            CreateCanCluster(root, "FieldCans", new Vector3(-2.1f, 0f, 2.9f), new Color(0.45f, 0.46f, 0.44f));
            CreateToolScatter(root, "BeaconTools", new Vector3(0.5f, 0f, -1.85f), new Color(0.42f, 0.39f, 0.33f));
            CreateSmallPropCluster(root, "ArchiveBenchB", new Vector3(4.1f, 0f, 1.55f), new Color(0.39f, 0.35f, 0.29f));
            CreateSmallPropCluster(root, "WorkerBenchB", new Vector3(-4.95f, 0f, -1.05f), new Color(0.4f, 0.35f, 0.28f));
            CreateCrateStack(root, "BeaconSupplies", new Vector3(1.25f, 0f, -1.95f), new Color(0.41f, 0.36f, 0.3f));
            CreateLooseTarp(root, "BeaconTarp", new Vector3(1.45f, 0f, -1.35f), new Color(0.25f, 0.29f, 0.31f));
            CreateBackdropMass(root, "RoadBackdropSouth", new Vector3(0.2f, 0f, -5.8f), new Vector3(4.6f, 0.9f, 0.8f), new Color(0.31f, 0.32f, 0.33f));
            CreateBackdropMass(root, "ArchiveCanopyMass", new Vector3(3.5f, 0f, 4.9f), new Vector3(2.6f, 0.9f, 0.8f), new Color(0.35f, 0.36f, 0.35f));
            CreateFenceBreak(root, "RoadBreakWest", new Vector3(-2.6f, 0f, -4.25f), new Color(0.36f, 0.36f, 0.37f));
            CreateFenceBreak(root, "RoadBreakEast", new Vector3(3.9f, 0f, -4.25f), new Color(0.36f, 0.36f, 0.37f));
            CreateToolScatter(root, "ArchiveToolsB", new Vector3(4.7f, 0f, 2.0f), new Color(0.4f, 0.37f, 0.32f));
            CreatePaperScatter(root, "WorkerNotes", new Vector3(-3.9f, 0f, -1.15f), new Color(0.63f, 0.6f, 0.54f));
            CreateStakeLine(root, "BeaconStakeLine", new Vector3(-0.2f, 0f, -0.4f), 4, new Vector3(0.38f, 0f, -0.22f), new Color(0.52f, 0.45f, 0.3f));
            CreateBackdropMass(root, "ArchiveRearBlock", new Vector3(4.8f, 0f, 5.4f), new Vector3(2.4f, 1.3f, 0.9f), new Color(0.34f, 0.35f, 0.34f));
            CreateBackdropMass(root, "WorkerRearBlock", new Vector3(-6.6f, 0f, 1.2f), new Vector3(1.6f, 1.2f, 1.4f), new Color(0.35f, 0.33f, 0.29f));
            CreateCrateStack(root, "ArchiveSupplySide", new Vector3(5.4f, 0f, 2.6f), new Color(0.39f, 0.35f, 0.3f));
            CreateCrateStack(root, "WorkerSupplySide", new Vector3(-5.55f, 0f, -1.55f), new Color(0.4f, 0.34f, 0.28f));
            CreateLooseTarp(root, "ArchiveTarpB", new Vector3(4.55f, 0f, 2.8f), new Color(0.26f, 0.29f, 0.3f));
            CreateFenceLine(root, "BeaconFenceShort", new Vector3(-0.9f, 0f, -2.35f), new Vector3(1.9f, 0f, -2.35f), new Color(0.41f, 0.37f, 0.31f));
            CreateGroundDebris(root, "ArchiveRearDebris", new Vector3(4.95f, 0f, 4.2f), new Color(0.38f, 0.36f, 0.33f));
            CreateGroundDebris(root, "WorkerRearDebris", new Vector3(-5.95f, 0f, 0.2f), new Color(0.37f, 0.34f, 0.3f));
            CreatePathPost(root, "ArchiveBackGuide", new Vector3(4.35f, 0f, 3.25f), new Color(0.52f, 0.48f, 0.37f));
            CreatePathPost(root, "WorkerBackGuide", new Vector3(-4.95f, 0f, -0.15f), new Color(0.5f, 0.45f, 0.33f));
            CreateSmallPropCluster(root, "ArchiveRearBench", new Vector3(5.2f, 0f, 3.6f), new Color(0.38f, 0.34f, 0.29f));
            CreateSmallPropCluster(root, "WorkerRearBench", new Vector3(-5.85f, 0f, 0.75f), new Color(0.39f, 0.34f, 0.28f));
            CreateCanCluster(root, "BeaconCans", new Vector3(0.9f, 0f, -1.2f), new Color(0.47f, 0.48f, 0.46f));
            CreateGroundDebris(root, "BeaconRearDebris", new Vector3(0.1f, 0f, -2.2f), new Color(0.39f, 0.36f, 0.32f));
            CreateFenceLine(root, "FieldFenceEast", new Vector3(5.9f, 0f, 0.8f), new Vector3(5.9f, 0f, 3.9f), new Color(0.4f, 0.37f, 0.31f));
            CreateCrateStack(root, "ArchiveRearCrates", new Vector3(5.55f, 0f, 4.45f), new Color(0.38f, 0.34f, 0.29f));
            CreateCrateStack(root, "WorkerRearCrates", new Vector3(-6.2f, 0f, 0.35f), new Color(0.39f, 0.33f, 0.28f));
            CreateToolScatter(root, "BeaconToolsB", new Vector3(1.4f, 0f, -1.55f), new Color(0.41f, 0.37f, 0.32f));
            CreatePaperScatter(root, "ArchiveRearPapers", new Vector3(4.75f, 0f, 3.95f), new Color(0.66f, 0.63f, 0.58f));
            CreateGroundDebris(root, "FieldEdgeDebrisEast", new Vector3(5.4f, 0f, 2.55f), new Color(0.38f, 0.35f, 0.31f));
            CreateStakeBundle(root, "ArchiveStakeRear", new Vector3(5.95f, 0f, 4.95f), new Color(0.44f, 0.38f, 0.3f));
            CreateStakeBundle(root, "WorkerStakeRear", new Vector3(-6.55f, 0f, 0.95f), new Color(0.43f, 0.37f, 0.29f));
            CreateGroundDebris(root, "ArchiveSideDebris", new Vector3(5.9f, 0f, 2.9f), new Color(0.39f, 0.36f, 0.32f));
            CreateGroundDebris(root, "WorkerSideDebris", new Vector3(-5.9f, 0f, -0.85f), new Color(0.38f, 0.34f, 0.3f));
            CreateCanCluster(root, "ArchiveSideCans", new Vector3(5.45f, 0f, 2.55f), new Color(0.47f, 0.48f, 0.46f));
            CreateBackdropMass(root, "ArchiveRearMassB", new Vector3(6.4f, 0f, 4.95f), new Vector3(1.4f, 1.25f, 0.95f), new Color(0.33f, 0.34f, 0.34f));
            CreateBackdropMass(root, "WorkerRearMassB", new Vector3(-6.95f, 0f, 1.85f), new Vector3(1.25f, 1.15f, 1.05f), new Color(0.35f, 0.33f, 0.29f));
            CreateGroundDebris(root, "ArchiveOuterDebris", new Vector3(6.35f, 0f, 3.85f), new Color(0.39f, 0.36f, 0.32f));
            CreateGroundDebris(root, "WorkerOuterDebris", new Vector3(-6.55f, 0f, -0.15f), new Color(0.38f, 0.34f, 0.3f));
            CreateCanCluster(root, "WorkerRearCans", new Vector3(-6.45f, 0f, 0.55f), new Color(0.46f, 0.47f, 0.45f));
            CreatePathPost(root, "ArchiveOuterGuide", new Vector3(5.95f, 0f, 3.25f), new Color(0.51f, 0.46f, 0.34f));
            CreatePathPost(root, "WorkerOuterGuide", new Vector3(-6.15f, 0f, -0.45f), new Color(0.49f, 0.44f, 0.32f));
            CreateFenceLine(root, "ArchiveRearFence", new Vector3(4.5f, 0f, 5.15f), new Vector3(6.6f, 0f, 5.15f), new Color(0.4f, 0.37f, 0.31f));
            CreateFenceLine(root, "WorkerRearFence", new Vector3(-6.55f, 0f, -0.55f), new Vector3(-6.55f, 0f, 1.85f), new Color(0.39f, 0.35f, 0.3f));
            CreateBackdropMass(root, "ArchiveOuterMass", new Vector3(6.95f, 0f, 3.55f), new Vector3(1.1f, 1.1f, 1.05f), new Color(0.33f, 0.34f, 0.34f));
            CreateBackdropMass(root, "WorkerOuterMass", new Vector3(-6.95f, 0f, -0.85f), new Vector3(1.05f, 1.0f, 0.95f), new Color(0.35f, 0.33f, 0.29f));
            CreateGroundDebris(root, "ArchiveFenceDebris", new Vector3(5.15f, 0f, 5.0f), new Color(0.39f, 0.36f, 0.32f));
            CreateGroundDebris(root, "WorkerFenceDebris", new Vector3(-6.2f, 0f, 1.55f), new Color(0.38f, 0.34f, 0.3f));
            CreateCanCluster(root, "BeaconRearCans", new Vector3(1.5f, 0f, -2.0f), new Color(0.46f, 0.47f, 0.45f));
            CreateStakeBundle(root, "BeaconStakeRear", new Vector3(1.95f, 0f, -2.25f), new Color(0.44f, 0.38f, 0.29f));
            CreatePathPost(root, "FieldOuterGuide", new Vector3(-5.55f, 0f, 3.65f), new Color(0.5f, 0.45f, 0.33f));
            CreateGroundDebris(root, "RoadEdgeDebrisWest", new Vector3(-4.8f, 0f, -4.55f), new Color(0.37f, 0.35f, 0.33f));
            CreateGroundDebris(root, "RoadEdgeDebrisEast", new Vector3(5.6f, 0f, -4.45f), new Color(0.37f, 0.35f, 0.33f));
            CreateStakeBundle(root, "FieldOuterStakeA", new Vector3(-5.05f, 0f, 4.25f), new Color(0.44f, 0.39f, 0.3f));
            CreateStakeBundle(root, "FieldOuterStakeB", new Vector3(-3.6f, 0f, 4.65f), new Color(0.44f, 0.39f, 0.3f));
            CreateBackdropMass(root, "FieldNorthMassA", new Vector3(-3.4f, 0f, 6.05f), new Vector3(1.4f, 0.9f, 0.85f), new Color(0.34f, 0.38f, 0.33f));
            CreateBackdropMass(root, "FieldNorthMassB", new Vector3(2.9f, 0f, 6.0f), new Vector3(1.2f, 0.85f, 0.8f), new Color(0.34f, 0.38f, 0.33f));
            CreateSmallPropCluster(root, "RoadWestCluster", new Vector3(-4.25f, 0f, -4.05f), new Color(0.39f, 0.35f, 0.31f));
            CreateSmallPropCluster(root, "RoadEastCluster", new Vector3(5.2f, 0f, -4.0f), new Color(0.39f, 0.35f, 0.31f));
            CreateGroundDebris(root, "ArchiveMidDebris", new Vector3(4.35f, 0f, 2.95f), new Color(0.39f, 0.36f, 0.32f));
            CreateGroundDebris(root, "WorkerMidDebris", new Vector3(-4.9f, 0f, -0.95f), new Color(0.38f, 0.34f, 0.3f));
            CreateCanCluster(root, "RoadCansWest", new Vector3(-3.9f, 0f, -4.35f), new Color(0.46f, 0.47f, 0.45f));
            CreateCanCluster(root, "RoadCansEast", new Vector3(4.95f, 0f, -4.2f), new Color(0.46f, 0.47f, 0.45f));
            CreateGroundDebris(root, "FieldLinkDebrisA", new Vector3(-2.2f, 0f, 1.1f), new Color(0.4f, 0.36f, 0.31f));
            CreateGroundDebris(root, "FieldLinkDebrisB", new Vector3(1.7f, 0f, 1.4f), new Color(0.4f, 0.36f, 0.31f));
            CreateSmallPropCluster(root, "ArchiveLinkCluster", new Vector3(2.8f, 0f, 1.8f), new Color(0.39f, 0.35f, 0.31f));
            CreateSmallPropCluster(root, "WorkerLinkCluster", new Vector3(-2.4f, 0f, -1.1f), new Color(0.39f, 0.35f, 0.31f));
            CreatePathPost(root, "ArchiveMidGuide", new Vector3(2.2f, 0f, 1.5f), new Color(0.5f, 0.45f, 0.33f));
            CreatePathPost(root, "WorkerMidGuide", new Vector3(-2.0f, 0f, -1.25f), new Color(0.49f, 0.44f, 0.32f));
        }

        private static void CreateLandmarkVisual(
            Transform root,
            WorldPrototype3DLandmark landmark,
            Material fieldMaterial,
            Material archiveMaterial,
            Material placementMaterial,
            Material roadMaterial,
            Material storageMaterial)
        {
            switch (landmark.kind)
            {
                case "field":
                    CreateFieldPatch(root, landmark, PickMaterial(fieldMaterial, landmark.accent, new Color(0.46f, 0.45f, 0.33f)));
                    break;
                case "road":
                    CreateRoadPatch(root, landmark, PickMaterial(roadMaterial, landmark.accent, new Color(0.32f, 0.34f, 0.36f)));
                    break;
                case "archive":
                    CreateArchiveOutpost(root, landmark, PickMaterial(archiveMaterial, landmark.accent, new Color(0.36f, 0.41f, 0.45f)));
                    break;
                case "storage":
                    CreateStorageNode(root, landmark, PickMaterial(storageMaterial, landmark.accent, new Color(0.42f, 0.36f, 0.28f)));
                    break;
                case "placement-zone":
                    CreatePlacementRing(root, landmark, PickMaterial(placementMaterial, landmark.accent, new Color(0.56f, 0.5f, 0.3f)));
                    break;
                case "plot-row":
                    CreatePlotRowPatch(root, landmark, PickMaterial(fieldMaterial, landmark.accent, new Color(0.47f, 0.36f, 0.22f)));
                    break;
                case "fence":
                    CreateFence(root, landmark, PickMaterial(storageMaterial, landmark.accent, new Color(0.48f, 0.4f, 0.3f)));
                    break;
                case "canopy":
                    CreateCanopy(root, landmark, PickMaterial(archiveMaterial, landmark.accent, new Color(0.48f, 0.46f, 0.4f)));
                    break;
                case "signal-post":
                    CreateSignalPost(root, landmark, PickMaterial(placementMaterial, landmark.accent, new Color(0.78f, 0.68f, 0.38f)));
                    break;
                case "crate-cluster":
                    CreateCrateCluster(root, landmark, PickMaterial(storageMaterial, landmark.accent, new Color(0.54f, 0.44f, 0.3f)));
                    break;
                case "route-strip":
                    CreateRouteStrip(root, landmark, PickMaterial(placementMaterial, landmark.accent, new Color(0.58f, 0.52f, 0.34f)));
                    break;
                default:
                    CreateVisualPiece(root, PrimitiveType.Cube, landmark.label, new Vector3(landmark.x, landmark.height * 0.5f, landmark.z), new Vector3(landmark.width, landmark.height, landmark.depth), PickMaterial(fieldMaterial, landmark.accent, new Color(0.58f, 0.58f, 0.58f)));
                    break;
            }
        }

        private static void CreateFieldPatch(Transform root, WorldPrototype3DLandmark landmark, Material material)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_Base", new Vector3(landmark.x, landmark.height * 0.35f, landmark.z), new Vector3(landmark.width * 0.52f, landmark.height * 0.35f, landmark.depth * 0.52f), material);
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_Crown", new Vector3(landmark.x, landmark.height * 0.45f, landmark.z), new Vector3(landmark.width * 0.42f, landmark.height * 0.18f, landmark.depth * 0.42f), MultiplyColor(material, 1.06f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_EdgeNorth", new Vector3(landmark.x, landmark.height * 0.38f, landmark.z + landmark.depth * 0.42f), new Vector3(landmark.width * 0.24f, landmark.height * 0.1f, 0.08f), MultiplyColor(material, 0.92f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_EdgeSouth", new Vector3(landmark.x, landmark.height * 0.38f, landmark.z - landmark.depth * 0.42f), new Vector3(landmark.width * 0.24f, landmark.height * 0.1f, 0.08f), MultiplyColor(material, 0.92f));
        }

        private static void CreateRoadPatch(Transform root, WorldPrototype3DLandmark landmark, Material material)
        {
            CreateVisualPiece(root, PrimitiveType.Cube, landmark.label + "_Strip", new Vector3(landmark.x, 0.025f, landmark.z), new Vector3(landmark.width, 0.05f, landmark.depth), material);
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_ShoulderA", new Vector3(landmark.x, 0.03f, landmark.z - landmark.depth * 0.44f), new Vector3(landmark.width * 0.24f, 0.01f, 0.08f), MultiplyColor(material, 0.88f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_ShoulderB", new Vector3(landmark.x, 0.03f, landmark.z + landmark.depth * 0.44f), new Vector3(landmark.width * 0.24f, 0.01f, 0.08f), MultiplyColor(material, 0.88f));
        }

        private static void CreatePlotRowPatch(Transform root, WorldPrototype3DLandmark landmark, Material material)
        {
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_Ridge", new Vector3(landmark.x, landmark.height * 0.45f, landmark.z), new Vector3(landmark.width * 0.48f, landmark.height * 0.44f, landmark.depth * 0.34f), material);
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_FurrowA", new Vector3(landmark.x, landmark.height * 0.24f, landmark.z - landmark.depth * 0.3f), new Vector3(landmark.width * 0.42f, landmark.height * 0.12f, landmark.depth * 0.08f), MultiplyColor(material, 0.82f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_FurrowB", new Vector3(landmark.x, landmark.height * 0.24f, landmark.z + landmark.depth * 0.3f), new Vector3(landmark.width * 0.42f, landmark.height * 0.12f, landmark.depth * 0.08f), MultiplyColor(material, 0.82f));
        }

        private static void CreateRouteStrip(Transform root, WorldPrototype3DLandmark landmark, Material material)
        {
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_Path", new Vector3(landmark.x, landmark.height * 0.4f, landmark.z), new Vector3(landmark.width * 0.48f, landmark.height * 0.32f, landmark.depth * 0.48f), material);
            CreateVisualPiece(root, PrimitiveType.Sphere, landmark.label + "_CapA", new Vector3(landmark.x - landmark.width * 0.42f, landmark.height * 0.44f, landmark.z), new Vector3(0.12f, 0.12f, 0.12f), MultiplyColor(material, 1.08f));
            CreateVisualPiece(root, PrimitiveType.Sphere, landmark.label + "_CapB", new Vector3(landmark.x + landmark.width * 0.42f, landmark.height * 0.44f, landmark.z), new Vector3(0.12f, 0.12f, 0.12f), MultiplyColor(material, 1.08f));
        }

        private static void CreateFence(Transform root, WorldPrototype3DLandmark landmark, Material material)
        {
            var horizontal = landmark.width >= landmark.depth;
            var rail = CreateVisualPiece(
                root,
                PrimitiveType.Capsule,
                landmark.label,
                new Vector3(landmark.x, 0.52f, landmark.z),
                horizontal ? new Vector3(landmark.width * 0.5f, 0.05f, 0.05f) : new Vector3(0.05f, 0.05f, landmark.depth * 0.5f),
                material);
            rail.transform.localScale = horizontal
                ? new Vector3(landmark.width * 0.5f, 0.05f, 0.05f)
                : new Vector3(0.05f, 0.05f, landmark.depth * 0.5f);

            var isVertical = landmark.depth > landmark.width;
            var span = isVertical ? landmark.depth : landmark.width;
            var postCount = Mathf.Max(2, Mathf.RoundToInt(span / 1.4f));
            for (var index = 0; index < postCount; index++)
            {
                var t = postCount == 1 ? 0.5f : index / (float)(postCount - 1);
                var x = isVertical ? landmark.x : Mathf.Lerp(landmark.x - landmark.width * 0.5f, landmark.x + landmark.width * 0.5f, t);
                var z = isVertical ? Mathf.Lerp(landmark.z - landmark.depth * 0.5f, landmark.z + landmark.depth * 0.5f, t) : landmark.z;
                CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_Post_" + index, new Vector3(x, 0.52f, z), new Vector3(0.08f, 0.5f, 0.08f), material);
            }
        }

        private static void CreateSignProp(Transform root)
        {
            CreateVisualPiece(root, PrimitiveType.Capsule, "Board", new Vector3(0f, 0.42f, 0f), new Vector3(0.54f, 0.28f, 0.09f), new Color(0.5f, 0.47f, 0.4f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "Backing", new Vector3(0f, 0.4f, -0.06f), new Vector3(0.56f, 0.3f, 0.04f), new Color(0.26f, 0.22f, 0.18f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "PostLeft", new Vector3(-0.3f, -0.05f, 0f), new Vector3(0.05f, 0.56f, 0.05f), new Color(0.31f, 0.26f, 0.19f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "PostRight", new Vector3(0.3f, -0.05f, 0f), new Vector3(0.05f, 0.56f, 0.05f), new Color(0.31f, 0.26f, 0.19f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "Header", new Vector3(0f, 0.68f, 0.02f), new Vector3(0.42f, 0.04f, 0.05f), new Color(0.4f, 0.35f, 0.22f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "NoticeStrip", new Vector3(0f, 0.38f, 0.07f), new Vector3(0.33f, 0.03f, 0.02f), new Color(0.67f, 0.63f, 0.55f));
        }

        private static void CreateSupplyProp(Transform root)
        {
            CreateVisualPiece(root, PrimitiveType.Capsule, "CrateBase", new Vector3(0f, 0.2f, 0f), new Vector3(0.56f, 0.18f, 0.46f), new Color(0.43f, 0.35f, 0.24f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "CrateTop", new Vector3(0.18f, 0.54f, 0.08f), new Vector3(0.32f, 0.14f, 0.24f), new Color(0.5f, 0.39f, 0.25f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "Tarp", new Vector3(-0.08f, 0.82f, -0.04f), new Vector3(0.58f, 0.03f, 0.42f), new Color(0.22f, 0.29f, 0.32f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "StrapA", new Vector3(-0.2f, 0.54f, 0.26f), new Vector3(0.03f, 0.14f, 0.02f), new Color(0.28f, 0.24f, 0.18f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "StrapB", new Vector3(0.12f, 0.54f, -0.2f), new Vector3(0.03f, 0.14f, 0.02f), new Color(0.28f, 0.24f, 0.18f));
        }

        private static void CreateObserverProp(Transform root)
        {
            CreateVisualPiece(root, PrimitiveType.Capsule, "CaseBody", new Vector3(0f, 0.28f, 0f), new Vector3(0.42f, 0.2f, 0.3f), new Color(0.31f, 0.36f, 0.39f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "CaseLid", new Vector3(0f, 0.58f, -0.04f), new Vector3(0.44f, 0.05f, 0.32f), new Color(0.42f, 0.45f, 0.48f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "FileBundle", new Vector3(0f, 0.66f, 0.04f), new Vector3(0.22f, 0.04f, 0.12f), new Color(0.63f, 0.58f, 0.46f));
            CreateVisualPiece(root, PrimitiveType.Capsule, "Latch", new Vector3(0f, 0.46f, 0.29f), new Vector3(0.04f, 0.04f, 0.02f), new Color(0.54f, 0.5f, 0.42f));
        }

        private static void CreateBeaconPreview(Transform root)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, "Base", new Vector3(0f, 0.28f, 0f), new Vector3(0.3f, 0.28f, 0.3f), new Color(0.36f, 0.38f, 0.44f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "Tower", new Vector3(0f, 0.92f, 0f), new Vector3(0.08f, 0.64f, 0.08f), new Color(0.54f, 0.58f, 0.66f));
            CreateVisualPiece(root, PrimitiveType.Sphere, "Lamp", new Vector3(0f, 1.58f, 0f), new Vector3(0.24f, 0.24f, 0.24f), new Color(0.88f, 0.78f, 0.44f));
        }

        private static void CreateRelayPreview(Transform root)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, "Base", new Vector3(0f, 0.18f, 0f), new Vector3(0.32f, 0.18f, 0.32f), new Color(0.34f, 0.38f, 0.44f));
            CreateVisualPiece(root, PrimitiveType.Cube, "RelayBox", new Vector3(0f, 0.54f, 0f), new Vector3(0.42f, 0.34f, 0.28f), new Color(0.48f, 0.52f, 0.58f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, "Antenna", new Vector3(0f, 1.02f, 0f), new Vector3(0.05f, 0.42f, 0.05f), new Color(0.64f, 0.58f, 0.4f));
        }

        private static void CreateArchiveOutpost(Transform root, WorldPrototype3DLandmark landmark, Material material)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_Floor", new Vector3(landmark.x, 0.08f, landmark.z), new Vector3((landmark.width + 0.3f) * 0.48f, 0.08f, (landmark.depth + 0.2f) * 0.48f), MultiplyColor(material, 0.88f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_RearWall", new Vector3(landmark.x, 1.05f, landmark.z - landmark.depth * 0.34f), new Vector3(landmark.width * 0.48f, 0.88f, 0.09f), material);
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_Desk", new Vector3(landmark.x - 0.6f, 0.42f, landmark.z + 0.5f), new Vector3(0.62f, 0.32f, 0.24f), MultiplyColor(material, 1.08f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_Shelf", new Vector3(landmark.x + 0.95f, 0.82f, landmark.z + 0.25f), new Vector3(0.22f, 0.58f, 0.16f), MultiplyColor(material, 0.94f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_Ledger", new Vector3(landmark.x - 0.35f, 0.94f, landmark.z + 0.52f), new Vector3(0.14f, 0.02f, 0.09f), new Color(0.84f, 0.78f, 0.62f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_PinBoard", new Vector3(landmark.x + 1.05f, 1.42f, landmark.z - 0.2f), new Vector3(0.16f, 0.19f, 0.02f), new Color(0.46f, 0.4f, 0.26f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_PaperA", new Vector3(landmark.x + 0.98f, 1.48f, landmark.z - 0.17f), new Vector3(0.05f, 0.06f, 0.01f), new Color(0.78f, 0.74f, 0.66f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_DeskRoll", new Vector3(landmark.x - 0.9f, 0.42f, landmark.z + 0.7f), new Vector3(0.18f, 0.12f, 0.18f), new Color(0.48f, 0.41f, 0.29f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_CanopyPoleA", new Vector3(landmark.x - 1.35f, 0.95f, landmark.z + 1.12f), new Vector3(0.06f, 0.95f, 0.06f), MultiplyColor(material, 0.86f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_CanopyPoleB", new Vector3(landmark.x + 1.35f, 0.95f, landmark.z + 1.12f), new Vector3(0.06f, 0.95f, 0.06f), MultiplyColor(material, 0.86f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_Canopy", new Vector3(landmark.x, 1.86f, landmark.z + 0.54f), new Vector3(1.46f, 0.08f, 0.92f), new Color(0.46f, 0.46f, 0.4f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_Chair", new Vector3(landmark.x - 0.98f, 0.2f, landmark.z + 0.86f), new Vector3(0.13f, 0.17f, 0.13f), new Color(0.33f, 0.29f, 0.22f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_BoxStack", new Vector3(landmark.x + 1.28f, 0.38f, landmark.z + 0.86f), new Vector3(0.24f, 0.22f, 0.22f), new Color(0.42f, 0.36f, 0.28f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_FloorRoll", new Vector3(landmark.x + 1.0f, 0.12f, landmark.z + 0.95f), new Vector3(0.16f, 0.06f, 0.12f), new Color(0.29f, 0.31f, 0.34f));
        }

        private static void CreateStorageNode(Transform root, WorldPrototype3DLandmark landmark, Material material)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_Pad", new Vector3(landmark.x, 0.08f, landmark.z), new Vector3((landmark.width + 0.2f) * 0.48f, 0.07f, (landmark.depth + 0.2f) * 0.48f), MultiplyColor(material, 0.84f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_ShelfA", new Vector3(landmark.x - 0.55f, 0.72f, landmark.z), new Vector3(0.09f, 0.52f, landmark.depth * 0.46f), MultiplyColor(material, 0.92f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_ShelfB", new Vector3(landmark.x + 0.55f, 0.72f, landmark.z), new Vector3(0.09f, 0.52f, landmark.depth * 0.46f), MultiplyColor(material, 0.92f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_TopRail", new Vector3(landmark.x, 1.18f, landmark.z), new Vector3(landmark.width * 0.48f, 0.04f, landmark.depth * 0.1f), MultiplyColor(material, 1.04f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_CrateA", new Vector3(landmark.x - 0.35f, 0.36f, landmark.z - 0.25f), new Vector3(0.28f, 0.22f, 0.28f), material);
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_CrateB", new Vector3(landmark.x + 0.3f, 0.64f, landmark.z + 0.18f), new Vector3(0.22f, 0.18f, 0.22f), MultiplyColor(material, 1.08f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_Tarp", new Vector3(landmark.x, 1.34f, landmark.z - 0.08f), new Vector3(landmark.width * 0.55f, 0.04f, landmark.depth * 0.42f), new Color(0.28f, 0.38f, 0.44f));
            CreateVisualPiece(root, PrimitiveType.Cube, landmark.label + "_Pallet", new Vector3(landmark.x - 0.1f, 0.13f, landmark.z + 0.52f), new Vector3(0.68f, 0.08f, 0.28f), new Color(0.34f, 0.28f, 0.2f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_MarkerPole", new Vector3(landmark.x - 1.02f, 0.72f, landmark.z + 0.82f), new Vector3(0.06f, 0.72f, 0.06f), new Color(0.56f, 0.49f, 0.32f));
            CreateVisualPiece(root, PrimitiveType.Sphere, landmark.label + "_MarkerLamp", new Vector3(landmark.x - 1.02f, 1.48f, landmark.z + 0.82f), new Vector3(0.16f, 0.16f, 0.16f), new Color(0.8f, 0.72f, 0.42f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_Bag", new Vector3(landmark.x + 0.98f, 0.24f, landmark.z - 0.58f), new Vector3(0.22f, 0.14f, 0.16f), new Color(0.34f, 0.31f, 0.24f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_BagB", new Vector3(landmark.x + 0.72f, 0.2f, landmark.z - 0.28f), new Vector3(0.18f, 0.12f, 0.13f), new Color(0.32f, 0.29f, 0.23f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_ShelfRoll", new Vector3(landmark.x - 0.54f, 1.06f, landmark.z + 0.38f), new Vector3(0.1f, 0.06f, 0.08f), new Color(0.3f, 0.33f, 0.35f));
        }

        private static void CreateTreeMass(Transform root, string label, Vector3 basePosition, Vector3 canopyScale)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_Trunk", basePosition + new Vector3(0f, 0.55f, 0f), new Vector3(0.12f, 0.55f, 0.12f), new Color(0.33f, 0.27f, 0.21f));
            CreateVisualPiece(root, PrimitiveType.Sphere, label + "_CanopyA", basePosition + new Vector3(0f, 1.55f, 0f), canopyScale, new Color(0.35f, 0.4f, 0.32f));
            CreateVisualPiece(root, PrimitiveType.Sphere, label + "_CanopyB", basePosition + new Vector3(-0.35f, 1.3f, 0.1f), canopyScale * 0.68f, new Color(0.32f, 0.37f, 0.29f));
            CreateVisualPiece(root, PrimitiveType.Sphere, label + "_CanopyC", basePosition + new Vector3(0.34f, 1.22f, -0.12f), canopyScale * 0.62f, new Color(0.36f, 0.41f, 0.33f));
        }

        private static void CreateBackdropMass(Transform root, string label, Vector3 basePosition, Vector3 scale, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_Main", basePosition + new Vector3(0f, scale.y * 0.5f, 0f), new Vector3(scale.x * 0.48f, scale.y * 0.44f, scale.z * 0.38f), color);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_Side", basePosition + new Vector3(scale.x * 0.18f, scale.y * 0.38f, scale.z * 0.14f), new Vector3(scale.x * 0.28f, scale.y * 0.28f, scale.z * 0.24f), color * 0.96f);
        }

        private static void CreateFenceLine(Transform root, string label, Vector3 from, Vector3 to, Color color)
        {
            var segments = 5;
            for (var index = 0; index <= segments; index++)
            {
                var t = index / (float)segments;
                var position = Vector3.Lerp(from, to, t);
                CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_Post_" + index, position + new Vector3(0f, 0.45f, 0f), new Vector3(0.05f, 0.45f, 0.05f), color * 0.95f);
                if (index < segments)
                {
                    var next = Vector3.Lerp(from, to, (index + 1) / (float)segments);
                    var mid = (position + next) * 0.5f;
                    var span = Vector3.Distance(position, next);
                    var horizontal = Mathf.Abs(to.x - from.x) > Mathf.Abs(to.z - from.z);
                    CreateVisualPiece(root, PrimitiveType.Capsule, label + "_RailA_" + index, mid + new Vector3(0f, 0.62f, 0f), horizontal ? new Vector3(span * 0.26f, 0.03f, 0.03f) : new Vector3(0.03f, 0.03f, span * 0.26f), color);
                    CreateVisualPiece(root, PrimitiveType.Capsule, label + "_RailB_" + index, mid + new Vector3(0f, 0.38f, 0f), horizontal ? new Vector3(span * 0.26f, 0.03f, 0.03f) : new Vector3(0.03f, 0.03f, span * 0.26f), color * 0.94f);
                }
            }
        }

        private static void CreateSmallPropCluster(Transform root, string label, Vector3 basePosition, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_Bench", basePosition + new Vector3(0f, 0.24f, 0f), new Vector3(0.64f, 0.12f, 0.26f), color);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_BoxA", basePosition + new Vector3(-0.46f, 0.18f, 0.3f), new Vector3(0.18f, 0.2f, 0.18f), color * 0.9f);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_BoxB", basePosition + new Vector3(0.42f, 0.14f, -0.24f), new Vector3(0.16f, 0.18f, 0.16f), color * 1.06f);
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_Lantern", basePosition + new Vector3(0.62f, 0.46f, 0.18f), new Vector3(0.07f, 0.24f, 0.07f), new Color(0.6f, 0.54f, 0.38f));
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_Plank", basePosition + new Vector3(-0.18f, 0.08f, -0.28f), new Vector3(0.28f, 0.03f, 0.06f), color * 0.82f);
        }

        private static void CreateLooseTarp(Transform root, string label, Vector3 basePosition, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_Main", basePosition + new Vector3(0f, 0.06f, 0f), new Vector3(0.55f, 0.03f, 0.36f), color);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_Fold", basePosition + new Vector3(0.16f, 0.08f, -0.06f), new Vector3(0.2f, 0.03f, 0.12f), color * 1.06f);
        }

        private static void CreateFenceBreak(Transform root, string label, Vector3 basePosition, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_PostA", basePosition + new Vector3(-0.22f, 0.32f, 0f), new Vector3(0.05f, 0.32f, 0.05f), color);
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_PostB", basePosition + new Vector3(0.22f, 0.26f, 0f), new Vector3(0.05f, 0.26f, 0.05f), color);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_Rail", basePosition + new Vector3(0f, 0.36f, 0f), new Vector3(0.22f, 0.025f, 0.03f), color * 0.92f);
        }

        private static void CreateStakeLine(Transform root, string label, Vector3 startPosition, int count, Vector3 step, Color color)
        {
            for (var index = 0; index < count; index++)
            {
                var position = startPosition + step * index;
                CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_Stake_" + index, position + new Vector3(0f, 0.16f, 0f), new Vector3(0.03f, 0.16f, 0.03f), color);
                CreateVisualPiece(root, PrimitiveType.Capsule, label + "_Tie_" + index, position + new Vector3(0.05f, 0.22f, 0.03f), new Vector3(0.06f, 0.015f, 0.015f), color * 0.88f);
            }
        }

        private static void CreateCableSpool(Transform root, string label, Vector3 basePosition, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_Roll", basePosition + new Vector3(0f, 0.16f, 0f), new Vector3(0.18f, 0.16f, 0.18f), color);
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_CapA", basePosition + new Vector3(0f, 0.16f, -0.12f), new Vector3(0.22f, 0.01f, 0.22f), color * 0.9f);
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_CapB", basePosition + new Vector3(0f, 0.16f, 0.12f), new Vector3(0.22f, 0.01f, 0.22f), color * 0.9f);
        }

        private static void CreateCrateStack(Transform root, string label, Vector3 basePosition, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_Base", basePosition + new Vector3(0f, 0.16f, 0f), new Vector3(0.24f, 0.16f, 0.2f), color);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_Top", basePosition + new Vector3(0.14f, 0.42f, 0.02f), new Vector3(0.18f, 0.14f, 0.16f), color * 1.04f);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_Tie", basePosition + new Vector3(-0.1f, 0.22f, 0.16f), new Vector3(0.02f, 0.12f, 0.02f), color * 0.84f);
        }

        private static void CreateMarkerFlag(Transform root, string label, Vector3 basePosition, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_Pole", basePosition + new Vector3(0f, 0.36f, 0f), new Vector3(0.03f, 0.36f, 0.03f), color * 0.82f);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_Cloth", basePosition + new Vector3(0.1f, 0.56f, 0f), new Vector3(0.14f, 0.05f, 0.02f), color);
        }

        private static void CreateToolScatter(Transform root, string label, Vector3 basePosition, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_HandleA", basePosition + new Vector3(0f, 0.06f, 0f), new Vector3(0.12f, 0.015f, 0.02f), color);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_HeadA", basePosition + new Vector3(0.12f, 0.08f, 0f), new Vector3(0.04f, 0.025f, 0.025f), color * 1.08f);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_HandleB", basePosition + new Vector3(-0.16f, 0.05f, 0.08f), new Vector3(0.1f, 0.015f, 0.02f), color * 0.92f);
        }

        private static void CreatePaperScatter(Transform root, string label, Vector3 basePosition, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_SheetA", basePosition + new Vector3(0f, 0.02f, 0f), new Vector3(0.12f, 0.004f, 0.08f), color);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_SheetB", basePosition + new Vector3(0.1f, 0.025f, -0.05f), new Vector3(0.1f, 0.004f, 0.07f), color * 0.96f);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_SheetC", basePosition + new Vector3(-0.08f, 0.018f, 0.06f), new Vector3(0.08f, 0.004f, 0.06f), color * 0.92f);
        }

        private static void CreateSupplyMarker(Transform root, string label, Vector3 basePosition, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_Pole", basePosition + new Vector3(0f, 0.44f, 0f), new Vector3(0.04f, 0.44f, 0.04f), color * 0.84f);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_Panel", basePosition + new Vector3(0.12f, 0.7f, 0f), new Vector3(0.16f, 0.05f, 0.02f), color);
        }

        private static void CreateArchiveShelf(Transform root, string label, Vector3 basePosition, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_PostA", basePosition + new Vector3(-0.18f, 0.46f, 0f), new Vector3(0.04f, 0.46f, 0.04f), color);
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_PostB", basePosition + new Vector3(0.18f, 0.46f, 0f), new Vector3(0.04f, 0.46f, 0.04f), color);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_ShelfA", basePosition + new Vector3(0f, 0.38f, 0f), new Vector3(0.24f, 0.025f, 0.08f), color * 0.96f);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_ShelfB", basePosition + new Vector3(0f, 0.68f, 0f), new Vector3(0.24f, 0.025f, 0.08f), color * 0.96f);
        }

        private static void CreateStackedPosts(Transform root, string label, Vector3 basePosition, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_A", basePosition + new Vector3(0f, 0.2f, 0f), new Vector3(0.08f, 0.2f, 0.08f), color);
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_B", basePosition + new Vector3(0.18f, 0.14f, 0.06f), new Vector3(0.06f, 0.14f, 0.06f), color * 0.94f);
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_C", basePosition + new Vector3(-0.14f, 0.12f, -0.04f), new Vector3(0.05f, 0.12f, 0.05f), color * 0.88f);
        }

        private static void CreateGroundDebris(Transform root, string label, Vector3 basePosition, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_A", basePosition + new Vector3(0f, 0.015f, 0f), new Vector3(0.16f, 0.008f, 0.03f), color);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_B", basePosition + new Vector3(0.12f, 0.018f, -0.06f), new Vector3(0.09f, 0.008f, 0.02f), color * 0.92f);
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_Stone", basePosition + new Vector3(-0.1f, 0.02f, 0.05f), new Vector3(0.05f, 0.02f, 0.03f), color * 1.04f);
        }

        private static void CreateStakeBundle(Transform root, string label, Vector3 basePosition, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_A", basePosition + new Vector3(-0.05f, 0.22f, 0f), new Vector3(0.025f, 0.22f, 0.025f), color);
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_B", basePosition + new Vector3(0.02f, 0.2f, 0.04f), new Vector3(0.025f, 0.2f, 0.025f), color * 0.96f);
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_C", basePosition + new Vector3(0.08f, 0.18f, -0.03f), new Vector3(0.025f, 0.18f, 0.025f), color * 0.92f);
        }

        private static void CreateCanCluster(Transform root, string label, Vector3 basePosition, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_CanA", basePosition + new Vector3(0f, 0.08f, 0f), new Vector3(0.06f, 0.08f, 0.06f), color);
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_CanB", basePosition + new Vector3(0.12f, 0.07f, -0.03f), new Vector3(0.055f, 0.07f, 0.055f), color * 0.94f);
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_CanC", basePosition + new Vector3(-0.1f, 0.065f, 0.04f), new Vector3(0.05f, 0.065f, 0.05f), color * 1.02f);
        }

        private static void CreateClothLine(Transform root, string label, Vector3 from, Vector3 to, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_PoleA", from + new Vector3(0f, 0.9f, 0f), new Vector3(0.04f, 0.9f, 0.04f), color * 0.82f);
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_PoleB", to + new Vector3(0f, 0.86f, 0f), new Vector3(0.04f, 0.86f, 0.04f), color * 0.82f);
            var mid = (from + to) * 0.5f;
            var span = Vector3.Distance(from, to);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_Line", mid + new Vector3(0f, 1.12f, 0f), new Vector3(span * 0.26f, 0.012f, 0.012f), color * 0.7f);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_ClothA", mid + new Vector3(-0.18f, 1.0f, 0.02f), new Vector3(0.11f, 0.05f, 0.02f), color * 1.04f);
            CreateVisualPiece(root, PrimitiveType.Capsule, label + "_ClothB", mid + new Vector3(0.15f, 0.97f, -0.02f), new Vector3(0.09f, 0.045f, 0.02f), color * 0.96f);
        }

        private static void CreatePathPost(Transform root, string label, Vector3 basePosition, Color color)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, label + "_Post", basePosition + new Vector3(0f, 0.32f, 0f), new Vector3(0.05f, 0.32f, 0.05f), color * 0.9f);
            CreateVisualPiece(root, PrimitiveType.Sphere, label + "_Cap", basePosition + new Vector3(0f, 0.68f, 0f), new Vector3(0.11f, 0.11f, 0.11f), color);
        }

        private static void CreatePlacementRing(Transform root, WorldPrototype3DLandmark landmark, Material material)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_Base", new Vector3(landmark.x, 0.02f, landmark.z), new Vector3(landmark.width * 0.5f, 0.03f, landmark.depth * 0.5f), material);
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_Core", new Vector3(landmark.x, 0.045f, landmark.z), new Vector3(landmark.width * 0.28f, 0.025f, landmark.depth * 0.28f), MultiplyColor(material, 1.15f));
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_PostA", new Vector3(landmark.x - landmark.width * 0.32f, 0.3f, landmark.z), new Vector3(0.06f, 0.28f, 0.06f), material);
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_PostB", new Vector3(landmark.x + landmark.width * 0.32f, 0.3f, landmark.z), new Vector3(0.06f, 0.28f, 0.06f), material);
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_GuideStripA", new Vector3(landmark.x, 0.02f, landmark.z - landmark.depth * 0.56f), new Vector3(landmark.width * 0.34f, 0.01f, 0.08f), MultiplyColor(material, 0.9f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_GuideStripB", new Vector3(landmark.x, 0.02f, landmark.z + landmark.depth * 0.56f), new Vector3(landmark.width * 0.34f, 0.01f, 0.08f), MultiplyColor(material, 0.9f));
            CreateVisualPiece(root, PrimitiveType.Sphere, landmark.label + "_GuideLampA", new Vector3(landmark.x - landmark.width * 0.44f, 0.18f, landmark.z - 0.08f), new Vector3(0.12f, 0.12f, 0.12f), new Color(0.8f, 0.72f, 0.44f));
            CreateVisualPiece(root, PrimitiveType.Sphere, landmark.label + "_GuideLampB", new Vector3(landmark.x + landmark.width * 0.44f, 0.18f, landmark.z + 0.08f), new Vector3(0.12f, 0.12f, 0.12f), new Color(0.8f, 0.72f, 0.44f));
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_RingDetail", new Vector3(landmark.x, 0.08f, landmark.z), new Vector3(landmark.width * 0.22f, 0.015f, landmark.depth * 0.22f), MultiplyColor(material, 1.12f));
        }

        private static void CreateCanopy(Transform root, WorldPrototype3DLandmark landmark, Material material)
        {
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label, new Vector3(landmark.x, 1.8f, landmark.z), new Vector3(landmark.width * 0.5f, 0.08f, landmark.depth * 0.42f), material);
            var dx = Mathf.Max(0.3f, landmark.width * 0.45f);
            var dz = Mathf.Max(0.3f, landmark.depth * 0.45f);
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_PoleA", new Vector3(landmark.x - dx, 0.9f, landmark.z - dz), new Vector3(0.08f, 0.9f, 0.08f), material);
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_PoleB", new Vector3(landmark.x + dx, 0.9f, landmark.z - dz), new Vector3(0.08f, 0.9f, 0.08f), material);
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_PoleC", new Vector3(landmark.x - dx, 0.9f, landmark.z + dz), new Vector3(0.08f, 0.9f, 0.08f), material);
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_PoleD", new Vector3(landmark.x + dx, 0.9f, landmark.z + dz), new Vector3(0.08f, 0.9f, 0.08f), material);
        }

        private static void CreateSignalPost(Transform root, WorldPrototype3DLandmark landmark, Material material)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_Pole", new Vector3(landmark.x, landmark.height * 0.5f, landmark.z), new Vector3(0.1f, landmark.height * 0.5f, 0.1f), material);
            CreateVisualPiece(root, PrimitiveType.Sphere, landmark.label + "_Lamp", new Vector3(landmark.x, landmark.height + 0.18f, landmark.z), new Vector3(0.26f, 0.26f, 0.26f), PickMaterial(material, "signal", new Color(1f, 0.9f, 0.54f)));
        }

        private static void CreateCrateCluster(Transform root, WorldPrototype3DLandmark landmark, Material material)
        {
            CreateVisualPiece(root, PrimitiveType.Cylinder, landmark.label + "_Base", new Vector3(landmark.x, 0.12f, landmark.z), new Vector3(landmark.width * 0.5f, 0.09f, landmark.depth * 0.5f), material);
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_CrateA", new Vector3(landmark.x - 0.28f, 0.46f, landmark.z - 0.18f), new Vector3(0.28f, 0.24f, 0.28f), material);
            CreateVisualPiece(root, PrimitiveType.Capsule, landmark.label + "_CrateB", new Vector3(landmark.x + 0.26f, 0.42f, landmark.z + 0.1f), new Vector3(0.24f, 0.21f, 0.24f), material);
        }

        private static GameObject CreateVisualPiece(Transform parent, PrimitiveType primitiveType, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var piece = GameObject.CreatePrimitive(primitiveType);
            piece.name = name;
            piece.transform.SetParent(parent, false);
            piece.transform.localPosition = localPosition;
            piece.transform.localScale = localScale;

            var renderer = piece.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            var collider = piece.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(collider);
                }
                else
                {
                    Object.DestroyImmediate(collider);
                }
            }

            return piece;
        }

        private static GameObject CreateVisualPiece(Transform parent, PrimitiveType primitiveType, string name, Vector3 localPosition, Vector3 localScale, Color color)
        {
            return CreateVisualPiece(parent, primitiveType, name, localPosition, localScale, CreateAutoMaterial(name, color));
        }

        private static Material CreateAutoMaterial(string name, Color fallbackColor)
        {
            var material = PickMaterial(null, string.Empty, fallbackColor);
            var texture = GetEnvironmentTexture(name);
            if (texture == null)
            {
                return material;
            }

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
            }
            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
            }

            var tiling = GetEnvironmentTiling(name);
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTextureScale("_BaseMap", tiling);
            }
            if (material.HasProperty("_MainTex"))
            {
                material.SetTextureScale("_MainTex", tiling);
            }

            return material;
        }

        private static Texture2D GetEnvironmentTexture(string name)
        {
            EnsureEnvironmentTexturesLoaded();
            var key = name == null ? string.Empty : name.ToLowerInvariant();
            if (key.Contains("wall") || key.Contains("fence") || key.Contains("rear") || key.Contains("store") || key.Contains("shack") || key.Contains("canopy"))
            {
                return cachedWallTextures.Length > 0 ? cachedWallTextures[Mathf.Abs(key.GetHashCode()) % cachedWallTextures.Length] : null;
            }

            if (key.Contains("road") || key.Contains("path") || key.Contains("strip") || key.Contains("lane") || key.Contains("walk"))
            {
                return cachedTileTextures.Length > 1 ? cachedTileTextures[1] : cachedTileTextures.Length > 0 ? cachedTileTextures[0] : null;
            }

            if (key.Contains("field") || key.Contains("farm") || key.Contains("patch") || key.Contains("mud") || key.Contains("grass") || key.Contains("stone") || key.Contains("site"))
            {
                return cachedTileTextures.Length > 0 ? cachedTileTextures[0] : null;
            }

            return null;
        }

        private static Vector2 GetEnvironmentTiling(string name)
        {
            var key = name == null ? string.Empty : name.ToLowerInvariant();
            if (key.Contains("road") || key.Contains("path") || key.Contains("strip") || key.Contains("lane") || key.Contains("walk"))
            {
                return new Vector2(4f, 2f);
            }

            if (key.Contains("wall") || key.Contains("fence") || key.Contains("rear") || key.Contains("store") || key.Contains("shack"))
            {
                return new Vector2(2f, 2f);
            }

            return new Vector2(3f, 3f);
        }

        private static void EnsureEnvironmentTexturesLoaded()
        {
            if (environmentTexturesLoaded)
            {
                return;
            }

            environmentTexturesLoaded = true;
            cachedTileTextures = Resources.LoadAll<Texture2D>("Environment/Tiles");
            cachedWallTextures = Resources.LoadAll<Texture2D>("Environment/Walls");
        }

        private static Material PickMaterial(Material baseMaterial, string accent, Color fallbackColor)
        {
            var accentColor = fallbackColor;
            switch (accent)
            {
                case "warm":
                    accentColor = new Color(0.5f, 0.45f, 0.34f);
                    break;
                case "calm":
                    accentColor = new Color(0.35f, 0.4f, 0.43f);
                    break;
                case "signal":
                    accentColor = new Color(0.53f, 0.49f, 0.35f);
                    break;
            }

            if (baseMaterial != null)
            {
                var clone = new Material(baseMaterial);
                if (clone.HasProperty("_BaseColor"))
                {
                    clone.SetColor("_BaseColor", accentColor);
                }
                if (clone.HasProperty("_Color"))
                {
                    clone.color = accentColor;
                }
                return clone;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", accentColor);
            }
            if (material.HasProperty("_Color"))
            {
                material.color = accentColor;
            }
            return material;
        }

        private static Material MultiplyColor(Material sourceMaterial, float multiplier)
        {
            if (sourceMaterial == null)
            {
                return PickMaterial(null, string.Empty, new Color(0.65f * multiplier, 0.65f * multiplier, 0.65f * multiplier));
            }

            var clone = new Material(sourceMaterial);
            var color = new Color(1f, 1f, 1f);
            if (clone.HasProperty("_BaseColor"))
            {
                color = clone.GetColor("_BaseColor");
                clone.SetColor("_BaseColor", color * multiplier);
            }
            if (clone.HasProperty("_Color"))
            {
                color = clone.color;
                clone.color = color * multiplier;
            }
            return clone;
        }
    }
}
