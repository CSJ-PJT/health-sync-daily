using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Text.RegularExpressions;

namespace DeepStake.Characters
{
    public enum ArticulatedHumanoidRole
    {
        Player,
        Archivist,
        FieldWorker,
        Monster,
    }

    public enum ArticulatedHumanoidAction
    {
        None,
        Inspect,
        Talk,
        Place,
        Attack,
        Hit,
    }

    public sealed class ArticulatedHumanoidView : MonoBehaviour
    {
        private const string VisualRootName = "__HumanoidVisual";
        private const string VisualVersionName = "__VisualVersion_91";
        private const bool UseImportedClipPlayback = true;
        private const string ImportedModelRootName = "__ImportedCharacterModel";
        private const string ImportedModelVisualName = "__ImportedCharacterVisual";
        private const string PlayerModelResourcePath = "AnimatedModels/PlayerAnimated/PlayerAnimated";
        private const string ArchivistModelResourcePath = "AnimatedModels/ArchivistAnimated/ArchivistAnimated";
        private const string WorkerModelResourcePath = "AnimatedModels/WorkerAnimated/WorkerAnimated";
        private const string MonsterModelResourcePath = "AnimatedModels/MonsterAnimated/MonsterAnimated";

        private Transform visualRoot;
        private Transform importedModelRoot;
        private Transform importedModelVisual;
        private Animator importedAnimator;
        private Transform importedHipsBone;
        private Transform importedSpineBone;
        private Transform importedChestBone;
        private Transform importedHeadBone;
        private Transform importedRightUpperArmBone;
        private Transform importedRightLowerArmBone;
        private Transform importedLeftUpperArmBone;
        private Transform importedLeftLowerArmBone;
        private Transform importedRightUpperLegBone;
        private Transform importedRightLowerLegBone;
        private Transform importedLeftUpperLegBone;
        private Transform importedLeftLowerLegBone;
        private Avatar importedAvatar;
        private AnimationClip importedIdleClip;
        private AnimationClip importedWalkClip;
        private AnimationClip importedTalkClip;
        private AnimationClip importedPlaceClip;
        private AnimationClip importedAttackClip;
        private AnimationClip importedHitClip;
        private AnimationClip importedDeathClip;
        private AnimationClip importedCurrentActionClip;
        private bool importedUsesRuntimeController;
        private string importedCurrentStateName;
        private PlayableGraph importedGraph;
        private AnimationClipPlayable importedWalkPlayable;
        private AnimationClipPlayable importedIdlePlayable;
        private AnimationClipPlayable importedActionPlayable;
        private bool importedGraphValid;
        private Transform hips;
        private Transform torso;
        private Transform head;
        private Transform upperArmLeft;
        private Transform lowerArmLeft;
        private Transform upperArmRight;
        private Transform lowerArmRight;
        private Transform upperLegLeft;
        private Transform lowerLegLeft;
        private Transform upperLegRight;
        private Transform lowerLegRight;

        private ArticulatedHumanoidRole role = ArticulatedHumanoidRole.Player;
        private float moveAmount;
        private float moveCycle;
        private float idleClock;
        private float moveBlend;
        private float attentionWeight;
        private Vector3 attentionDirection = Vector3.forward;
        private float actionClock;
        private float actionDuration;
        private ArticulatedHumanoidAction currentAction = ArticulatedHumanoidAction.None;

        private struct HumanoidStyle
        {
            public Color SkinColor;
            public Color BodyColor;
            public Color AccentColor;
            public Color HairColor;
            public Color TrouserColor;
            public Color OuterwearColor;
            public Color GearColor;
            public Vector3 HeadScale;
            public Vector3 TorsoScale;
            public Vector3 HipScale;
            public Vector3 UpperArmScale;
            public Vector3 LowerArmScale;
            public Vector3 UpperLegScale;
            public Vector3 LowerLegScale;
            public Vector3 ChestScale;
            public Vector3 WaistScale;
            public float ShoulderWidth;
            public float HipWidth;
            public float NeckHeight;
            public Vector3 EyeScale;
            public Vector3 HandScale;
            public Vector3 FootScale;
        }

        public void Configure(ArticulatedHumanoidRole nextRole)
        {
            if (role != nextRole || NeedsRebuild())
            {
                role = nextRole;
                RebuildVisual();
            }
            else
            {
                role = nextRole;
                EnsureBuilt();
            }
        }

        public void SetMotion(Vector3 planarVelocity, float maxSpeed)
        {
            var speed = planarVelocity.magnitude;
            var targetMove = maxSpeed > 0.01f ? Mathf.Clamp01(speed / maxSpeed) : 0f;
            moveAmount = Mathf.MoveTowards(moveAmount, targetMove, Time.deltaTime * 4.8f);
            moveBlend = Mathf.SmoothStep(0f, 1f, moveAmount);
            moveCycle += Time.deltaTime * Mathf.Lerp(1.2f, 6.4f, moveAmount);
        }

        public void SetAttentionTarget(Vector3 worldPosition, float weight)
        {
            var flat = worldPosition - transform.position;
            flat.y = 0f;
            if (flat.sqrMagnitude > 0.001f)
            {
                attentionDirection = flat.normalized;
            }

            attentionWeight = Mathf.Clamp01(weight);
        }

        public void ClearAttention()
        {
            attentionWeight = 0f;
        }

        public void PlayAction(ArticulatedHumanoidAction action, Vector3 worldPosition)
        {
            currentAction = action;
            actionClock = 0f;
            actionDuration = action == ArticulatedHumanoidAction.Place
                ? 0.38f
                : action == ArticulatedHumanoidAction.Attack
                    ? 0.52f
                    : action == ArticulatedHumanoidAction.Hit
                        ? 0.34f
                        : 0.28f;
            SetAttentionTarget(worldPosition, 1f);
        }

        private void Awake()
        {
            EnsureBuilt();
        }

        private void OnDisable()
        {
            DestroyImportedAnimationGraph();
        }

        private void OnDestroy()
        {
            DestroyImportedAnimationGraph();
        }

        private void LateUpdate()
        {
            EnsureBuilt();
            idleClock += Time.deltaTime;
            if (actionDuration > 0f)
            {
                actionClock += Time.deltaTime;
                if (actionClock >= actionDuration)
                {
                    currentAction = ArticulatedHumanoidAction.None;
                    actionClock = 0f;
                    actionDuration = 0f;
                }
            }

            AnimateBody();
        }

        private void EnsureBuilt()
        {
            HideLegacyRenderers();

            if (visualRoot != null)
            {
                return;
            }

            var existing = transform.Find(VisualRootName);
            if (existing != null)
            {
                visualRoot = existing;
                CacheBones();
                importedModelRoot = existing.Find(ImportedModelRootName);
                importedModelVisual = importedModelRoot != null ? importedModelRoot.Find(ImportedModelVisualName) : null;
                importedAnimator = importedModelRoot != null ? importedModelRoot.GetComponentInChildren<Animator>() : null;
                if (hips != null || importedModelRoot != null)
                {
                    return;
                }
            }

            var rootObject = existing != null ? existing.gameObject : new GameObject(VisualRootName);
            rootObject.name = VisualRootName;
            rootObject.transform.SetParent(transform, false);
            rootObject.transform.localPosition = new Vector3(0f, -0.92f, 0f);
            rootObject.transform.localScale = role == ArticulatedHumanoidRole.Player
                ? new Vector3(1.42f, 1.42f, 1.42f)
                : role == ArticulatedHumanoidRole.Archivist
                    ? new Vector3(1.4f, 1.4f, 1.4f)
                    : new Vector3(1.41f, 1.41f, 1.41f);
            visualRoot = rootObject.transform;

            BuildHumanoidRig();
        }

        private bool NeedsRebuild()
        {
            var existing = transform.Find(VisualRootName);
            if (existing == null)
            {
                return true;
            }

            return existing.Find(VisualVersionName) == null;
        }

        private void RebuildVisual()
        {
            var existing = transform.Find(VisualRootName);
            if (existing != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(existing.gameObject);
                }
                else
                {
                    DestroyImmediate(existing.gameObject);
                }
            }

            visualRoot = null;
            importedModelRoot = null;
            importedModelVisual = null;
            importedAnimator = null;
            importedAvatar = null;
            importedWalkClip = null;
            importedHipsBone = null;
            importedSpineBone = null;
            importedChestBone = null;
            importedHeadBone = null;
            importedRightUpperArmBone = null;
            importedRightLowerArmBone = null;
            importedLeftUpperArmBone = null;
            importedLeftLowerArmBone = null;
            importedRightUpperLegBone = null;
            importedRightLowerLegBone = null;
            importedLeftUpperLegBone = null;
            importedLeftLowerLegBone = null;
            DestroyImportedAnimationGraph();
            hips = null;
            torso = null;
            head = null;
            upperArmLeft = null;
            lowerArmLeft = null;
            upperArmRight = null;
            lowerArmRight = null;
            upperLegLeft = null;
            lowerLegLeft = null;
            upperLegRight = null;
            lowerLegRight = null;
            EnsureBuilt();
        }

        private void HideLegacyRenderers()
        {
            var renderers = GetComponents<Renderer>();
            for (var index = 0; index < renderers.Length; index++)
            {
                if (renderers[index] != null)
                {
                    renderers[index].enabled = false;
                }
            }
        }

        private void BuildHumanoidRig()
        {
            var versionMarker = new GameObject(VisualVersionName);
            versionMarker.transform.SetParent(visualRoot, false);

            var style = GetStyle(role);

            if (TryBuildImportedModel(style))
            {
                return;
            }

            BuildSimplifiedHumanoid(style);
            CreatePiece(head, "HairTempleL", PrimitiveType.Sphere, new Vector3(-0.16f, 0.1f, 0.12f), new Vector3(0.08f, 0.1f, 0.06f), style.HairColor * 0.94f);
            CreatePiece(head, "HairTempleR", PrimitiveType.Sphere, new Vector3(0.16f, 0.1f, 0.12f), new Vector3(0.08f, 0.1f, 0.06f), style.HairColor * 0.94f);
            CreatePiece(head, "EarLeft", PrimitiveType.Sphere, new Vector3(-0.22f, -0.01f, 0.01f), new Vector3(0.06f, 0.11f, 0.05f), style.SkinColor * 0.98f);
            CreatePiece(head, "EarRight", PrimitiveType.Sphere, new Vector3(0.22f, -0.01f, 0.01f), new Vector3(0.06f, 0.11f, 0.05f), style.SkinColor * 0.98f);
            CreatePiece(head, "JawShadow", PrimitiveType.Capsule, new Vector3(0f, -0.18f, 0.13f), new Vector3(0.11f, 0.02f, 0.03f), style.HairColor * 0.38f);
            CreatePiece(head, "EyeLeft", PrimitiveType.Sphere, new Vector3(-0.08f, 0.0f, 0.25f), style.EyeScale, new Color(0.18f, 0.16f, 0.15f));
            CreatePiece(head, "EyeRight", PrimitiveType.Sphere, new Vector3(0.08f, 0.0f, 0.25f), style.EyeScale, new Color(0.18f, 0.16f, 0.15f));
            CreatePiece(head, "EyeDotLeft", PrimitiveType.Sphere, new Vector3(-0.08f, 0.0f, 0.265f), new Vector3(style.EyeScale.x * 0.42f, style.EyeScale.y * 0.7f, 0.01f), new Color(0.08f, 0.07f, 0.07f));
            CreatePiece(head, "EyeDotRight", PrimitiveType.Sphere, new Vector3(0.08f, 0.0f, 0.265f), new Vector3(style.EyeScale.x * 0.42f, style.EyeScale.y * 0.7f, 0.01f), new Color(0.08f, 0.07f, 0.07f));
            CreatePiece(head, "BrowLeft", PrimitiveType.Capsule, new Vector3(-0.08f, 0.08f, 0.24f), new Vector3(0.05f, 0.03f, 0.02f), style.HairColor * 0.8f);
            CreatePiece(head, "BrowRight", PrimitiveType.Capsule, new Vector3(0.08f, 0.08f, 0.24f), new Vector3(0.05f, 0.03f, 0.02f), style.HairColor * 0.8f);
            CreatePiece(head, "LashShadowL", PrimitiveType.Capsule, new Vector3(-0.08f, 0.03f, 0.252f), new Vector3(0.04f, 0.012f, 0.012f), style.HairColor * 0.66f);
            CreatePiece(head, "LashShadowR", PrimitiveType.Capsule, new Vector3(0.08f, 0.03f, 0.252f), new Vector3(0.04f, 0.012f, 0.012f), style.HairColor * 0.66f);
            CreatePiece(head, "CheekShadowL", PrimitiveType.Capsule, new Vector3(-0.12f, -0.06f, 0.21f), new Vector3(0.035f, 0.03f, 0.014f), style.HairColor * 0.36f);
            CreatePiece(head, "CheekShadowR", PrimitiveType.Capsule, new Vector3(0.12f, -0.06f, 0.21f), new Vector3(0.035f, 0.03f, 0.014f), style.HairColor * 0.36f);
            CreatePiece(head, "MouthLine", PrimitiveType.Capsule, new Vector3(0f, -0.12f, 0.25f), new Vector3(0.05f, 0.015f, 0.02f), new Color(0.28f, 0.22f, 0.2f));
            CreatePiece(head, "Nose", PrimitiveType.Sphere, new Vector3(0f, -0.04f, 0.26f), new Vector3(0.03f, 0.045f, 0.02f), style.SkinColor * 0.92f);
            CreatePiece(head, "Philtrum", PrimitiveType.Capsule, new Vector3(0f, -0.08f, 0.255f), new Vector3(0.01f, 0.024f, 0.012f), style.SkinColor * 0.86f);
            CreatePiece(head, "CheekL", PrimitiveType.Sphere, new Vector3(-0.13f, -0.08f, 0.2f), new Vector3(0.055f, 0.04f, 0.025f), style.SkinColor * 0.97f);
            CreatePiece(head, "CheekR", PrimitiveType.Sphere, new Vector3(0.13f, -0.08f, 0.2f), new Vector3(0.055f, 0.04f, 0.025f), style.SkinColor * 0.97f);
            CreatePiece(head, "UnderChinShadow", PrimitiveType.Capsule, new Vector3(0f, -0.2f, 0.08f), new Vector3(0.11f, 0.02f, 0.04f), style.HairColor * 0.7f);
            CreatePiece(head, "TempleShadeL", PrimitiveType.Capsule, new Vector3(-0.18f, 0.02f, 0.12f), new Vector3(0.035f, 0.08f, 0.03f), style.HairColor * 0.78f);
            CreatePiece(head, "TempleShadeR", PrimitiveType.Capsule, new Vector3(0.18f, 0.02f, 0.12f), new Vector3(0.035f, 0.08f, 0.03f), style.HairColor * 0.78f);
            CreatePiece(head, "NasalBridge", PrimitiveType.Capsule, new Vector3(0f, 0.02f, 0.245f), new Vector3(0.018f, 0.05f, 0.014f), style.SkinColor * 0.9f);
            CreatePiece(head, "FaceShadow", PrimitiveType.Capsule, new Vector3(0f, -0.03f, 0.2f), new Vector3(0.16f, 0.18f, 0.03f), style.HairColor * 0.44f);
            CreatePiece(head, "LipShadow", PrimitiveType.Capsule, new Vector3(0f, -0.095f, 0.24f), new Vector3(0.03f, 0.008f, 0.018f), style.HairColor * 0.64f);

            upperArmLeft = CreateJoint(torso, "UpperArmLeft", new Vector3(-style.ShoulderWidth - 0.08f, 0.14f, 0f));
            lowerArmLeft = CreateJoint(upperArmLeft, "LowerArmLeft", new Vector3(0f, -0.34f, 0f));
            upperArmRight = CreateJoint(torso, "UpperArmRight", new Vector3(style.ShoulderWidth + 0.08f, 0.14f, 0f));
            lowerArmRight = CreateJoint(upperArmRight, "LowerArmRight", new Vector3(0f, -0.34f, 0f));
            CreatePiece(upperArmLeft, "UpperArmVisual", PrimitiveType.Capsule, new Vector3(0f, -0.17f, 0f), style.UpperArmScale, style.OuterwearColor);
            CreatePiece(lowerArmLeft, "LowerArmVisual", PrimitiveType.Capsule, new Vector3(0f, -0.17f, 0f), style.LowerArmScale, style.SkinColor);
            CreatePiece(upperArmRight, "UpperArmVisual", PrimitiveType.Capsule, new Vector3(0f, -0.17f, 0f), style.UpperArmScale, style.OuterwearColor);
            CreatePiece(lowerArmRight, "LowerArmVisual", PrimitiveType.Capsule, new Vector3(0f, -0.17f, 0f), style.LowerArmScale, style.SkinColor);
            CreatePiece(lowerArmLeft, "CuffLeft", PrimitiveType.Capsule, new Vector3(0f, -0.05f, 0.02f), new Vector3(style.LowerArmScale.x * 0.92f, 0.05f, style.LowerArmScale.z * 0.92f), style.OuterwearColor * 0.92f);
            CreatePiece(lowerArmRight, "CuffRight", PrimitiveType.Capsule, new Vector3(0f, -0.05f, 0.02f), new Vector3(style.LowerArmScale.x * 0.92f, 0.05f, style.LowerArmScale.z * 0.92f), style.OuterwearColor * 0.92f);
            CreatePiece(upperArmLeft, "ElbowPadLeft", PrimitiveType.Capsule, new Vector3(0f, -0.28f, 0.03f), new Vector3(style.UpperArmScale.x * 0.9f, 0.045f, style.UpperArmScale.z * 0.9f), style.GearColor * 0.86f);
            CreatePiece(upperArmRight, "ElbowPadRight", PrimitiveType.Capsule, new Vector3(0f, -0.28f, 0.03f), new Vector3(style.UpperArmScale.x * 0.9f, 0.045f, style.UpperArmScale.z * 0.9f), style.GearColor * 0.86f);
            CreatePiece(lowerArmLeft, "HandLeft", PrimitiveType.Sphere, new Vector3(0f, -0.34f, 0.04f), style.HandScale, style.SkinColor);
            CreatePiece(lowerArmRight, "HandRight", PrimitiveType.Sphere, new Vector3(0f, -0.34f, 0.04f), style.HandScale, style.SkinColor);
            CreatePiece(lowerArmLeft, "ThumbMassL", PrimitiveType.Sphere, new Vector3(-0.04f, -0.32f, 0.08f), style.HandScale * 0.34f, style.SkinColor * 0.98f);
            CreatePiece(lowerArmRight, "ThumbMassR", PrimitiveType.Sphere, new Vector3(0.04f, -0.32f, 0.08f), style.HandScale * 0.34f, style.SkinColor * 0.98f);

            upperLegLeft = CreateJoint(hips, "UpperLegLeft", new Vector3(-style.HipWidth, -0.11f, 0f));
            lowerLegLeft = CreateJoint(upperLegLeft, "LowerLegLeft", new Vector3(0f, -0.32f, 0f));
            upperLegRight = CreateJoint(hips, "UpperLegRight", new Vector3(style.HipWidth, -0.11f, 0f));
            lowerLegRight = CreateJoint(upperLegRight, "LowerLegRight", new Vector3(0f, -0.32f, 0f));
            CreatePiece(upperLegLeft, "UpperLegVisual", PrimitiveType.Capsule, new Vector3(0f, -0.17f, 0f), style.UpperLegScale, style.TrouserColor);
            CreatePiece(lowerLegLeft, "LowerLegVisual", PrimitiveType.Capsule, new Vector3(0f, -0.17f, 0f), style.LowerLegScale, style.TrouserColor * 0.9f);
            CreatePiece(upperLegRight, "UpperLegVisual", PrimitiveType.Capsule, new Vector3(0f, -0.17f, 0f), style.UpperLegScale, style.TrouserColor);
            CreatePiece(lowerLegRight, "LowerLegVisual", PrimitiveType.Capsule, new Vector3(0f, -0.17f, 0f), style.LowerLegScale, style.TrouserColor * 0.9f);
            CreatePiece(lowerLegLeft, "KneeBandL", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0.03f), new Vector3(style.LowerLegScale.x * 0.85f, 0.045f, style.LowerLegScale.z * 0.85f), style.TrouserColor * 0.76f);
            CreatePiece(lowerLegRight, "KneeBandR", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0.03f), new Vector3(style.LowerLegScale.x * 0.85f, 0.045f, style.LowerLegScale.z * 0.85f), style.TrouserColor * 0.76f);
            CreatePiece(lowerLegLeft, "Boot", PrimitiveType.Capsule, new Vector3(0f, -0.36f, 0.08f), style.FootScale, new Color(0.24f, 0.2f, 0.18f));
            CreatePiece(lowerLegRight, "Boot", PrimitiveType.Capsule, new Vector3(0f, -0.36f, 0.08f), style.FootScale, new Color(0.24f, 0.2f, 0.18f));
            CreatePiece(upperLegLeft, "ThighPanelL", PrimitiveType.Capsule, new Vector3(0f, -0.12f, 0.06f), new Vector3(style.UpperLegScale.x * 0.8f, 0.1f, 0.05f), style.TrouserColor * 0.82f);
            CreatePiece(upperLegRight, "ThighPanelR", PrimitiveType.Capsule, new Vector3(0f, -0.12f, 0.06f), new Vector3(style.UpperLegScale.x * 0.8f, 0.1f, 0.05f), style.TrouserColor * 0.82f);
            CreatePiece(upperLegLeft, "OuterSeamL", PrimitiveType.Capsule, new Vector3(-0.04f, -0.1f, 0.03f), new Vector3(0.018f, 0.16f, 0.018f), style.TrouserColor * 0.7f);
            CreatePiece(upperLegRight, "OuterSeamR", PrimitiveType.Capsule, new Vector3(0.04f, -0.1f, 0.03f), new Vector3(0.018f, 0.16f, 0.018f), style.TrouserColor * 0.7f);
            CreatePiece(lowerLegLeft, "ShinPanelL", PrimitiveType.Capsule, new Vector3(0f, -0.16f, 0.06f), new Vector3(style.LowerLegScale.x * 0.66f, 0.12f, 0.04f), style.TrouserColor * 0.8f);
            CreatePiece(lowerLegRight, "ShinPanelR", PrimitiveType.Capsule, new Vector3(0f, -0.16f, 0.06f), new Vector3(style.LowerLegScale.x * 0.66f, 0.12f, 0.04f), style.TrouserColor * 0.8f);
            CreatePiece(lowerLegLeft, "AnkleBandL", PrimitiveType.Capsule, new Vector3(0f, -0.27f, 0.05f), new Vector3(style.LowerLegScale.x * 0.7f, 0.025f, 0.03f), style.TrouserColor * 0.68f);
            CreatePiece(lowerLegRight, "AnkleBandR", PrimitiveType.Capsule, new Vector3(0f, -0.27f, 0.05f), new Vector3(style.LowerLegScale.x * 0.7f, 0.025f, 0.03f), style.TrouserColor * 0.68f);

            if (role == ArticulatedHumanoidRole.Player)
            {
                CreatePiece(head, "PlayerCrownL", PrimitiveType.Sphere, new Vector3(-0.08f, 0.25f, 0.02f), new Vector3(0.08f, 0.11f, 0.08f), style.HairColor * 0.96f);
                CreatePiece(head, "PlayerCrownR", PrimitiveType.Sphere, new Vector3(0.08f, 0.25f, 0.02f), new Vector3(0.08f, 0.11f, 0.08f), style.HairColor * 0.96f);
                CreatePiece(head, "PlayerHairSweepL", PrimitiveType.Capsule, new Vector3(-0.22f, 0.11f, 0.12f), new Vector3(0.08f, 0.18f, 0.08f), style.HairColor * 0.98f);
                CreatePiece(head, "PlayerHairSweepR", PrimitiveType.Capsule, new Vector3(0.22f, 0.11f, 0.12f), new Vector3(0.08f, 0.18f, 0.08f), style.HairColor * 0.98f);
                CreatePiece(head, "PlayerBackHair", PrimitiveType.Capsule, new Vector3(0f, -0.08f, -0.18f), new Vector3(0.18f, 0.18f, 0.09f), style.HairColor * 0.94f);
                CreatePiece(head, "PlayerSideBurnL", PrimitiveType.Capsule, new Vector3(-0.18f, -0.04f, 0.12f), new Vector3(0.05f, 0.12f, 0.04f), style.HairColor * 0.92f);
                CreatePiece(head, "PlayerSideBurnR", PrimitiveType.Capsule, new Vector3(0.18f, -0.04f, 0.12f), new Vector3(0.05f, 0.12f, 0.04f), style.HairColor * 0.92f);
                CreatePiece(head, "PlayerForelock", PrimitiveType.Capsule, new Vector3(0f, 0.12f, 0.28f), new Vector3(0.09f, 0.14f, 0.05f), style.HairColor * 0.96f);
                CreatePiece(head, "PlayerCrownBack", PrimitiveType.Sphere, new Vector3(0f, 0.18f, -0.08f), new Vector3(0.12f, 0.1f, 0.1f), style.HairColor * 0.94f);
                CreatePiece(head, "PlayerForeTuftL", PrimitiveType.Capsule, new Vector3(-0.08f, 0.1f, 0.27f), new Vector3(0.05f, 0.1f, 0.03f), style.HairColor * 0.94f);
                CreatePiece(head, "PlayerForeTuftR", PrimitiveType.Capsule, new Vector3(0.08f, 0.1f, 0.27f), new Vector3(0.05f, 0.1f, 0.03f), style.HairColor * 0.94f);
                CreatePiece(head, "PlayerNeckLockL", PrimitiveType.Capsule, new Vector3(-0.12f, -0.16f, -0.08f), new Vector3(0.05f, 0.08f, 0.03f), style.HairColor * 0.9f);
                CreatePiece(head, "PlayerNeckLockR", PrimitiveType.Capsule, new Vector3(0.12f, -0.16f, -0.08f), new Vector3(0.05f, 0.08f, 0.03f), style.HairColor * 0.9f);
                CreatePiece(torso, "Backpack", PrimitiveType.Capsule, new Vector3(0f, 0.05f, -0.22f), new Vector3(0.3f, 0.29f, 0.19f), style.GearColor);
                CreatePiece(torso, "BackpackFlap", PrimitiveType.Capsule, new Vector3(0f, 0.13f, -0.28f), new Vector3(0.18f, 0.08f, 0.05f), style.GearColor * 1.08f);
                CreatePiece(torso, "BackpackPocket", PrimitiveType.Capsule, new Vector3(0f, -0.02f, -0.31f), new Vector3(0.12f, 0.1f, 0.04f), style.GearColor * 1.02f);
                CreatePiece(torso, "BackpackBedroll", PrimitiveType.Capsule, new Vector3(0f, 0.28f, -0.28f), new Vector3(0.18f, 0.05f, 0.1f), style.AccentColor * 0.74f);
                CreatePiece(torso, "BackpackTieL", PrimitiveType.Capsule, new Vector3(-0.09f, 0.26f, -0.34f), new Vector3(0.018f, 0.08f, 0.018f), style.GearColor * 0.78f);
                CreatePiece(torso, "BackpackTieR", PrimitiveType.Capsule, new Vector3(0.09f, 0.26f, -0.34f), new Vector3(0.018f, 0.08f, 0.018f), style.GearColor * 0.78f);
                CreatePiece(torso, "ShoulderStrapL", PrimitiveType.Capsule, new Vector3(-0.16f, 0.03f, 0.1f), new Vector3(0.045f, 0.24f, 0.04f), style.GearColor * 0.84f);
                CreatePiece(torso, "ShoulderStrapR", PrimitiveType.Capsule, new Vector3(0.16f, 0.03f, 0.1f), new Vector3(0.045f, 0.24f, 0.04f), style.GearColor * 0.84f);
                CreatePiece(torso, "Pouch", PrimitiveType.Capsule, new Vector3(0.3f, -0.05f, 0.14f), new Vector3(0.12f, 0.13f, 0.08f), style.GearColor * 1.06f);
                CreatePiece(torso, "PouchFlap", PrimitiveType.Capsule, new Vector3(0.3f, 0.02f, 0.18f), new Vector3(0.08f, 0.03f, 0.04f), style.GearColor * 1.12f);
                CreatePiece(lowerArmLeft, "Wrap", PrimitiveType.Capsule, new Vector3(0f, -0.22f, 0.12f), new Vector3(0.06f, 0.03f, 0.05f), style.AccentColor * 0.92f);
                CreatePiece(torso, "ScarfKnot", PrimitiveType.Sphere, new Vector3(0.06f, 0.18f, 0.24f), new Vector3(0.1f, 0.1f, 0.1f), style.AccentColor);
                CreatePiece(torso, "ScarfTail", PrimitiveType.Capsule, new Vector3(0.15f, 0.02f, 0.23f), new Vector3(0.06f, 0.18f, 0.04f), style.AccentColor * 0.96f);
                CreatePiece(torso, "BackpackRoll", PrimitiveType.Capsule, new Vector3(0f, 0.3f, -0.24f), new Vector3(0.16f, 0.05f, 0.08f), style.AccentColor * 0.78f);
                CreatePiece(torso, "BackpackBottom", PrimitiveType.Capsule, new Vector3(0f, -0.09f, -0.25f), new Vector3(0.2f, 0.08f, 0.14f), style.GearColor * 0.9f);
                CreatePiece(head, "HairSpikeL", PrimitiveType.Sphere, new Vector3(-0.12f, 0.18f, 0.06f), new Vector3(0.08f, 0.1f, 0.08f), style.HairColor * 0.96f);
                CreatePiece(head, "HairSpikeR", PrimitiveType.Sphere, new Vector3(0.12f, 0.18f, 0.06f), new Vector3(0.08f, 0.1f, 0.08f), style.HairColor * 0.96f);
                CreatePiece(torso, "BackBuckle", PrimitiveType.Capsule, new Vector3(0f, -0.04f, -0.28f), new Vector3(0.04f, 0.09f, 0.03f), style.AccentColor * 0.86f);
                CreatePiece(torso, "FrontPocketL", PrimitiveType.Capsule, new Vector3(-0.15f, -0.06f, 0.17f), new Vector3(0.07f, 0.08f, 0.05f), style.GearColor * 1.02f);
                CreatePiece(torso, "FrontPocketR", PrimitiveType.Capsule, new Vector3(0.15f, -0.06f, 0.17f), new Vector3(0.07f, 0.08f, 0.05f), style.GearColor * 1.02f);
                CreatePiece(torso, "FrontPocketFlapL", PrimitiveType.Capsule, new Vector3(-0.15f, -0.01f, 0.21f), new Vector3(0.05f, 0.02f, 0.025f), style.GearColor * 1.08f);
                CreatePiece(torso, "FrontPocketFlapR", PrimitiveType.Capsule, new Vector3(0.15f, -0.01f, 0.21f), new Vector3(0.05f, 0.02f, 0.025f), style.GearColor * 1.08f);
                CreatePiece(torso, "BackpackSideL", PrimitiveType.Capsule, new Vector3(-0.14f, 0.02f, -0.19f), new Vector3(0.07f, 0.16f, 0.1f), style.GearColor * 0.96f);
                CreatePiece(torso, "BackpackSideR", PrimitiveType.Capsule, new Vector3(0.14f, 0.02f, -0.19f), new Vector3(0.07f, 0.16f, 0.1f), style.GearColor * 0.96f);
                CreatePiece(torso, "HarnessBand", PrimitiveType.Capsule, new Vector3(0f, 0.02f, 0.09f), new Vector3(0.19f, 0.02f, 0.03f), style.GearColor * 0.88f);
                CreatePiece(torso, "HarnessDropL", PrimitiveType.Capsule, new Vector3(-0.1f, -0.08f, 0.1f), new Vector3(0.03f, 0.12f, 0.025f), style.GearColor * 0.82f);
                CreatePiece(torso, "HarnessDropR", PrimitiveType.Capsule, new Vector3(0.1f, -0.08f, 0.1f), new Vector3(0.03f, 0.12f, 0.025f), style.GearColor * 0.82f);
                CreatePiece(torso, "SurvivorPatch", PrimitiveType.Capsule, new Vector3(-0.11f, 0.11f, 0.19f), new Vector3(0.05f, 0.045f, 0.02f), style.AccentColor * 0.84f);
                CreatePiece(torso, "SurvivorPanel", PrimitiveType.Capsule, new Vector3(0.02f, 0.02f, 0.18f), new Vector3(0.08f, 0.18f, 0.03f), style.BodyColor * 0.9f);
                CreatePiece(torso, "SurvivorChestPocket", PrimitiveType.Capsule, new Vector3(0.11f, 0.08f, 0.2f), new Vector3(0.045f, 0.055f, 0.025f), style.GearColor * 0.96f);
                CreatePiece(torso, "SurvivorShoulderTabL", PrimitiveType.Capsule, new Vector3(-0.12f, 0.2f, 0.17f), new Vector3(0.045f, 0.03f, 0.02f), style.GearColor * 0.9f);
                CreatePiece(torso, "SurvivorShoulderTabR", PrimitiveType.Capsule, new Vector3(0.12f, 0.2f, 0.17f), new Vector3(0.045f, 0.03f, 0.02f), style.GearColor * 0.9f);
                CreatePiece(torso, "SurvivorChestSeamL", PrimitiveType.Capsule, new Vector3(-0.06f, 0.03f, 0.2f), new Vector3(0.018f, 0.18f, 0.015f), style.GearColor * 0.68f);
                CreatePiece(torso, "SurvivorChestSeamR", PrimitiveType.Capsule, new Vector3(0.06f, 0.03f, 0.2f), new Vector3(0.018f, 0.18f, 0.015f), style.GearColor * 0.68f);
                CreatePiece(torso, "SurvivorShoulderPatchL", PrimitiveType.Capsule, new Vector3(-0.2f, 0.18f, 0.08f), new Vector3(0.04f, 0.07f, 0.025f), style.AccentColor * 0.86f);
                CreatePiece(torso, "SurvivorHipPatchR", PrimitiveType.Capsule, new Vector3(0.18f, -0.12f, 0.14f), new Vector3(0.05f, 0.08f, 0.025f), style.AccentColor * 0.82f);
                CreatePiece(torso, "WaistSatchel", PrimitiveType.Capsule, new Vector3(-0.24f, -0.14f, 0.12f), new Vector3(0.09f, 0.11f, 0.06f), style.GearColor * 0.96f);
                CreatePiece(torso, "WaistSatchelFlap", PrimitiveType.Capsule, new Vector3(-0.24f, -0.08f, 0.16f), new Vector3(0.06f, 0.03f, 0.03f), style.GearColor * 1.06f);
                CreatePiece(torso, "SatchelStrap", PrimitiveType.Capsule, new Vector3(-0.19f, -0.05f, 0.12f), new Vector3(0.02f, 0.16f, 0.02f), style.GearColor * 0.84f);
                CreatePiece(torso, "SurvivorBuckle", PrimitiveType.Capsule, new Vector3(0.02f, -0.16f, 0.18f), new Vector3(0.035f, 0.03f, 0.02f), style.AccentColor * 0.92f);
                CreatePiece(torso, "SurvivorWaistBand", PrimitiveType.Capsule, new Vector3(0f, -0.13f, 0.14f), new Vector3(0.18f, 0.02f, 0.03f), style.GearColor * 0.74f);
                CreatePiece(torso, "SurvivorBeltLoopL", PrimitiveType.Capsule, new Vector3(-0.1f, -0.14f, 0.16f), new Vector3(0.014f, 0.04f, 0.012f), style.GearColor * 0.9f);
                CreatePiece(torso, "SurvivorBeltLoopR", PrimitiveType.Capsule, new Vector3(0.1f, -0.14f, 0.16f), new Vector3(0.014f, 0.04f, 0.012f), style.GearColor * 0.9f);
                CreatePiece(torso, "SurvivorRearPatch", PrimitiveType.Capsule, new Vector3(-0.08f, 0.0f, -0.14f), new Vector3(0.05f, 0.08f, 0.022f), style.AccentColor * 0.74f);
                CreatePiece(head, "PlayerJawTuft", PrimitiveType.Capsule, new Vector3(0f, -0.17f, 0.18f), new Vector3(0.06f, 0.024f, 0.018f), style.HairColor * 0.78f);
                CreatePiece(torso, "SurvivorChestPatchR", PrimitiveType.Capsule, new Vector3(0.13f, 0.13f, 0.19f), new Vector3(0.04f, 0.05f, 0.02f), style.AccentColor * 0.8f);
                CreatePiece(torso, "SurvivorHipTabL", PrimitiveType.Capsule, new Vector3(-0.18f, -0.11f, 0.13f), new Vector3(0.038f, 0.07f, 0.022f), style.GearColor * 0.84f);
                CreatePiece(head, "PlayerRearLockL", PrimitiveType.Capsule, new Vector3(-0.14f, -0.08f, -0.15f), new Vector3(0.045f, 0.1f, 0.03f), style.HairColor * 0.84f);
                CreatePiece(head, "PlayerRearLockR", PrimitiveType.Capsule, new Vector3(0.14f, -0.08f, -0.15f), new Vector3(0.045f, 0.1f, 0.03f), style.HairColor * 0.84f);
                CreatePiece(head, "PlayerTempleTuftL", PrimitiveType.Capsule, new Vector3(-0.18f, 0.02f, 0.16f), new Vector3(0.03f, 0.07f, 0.022f), style.HairColor * 0.88f);
                CreatePiece(head, "PlayerTempleTuftR", PrimitiveType.Capsule, new Vector3(0.18f, 0.02f, 0.16f), new Vector3(0.03f, 0.07f, 0.022f), style.HairColor * 0.88f);
                CreatePiece(torso, "SurvivorBackBand", PrimitiveType.Capsule, new Vector3(0f, -0.02f, -0.16f), new Vector3(0.16f, 0.018f, 0.022f), style.GearColor * 0.72f);
                CreatePiece(torso, "SurvivorSidePanelL", PrimitiveType.Capsule, new Vector3(-0.19f, 0.01f, 0.09f), new Vector3(0.03f, 0.2f, 0.02f), style.BodyColor * 0.86f);
                CreatePiece(torso, "SurvivorSidePanelR", PrimitiveType.Capsule, new Vector3(0.19f, 0.01f, 0.09f), new Vector3(0.03f, 0.2f, 0.02f), style.BodyColor * 0.86f);
                CreatePiece(hips, "SurvivorLegBandL", PrimitiveType.Capsule, new Vector3(-0.12f, -0.04f, 0.12f), new Vector3(0.035f, 0.08f, 0.02f), style.GearColor * 0.76f);
                CreatePiece(hips, "SurvivorLegBandR", PrimitiveType.Capsule, new Vector3(0.12f, -0.04f, 0.12f), new Vector3(0.035f, 0.08f, 0.02f), style.GearColor * 0.76f);
                CreatePiece(head, "PlayerTopWave", PrimitiveType.Sphere, new Vector3(0f, 0.3f, 0.06f), new Vector3(0.12f, 0.09f, 0.08f), style.HairColor * 0.9f);
                CreatePiece(upperArmLeft, "SurvivorArmBandL", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0.12f), new Vector3(0.05f, 0.03f, 0.03f), style.AccentColor * 0.8f);
                CreatePiece(upperArmRight, "SurvivorArmBandR", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0.12f), new Vector3(0.05f, 0.03f, 0.03f), style.AccentColor * 0.8f);
                CreatePiece(lowerLegLeft, "SurvivorKneePatchL", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0.12f), new Vector3(0.05f, 0.06f, 0.025f), style.GearColor * 0.78f);
                CreatePiece(lowerLegRight, "SurvivorKneePatchR", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0.12f), new Vector3(0.05f, 0.06f, 0.025f), style.GearColor * 0.78f);
                CreatePiece(lowerArmLeft, "SurvivorWristTabL", PrimitiveType.Capsule, new Vector3(0f, -0.22f, 0.06f), new Vector3(0.04f, 0.02f, 0.028f), style.GearColor * 0.82f);
                CreatePiece(lowerArmRight, "SurvivorWristTabR", PrimitiveType.Capsule, new Vector3(0f, -0.22f, 0.06f), new Vector3(0.04f, 0.02f, 0.028f), style.GearColor * 0.82f);
                CreatePiece(lowerLegLeft, "SurvivorBootCapL", PrimitiveType.Capsule, new Vector3(0f, -0.33f, 0.16f), new Vector3(0.055f, 0.025f, 0.03f), style.AccentColor * 0.72f);
                CreatePiece(lowerLegRight, "SurvivorBootCapR", PrimitiveType.Capsule, new Vector3(0f, -0.33f, 0.16f), new Vector3(0.055f, 0.025f, 0.03f), style.AccentColor * 0.72f);
                CreatePiece(lowerLegLeft, "BootCollarL", PrimitiveType.Capsule, new Vector3(0f, -0.26f, 0.08f), new Vector3(0.06f, 0.03f, 0.05f), style.AccentColor * 0.76f);
                CreatePiece(lowerLegRight, "BootCollarR", PrimitiveType.Capsule, new Vector3(0f, -0.26f, 0.08f), new Vector3(0.06f, 0.03f, 0.05f), style.AccentColor * 0.76f);
                CreatePiece(torso, "BootTongueL", PrimitiveType.Capsule, new Vector3(-0.11f, -0.36f, 0.18f), new Vector3(0.05f, 0.06f, 0.03f), style.AccentColor * 0.84f);
                CreatePiece(torso, "BootTongueR", PrimitiveType.Capsule, new Vector3(0.11f, -0.36f, 0.18f), new Vector3(0.05f, 0.06f, 0.03f), style.AccentColor * 0.84f);
                CreatePiece(torso, "ScarfFold", PrimitiveType.Capsule, new Vector3(-0.06f, 0.16f, 0.22f), new Vector3(0.12f, 0.03f, 0.03f), style.AccentColor * 1.04f);
                CreatePiece(lowerArmRight, "WristWrapPlayer", PrimitiveType.Capsule, new Vector3(0f, -0.16f, 0.04f), new Vector3(0.05f, 0.024f, 0.03f), style.AccentColor * 0.82f);
            }
            else if (role == ArticulatedHumanoidRole.Archivist)
            {
                CreatePiece(head, "ArchivistPartL", PrimitiveType.Capsule, new Vector3(-0.11f, 0.12f, 0.14f), new Vector3(0.06f, 0.16f, 0.05f), style.HairColor * 0.96f);
                CreatePiece(head, "ArchivistPartR", PrimitiveType.Capsule, new Vector3(0.11f, 0.12f, 0.12f), new Vector3(0.06f, 0.14f, 0.05f), style.HairColor * 0.96f);
                CreatePiece(head, "ArchivistBackBob", PrimitiveType.Capsule, new Vector3(0f, -0.02f, -0.16f), new Vector3(0.18f, 0.16f, 0.08f), style.HairColor * 0.95f);
                CreatePiece(head, "ArchivistTopPart", PrimitiveType.Capsule, new Vector3(0f, 0.16f, 0.12f), new Vector3(0.05f, 0.14f, 0.04f), style.HairColor * 0.94f);
                CreatePiece(head, "ArchivistSideL", PrimitiveType.Capsule, new Vector3(-0.18f, -0.02f, 0.06f), new Vector3(0.05f, 0.14f, 0.04f), style.HairColor * 0.92f);
                CreatePiece(head, "ArchivistSideR", PrimitiveType.Capsule, new Vector3(0.18f, -0.02f, 0.06f), new Vector3(0.05f, 0.14f, 0.04f), style.HairColor * 0.92f);
                CreatePiece(head, "ArchivistCrown", PrimitiveType.Sphere, new Vector3(0f, 0.22f, 0.02f), new Vector3(0.13f, 0.11f, 0.11f), style.HairColor * 0.94f);
                CreatePiece(head, "ArchivistNapeBob", PrimitiveType.Capsule, new Vector3(0f, -0.12f, -0.17f), new Vector3(0.12f, 0.1f, 0.05f), style.HairColor * 0.92f);
                CreatePiece(head, "ArchivistFringe", PrimitiveType.Capsule, new Vector3(0f, 0.08f, 0.27f), new Vector3(0.08f, 0.08f, 0.035f), style.HairColor * 0.94f);
                CreatePiece(head, "ArchivistTempleLockL", PrimitiveType.Capsule, new Vector3(-0.14f, -0.08f, 0.14f), new Vector3(0.04f, 0.1f, 0.03f), style.HairColor * 0.9f);
                CreatePiece(head, "ArchivistTempleLockR", PrimitiveType.Capsule, new Vector3(0.14f, -0.08f, 0.14f), new Vector3(0.04f, 0.1f, 0.03f), style.HairColor * 0.9f);
                CreatePiece(torso, "ArchivePad", PrimitiveType.Capsule, new Vector3(-0.18f, -0.01f, 0.18f), new Vector3(0.09f, 0.11f, 0.04f), new Color(0.68f, 0.65f, 0.58f));
                CreatePiece(head, "GlassesBridge", PrimitiveType.Capsule, new Vector3(0f, 0.02f, 0.3f), new Vector3(0.17f, 0.03f, 0.02f), new Color(0.1f, 0.1f, 0.12f));
                CreatePiece(head, "GlassesLeft", PrimitiveType.Capsule, new Vector3(-0.14f, 0.02f, 0.3f), new Vector3(0.115f, 0.08f, 0.02f), new Color(0.1f, 0.1f, 0.12f));
                CreatePiece(head, "GlassesRight", PrimitiveType.Capsule, new Vector3(0.14f, 0.02f, 0.3f), new Vector3(0.115f, 0.08f, 0.02f), new Color(0.1f, 0.1f, 0.12f));
                CreatePiece(head, "HeadsetBand", PrimitiveType.Capsule, new Vector3(0f, 0.21f, -0.03f), new Vector3(0.2f, 0.055f, 0.18f), new Color(0.18f, 0.2f, 0.2f));
                CreatePiece(head, "HeadsetCupLeft", PrimitiveType.Sphere, new Vector3(-0.32f, 0.05f, 0f), new Vector3(0.18f, 0.2f, 0.12f), new Color(0.18f, 0.2f, 0.2f));
                CreatePiece(head, "HeadsetCupRight", PrimitiveType.Sphere, new Vector3(0.32f, 0.05f, 0f), new Vector3(0.18f, 0.2f, 0.12f), new Color(0.18f, 0.2f, 0.2f));
                CreatePiece(head, "HeadsetMic", PrimitiveType.Capsule, new Vector3(0.22f, -0.05f, 0.25f), new Vector3(0.024f, 0.1f, 0.02f), new Color(0.18f, 0.2f, 0.2f));
                CreatePiece(torso, "DocumentStrap", PrimitiveType.Capsule, new Vector3(0.14f, 0.02f, 0.16f), new Vector3(0.04f, 0.24f, 0.04f), new Color(0.3f, 0.28f, 0.24f));
                CreatePiece(torso, "LapelLeft", PrimitiveType.Capsule, new Vector3(-0.08f, 0.09f, 0.2f), new Vector3(0.05f, 0.14f, 0.03f), style.OuterwearColor * 1.04f);
                CreatePiece(torso, "LapelRight", PrimitiveType.Capsule, new Vector3(0.08f, 0.09f, 0.2f), new Vector3(0.05f, 0.14f, 0.03f), style.OuterwearColor * 1.04f);
                CreatePiece(head, "SideLockL", PrimitiveType.Sphere, new Vector3(-0.19f, -0.08f, 0.09f), new Vector3(0.08f, 0.18f, 0.08f), style.HairColor);
                CreatePiece(head, "SideLockR", PrimitiveType.Sphere, new Vector3(0.19f, -0.08f, 0.09f), new Vector3(0.08f, 0.18f, 0.08f), style.HairColor);
                CreatePiece(head, "HairBackArchivist", PrimitiveType.Sphere, new Vector3(0f, -0.05f, -0.16f), new Vector3(0.18f, 0.2f, 0.12f), style.HairColor * 0.98f);
                CreatePiece(head, "HeadsetBandTop", PrimitiveType.Capsule, new Vector3(0f, 0.24f, -0.02f), new Vector3(0.16f, 0.02f, 0.04f), new Color(0.18f, 0.2f, 0.2f));
                CreatePiece(torso, "NotebookEdge", PrimitiveType.Capsule, new Vector3(-0.24f, 0.06f, 0.2f), new Vector3(0.02f, 0.08f, 0.1f), new Color(0.52f, 0.48f, 0.38f));
                CreatePiece(torso, "NotebookFace", PrimitiveType.Capsule, new Vector3(-0.18f, 0.02f, 0.2f), new Vector3(0.1f, 0.12f, 0.04f), new Color(0.64f, 0.6f, 0.52f));
                CreatePiece(torso, "NotebookBand", PrimitiveType.Capsule, new Vector3(-0.18f, -0.04f, 0.24f), new Vector3(0.06f, 0.02f, 0.025f), new Color(0.3f, 0.28f, 0.24f));
                CreatePiece(torso, "NotebookTab", PrimitiveType.Capsule, new Vector3(-0.11f, 0.08f, 0.22f), new Vector3(0.025f, 0.03f, 0.015f), new Color(0.54f, 0.5f, 0.42f));
                CreatePiece(torso, "SleevePadL", PrimitiveType.Capsule, new Vector3(-0.21f, 0.06f, 0.1f), new Vector3(0.06f, 0.16f, 0.06f), style.OuterwearColor * 1.02f);
                CreatePiece(torso, "CoatPocketL", PrimitiveType.Capsule, new Vector3(-0.12f, -0.08f, 0.18f), new Vector3(0.07f, 0.09f, 0.04f), style.GearColor * 0.94f);
                CreatePiece(torso, "CoatPocketR", PrimitiveType.Capsule, new Vector3(0.12f, -0.08f, 0.18f), new Vector3(0.07f, 0.09f, 0.04f), style.GearColor * 0.94f);
                CreatePiece(torso, "ArchivistFrontPanel", PrimitiveType.Capsule, new Vector3(0f, 0.01f, 0.19f), new Vector3(0.08f, 0.22f, 0.03f), style.BodyColor * 0.88f);
                CreatePiece(torso, "ArchivistChestPocket", PrimitiveType.Capsule, new Vector3(-0.1f, 0.06f, 0.2f), new Vector3(0.045f, 0.06f, 0.025f), style.GearColor * 0.92f);
                CreatePiece(torso, "ArchivistShoulderTabL", PrimitiveType.Capsule, new Vector3(-0.1f, 0.2f, 0.16f), new Vector3(0.04f, 0.028f, 0.02f), style.GearColor * 0.86f);
                CreatePiece(torso, "ArchivistShoulderTabR", PrimitiveType.Capsule, new Vector3(0.1f, 0.2f, 0.16f), new Vector3(0.04f, 0.028f, 0.02f), style.GearColor * 0.86f);
                CreatePiece(torso, "ArchivistFrontSeam", PrimitiveType.Capsule, new Vector3(0f, 0.02f, 0.205f), new Vector3(0.018f, 0.22f, 0.014f), style.GearColor * 0.66f);
                CreatePiece(torso, "ArchivistShoulderPatchR", PrimitiveType.Capsule, new Vector3(0.19f, 0.18f, 0.08f), new Vector3(0.038f, 0.065f, 0.024f), style.AccentColor * 0.82f);
                CreatePiece(torso, "ArchivistHipPatchL", PrimitiveType.Capsule, new Vector3(-0.17f, -0.1f, 0.15f), new Vector3(0.045f, 0.08f, 0.024f), style.AccentColor * 0.78f);
                CreatePiece(torso, "ArchivistBeltLoopR", PrimitiveType.Capsule, new Vector3(0.1f, -0.14f, 0.16f), new Vector3(0.014f, 0.04f, 0.012f), style.GearColor * 0.88f);
                CreatePiece(torso, "ChestTag", PrimitiveType.Capsule, new Vector3(0.09f, 0.08f, 0.2f), new Vector3(0.04f, 0.06f, 0.02f), style.AccentColor * 0.86f);
                CreatePiece(head, "HeadsetWire", PrimitiveType.Capsule, new Vector3(0.24f, -0.12f, 0.12f), new Vector3(0.014f, 0.11f, 0.014f), new Color(0.16f, 0.18f, 0.18f));
                CreatePiece(torso, "ArchiveBadge", PrimitiveType.Capsule, new Vector3(-0.04f, 0.12f, 0.2f), new Vector3(0.035f, 0.055f, 0.02f), new Color(0.54f, 0.5f, 0.4f));
                CreatePiece(torso, "PenSlot", PrimitiveType.Capsule, new Vector3(0.18f, 0.04f, 0.18f), new Vector3(0.016f, 0.09f, 0.016f), new Color(0.34f, 0.34f, 0.38f));
                CreatePiece(torso, "ArchiveLanyard", PrimitiveType.Capsule, new Vector3(0f, 0.02f, 0.18f), new Vector3(0.025f, 0.18f, 0.02f), style.AccentColor * 0.78f);
                CreatePiece(torso, "ClipboardTab", PrimitiveType.Capsule, new Vector3(-0.24f, 0.12f, 0.22f), new Vector3(0.025f, 0.025f, 0.018f), new Color(0.44f, 0.42f, 0.36f));
                CreatePiece(head, "ArchivistSpectacleArmL", PrimitiveType.Capsule, new Vector3(-0.24f, 0.03f, 0.12f), new Vector3(0.012f, 0.08f, 0.012f), new Color(0.1f, 0.1f, 0.12f));
                CreatePiece(head, "ArchivistSpectacleArmR", PrimitiveType.Capsule, new Vector3(0.24f, 0.03f, 0.12f), new Vector3(0.012f, 0.08f, 0.012f), new Color(0.1f, 0.1f, 0.12f));
                CreatePiece(torso, "ArchiveNotch", PrimitiveType.Capsule, new Vector3(-0.01f, 0.18f, 0.2f), new Vector3(0.03f, 0.05f, 0.018f), style.AccentColor * 0.88f);
                CreatePiece(torso, "ArchiveSatchel", PrimitiveType.Capsule, new Vector3(0.22f, -0.08f, -0.12f), new Vector3(0.08f, 0.12f, 0.07f), style.GearColor * 0.94f);
                CreatePiece(torso, "ArchiveSatchelStrap", PrimitiveType.Capsule, new Vector3(0.12f, 0.02f, 0.08f), new Vector3(0.025f, 0.22f, 0.02f), style.GearColor * 0.76f);
                CreatePiece(torso, "ArchiveCoatHem", PrimitiveType.Capsule, new Vector3(0f, -0.22f, 0.17f), new Vector3(0.16f, 0.025f, 0.03f), style.OuterwearColor * 0.9f);
                CreatePiece(torso, "ArchiveWaistBand", PrimitiveType.Capsule, new Vector3(0f, -0.15f, 0.15f), new Vector3(0.16f, 0.02f, 0.03f), style.GearColor * 0.72f);
                CreatePiece(torso, "ArchivistRearTab", PrimitiveType.Capsule, new Vector3(0.07f, -0.02f, -0.14f), new Vector3(0.045f, 0.085f, 0.02f), style.AccentColor * 0.72f);
                CreatePiece(head, "ArchivistUnderBob", PrimitiveType.Capsule, new Vector3(0f, -0.19f, -0.14f), new Vector3(0.09f, 0.04f, 0.03f), style.HairColor * 0.84f);
                CreatePiece(torso, "ArchivistChestBand", PrimitiveType.Capsule, new Vector3(0f, 0.12f, 0.18f), new Vector3(0.15f, 0.018f, 0.02f), style.GearColor * 0.74f);
                CreatePiece(torso, "ArchivistHipTabR", PrimitiveType.Capsule, new Vector3(0.17f, -0.11f, 0.13f), new Vector3(0.038f, 0.07f, 0.022f), style.GearColor * 0.82f);
                CreatePiece(head, "ArchivistRearCurlL", PrimitiveType.Capsule, new Vector3(-0.12f, -0.11f, -0.14f), new Vector3(0.04f, 0.08f, 0.028f), style.HairColor * 0.86f);
                CreatePiece(head, "ArchivistRearCurlR", PrimitiveType.Capsule, new Vector3(0.12f, -0.11f, -0.14f), new Vector3(0.04f, 0.08f, 0.028f), style.HairColor * 0.86f);
                CreatePiece(head, "ArchivistTempleTabL", PrimitiveType.Capsule, new Vector3(-0.19f, 0.03f, 0.15f), new Vector3(0.028f, 0.07f, 0.02f), style.HairColor * 0.86f);
                CreatePiece(head, "ArchivistTempleTabR", PrimitiveType.Capsule, new Vector3(0.19f, 0.03f, 0.15f), new Vector3(0.028f, 0.07f, 0.02f), style.HairColor * 0.86f);
                CreatePiece(torso, "ArchivistBackBand", PrimitiveType.Capsule, new Vector3(0f, -0.02f, -0.16f), new Vector3(0.15f, 0.018f, 0.022f), style.GearColor * 0.7f);
                CreatePiece(torso, "ArchivistSidePanelL", PrimitiveType.Capsule, new Vector3(-0.18f, 0.01f, 0.09f), new Vector3(0.03f, 0.2f, 0.02f), style.BodyColor * 0.84f);
                CreatePiece(torso, "ArchivistSidePanelR", PrimitiveType.Capsule, new Vector3(0.18f, 0.01f, 0.09f), new Vector3(0.03f, 0.2f, 0.02f), style.BodyColor * 0.84f);
                CreatePiece(hips, "ArchivistLegBandL", PrimitiveType.Capsule, new Vector3(-0.11f, -0.04f, 0.12f), new Vector3(0.03f, 0.08f, 0.02f), style.GearColor * 0.72f);
                CreatePiece(hips, "ArchivistLegBandR", PrimitiveType.Capsule, new Vector3(0.11f, -0.04f, 0.12f), new Vector3(0.03f, 0.08f, 0.02f), style.GearColor * 0.72f);
                CreatePiece(head, "ArchivistTopWave", PrimitiveType.Sphere, new Vector3(0f, 0.28f, 0.04f), new Vector3(0.1f, 0.08f, 0.07f), style.HairColor * 0.88f);
                CreatePiece(upperArmLeft, "ArchivistSleeveBandL", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0.11f), new Vector3(0.045f, 0.028f, 0.028f), style.GearColor * 0.76f);
                CreatePiece(upperArmRight, "ArchivistSleeveBandR", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0.11f), new Vector3(0.045f, 0.028f, 0.028f), style.GearColor * 0.76f);
                CreatePiece(lowerLegLeft, "ArchivistKneePatchL", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0.12f), new Vector3(0.045f, 0.06f, 0.024f), style.GearColor * 0.74f);
                CreatePiece(lowerLegRight, "ArchivistKneePatchR", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0.12f), new Vector3(0.045f, 0.06f, 0.024f), style.GearColor * 0.74f);
                CreatePiece(lowerArmLeft, "ArchivistWristTabL", PrimitiveType.Capsule, new Vector3(0f, -0.22f, 0.06f), new Vector3(0.038f, 0.02f, 0.026f), style.GearColor * 0.8f);
                CreatePiece(lowerArmRight, "ArchivistWristTabR", PrimitiveType.Capsule, new Vector3(0f, -0.22f, 0.06f), new Vector3(0.038f, 0.02f, 0.026f), style.GearColor * 0.8f);
                CreatePiece(lowerLegLeft, "ArchivistShoeCapL", PrimitiveType.Capsule, new Vector3(0f, -0.33f, 0.15f), new Vector3(0.05f, 0.022f, 0.028f), style.GearColor * 0.68f);
                CreatePiece(lowerLegRight, "ArchivistShoeCapR", PrimitiveType.Capsule, new Vector3(0f, -0.33f, 0.15f), new Vector3(0.05f, 0.022f, 0.028f), style.GearColor * 0.68f);
            }
            else
            {
                CreatePiece(head, "WorkerBeanieRim", PrimitiveType.Capsule, new Vector3(0f, 0.03f, 0.04f), new Vector3(0.18f, 0.025f, 0.12f), new Color(0.24f, 0.26f, 0.22f));
                CreatePiece(head, "WorkerBackCap", PrimitiveType.Capsule, new Vector3(0f, 0.05f, -0.12f), new Vector3(0.09f, 0.05f, 0.07f), new Color(0.23f, 0.25f, 0.21f));
                CreatePiece(head, "WorkerCapFront", PrimitiveType.Capsule, new Vector3(0f, 0.09f, 0.12f), new Vector3(0.11f, 0.06f, 0.06f), new Color(0.27f, 0.29f, 0.24f));
                CreatePiece(head, "WorkerCapButton", PrimitiveType.Sphere, new Vector3(0f, 0.16f, 0.02f), new Vector3(0.04f, 0.04f, 0.04f), new Color(0.31f, 0.33f, 0.27f));
                CreatePiece(head, "WorkerCapRear", PrimitiveType.Capsule, new Vector3(0f, 0.08f, -0.16f), new Vector3(0.08f, 0.06f, 0.05f), new Color(0.27f, 0.29f, 0.24f));
                CreatePiece(head, "WorkerSideFlapL", PrimitiveType.Capsule, new Vector3(-0.16f, -0.04f, 0.06f), new Vector3(0.04f, 0.08f, 0.03f), new Color(0.25f, 0.27f, 0.22f));
                CreatePiece(head, "WorkerSideFlapR", PrimitiveType.Capsule, new Vector3(0.16f, -0.04f, 0.06f), new Vector3(0.04f, 0.08f, 0.03f), new Color(0.25f, 0.27f, 0.22f));
                CreatePiece(torso, "UtilityPack", PrimitiveType.Capsule, new Vector3(0.18f, -0.04f, -0.14f), new Vector3(0.08f, 0.12f, 0.12f), style.GearColor);
                CreatePiece(upperArmLeft, "WorkBand", PrimitiveType.Capsule, new Vector3(0f, -0.08f, 0.16f), new Vector3(0.07f, 0.03f, 0.08f), new Color(0.78f, 0.66f, 0.34f));
                CreatePiece(head, "CapCrown", PrimitiveType.Capsule, new Vector3(0f, 0.11f, 0.01f), new Vector3(0.22f, 0.09f, 0.2f), new Color(0.32f, 0.34f, 0.28f));
                CreatePiece(head, "CapBrim", PrimitiveType.Cylinder, new Vector3(0f, 0.03f, 0.18f), new Vector3(0.18f, 0.015f, 0.11f), new Color(0.28f, 0.3f, 0.24f));
                CreatePiece(head, "WorkerEarFlapL", PrimitiveType.Capsule, new Vector3(-0.18f, 0.0f, 0.02f), new Vector3(0.05f, 0.12f, 0.035f), new Color(0.26f, 0.28f, 0.23f));
                CreatePiece(head, "WorkerEarFlapR", PrimitiveType.Capsule, new Vector3(0.18f, 0.0f, 0.02f), new Vector3(0.05f, 0.12f, 0.035f), new Color(0.26f, 0.28f, 0.23f));
                CreatePiece(torso, "SafetyVest", PrimitiveType.Capsule, new Vector3(0f, 0.06f, 0.19f), new Vector3(0.22f, 0.23f, 0.08f), new Color(0.74f, 0.58f, 0.24f));
                CreatePiece(lowerArmRight, "Tool", PrimitiveType.Capsule, new Vector3(0.04f, -0.24f, 0.16f), new Vector3(0.04f, 0.1f, 0.03f), new Color(0.34f, 0.36f, 0.4f));
                CreatePiece(torso, "Apron", PrimitiveType.Capsule, new Vector3(0f, -0.06f, 0.17f), new Vector3(0.16f, 0.14f, 0.05f), new Color(0.42f, 0.36f, 0.24f));
                CreatePiece(torso, "ApronPocket", PrimitiveType.Capsule, new Vector3(0f, -0.13f, 0.21f), new Vector3(0.09f, 0.06f, 0.035f), new Color(0.36f, 0.31f, 0.22f));
                CreatePiece(lowerArmLeft, "GloveL", PrimitiveType.Sphere, new Vector3(0f, -0.3f, 0.08f), new Vector3(0.08f, 0.08f, 0.08f), new Color(0.26f, 0.28f, 0.22f));
                CreatePiece(lowerArmRight, "GloveR", PrimitiveType.Sphere, new Vector3(0f, -0.3f, 0.08f), new Vector3(0.08f, 0.08f, 0.08f), new Color(0.26f, 0.28f, 0.22f));
                CreatePiece(head, "CapBack", PrimitiveType.Capsule, new Vector3(0f, 0.08f, -0.12f), new Vector3(0.1f, 0.05f, 0.08f), new Color(0.28f, 0.3f, 0.24f));
                CreatePiece(torso, "VestStripeL", PrimitiveType.Capsule, new Vector3(-0.08f, 0.04f, 0.24f), new Vector3(0.02f, 0.17f, 0.03f), new Color(0.64f, 0.54f, 0.2f));
                CreatePiece(torso, "VestStripeR", PrimitiveType.Capsule, new Vector3(0.08f, 0.04f, 0.24f), new Vector3(0.02f, 0.17f, 0.03f), new Color(0.64f, 0.54f, 0.2f));
                CreatePiece(torso, "WorkerVestPanel", PrimitiveType.Capsule, new Vector3(0f, 0.02f, 0.2f), new Vector3(0.09f, 0.2f, 0.03f), new Color(0.54f, 0.44f, 0.18f));
                CreatePiece(torso, "WorkerChestPocket", PrimitiveType.Capsule, new Vector3(0.1f, 0.04f, 0.21f), new Vector3(0.045f, 0.06f, 0.025f), new Color(0.46f, 0.38f, 0.17f));
                CreatePiece(torso, "WorkerShoulderTabL", PrimitiveType.Capsule, new Vector3(-0.1f, 0.2f, 0.17f), new Vector3(0.04f, 0.03f, 0.02f), new Color(0.48f, 0.39f, 0.18f));
                CreatePiece(torso, "WorkerShoulderTabR", PrimitiveType.Capsule, new Vector3(0.1f, 0.2f, 0.17f), new Vector3(0.04f, 0.03f, 0.02f), new Color(0.48f, 0.39f, 0.18f));
                CreatePiece(torso, "WorkerFrontSeam", PrimitiveType.Capsule, new Vector3(0f, 0.02f, 0.205f), new Vector3(0.018f, 0.2f, 0.014f), new Color(0.36f, 0.3f, 0.14f));
                CreatePiece(torso, "WorkerShoulderPatchL", PrimitiveType.Capsule, new Vector3(-0.19f, 0.18f, 0.08f), new Vector3(0.04f, 0.065f, 0.024f), new Color(0.46f, 0.38f, 0.16f));
                CreatePiece(torso, "WorkerHipPatchR", PrimitiveType.Capsule, new Vector3(0.18f, -0.11f, 0.14f), new Vector3(0.045f, 0.08f, 0.024f), new Color(0.42f, 0.35f, 0.15f));
                CreatePiece(torso, "WorkerBeltLoopL", PrimitiveType.Capsule, new Vector3(-0.1f, -0.14f, 0.16f), new Vector3(0.014f, 0.04f, 0.012f), style.GearColor * 0.86f);
                CreatePiece(torso, "ToolBelt", PrimitiveType.Capsule, new Vector3(0f, -0.16f, 0.15f), new Vector3(0.2f, 0.03f, 0.04f), style.GearColor);
                CreatePiece(torso, "VestPocket", PrimitiveType.Capsule, new Vector3(0.12f, -0.01f, 0.21f), new Vector3(0.06f, 0.08f, 0.04f), new Color(0.6f, 0.46f, 0.2f));
                CreatePiece(lowerArmRight, "ToolHead", PrimitiveType.Capsule, new Vector3(0.04f, -0.16f, 0.2f), new Vector3(0.06f, 0.03f, 0.03f), new Color(0.48f, 0.5f, 0.52f));
                CreatePiece(torso, "RadioStub", PrimitiveType.Capsule, new Vector3(-0.18f, 0.1f, 0.16f), new Vector3(0.04f, 0.08f, 0.03f), style.GearColor * 1.08f);
                CreatePiece(torso, "ShoulderTapeL", PrimitiveType.Capsule, new Vector3(-0.1f, 0.18f, 0.16f), new Vector3(0.03f, 0.12f, 0.02f), new Color(0.7f, 0.58f, 0.2f));
                CreatePiece(torso, "ShoulderTapeR", PrimitiveType.Capsule, new Vector3(0.1f, 0.18f, 0.16f), new Vector3(0.03f, 0.12f, 0.02f), new Color(0.7f, 0.58f, 0.2f));
                CreatePiece(torso, "ToolLoop", PrimitiveType.Capsule, new Vector3(0.18f, -0.16f, 0.18f), new Vector3(0.03f, 0.08f, 0.02f), style.GearColor * 0.84f);
                CreatePiece(lowerArmLeft, "WristBandWorker", PrimitiveType.Capsule, new Vector3(0f, -0.18f, 0.03f), new Vector3(0.045f, 0.025f, 0.03f), new Color(0.6f, 0.49f, 0.18f));
                CreatePiece(torso, "WorkTag", PrimitiveType.Capsule, new Vector3(-0.14f, 0.03f, 0.2f), new Vector3(0.05f, 0.04f, 0.02f), new Color(0.58f, 0.48f, 0.18f));
                CreatePiece(torso, "WorkerBackPouch", PrimitiveType.Capsule, new Vector3(-0.18f, -0.02f, -0.14f), new Vector3(0.07f, 0.1f, 0.08f), style.GearColor * 0.92f);
                CreatePiece(lowerArmRight, "ToolHandleWrap", PrimitiveType.Capsule, new Vector3(0.04f, -0.22f, 0.14f), new Vector3(0.024f, 0.05f, 0.02f), new Color(0.56f, 0.46f, 0.18f));
                CreatePiece(torso, "VestClip", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0.22f), new Vector3(0.03f, 0.05f, 0.02f), new Color(0.7f, 0.58f, 0.2f));
                CreatePiece(torso, "WorkerApronTie", PrimitiveType.Capsule, new Vector3(0f, -0.16f, -0.03f), new Vector3(0.02f, 0.18f, 0.02f), style.GearColor * 0.82f);
                CreatePiece(torso, "WorkerSideSatchel", PrimitiveType.Capsule, new Vector3(0.22f, -0.12f, 0.04f), new Vector3(0.08f, 0.11f, 0.06f), style.GearColor * 0.9f);
                CreatePiece(torso, "WorkerApronHem", PrimitiveType.Capsule, new Vector3(0f, -0.16f, 0.2f), new Vector3(0.12f, 0.02f, 0.03f), new Color(0.34f, 0.29f, 0.2f));
                CreatePiece(torso, "WorkerWaistBand", PrimitiveType.Capsule, new Vector3(0f, -0.14f, 0.15f), new Vector3(0.17f, 0.02f, 0.03f), style.GearColor * 0.74f);
                CreatePiece(torso, "WorkerRearPatch", PrimitiveType.Capsule, new Vector3(0.09f, -0.02f, -0.13f), new Vector3(0.05f, 0.085f, 0.022f), new Color(0.4f, 0.33f, 0.15f));
                CreatePiece(head, "WorkerCapSeam", PrimitiveType.Capsule, new Vector3(0f, 0.09f, 0.16f), new Vector3(0.045f, 0.018f, 0.018f), new Color(0.19f, 0.2f, 0.17f));
                CreatePiece(torso, "WorkerChestBand", PrimitiveType.Capsule, new Vector3(0f, 0.11f, 0.19f), new Vector3(0.14f, 0.018f, 0.02f), new Color(0.42f, 0.34f, 0.15f));
                CreatePiece(torso, "WorkerHipTabL", PrimitiveType.Capsule, new Vector3(-0.17f, -0.11f, 0.13f), new Vector3(0.038f, 0.07f, 0.022f), style.GearColor * 0.8f);
                CreatePiece(head, "WorkerRearFlapL", PrimitiveType.Capsule, new Vector3(-0.1f, -0.08f, -0.13f), new Vector3(0.038f, 0.07f, 0.026f), new Color(0.24f, 0.26f, 0.22f));
                CreatePiece(head, "WorkerRearFlapR", PrimitiveType.Capsule, new Vector3(0.1f, -0.08f, -0.13f), new Vector3(0.038f, 0.07f, 0.026f), new Color(0.24f, 0.26f, 0.22f));
                CreatePiece(head, "WorkerTemplePatchL", PrimitiveType.Capsule, new Vector3(-0.18f, 0.02f, 0.14f), new Vector3(0.028f, 0.06f, 0.02f), new Color(0.24f, 0.26f, 0.22f));
                CreatePiece(head, "WorkerTemplePatchR", PrimitiveType.Capsule, new Vector3(0.18f, 0.02f, 0.14f), new Vector3(0.028f, 0.06f, 0.02f), new Color(0.24f, 0.26f, 0.22f));
                CreatePiece(torso, "WorkerBackBand", PrimitiveType.Capsule, new Vector3(0f, -0.03f, -0.15f), new Vector3(0.15f, 0.018f, 0.022f), style.GearColor * 0.7f);
                CreatePiece(torso, "WorkerSidePanelL", PrimitiveType.Capsule, new Vector3(-0.18f, 0.01f, 0.09f), new Vector3(0.03f, 0.2f, 0.02f), new Color(0.46f, 0.38f, 0.18f));
                CreatePiece(torso, "WorkerSidePanelR", PrimitiveType.Capsule, new Vector3(0.18f, 0.01f, 0.09f), new Vector3(0.03f, 0.2f, 0.02f), new Color(0.46f, 0.38f, 0.18f));
                CreatePiece(hips, "WorkerLegBandL", PrimitiveType.Capsule, new Vector3(-0.11f, -0.04f, 0.12f), new Vector3(0.03f, 0.08f, 0.02f), style.GearColor * 0.72f);
                CreatePiece(hips, "WorkerLegBandR", PrimitiveType.Capsule, new Vector3(0.11f, -0.04f, 0.12f), new Vector3(0.03f, 0.08f, 0.02f), style.GearColor * 0.72f);
                CreatePiece(head, "WorkerTopFold", PrimitiveType.Capsule, new Vector3(0f, 0.17f, 0.03f), new Vector3(0.07f, 0.035f, 0.05f), new Color(0.25f, 0.27f, 0.22f));
                CreatePiece(upperArmLeft, "WorkerArmBandL", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0.11f), new Vector3(0.045f, 0.028f, 0.028f), new Color(0.48f, 0.39f, 0.18f));
                CreatePiece(upperArmRight, "WorkerArmBandR", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0.11f), new Vector3(0.045f, 0.028f, 0.028f), new Color(0.48f, 0.39f, 0.18f));
                CreatePiece(lowerLegLeft, "WorkerKneePatchL", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0.12f), new Vector3(0.045f, 0.06f, 0.024f), style.GearColor * 0.74f);
                CreatePiece(lowerLegRight, "WorkerKneePatchR", PrimitiveType.Capsule, new Vector3(0f, -0.02f, 0.12f), new Vector3(0.045f, 0.06f, 0.024f), style.GearColor * 0.74f);
                CreatePiece(lowerArmLeft, "WorkerWristTabL", PrimitiveType.Capsule, new Vector3(0f, -0.22f, 0.06f), new Vector3(0.038f, 0.02f, 0.026f), style.GearColor * 0.8f);
                CreatePiece(lowerArmRight, "WorkerWristTabR", PrimitiveType.Capsule, new Vector3(0f, -0.22f, 0.06f), new Vector3(0.038f, 0.02f, 0.026f), style.GearColor * 0.8f);
                CreatePiece(lowerLegLeft, "WorkerBootCapL", PrimitiveType.Capsule, new Vector3(0f, -0.33f, 0.15f), new Vector3(0.05f, 0.022f, 0.028f), new Color(0.4f, 0.34f, 0.18f));
                CreatePiece(lowerLegRight, "WorkerBootCapR", PrimitiveType.Capsule, new Vector3(0f, -0.33f, 0.15f), new Vector3(0.05f, 0.022f, 0.028f), new Color(0.4f, 0.34f, 0.18f));
            }
        }

        private void BuildSimplifiedHumanoid(HumanoidStyle style)
        {
            CreatePiece(visualRoot, "ShadowDisc", PrimitiveType.Cylinder, new Vector3(0f, 0.02f, 0f), new Vector3(0.8f, 0.02f, 0.8f), new Color(0.15f, 0.15f, 0.18f, 0.18f));

            hips = CreateJoint(visualRoot, "Hips", new Vector3(0f, 0.82f, 0f));
            torso = CreateJoint(hips, "Torso", new Vector3(0f, 0.44f, 0f));
            head = CreateJoint(torso, "Head", new Vector3(0f, 0.54f, 0.03f));

            BuildBaseSilhouette(style);

            switch (role)
            {
                case ArticulatedHumanoidRole.Player:
                    BuildPlayerSilhouette(style);
                    break;
                case ArticulatedHumanoidRole.Archivist:
                    BuildArchivistSilhouette(style);
                    break;
                case ArticulatedHumanoidRole.FieldWorker:
                    BuildWorkerSilhouette(style);
                    break;
            }
        }

        private void BuildBaseSilhouette(HumanoidStyle style)
        {
            CreatePiece(hips, "Pelvis", PrimitiveType.Capsule, new Vector3(0f, -0.015f, 0f), new Vector3(0.21f, 0.14f, 0.16f), style.TrouserColor);
            CreatePiece(hips, "HipDrape", PrimitiveType.Capsule, new Vector3(0f, 0.01f, 0.03f), new Vector3(0.22f, 0.09f, 0.12f), style.OuterwearColor * 0.94f);
            CreatePiece(hips, "Belt", PrimitiveType.Capsule, new Vector3(0f, 0.055f, 0.085f), new Vector3(0.14f, 0.017f, 0.022f), style.GearColor * 0.8f);

            CreatePiece(torso, "TorsoBase", PrimitiveType.Capsule, new Vector3(0f, 0.02f, 0.01f), new Vector3(0.24f, 0.42f, 0.17f), style.BodyColor);
            CreatePiece(torso, "Jacket", PrimitiveType.Capsule, new Vector3(0f, 0.05f, 0.04f), new Vector3(0.27f, 0.39f, 0.185f), style.OuterwearColor);
            CreatePiece(torso, "Collar", PrimitiveType.Capsule, new Vector3(0f, 0.22f, 0.14f), new Vector3(0.12f, 0.022f, 0.032f), style.OuterwearColor * 0.9f);
            CreatePiece(torso, "FrontPanel", PrimitiveType.Capsule, new Vector3(0f, 0.015f, 0.155f), new Vector3(0.085f, 0.22f, 0.02f), style.BodyColor * 1.03f);
            CreatePiece(torso, "WaistInset", PrimitiveType.Capsule, new Vector3(0f, -0.075f, 0.075f), new Vector3(0.17f, 0.085f, 0.06f), style.BodyColor * 0.96f);
            CreatePiece(torso, "ShoulderL", PrimitiveType.Capsule, new Vector3(-0.14f, 0.17f, 0.03f), new Vector3(0.05f, 0.055f, 0.055f), style.OuterwearColor * 0.98f);
            CreatePiece(torso, "ShoulderR", PrimitiveType.Capsule, new Vector3(0.14f, 0.17f, 0.03f), new Vector3(0.05f, 0.055f, 0.055f), style.OuterwearColor * 0.98f);
            CreatePiece(torso, "ChestLeft", PrimitiveType.Capsule, new Vector3(-0.072f, 0.05f, 0.11f), new Vector3(0.058f, 0.16f, 0.04f), style.BodyColor * 1.02f);
            CreatePiece(torso, "ChestRight", PrimitiveType.Capsule, new Vector3(0.072f, 0.05f, 0.11f), new Vector3(0.058f, 0.16f, 0.04f), style.BodyColor * 1.02f);
            CreatePiece(torso, "Neck", PrimitiveType.Capsule, new Vector3(0f, 0.285f, 0.03f), new Vector3(0.055f, 0.08f, 0.055f), style.SkinColor);

            CreatePiece(head, "HeadCore", PrimitiveType.Sphere, new Vector3(0f, 0.05f, 0f), new Vector3(0.31f, 0.35f, 0.32f), style.SkinColor);
            CreatePiece(head, "FacePlane", PrimitiveType.Sphere, new Vector3(0f, 0f, 0.1f), new Vector3(0.25f, 0.21f, 0.14f), style.SkinColor * 1.01f);
            CreatePiece(head, "CheekLeft", PrimitiveType.Sphere, new Vector3(-0.085f, -0.028f, 0.1f), new Vector3(0.06f, 0.068f, 0.055f), style.SkinColor * 0.995f);
            CreatePiece(head, "CheekRight", PrimitiveType.Sphere, new Vector3(0.085f, -0.028f, 0.1f), new Vector3(0.06f, 0.068f, 0.055f), style.SkinColor * 0.995f);
            CreatePiece(head, "Jaw", PrimitiveType.Capsule, new Vector3(0f, -0.095f, 0.068f), new Vector3(0.15f, 0.065f, 0.09f), style.SkinColor * 0.97f);
            CreatePiece(head, "HairCap", PrimitiveType.Sphere, new Vector3(0f, 0.145f, 0f), new Vector3(0.35f, 0.2f, 0.32f), style.HairColor);
            CreatePiece(head, "CrownVolume", PrimitiveType.Capsule, new Vector3(0f, 0.18f, 0.02f), new Vector3(0.15f, 0.055f, 0.085f), style.HairColor * 0.98f);
            CreatePiece(head, "HeadBack", PrimitiveType.Sphere, new Vector3(0f, 0.03f, -0.095f), new Vector3(0.22f, 0.22f, 0.13f), style.SkinColor * 0.99f);
            CreatePiece(head, "HairBackBase", PrimitiveType.Capsule, new Vector3(0f, 0.02f, -0.13f), new Vector3(0.16f, 0.15f, 0.06f), style.HairColor * 0.94f);
            CreatePiece(head, "Hairline", PrimitiveType.Capsule, new Vector3(0f, 0.1f, 0.145f), new Vector3(0.17f, 0.028f, 0.022f), style.HairColor * 0.98f);
            CreatePiece(head, "SideHairBaseL", PrimitiveType.Capsule, new Vector3(-0.15f, 0.03f, 0.02f), new Vector3(0.045f, 0.13f, 0.035f), style.HairColor * 0.95f);
            CreatePiece(head, "SideHairBaseR", PrimitiveType.Capsule, new Vector3(0.15f, 0.03f, 0.02f), new Vector3(0.045f, 0.13f, 0.035f), style.HairColor * 0.95f);
            CreatePiece(head, "NapeHair", PrimitiveType.Capsule, new Vector3(0f, -0.075f, -0.12f), new Vector3(0.095f, 0.075f, 0.034f), style.HairColor * 0.92f);
            CreatePiece(head, "BrowLeft", PrimitiveType.Capsule, new Vector3(-0.055f, 0.06f, 0.17f), new Vector3(0.034f, 0.01f, 0.01f), style.HairColor * 0.9f);
            CreatePiece(head, "BrowRight", PrimitiveType.Capsule, new Vector3(0.055f, 0.06f, 0.17f), new Vector3(0.034f, 0.01f, 0.01f), style.HairColor * 0.9f);
            CreatePiece(head, "EyeLeft", PrimitiveType.Sphere, new Vector3(-0.05f, 0.012f, 0.175f), new Vector3(0.018f, 0.013f, 0.01f), new Color(0.11f, 0.1f, 0.1f));
            CreatePiece(head, "EyeRight", PrimitiveType.Sphere, new Vector3(0.05f, 0.012f, 0.175f), new Vector3(0.018f, 0.013f, 0.01f), new Color(0.11f, 0.1f, 0.1f));
            CreatePiece(head, "Nose", PrimitiveType.Capsule, new Vector3(0f, -0.015f, 0.17f), new Vector3(0.013f, 0.03f, 0.01f), style.SkinColor * 0.94f);
            CreatePiece(head, "Mouth", PrimitiveType.Capsule, new Vector3(0f, -0.08f, 0.15f), new Vector3(0.024f, 0.007f, 0.01f), new Color(0.23f, 0.18f, 0.17f));
            CreatePiece(head, "Chin", PrimitiveType.Sphere, new Vector3(0f, -0.115f, 0.085f), new Vector3(0.07f, 0.044f, 0.044f), style.SkinColor * 0.96f);
            CreatePiece(head, "EarLeft", PrimitiveType.Sphere, new Vector3(-0.19f, 0.01f, 0.01f), new Vector3(0.045f, 0.07f, 0.03f), style.SkinColor * 0.98f);
            CreatePiece(head, "EarRight", PrimitiveType.Sphere, new Vector3(0.19f, 0.01f, 0.01f), new Vector3(0.045f, 0.07f, 0.03f), style.SkinColor * 0.98f);

            upperArmLeft = CreateJoint(torso, "UpperArmLeft", new Vector3(-0.22f, 0.12f, 0f));
            lowerArmLeft = CreateJoint(upperArmLeft, "LowerArmLeft", new Vector3(0f, -0.25f, 0f));
            upperArmRight = CreateJoint(torso, "UpperArmRight", new Vector3(0.22f, 0.12f, 0f));
            lowerArmRight = CreateJoint(upperArmRight, "LowerArmRight", new Vector3(0f, -0.25f, 0f));
            CreatePiece(upperArmLeft, "UpperArmVisual", PrimitiveType.Capsule, new Vector3(0f, -0.13f, 0f), new Vector3(0.058f, 0.22f, 0.058f), style.OuterwearColor);
            CreatePiece(upperArmRight, "UpperArmVisual", PrimitiveType.Capsule, new Vector3(0f, -0.13f, 0f), new Vector3(0.058f, 0.22f, 0.058f), style.OuterwearColor);
            CreatePiece(lowerArmLeft, "LowerArmVisual", PrimitiveType.Capsule, new Vector3(0f, -0.13f, 0f), new Vector3(0.05f, 0.2f, 0.05f), style.SkinColor);
            CreatePiece(lowerArmRight, "LowerArmVisual", PrimitiveType.Capsule, new Vector3(0f, -0.13f, 0f), new Vector3(0.05f, 0.2f, 0.05f), style.SkinColor);
            CreatePiece(lowerArmLeft, "HandLeft", PrimitiveType.Sphere, new Vector3(0f, -0.25f, 0.03f), new Vector3(0.05f, 0.05f, 0.048f), style.SkinColor);
            CreatePiece(lowerArmRight, "HandRight", PrimitiveType.Sphere, new Vector3(0f, -0.25f, 0.03f), new Vector3(0.05f, 0.05f, 0.048f), style.SkinColor);

            upperLegLeft = CreateJoint(hips, "UpperLegLeft", new Vector3(-0.11f, -0.08f, 0f));
            lowerLegLeft = CreateJoint(upperLegLeft, "LowerLegLeft", new Vector3(0f, -0.35f, 0f));
            upperLegRight = CreateJoint(hips, "UpperLegRight", new Vector3(0.11f, -0.08f, 0f));
            lowerLegRight = CreateJoint(upperLegRight, "LowerLegRight", new Vector3(0f, -0.35f, 0f));
            CreatePiece(upperLegLeft, "UpperLegVisual", PrimitiveType.Capsule, new Vector3(0f, -0.19f, 0f), new Vector3(0.084f, 0.43f, 0.084f), style.TrouserColor);
            CreatePiece(upperLegRight, "UpperLegVisual", PrimitiveType.Capsule, new Vector3(0f, -0.19f, 0f), new Vector3(0.084f, 0.43f, 0.084f), style.TrouserColor);
            CreatePiece(lowerLegLeft, "LowerLegVisual", PrimitiveType.Capsule, new Vector3(0f, -0.2f, 0f), new Vector3(0.068f, 0.35f, 0.068f), style.TrouserColor * 0.93f);
            CreatePiece(lowerLegRight, "LowerLegVisual", PrimitiveType.Capsule, new Vector3(0f, -0.2f, 0f), new Vector3(0.068f, 0.35f, 0.068f), style.TrouserColor * 0.93f);
            CreatePiece(lowerLegLeft, "FootLeft", PrimitiveType.Capsule, new Vector3(0f, -0.355f, 0.08f), new Vector3(0.072f, 0.04f, 0.145f), style.GearColor);
            CreatePiece(lowerLegRight, "FootRight", PrimitiveType.Capsule, new Vector3(0f, -0.355f, 0.08f), new Vector3(0.072f, 0.04f, 0.145f), style.GearColor);
            CreatePiece(lowerLegLeft, "AnkleLeft", PrimitiveType.Capsule, new Vector3(0f, -0.265f, 0.04f), new Vector3(0.048f, 0.028f, 0.04f), style.GearColor * 0.88f);
            CreatePiece(lowerLegRight, "AnkleRight", PrimitiveType.Capsule, new Vector3(0f, -0.265f, 0.04f), new Vector3(0.048f, 0.028f, 0.04f), style.GearColor * 0.88f);
        }

        private void BuildPlayerSilhouette(HumanoidStyle style)
        {
            CreatePiece(head, "PlayerSweep", PrimitiveType.Capsule, new Vector3(0.09f, 0.075f, 0.14f), new Vector3(0.075f, 0.165f, 0.042f), style.HairColor * 0.95f);
            CreatePiece(head, "PlayerBackHair", PrimitiveType.Capsule, new Vector3(0f, -0.035f, -0.16f), new Vector3(0.21f, 0.18f, 0.075f), style.HairColor * 0.92f);
            CreatePiece(head, "PlayerSideLockL", PrimitiveType.Capsule, new Vector3(-0.16f, 0f, 0.07f), new Vector3(0.05f, 0.125f, 0.036f), style.HairColor * 0.9f);
            CreatePiece(head, "PlayerSideLockR", PrimitiveType.Capsule, new Vector3(0.14f, 0f, 0.06f), new Vector3(0.045f, 0.105f, 0.032f), style.HairColor * 0.88f);
            CreatePiece(head, "PlayerCrown", PrimitiveType.Capsule, new Vector3(0.015f, 0.19f, 0.03f), new Vector3(0.145f, 0.065f, 0.09f), style.HairColor * 0.98f);
            CreatePiece(head, "PlayerForelock", PrimitiveType.Capsule, new Vector3(-0.01f, 0.11f, 0.215f), new Vector3(0.1f, 0.07f, 0.03f), style.HairColor * 0.97f);
            CreatePiece(head, "PlayerTempleFill", PrimitiveType.Capsule, new Vector3(-0.11f, 0.08f, 0.11f), new Vector3(0.04f, 0.08f, 0.032f), style.HairColor * 0.93f);
            CreatePiece(head, "PlayerTempleFillR", PrimitiveType.Capsule, new Vector3(0.11f, 0.075f, 0.09f), new Vector3(0.034f, 0.072f, 0.03f), style.HairColor * 0.9f);
            CreatePiece(head, "PlayerCrownRear", PrimitiveType.Capsule, new Vector3(0f, 0.12f, -0.14f), new Vector3(0.14f, 0.09f, 0.05f), style.HairColor * 0.94f);
            CreatePiece(head, "PlayerTopWave", PrimitiveType.Capsule, new Vector3(0.06f, 0.2f, 0.05f), new Vector3(0.08f, 0.04f, 0.05f), style.HairColor * 0.98f);
            CreatePiece(head, "PlayerNeckTuft", PrimitiveType.Capsule, new Vector3(0f, -0.1f, -0.12f), new Vector3(0.09f, 0.06f, 0.03f), style.HairColor * 0.88f);
            CreatePiece(torso, "Backpack", PrimitiveType.Capsule, new Vector3(0f, 0.025f, -0.17f), new Vector3(0.215f, 0.235f, 0.145f), style.GearColor);
            CreatePiece(torso, "BackpackRoll", PrimitiveType.Capsule, new Vector3(0f, 0.17f, -0.17f), new Vector3(0.17f, 0.055f, 0.075f), style.GearColor * 1.04f);
            CreatePiece(torso, "ScarfFront", PrimitiveType.Capsule, new Vector3(0f, 0.16f, 0.17f), new Vector3(0.19f, 0.032f, 0.026f), style.AccentColor);
            CreatePiece(torso, "ScarfKnot", PrimitiveType.Sphere, new Vector3(-0.05f, 0.11f, 0.18f), new Vector3(0.05f, 0.05f, 0.04f), style.AccentColor * 0.98f);
            CreatePiece(torso, "ScarfTail", PrimitiveType.Capsule, new Vector3(-0.02f, -0.02f, 0.18f), new Vector3(0.035f, 0.15f, 0.02f), style.AccentColor * 0.92f);
            CreatePiece(torso, "FrontPocketL", PrimitiveType.Capsule, new Vector3(-0.11f, -0.03f, 0.17f), new Vector3(0.055f, 0.07f, 0.035f), style.GearColor * 0.96f);
            CreatePiece(torso, "FrontPocketR", PrimitiveType.Capsule, new Vector3(0.11f, -0.03f, 0.17f), new Vector3(0.055f, 0.07f, 0.035f), style.GearColor * 0.96f);
            CreatePiece(torso, "HarnessBandL", PrimitiveType.Capsule, new Vector3(-0.08f, 0.05f, 0.1f), new Vector3(0.02f, 0.25f, 0.016f), style.GearColor * 0.82f);
            CreatePiece(torso, "HarnessBandR", PrimitiveType.Capsule, new Vector3(0.08f, 0.05f, 0.1f), new Vector3(0.02f, 0.25f, 0.016f), style.GearColor * 0.82f);
            CreatePiece(torso, "SurvivorHemL", PrimitiveType.Capsule, new Vector3(-0.095f, -0.145f, 0.11f), new Vector3(0.045f, 0.07f, 0.022f), style.OuterwearColor * 0.9f);
            CreatePiece(torso, "SurvivorHemR", PrimitiveType.Capsule, new Vector3(0.095f, -0.145f, 0.11f), new Vector3(0.045f, 0.07f, 0.022f), style.OuterwearColor * 0.9f);
            CreatePiece(torso, "SurvivorShoulderPatchL", PrimitiveType.Capsule, new Vector3(-0.18f, 0.18f, 0.14f), new Vector3(0.05f, 0.03f, 0.02f), style.AccentColor * 0.9f);
            CreatePiece(torso, "SurvivorSideHemL", PrimitiveType.Capsule, new Vector3(-0.15f, -0.1f, 0.06f), new Vector3(0.026f, 0.1f, 0.018f), style.OuterwearColor * 0.88f);
            CreatePiece(torso, "SurvivorSideHemR", PrimitiveType.Capsule, new Vector3(0.15f, -0.1f, 0.06f), new Vector3(0.026f, 0.1f, 0.018f), style.OuterwearColor * 0.88f);
            CreatePiece(torso, "SurvivorCenterZip", PrimitiveType.Capsule, new Vector3(0f, 0.01f, 0.18f), new Vector3(0.016f, 0.28f, 0.012f), style.GearColor * 0.86f);
            CreatePiece(torso, "SurvivorChestPanelL", PrimitiveType.Capsule, new Vector3(-0.082f, 0.06f, 0.15f), new Vector3(0.048f, 0.13f, 0.022f), style.BodyColor * 1.03f);
            CreatePiece(torso, "SurvivorChestPanelR", PrimitiveType.Capsule, new Vector3(0.082f, 0.06f, 0.15f), new Vector3(0.048f, 0.13f, 0.022f), style.BodyColor * 1.03f);
            CreatePiece(torso, "SurvivorShoulderPatchR", PrimitiveType.Capsule, new Vector3(0.18f, 0.18f, 0.14f), new Vector3(0.045f, 0.028f, 0.02f), style.AccentColor * 0.86f);
            CreatePiece(torso, "SurvivorShoulderPatchL", PrimitiveType.Capsule, new Vector3(-0.18f, 0.18f, 0.14f), new Vector3(0.045f, 0.028f, 0.02f), style.AccentColor * 0.86f);
            CreatePiece(torso, "SurvivorWaistPocket", PrimitiveType.Capsule, new Vector3(0.12f, -0.09f, 0.16f), new Vector3(0.06f, 0.055f, 0.03f), style.GearColor * 0.94f);
        }

        private void BuildArchivistSilhouette(HumanoidStyle style)
        {
            CreatePiece(head, "ArchivistBob", PrimitiveType.Capsule, new Vector3(0f, -0.015f, -0.13f), new Vector3(0.215f, 0.145f, 0.082f), style.HairColor * 0.96f);
            CreatePiece(head, "ArchivistFringe", PrimitiveType.Capsule, new Vector3(0.01f, 0.1f, 0.21f), new Vector3(0.2f, 0.065f, 0.03f), style.HairColor);
            CreatePiece(head, "ArchivistSidePartL", PrimitiveType.Capsule, new Vector3(-0.15f, 0.04f, 0.08f), new Vector3(0.042f, 0.115f, 0.03f), style.HairColor * 0.93f);
            CreatePiece(head, "ArchivistSidePartR", PrimitiveType.Capsule, new Vector3(0.15f, 0.04f, 0.08f), new Vector3(0.042f, 0.115f, 0.03f), style.HairColor * 0.93f);
            CreatePiece(head, "ArchivistBackVolume", PrimitiveType.Capsule, new Vector3(0f, -0.02f, -0.165f), new Vector3(0.18f, 0.12f, 0.065f), style.HairColor * 0.92f);
            CreatePiece(head, "ArchivistForePart", PrimitiveType.Capsule, new Vector3(0.05f, 0.1f, 0.2f), new Vector3(0.085f, 0.055f, 0.03f), style.HairColor * 0.97f);
            CreatePiece(head, "ArchivistTempleFill", PrimitiveType.Capsule, new Vector3(0.11f, 0.08f, 0.11f), new Vector3(0.038f, 0.075f, 0.03f), style.HairColor * 0.93f);
            CreatePiece(head, "ArchivistTempleFillL", PrimitiveType.Capsule, new Vector3(-0.11f, 0.075f, 0.09f), new Vector3(0.034f, 0.07f, 0.028f), style.HairColor * 0.91f);
            CreatePiece(head, "ArchivistCrownRear", PrimitiveType.Capsule, new Vector3(0f, 0.11f, -0.14f), new Vector3(0.13f, 0.08f, 0.05f), style.HairColor * 0.94f);
            CreatePiece(head, "ArchivistTopCurve", PrimitiveType.Capsule, new Vector3(0f, 0.18f, 0.03f), new Vector3(0.14f, 0.05f, 0.08f), style.HairColor * 0.97f);
            CreatePiece(head, "ArchivistNapeCurve", PrimitiveType.Capsule, new Vector3(0f, -0.085f, -0.12f), new Vector3(0.12f, 0.05f, 0.03f), style.HairColor * 0.9f);
            CreatePiece(head, "GlassesBridge", PrimitiveType.Capsule, new Vector3(0f, 0.02f, 0.25f), new Vector3(0.16f, 0.02f, 0.015f), new Color(0.08f, 0.08f, 0.1f));
            CreatePiece(head, "GlassesLeft", PrimitiveType.Capsule, new Vector3(-0.105f, 0.02f, 0.25f), new Vector3(0.082f, 0.052f, 0.015f), new Color(0.08f, 0.08f, 0.1f));
            CreatePiece(head, "GlassesRight", PrimitiveType.Capsule, new Vector3(0.105f, 0.02f, 0.25f), new Vector3(0.082f, 0.052f, 0.015f), new Color(0.08f, 0.08f, 0.1f));
            CreatePiece(head, "HeadsetBand", PrimitiveType.Capsule, new Vector3(0f, 0.18f, -0.03f), new Vector3(0.17f, 0.035f, 0.13f), new Color(0.16f, 0.18f, 0.19f));
            CreatePiece(head, "HeadsetCupLeft", PrimitiveType.Sphere, new Vector3(-0.235f, 0.03f, 0f), new Vector3(0.11f, 0.12f, 0.085f), new Color(0.16f, 0.18f, 0.19f));
            CreatePiece(head, "HeadsetCupRight", PrimitiveType.Sphere, new Vector3(0.235f, 0.03f, 0f), new Vector3(0.11f, 0.12f, 0.085f), new Color(0.16f, 0.18f, 0.19f));
            CreatePiece(head, "HeadsetMic", PrimitiveType.Capsule, new Vector3(0.16f, -0.04f, 0.2f), new Vector3(0.015f, 0.07f, 0.012f), new Color(0.16f, 0.18f, 0.19f));
            CreatePiece(torso, "ArchivePad", PrimitiveType.Capsule, new Vector3(-0.185f, 0.015f, 0.165f), new Vector3(0.075f, 0.12f, 0.03f), new Color(0.65f, 0.63f, 0.58f));
            CreatePiece(torso, "ArchiveStrap", PrimitiveType.Capsule, new Vector3(-0.075f, 0.02f, 0.1f), new Vector3(0.025f, 0.28f, 0.018f), style.GearColor * 0.9f);
            CreatePiece(torso, "CoatFrontL", PrimitiveType.Capsule, new Vector3(-0.078f, -0.01f, 0.18f), new Vector3(0.062f, 0.33f, 0.028f), style.OuterwearColor * 0.92f);
            CreatePiece(torso, "CoatFrontR", PrimitiveType.Capsule, new Vector3(0.078f, -0.01f, 0.18f), new Vector3(0.062f, 0.33f, 0.028f), style.OuterwearColor * 0.92f);
            CreatePiece(torso, "ArchivistHem", PrimitiveType.Capsule, new Vector3(0f, -0.2f, 0.12f), new Vector3(0.16f, 0.02f, 0.024f), style.OuterwearColor * 0.88f);
            CreatePiece(torso, "ArchivistCollarTag", PrimitiveType.Capsule, new Vector3(0.12f, 0.14f, 0.15f), new Vector3(0.035f, 0.05f, 0.018f), style.GearColor * 0.92f);
            CreatePiece(torso, "ArchivistSideCoatL", PrimitiveType.Capsule, new Vector3(-0.14f, -0.1f, 0.07f), new Vector3(0.028f, 0.2f, 0.02f), style.OuterwearColor * 0.9f);
            CreatePiece(torso, "ArchivistSideCoatR", PrimitiveType.Capsule, new Vector3(0.14f, -0.1f, 0.07f), new Vector3(0.028f, 0.2f, 0.02f), style.OuterwearColor * 0.9f);
            CreatePiece(torso, "ArchivistCenterLine", PrimitiveType.Capsule, new Vector3(0f, 0.01f, 0.18f), new Vector3(0.014f, 0.3f, 0.012f), style.GearColor * 0.8f);
            CreatePiece(torso, "ArchivistLapelL", PrimitiveType.Capsule, new Vector3(-0.06f, 0.13f, 0.17f), new Vector3(0.035f, 0.08f, 0.018f), style.OuterwearColor * 0.86f);
            CreatePiece(torso, "ArchivistLapelR", PrimitiveType.Capsule, new Vector3(0.06f, 0.13f, 0.17f), new Vector3(0.035f, 0.08f, 0.018f), style.OuterwearColor * 0.86f);
            CreatePiece(torso, "ArchivistChestInset", PrimitiveType.Capsule, new Vector3(0f, 0.07f, 0.16f), new Vector3(0.09f, 0.16f, 0.02f), style.BodyColor * 1.02f);
            CreatePiece(torso, "ArchivistShoulderInsetL", PrimitiveType.Capsule, new Vector3(-0.13f, 0.16f, 0.15f), new Vector3(0.04f, 0.03f, 0.02f), style.GearColor * 0.84f);
            CreatePiece(torso, "ArchivistShoulderInsetR", PrimitiveType.Capsule, new Vector3(0.13f, 0.16f, 0.15f), new Vector3(0.04f, 0.03f, 0.02f), style.GearColor * 0.84f);
        }

        private void BuildWorkerSilhouette(HumanoidStyle style)
        {
            CreatePiece(head, "CapCrown", PrimitiveType.Capsule, new Vector3(0f, 0.125f, 0f), new Vector3(0.225f, 0.082f, 0.185f), new Color(0.28f, 0.3f, 0.25f));
            CreatePiece(head, "CapBrim", PrimitiveType.Capsule, new Vector3(0f, 0.05f, 0.18f), new Vector3(0.15f, 0.02f, 0.06f), new Color(0.25f, 0.27f, 0.22f));
            CreatePiece(head, "CapBack", PrimitiveType.Capsule, new Vector3(0f, 0.05f, -0.14f), new Vector3(0.085f, 0.05f, 0.06f), new Color(0.25f, 0.27f, 0.22f));
            CreatePiece(head, "WorkerSideburnL", PrimitiveType.Capsule, new Vector3(-0.15f, -0.01f, 0.05f), new Vector3(0.035f, 0.08f, 0.028f), style.HairColor * 0.88f);
            CreatePiece(head, "WorkerSideburnR", PrimitiveType.Capsule, new Vector3(0.15f, -0.01f, 0.05f), new Vector3(0.035f, 0.08f, 0.028f), style.HairColor * 0.88f);
            CreatePiece(head, "WorkerCapButton", PrimitiveType.Sphere, new Vector3(0f, 0.16f, 0.02f), new Vector3(0.028f, 0.02f, 0.028f), new Color(0.23f, 0.25f, 0.2f));
            CreatePiece(head, "WorkerCapSideL", PrimitiveType.Capsule, new Vector3(-0.11f, 0.07f, 0.05f), new Vector3(0.05f, 0.05f, 0.03f), new Color(0.24f, 0.26f, 0.21f));
            CreatePiece(head, "WorkerCapSideR", PrimitiveType.Capsule, new Vector3(0.11f, 0.07f, 0.05f), new Vector3(0.05f, 0.05f, 0.03f), new Color(0.24f, 0.26f, 0.21f));
            CreatePiece(head, "WorkerCapTop", PrimitiveType.Capsule, new Vector3(0f, 0.16f, 0.03f), new Vector3(0.11f, 0.04f, 0.08f), new Color(0.24f, 0.26f, 0.21f));
            CreatePiece(head, "WorkerCapRear", PrimitiveType.Capsule, new Vector3(0f, 0.08f, -0.14f), new Vector3(0.09f, 0.05f, 0.05f), new Color(0.24f, 0.26f, 0.21f));
            CreatePiece(head, "WorkerCapFront", PrimitiveType.Capsule, new Vector3(0f, 0.1f, 0.11f), new Vector3(0.13f, 0.03f, 0.05f), new Color(0.24f, 0.26f, 0.21f));
            CreatePiece(head, "WorkerNeckHair", PrimitiveType.Capsule, new Vector3(0f, -0.08f, -0.11f), new Vector3(0.1f, 0.045f, 0.03f), style.HairColor * 0.86f);
            CreatePiece(torso, "SafetyVest", PrimitiveType.Capsule, new Vector3(0f, 0.05f, 0.17f), new Vector3(0.21f, 0.205f, 0.06f), new Color(0.54f, 0.45f, 0.2f));
            CreatePiece(torso, "VestStripeL", PrimitiveType.Capsule, new Vector3(-0.08f, 0.03f, 0.22f), new Vector3(0.016f, 0.14f, 0.018f), new Color(0.7f, 0.6f, 0.28f));
            CreatePiece(torso, "VestStripeR", PrimitiveType.Capsule, new Vector3(0.08f, 0.03f, 0.22f), new Vector3(0.016f, 0.14f, 0.018f), new Color(0.7f, 0.6f, 0.28f));
            CreatePiece(torso, "Apron", PrimitiveType.Capsule, new Vector3(0f, -0.04f, 0.15f), new Vector3(0.13f, 0.11f, 0.04f), new Color(0.38f, 0.32f, 0.21f));
            CreatePiece(torso, "ToolBelt", PrimitiveType.Capsule, new Vector3(0f, -0.15f, 0.12f), new Vector3(0.17f, 0.024f, 0.03f), style.GearColor);
            CreatePiece(torso, "ToolPouch", PrimitiveType.Capsule, new Vector3(0.155f, -0.1f, 0.1f), new Vector3(0.065f, 0.085f, 0.045f), style.GearColor * 0.94f);
            CreatePiece(lowerArmRight, "Tool", PrimitiveType.Capsule, new Vector3(0.03f, -0.21f, 0.14f), new Vector3(0.032f, 0.075f, 0.022f), new Color(0.34f, 0.36f, 0.4f));
            CreatePiece(torso, "WorkerPocketL", PrimitiveType.Capsule, new Vector3(-0.08f, -0.03f, 0.18f), new Vector3(0.05f, 0.08f, 0.03f), style.GearColor * 0.9f);
            CreatePiece(torso, "WorkerPocketR", PrimitiveType.Capsule, new Vector3(0.08f, -0.03f, 0.18f), new Vector3(0.05f, 0.08f, 0.03f), style.GearColor * 0.9f);
            CreatePiece(torso, "ApronTie", PrimitiveType.Capsule, new Vector3(0f, -0.08f, -0.02f), new Vector3(0.018f, 0.14f, 0.018f), style.GearColor * 0.82f);
            CreatePiece(torso, "WorkerShoulderPatchR", PrimitiveType.Capsule, new Vector3(0.18f, 0.18f, 0.14f), new Vector3(0.05f, 0.03f, 0.02f), new Color(0.62f, 0.52f, 0.22f));
            CreatePiece(torso, "WorkerVestHem", PrimitiveType.Capsule, new Vector3(0f, -0.13f, 0.14f), new Vector3(0.13f, 0.02f, 0.022f), new Color(0.46f, 0.39f, 0.18f));
            CreatePiece(torso, "WorkerCenterLine", PrimitiveType.Capsule, new Vector3(0f, -0.005f, 0.18f), new Vector3(0.014f, 0.24f, 0.012f), new Color(0.44f, 0.37f, 0.18f));
            CreatePiece(torso, "WorkerBib", PrimitiveType.Capsule, new Vector3(0f, 0.04f, 0.17f), new Vector3(0.085f, 0.11f, 0.024f), new Color(0.42f, 0.35f, 0.19f));
            CreatePiece(torso, "WorkerShoulderPatchL", PrimitiveType.Capsule, new Vector3(-0.18f, 0.18f, 0.14f), new Vector3(0.045f, 0.028f, 0.02f), new Color(0.58f, 0.49f, 0.22f));
            CreatePiece(torso, "WorkerChestInset", PrimitiveType.Capsule, new Vector3(0f, 0.05f, 0.16f), new Vector3(0.095f, 0.145f, 0.02f), new Color(0.36f, 0.3f, 0.18f));
        }

        private static HumanoidStyle GetStyle(ArticulatedHumanoidRole nextRole)
        {
            var style = new HumanoidStyle
            {
                SkinColor = nextRole == ArticulatedHumanoidRole.Player
                    ? new Color(0.94f, 0.82f, 0.73f)
                    : nextRole == ArticulatedHumanoidRole.Archivist
                        ? new Color(0.96f, 0.84f, 0.73f)
                        : new Color(0.95f, 0.82f, 0.72f),
                BodyColor = nextRole == ArticulatedHumanoidRole.Player
                    ? new Color(0.09f, 0.10f, 0.11f)
                    : nextRole == ArticulatedHumanoidRole.Archivist
                        ? new Color(0.24f, 0.25f, 0.27f)
                        : new Color(0.13f, 0.14f, 0.15f),
                AccentColor = nextRole == ArticulatedHumanoidRole.Player
                    ? new Color(0.20f, 0.21f, 0.23f)
                    : nextRole == ArticulatedHumanoidRole.Archivist
                        ? new Color(0.81f, 0.79f, 0.72f)
                        : new Color(0.82f, 0.77f, 0.67f),
                HairColor = nextRole == ArticulatedHumanoidRole.Player
                    ? new Color(0.07f, 0.07f, 0.08f)
                    : nextRole == ArticulatedHumanoidRole.Archivist
                        ? new Color(0.62f, 0.37f, 0.20f)
                        : new Color(0.46f, 0.29f, 0.18f),
                TrouserColor = nextRole == ArticulatedHumanoidRole.Player
                    ? new Color(0.11f, 0.12f, 0.13f)
                    : nextRole == ArticulatedHumanoidRole.Archivist
                        ? new Color(0.17f, 0.17f, 0.18f)
                        : new Color(0.23f, 0.21f, 0.18f),
                OuterwearColor = nextRole == ArticulatedHumanoidRole.Player
                    ? new Color(0.10f, 0.11f, 0.12f)
                    : nextRole == ArticulatedHumanoidRole.Archivist
                        ? new Color(0.39f, 0.40f, 0.42f)
                        : new Color(0.64f, 0.62f, 0.57f),
                GearColor = nextRole == ArticulatedHumanoidRole.Player
                    ? new Color(0.10f, 0.10f, 0.11f)
                    : nextRole == ArticulatedHumanoidRole.Archivist
                        ? new Color(0.48f, 0.31f, 0.19f)
                        : new Color(0.49f, 0.31f, 0.19f),
                HeadScale = nextRole == ArticulatedHumanoidRole.Player
                    ? new Vector3(0.56f, 0.56f, 0.54f)
                    : nextRole == ArticulatedHumanoidRole.FieldWorker
                        ? new Vector3(0.55f, 0.55f, 0.52f)
                        : new Vector3(0.55f, 0.56f, 0.53f),
                TorsoScale = nextRole == ArticulatedHumanoidRole.Player
                    ? new Vector3(0.5f, 0.82f, 0.34f)
                    : new Vector3(0.49f, 0.8f, 0.33f),
                HipScale = new Vector3(0.4f, 0.32f, 0.27f),
                UpperArmScale = new Vector3(0.094f, 0.4f, 0.094f),
                LowerArmScale = new Vector3(0.086f, 0.37f, 0.086f),
                UpperLegScale = new Vector3(0.132f, 0.52f, 0.132f),
                LowerLegScale = new Vector3(0.115f, 0.45f, 0.115f),
                ChestScale = nextRole == ArticulatedHumanoidRole.Player
                    ? new Vector3(0.48f, 0.56f, 0.29f)
                    : nextRole == ArticulatedHumanoidRole.FieldWorker
                        ? new Vector3(0.48f, 0.56f, 0.29f)
                        : new Vector3(0.47f, 0.56f, 0.28f),
                WaistScale = nextRole == ArticulatedHumanoidRole.Player
                    ? new Vector3(0.24f, 0.2f, 0.2f)
                    : new Vector3(0.235f, 0.2f, 0.195f),
                ShoulderWidth = nextRole == ArticulatedHumanoidRole.Player ? 0.24f : 0.23f,
                HipWidth = nextRole == ArticulatedHumanoidRole.FieldWorker ? 0.19f : 0.18f,
                NeckHeight = nextRole == ArticulatedHumanoidRole.Player ? 0.064f : 0.062f,
                EyeScale = nextRole == ArticulatedHumanoidRole.Player
                    ? new Vector3(0.05f, 0.03f, 0.016f)
                    : new Vector3(0.048f, 0.03f, 0.016f),
                HandScale = new Vector3(0.088f, 0.092f, 0.082f),
                FootScale = nextRole == ArticulatedHumanoidRole.Player
                    ? new Vector3(0.15f, 0.085f, 0.21f)
                    : new Vector3(0.15f, 0.085f, 0.2f)
            };

            if (nextRole == ArticulatedHumanoidRole.Monster)
            {
                style.SkinColor = new Color(0.43f, 0.44f, 0.47f);
                style.BodyColor = new Color(0.16f, 0.12f, 0.14f);
                style.AccentColor = new Color(0.69f, 0.19f, 0.18f);
                style.HairColor = new Color(0.09f, 0.07f, 0.08f);
                style.TrouserColor = new Color(0.13f, 0.11f, 0.12f);
                style.OuterwearColor = new Color(0.18f, 0.13f, 0.16f);
                style.GearColor = new Color(0.28f, 0.19f, 0.2f);
                style.HeadScale = new Vector3(0.6f, 0.6f, 0.56f);
                style.TorsoScale = new Vector3(0.56f, 0.92f, 0.4f);
                style.HipScale = new Vector3(0.42f, 0.34f, 0.3f);
                style.ChestScale = new Vector3(0.54f, 0.62f, 0.34f);
                style.WaistScale = new Vector3(0.26f, 0.22f, 0.22f);
                style.ShoulderWidth = 0.27f;
                style.HipWidth = 0.2f;
            }
            return style;
        }

        private bool TryBuildImportedModel(HumanoidStyle style)
        {
            var resourcePath = GetRoleModelResourcePath();
            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                return false;
            }

            CreatePiece(visualRoot, "ShadowDisc", PrimitiveType.Cylinder, new Vector3(0f, 0.02f, 0f), new Vector3(0.8f, 0.02f, 0.8f), new Color(0.15f, 0.15f, 0.18f, 0.18f));

            var poseRoot = new GameObject(ImportedModelRootName).transform;
            poseRoot.SetParent(visualRoot, false);
            poseRoot.localPosition = Vector3.zero;
            poseRoot.localRotation = Quaternion.identity;
            poseRoot.localScale = Vector3.one;
            importedModelRoot = poseRoot;

            var instance = Instantiate(prefab, importedModelRoot);
            instance.name = ImportedModelVisualName;
            instance.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            instance.transform.localRotation = GetImportedModelBaseRotation();
            instance.transform.localScale = Vector3.one;
            importedModelVisual = instance.transform;

            var renderers = importedModelVisual.GetComponentsInChildren<Renderer>(true);
            RemoveImportedColliders(importedModelVisual);
            AutoOrientImportedModel(renderers);
            FitImportedModelToRoleHeight(renderers);
            ApplyImportedRolePalette(renderers, style, GetRoleTextureFolderPath(resourcePath));
            AddImportedRoleAccessories(style);
            ConfigureImportedAnimation(resourcePath);
            return true;
        }

        private string GetRoleModelResourcePath()
        {
            switch (role)
            {
                case ArticulatedHumanoidRole.Archivist:
                    return ArchivistModelResourcePath;
                case ArticulatedHumanoidRole.FieldWorker:
                    return WorkerModelResourcePath;
                case ArticulatedHumanoidRole.Monster:
                    return MonsterModelResourcePath;
                default:
                    return PlayerModelResourcePath;
            }
        }

        private Quaternion GetImportedModelBaseRotation()
        {
            return Quaternion.identity;
        }

        private void AutoOrientImportedModel(Renderer[] renderers)
        {
            if (importedModelVisual == null || renderers == null || renderers.Length == 0)
            {
                return;
            }

            var candidates = new[]
            {
                Quaternion.identity,
                Quaternion.Euler(0f, 180f, 0f),
                Quaternion.Euler(-90f, 180f, 0f),
                Quaternion.Euler(-90f, 0f, 0f),
                Quaternion.Euler(90f, 180f, 0f),
                Quaternion.Euler(90f, 0f, 0f),
                Quaternion.Euler(0f, 180f, 90f),
                Quaternion.Euler(0f, 180f, -90f),
                Quaternion.Euler(0f, 0f, 90f),
                Quaternion.Euler(0f, 0f, -90f),
            };

            var bestRotation = candidates[0];
            var bestScore = float.MinValue;
            for (var i = 0; i < candidates.Length; i++)
            {
                importedModelVisual.localRotation = candidates[i];
                var bounds = CalculateRendererBounds(renderers);
                var score = bounds.size.y - Mathf.Max(bounds.size.x, bounds.size.z) * 0.35f;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestRotation = candidates[i];
                }
            }

            importedModelVisual.localRotation = bestRotation;
        }

        private static string GetRoleTextureFolderPath(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return string.Empty;
            }

            var slashIndex = resourcePath.LastIndexOf('/');
            return slashIndex > 0 ? resourcePath.Substring(0, slashIndex) : string.Empty;
        }

        private static string GetRoleControllerResourcePath(string resourcePath)
        {
            return string.IsNullOrWhiteSpace(resourcePath)
                ? string.Empty
                : resourcePath + "_Auto";
        }

        private void FitImportedModelToRoleHeight(Renderer[] renderers)
        {
            if (importedModelVisual == null || renderers == null || renderers.Length == 0)
            {
                return;
            }

            var bounds = CalculateRendererBounds(renderers);
            if (bounds.size.y <= 0.001f)
            {
                return;
            }

            var targetHeight = role == ArticulatedHumanoidRole.Monster ? 1.95f : 1.68f;
            var scale = targetHeight / bounds.size.y;
            importedModelVisual.localScale = Vector3.one * scale;

            var scaledBounds = CalculateRendererBounds(renderers);
            var bottomOffset = scaledBounds.min.y - visualRoot.position.y;
            importedModelVisual.position -= new Vector3(0f, bottomOffset, 0f);
        }

        private void ApplyImportedRolePalette(Renderer[] renderers, HumanoidStyle style, string textureFolderPath)
        {
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            var textures = string.IsNullOrWhiteSpace(textureFolderPath)
                ? System.Array.Empty<Texture2D>()
                : Resources.LoadAll<Texture2D>(textureFolderPath);

            Texture2D albedo = null;
            Texture2D normal = null;
            Texture2D metallic = null;
            Texture2D roughness = null;

            for (var index = 0; index < textures.Length; index++)
            {
                var texture = textures[index];
                if (texture == null)
                {
                    continue;
                }

                var key = texture.name.ToLowerInvariant();
                if (key.Contains("normal"))
                {
                    normal = texture;
                }
                else if (key.Contains("metal"))
                {
                    metallic = texture;
                }
                else if (key.Contains("rough"))
                {
                    roughness = texture;
                }
                else if (albedo == null)
                {
                    albedo = texture;
                }
            }

            var litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null)
            {
                litShader = Shader.Find("Standard");
            }

            for (var rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];
                if (renderer == null)
                {
                    continue;
                }

                var sourceMaterials = renderer.sharedMaterials;
                if (sourceMaterials == null || sourceMaterials.Length == 0)
                {
                    sourceMaterials = new Material[1];
                }

                var cloned = new Material[sourceMaterials.Length];
                for (var materialIndex = 0; materialIndex < sourceMaterials.Length; materialIndex++)
                {
                    var source = sourceMaterials[materialIndex];
                    var material = new Material(litShader);
                    material.name = source != null ? $"{source.name}_Runtime" : $"RuntimeMat_{rendererIndex}_{materialIndex}";

                    if (albedo != null)
                    {
                        if (material.HasProperty("_BaseMap"))
                        {
                            material.SetTexture("_BaseMap", albedo);
                        }
                        if (material.HasProperty("_MainTex"))
                        {
                            material.SetTexture("_MainTex", albedo);
                        }
                        if (material.HasProperty("_BaseColor"))
                        {
                            material.SetColor("_BaseColor", Color.white);
                        }
                        if (material.HasProperty("_Color"))
                        {
                            material.color = Color.white;
                        }
                    }
                    else
                    {
                        var fallback = ResolveImportedMaterialColor(source != null ? source.name : string.Empty, materialIndex, style);
                        if (material.HasProperty("_BaseColor"))
                        {
                            material.SetColor("_BaseColor", fallback);
                        }
                        if (material.HasProperty("_Color"))
                        {
                            material.color = fallback;
                        }
                    }

                    if (normal != null)
                    {
                        if (material.HasProperty("_BumpMap"))
                        {
                            material.SetTexture("_BumpMap", normal);
                            material.EnableKeyword("_NORMALMAP");
                        }
                        if (material.HasProperty("_NormalMap"))
                        {
                            material.SetTexture("_NormalMap", normal);
                            material.EnableKeyword("_NORMALMAP");
                        }
                    }

                    if (metallic != null && material.HasProperty("_MetallicGlossMap"))
                    {
                        material.SetTexture("_MetallicGlossMap", metallic);
                        material.SetFloat("_Metallic", 0.25f);
                        material.EnableKeyword("_METALLICSPECGLOSSMAP");
                    }
                    else if (material.HasProperty("_Metallic"))
                    {
                        material.SetFloat("_Metallic", 0f);
                    }

                    if (material.HasProperty("_Smoothness"))
                    {
                        material.SetFloat("_Smoothness", roughness != null ? 0.18f : 0.12f);
                    }
                    if (material.HasProperty("_Surface"))
                    {
                        material.SetFloat("_Surface", 0f);
                    }
                    if (material.HasProperty("_AlphaClip"))
                    {
                        material.SetFloat("_AlphaClip", 0f);
                    }

                    cloned[materialIndex] = material;
                }

                renderer.sharedMaterials = cloned;
            }
        }

        private Color ResolveImportedMaterialColor(string materialName, int materialIndex, HumanoidStyle style)
        {
            var key = materialName == null ? string.Empty : materialName.ToLowerInvariant();
            var shoeColor = new Color(0.12f, 0.12f, 0.13f);
            var paperColor = new Color(0.73f, 0.67f, 0.49f);
            var metalColor = new Color(0.18f, 0.19f, 0.21f);

            if (key.Contains("skin") || key.Contains("face") || key.Contains("head") || key.Contains("hand"))
            {
                return style.SkinColor;
            }
            if (key.Contains("hair"))
            {
                return style.HairColor;
            }
            if (key.Contains("eye") || key.Contains("lash") || key.Contains("brow"))
            {
                return new Color(0.09f, 0.09f, 0.1f);
            }
            if (key.Contains("glass"))
            {
                return new Color(0.1f, 0.1f, 0.12f);
            }
            if (key.Contains("metal") || key.Contains("headset") || key.Contains("tool") || key.Contains("mic"))
            {
                return metalColor;
            }
            if (key.Contains("paper") || key.Contains("note") || key.Contains("book") || key.Contains("pad") || key.Contains("journal"))
            {
                return paperColor;
            }
            if (key.Contains("bag") || key.Contains("strap") || key.Contains("pouch") || key.Contains("belt") || key.Contains("satchel"))
            {
                return style.GearColor;
            }
            if (key.Contains("shoe") || key.Contains("boot") || key.Contains("sole"))
            {
                return shoeColor;
            }
            if (key.Contains("skirt"))
            {
                return role == ArticulatedHumanoidRole.Archivist ? new Color(0.14f, 0.14f, 0.15f) : style.TrouserColor;
            }
            if (key.Contains("tight") || key.Contains("legging") || key.Contains("sock") || key.Contains("stocking"))
            {
                return new Color(0.10f, 0.10f, 0.11f);
            }
            if (key.Contains("hood") || key.Contains("hoodie"))
            {
                return role == ArticulatedHumanoidRole.Player ? new Color(0.08f, 0.09f, 0.10f) : style.OuterwearColor;
            }
            if (key.Contains("coat") || key.Contains("jacket"))
            {
                return style.OuterwearColor;
            }
            if (key.Contains("vest") || key.Contains("apron"))
            {
                return role == ArticulatedHumanoidRole.FieldWorker ? style.OuterwearColor : style.BodyColor;
            }
            if (key.Contains("hat") || key.Contains("cap"))
            {
                return style.OuterwearColor;
            }
            if (key.Contains("shirt") || key.Contains("inner") || key.Contains("sweater"))
            {
                return style.BodyColor;
            }
            if (key.Contains("top") || key.Contains("cloth"))
            {
                return role == ArticulatedHumanoidRole.Archivist ? style.OuterwearColor : style.BodyColor;
            }
            if (key.Contains("pant") || key.Contains("trouser") || key.Contains("leg"))
            {
                return style.TrouserColor;
            }

            switch (materialIndex % 6)
            {
                case 0: return style.SkinColor;
                case 1: return style.HairColor;
                case 2: return style.OuterwearColor;
                case 3: return style.BodyColor;
                case 4: return style.TrouserColor;
                default: return style.GearColor;
            }
        }

        private void AddImportedRoleAccessories(HumanoidStyle style)
        {
            return;
        }

        private void ConfigureImportedAnimation(string resourcePath)
        {
            if (importedModelRoot == null)
            {
                return;
            }

            importedAnimator = importedModelVisual.GetComponentInChildren<Animator>();
            if (importedAnimator == null)
            {
                importedAnimator = importedModelVisual.gameObject.AddComponent<Animator>();
            }

            importedAnimator.applyRootMotion = false;
            importedAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            importedAnimator.updateMode = AnimatorUpdateMode.Normal;

            importedIdleClip = null;
            importedWalkClip = null;
            importedTalkClip = null;
            importedPlaceClip = null;
            importedAttackClip = null;
            importedHitClip = null;
            importedDeathClip = null;
            importedCurrentActionClip = null;
            importedUsesRuntimeController = false;
            importedCurrentStateName = string.Empty;
            importedAvatar = importedAnimator.avatar;
            var controller = Resources.Load<RuntimeAnimatorController>(GetRoleControllerResourcePath(resourcePath));
            if (controller != null)
            {
                importedAnimator.runtimeAnimatorController = controller;
                importedUsesRuntimeController = true;
                importedAvatar = importedAnimator.avatar;
                importedAnimator.Rebind();
                importedAnimator.Update(0f);
                var controllerClips = controller.animationClips;
                for (var clipIndex = 0; clipIndex < controllerClips.Length; clipIndex++)
                {
                    var controllerClip = controllerClips[clipIndex];
                    if (controllerClip != null && !controllerClip.name.StartsWith("__preview__", System.StringComparison.OrdinalIgnoreCase))
                    {
                        RegisterImportedClip(controllerClip);
                    }
                }
            }

            var folderPath = GetRoleTextureFolderPath(resourcePath);
            if (!importedUsesRuntimeController)
            {
                var assets = Resources.LoadAll<Object>(resourcePath);
                for (var index = 0; index < assets.Length; index++)
                {
                    if (assets[index] is AnimationClip clip && !clip.name.StartsWith("__preview__", System.StringComparison.OrdinalIgnoreCase))
                    {
                        RegisterImportedClip(clip);
                    }
                    else if (assets[index] is Avatar avatar)
                    {
                        importedAvatar = avatar;
                    }
                }

                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    var folderAssets = Resources.LoadAll<Object>(folderPath);
                    for (var index = 0; index < folderAssets.Length; index++)
                    {
                        if (folderAssets[index] is AnimationClip clip && !clip.name.StartsWith("__preview__", System.StringComparison.OrdinalIgnoreCase))
                        {
                            RegisterImportedClip(clip);
                        }
                    }
                }
            }

            if (importedAvatar != null)
            {
                importedAnimator.avatar = importedAvatar;
            }

            CacheImportedHumanoidBones();

            if (!importedUsesRuntimeController)
            {
                SetupImportedAnimationGraph();
            }
        }

        private void CacheImportedHumanoidBones()
        {
            importedHipsBone = null;
            importedSpineBone = null;
            importedChestBone = null;
            importedHeadBone = null;
            importedRightUpperArmBone = null;
            importedRightLowerArmBone = null;
            importedLeftUpperArmBone = null;
            importedLeftLowerArmBone = null;
            importedRightUpperLegBone = null;
            importedRightLowerLegBone = null;
            importedLeftUpperLegBone = null;
            importedLeftLowerLegBone = null;

            if (importedAnimator == null)
            {
                return;
            }

            if (importedAvatar != null)
            {
                importedHipsBone = importedAnimator.GetBoneTransform(HumanBodyBones.Hips);
                importedSpineBone = importedAnimator.GetBoneTransform(HumanBodyBones.Spine);
                importedChestBone = importedAnimator.GetBoneTransform(HumanBodyBones.Chest);
                importedHeadBone = importedAnimator.GetBoneTransform(HumanBodyBones.Head);
                importedRightUpperArmBone = importedAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                importedRightLowerArmBone = importedAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                importedLeftUpperArmBone = importedAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                importedLeftLowerArmBone = importedAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                importedRightUpperLegBone = importedAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                importedRightLowerLegBone = importedAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                importedLeftUpperLegBone = importedAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                importedLeftLowerLegBone = importedAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            }

            importedHipsBone ??= FindImportedBone("hips", "pelvis");
            importedSpineBone ??= FindImportedBone("spine", "spine01");
            importedChestBone ??= FindImportedBone("chest", "upperchest", "spine02");
            importedHeadBone ??= FindImportedBone("head", "neck");
            importedRightUpperArmBone ??= FindImportedBone("rightupperarm", "rightarm", "upperarmr", "armr", "rupperarm", "rarm");
            importedRightLowerArmBone ??= FindImportedBone("rightlowerarm", "rightforearm", "lowerarmr", "forearmr", "rforearm");
            importedLeftUpperArmBone ??= FindImportedBone("leftupperarm", "leftarm", "upperarml", "arml", "lupperarm", "larm");
            importedLeftLowerArmBone ??= FindImportedBone("leftlowerarm", "leftforearm", "lowerarml", "forearml", "lforearm");
            importedRightUpperLegBone ??= FindImportedBone("rightupperleg", "rightupleg", "rightleg", "thighr", "rthigh");
            importedRightLowerLegBone ??= FindImportedBone("rightlowerleg", "rightcalf", "calfr", "rcalf", "rightfoot");
            importedLeftUpperLegBone ??= FindImportedBone("leftupperleg", "leftupleg", "leftleg", "thighl", "lthigh");
            importedLeftLowerLegBone ??= FindImportedBone("leftlowerleg", "leftcalf", "calfl", "lcalf", "leftfoot");
        }

        private Transform FindImportedBone(params string[] keys)
        {
            if (importedModelVisual == null || keys == null || keys.Length == 0)
            {
                return null;
            }

            var transforms = importedModelVisual.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var candidate = transforms[i];
                if (candidate == null)
                {
                    continue;
                }

                var normalizedName = NormalizeBoneName(candidate.name);
                for (var k = 0; k < keys.Length; k++)
                {
                    var key = keys[k];
                    if (!string.IsNullOrWhiteSpace(key) && normalizedName == NormalizeBoneName(key))
                    {
                        return candidate;
                    }
                }
            }

            for (var i = 0; i < transforms.Length; i++)
            {
                var candidate = transforms[i];
                if (candidate == null)
                {
                    continue;
                }

                var name = NormalizeBoneName(candidate.name);
                for (var k = 0; k < keys.Length; k++)
                {
                    var key = keys[k];
                    if (!string.IsNullOrWhiteSpace(key) && name.Contains(NormalizeBoneName(key)))
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }

        private static string NormalizeBoneName(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.ToLowerInvariant().Replace(" ", string.Empty).Replace("_", string.Empty);
        }

        private void SetupImportedAnimationGraph()
        {
            DestroyImportedAnimationGraph();

            if (!UseImportedClipPlayback)
            {
                return;
            }

            if (importedAnimator == null || (importedWalkClip == null && importedIdleClip == null))
            {
                return;
            }

            importedGraph = PlayableGraph.Create($"{name}_ImportedWalkGraph");
            var output = AnimationPlayableOutput.Create(importedGraph, "Animation", importedAnimator);
            importedWalkPlayable = AnimationClipPlayable.Create(importedGraph, importedWalkClip ?? importedIdleClip);
            importedWalkPlayable.SetApplyFootIK(false);
            importedWalkPlayable.SetApplyPlayableIK(false);
            importedWalkPlayable.SetSpeed(0d);
            importedIdlePlayable = AnimationClipPlayable.Create(importedGraph, importedIdleClip ?? importedWalkClip);
            importedIdlePlayable.SetApplyFootIK(false);
            importedIdlePlayable.SetApplyPlayableIK(false);
            importedIdlePlayable.SetSpeed(importedIdleClip != null ? 1d : 0d);
            importedCurrentActionClip = importedPlaceClip ?? importedTalkClip ?? importedAttackClip ?? importedHitClip ?? importedIdleClip ?? importedWalkClip;
            importedActionPlayable = AnimationClipPlayable.Create(importedGraph, importedCurrentActionClip ?? importedIdleClip ?? importedWalkClip);
            importedActionPlayable.SetApplyFootIK(false);
            importedActionPlayable.SetApplyPlayableIK(false);
            importedActionPlayable.SetSpeed(0d);

            var mixer = AnimationMixerPlayable.Create(importedGraph, 3);
            importedGraph.Connect(importedWalkPlayable, 0, mixer, 0);
            importedGraph.Connect(importedIdlePlayable, 0, mixer, 1);
            importedGraph.Connect(importedActionPlayable, 0, mixer, 2);
            mixer.SetInputWeight(0, 1f);
            mixer.SetInputWeight(1, 0f);
            mixer.SetInputWeight(2, 0f);
            output.SetSourcePlayable(mixer);
            importedGraph.Play();
            importedGraphValid = true;
        }

        private void RegisterImportedClip(AnimationClip clip)
        {
            if (clip == null)
            {
                return;
            }

            var key = clip.name.ToLowerInvariant();
            if (key.Contains("idle"))
            {
                importedIdleClip = clip;
                return;
            }

            if (key.Contains("walk") || key.Contains("run") || key.Contains("move"))
            {
                importedWalkClip = clip;
                return;
            }

            if (key.Contains("talk"))
            {
                importedTalkClip = clip;
                return;
            }

            if (key.Contains("place") || key.Contains("cast") || key.Contains("interact") || key.Contains("use"))
            {
                importedPlaceClip = clip;
                return;
            }

            if (key.Contains("attack") || key.Contains("slash") || key.Contains("hit_1") || key.Contains("melee") || key.Contains("pound") || key.Contains("bite") || key.Contains("strike"))
            {
                importedAttackClip = clip;
                return;
            }

            if (key.Contains("damage") || key.Contains("hit"))
            {
                importedHitClip = clip;
                return;
            }

            if (key.Contains("death") || key.Contains("die") || key.Contains("knockout"))
            {
                importedDeathClip = clip;
                return;
            }

            if (importedWalkClip == null)
            {
                importedWalkClip = clip;
                return;
            }

            if (importedIdleClip == null)
            {
                importedIdleClip = clip;
                return;
            }

            if (importedTalkClip == null)
            {
                importedTalkClip = clip;
                return;
            }

            if (importedPlaceClip == null)
            {
                importedPlaceClip = clip;
                return;
            }

            if (importedAttackClip == null)
            {
                importedAttackClip = clip;
                return;
            }

            if (importedHitClip == null)
            {
                importedHitClip = clip;
            }
        }

        private void DestroyImportedAnimationGraph()
        {
            if (!importedGraphValid)
            {
                return;
            }

            importedGraph.Destroy();
            importedGraphValid = false;
            importedCurrentActionClip = null;
        }

        private static Bounds CalculateRendererBounds(Renderer[] renderers)
        {
            var bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++)
            {
                if (renderers[index] != null)
                {
                    bounds.Encapsulate(renderers[index].bounds);
                }
            }

            return bounds;
        }

        private static void RemoveImportedColliders(Transform root)
        {
            var colliders = root.GetComponentsInChildren<Collider>(true);
            for (var index = 0; index < colliders.Length; index++)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(colliders[index]);
                }
                else
                {
                    Object.DestroyImmediate(colliders[index]);
                }
            }
        }

        private void CacheBones()
        {
            hips = visualRoot.Find("Hips");
            torso = hips != null ? hips.Find("Torso") : null;
            head = torso != null ? torso.Find("Head") : null;
            upperArmLeft = torso != null ? torso.Find("UpperArmLeft") : null;
            lowerArmLeft = upperArmLeft != null ? upperArmLeft.Find("LowerArmLeft") : null;
            upperArmRight = torso != null ? torso.Find("UpperArmRight") : null;
            lowerArmRight = upperArmRight != null ? upperArmRight.Find("LowerArmRight") : null;
            upperLegLeft = hips != null ? hips.Find("UpperLegLeft") : null;
            lowerLegLeft = upperLegLeft != null ? upperLegLeft.Find("LowerLegLeft") : null;
            upperLegRight = hips != null ? hips.Find("UpperLegRight") : null;
            lowerLegRight = upperLegRight != null ? upperLegRight.Find("LowerLegRight") : null;
        }

        private void AnimateBody()
        {
            if (importedModelRoot != null)
            {
                AnimateImportedModel();
                return;
            }

            if (hips == null || torso == null || head == null)
            {
                return;
            }

            var walk = moveBlend;
            var swing = Mathf.Sin(moveCycle * Mathf.PI * 2f);
            var counterSwing = Mathf.Sin(moveCycle * Mathf.PI * 2f + Mathf.PI);
            var idleBreath = Mathf.Sin(idleClock * 1.7f) * 0.8f;
            var roleIdleOffset = role == ArticulatedHumanoidRole.FieldWorker ? Mathf.Sin(idleClock * 1.15f + 0.8f) * 1.8f : 0f;
            var bob = Mathf.Abs(swing) * walk * 0.06f;
            var torsoPitch = -walk * 6f + idleBreath + roleIdleOffset * 0.25f;
            var hipsYaw = role == ArticulatedHumanoidRole.FieldWorker ? Mathf.Sin(idleClock * 0.8f) * 3f : role == ArticulatedHumanoidRole.Archivist ? Mathf.Sin(idleClock * 0.42f + 0.7f) * 1.1f : 0f;
            var torsoRoll = role == ArticulatedHumanoidRole.FieldWorker ? Mathf.Sin(idleClock * 0.65f + 0.3f) * 2.5f : role == ArticulatedHumanoidRole.Archivist ? -1.6f + Mathf.Sin(idleClock * 0.55f) * 0.8f : 0f;
            var idleSway = Mathf.Sin(idleClock * 1.1f + (role == ArticulatedHumanoidRole.Player ? 0.4f : 1.1f)) * (1f - walk) * 2.4f;
            var baseHeadPitch = role == ArticulatedHumanoidRole.Archivist ? -2.5f : role == ArticulatedHumanoidRole.FieldWorker ? 1.25f : 0f;

            var actionBlend = actionDuration > 0f ? Mathf.Sin(Mathf.Clamp01(actionClock / actionDuration) * Mathf.PI) : 0f;

            if (currentAction == ArticulatedHumanoidAction.Inspect)
            {
                torsoPitch -= actionBlend * 14f;
            }
            else if (currentAction == ArticulatedHumanoidAction.Talk)
            {
                torsoPitch += actionBlend * 4f;
                hipsYaw += Mathf.Sin(idleClock * 6f) * 2f * actionBlend;
            }
            else if (currentAction == ArticulatedHumanoidAction.Place)
            {
                torsoPitch -= actionBlend * 18f;
            }

            hips.localPosition = new Vector3(0f, 0.8f + bob, 0f);
            hips.localRotation = Quaternion.Euler(0f, hipsYaw + idleSway * 0.55f, 0f);
            torso.localRotation = Quaternion.Euler(torsoPitch, 0f, torsoRoll + idleSway * 0.35f);

            var headYaw = 0f;
            if (attentionWeight > 0.01f)
            {
                var localAttention = transform.InverseTransformDirection(attentionDirection);
                headYaw = Mathf.Atan2(localAttention.x, localAttention.z) * Mathf.Rad2Deg * 0.45f * attentionWeight;
            }
            head.localRotation = Quaternion.Euler(baseHeadPitch + idleBreath * 0.5f, headYaw, 0f);

            var armSwing = 24f * walk;
            var legSwing = 26f * walk;
            var armBend = 10f + walk * 8f;
            var kneeBend = 16f + walk * 10f;
            var idleArmSettle = 6f + Mathf.Sin(idleClock * 1.2f) * 2f;
            var idleLegSettle = 2.5f + Mathf.Sin(idleClock * 0.9f + 0.6f) * 1.5f;

            if (currentAction == ArticulatedHumanoidAction.Inspect)
            {
                upperArmRight.localRotation = Quaternion.Euler(-26f * actionBlend + counterSwing * armSwing * 0.35f, 0f, -10f);
                lowerArmRight.localRotation = Quaternion.Euler(-34f * actionBlend, 0f, 0f);
            }
            else if (currentAction == ArticulatedHumanoidAction.Talk)
            {
                upperArmRight.localRotation = Quaternion.Euler(-18f * actionBlend + counterSwing * armSwing * 0.4f, 0f, -8f);
                lowerArmRight.localRotation = Quaternion.Euler(-20f * actionBlend, 0f, 0f);
            }
            else if (currentAction == ArticulatedHumanoidAction.Place)
            {
                upperArmRight.localRotation = Quaternion.Euler(-34f * actionBlend + counterSwing * armSwing * 0.25f, 0f, -12f);
                lowerArmRight.localRotation = Quaternion.Euler(-48f * actionBlend, 0f, 0f);
            }
            else
            {
                upperArmRight.localRotation = Quaternion.Euler(counterSwing * armSwing - idleArmSettle * (1f - walk), 0f, -6f);
                lowerArmRight.localRotation = Quaternion.Euler(-armBend * walk - 8f * (1f - walk), 0f, 0f);
            }

            var leftIdleRoll = role == ArticulatedHumanoidRole.Archivist ? 2f : 6f;
            upperArmLeft.localRotation = Quaternion.Euler(swing * armSwing - idleArmSettle * 0.55f * (1f - walk), 0f, leftIdleRoll);
            lowerArmLeft.localRotation = Quaternion.Euler(-armBend * walk * 0.8f - (role == ArticulatedHumanoidRole.Archivist ? 12f : 6f) * (1f - walk), 0f, 0f);

            upperLegRight.localRotation = Quaternion.Euler(swing * legSwing + idleLegSettle * (1f - walk), 0f, 0f);
            lowerLegRight.localRotation = Quaternion.Euler(kneeBend * Mathf.Max(0f, -swing) * walk, 0f, 0f);
            upperLegLeft.localRotation = Quaternion.Euler(counterSwing * legSwing - idleLegSettle * (1f - walk), 0f, 0f);
            lowerLegLeft.localRotation = Quaternion.Euler(kneeBend * Mathf.Max(0f, -counterSwing) * walk, 0f, 0f);
        }

        private void AnimateImportedModel()
        {
            if (importedModelRoot == null || importedModelVisual == null)
            {
                return;
            }

            var walk = moveBlend;
            var swing = Mathf.Sin(moveCycle * Mathf.PI * 2f);
            var bob = Mathf.Abs(swing) * walk * 0.06f;
            var idle = Mathf.Sin(idleClock * 1.35f) * 0.9f;
            var bodyPitch = idle * 0.2f;
            var bodyRoll = Mathf.Sin(idleClock * 0.95f + (role == ArticulatedHumanoidRole.FieldWorker ? 0.7f : 0f)) * (1f - walk) * 1.35f;
            var yaw = 0f;

            if (attentionWeight > 0.01f)
            {
                var localAttention = transform.InverseTransformDirection(attentionDirection);
                yaw = Mathf.Atan2(localAttention.x, localAttention.z) * Mathf.Rad2Deg * 0.5f * attentionWeight;
            }

            if (currentAction == ArticulatedHumanoidAction.Inspect)
            {
                bodyPitch -= 4f;
            }
            else if (currentAction == ArticulatedHumanoidAction.Place)
            {
                bodyPitch -= 6f;
            }
            else if (currentAction == ArticulatedHumanoidAction.Talk)
            {
                bodyPitch += 2f;
            }
            else if (currentAction == ArticulatedHumanoidAction.Attack)
            {
                bodyPitch -= 10f;
                bodyRoll += Mathf.Sin(actionClock * 18f) * 3.8f;
            }
            else if (currentAction == ArticulatedHumanoidAction.Hit)
            {
                bodyPitch += 5f;
                bodyRoll -= 5f;
            }

            importedModelRoot.localPosition = new Vector3(0f, bob, 0f);
            importedModelRoot.localRotation = Quaternion.Euler(0f, yaw, bodyRoll);
            importedModelVisual.localRotation = GetImportedModelBaseRotation() * Quaternion.Euler(bodyPitch, 0f, 0f);
            ApplyImportedBoneFallback(walk, swing);
            UpdateImportedAnimationPlayback(walk);
        }

        private void ApplyImportedBoneFallback(float walk, float swing)
        {
            if ((UseImportedClipPlayback && importedGraphValid) || importedUsesRuntimeController)
            {
                return;
            }

            var armSwing = 18f * walk;
            var legSwing = 22f * walk;
            var kneeSwing = 24f * walk;
            var idleArm = Mathf.Sin(idleClock * 1.6f) * 3f;
            var idleHead = Mathf.Sin(idleClock * 1.2f) * 1.2f;
            var spinePitch = -4f * walk + Mathf.Sin(idleClock * 1.4f) * 1.6f;
            var chestPitch = currentAction == ArticulatedHumanoidAction.Attack ? -8f : currentAction == ArticulatedHumanoidAction.Place ? -5f : currentAction == ArticulatedHumanoidAction.Talk ? 3f : 0f;
            var rootSide = Mathf.Sin(moveCycle * Mathf.PI * 2f) * walk * 0.018f;
            var rootRoll = Mathf.Sin(moveCycle * Mathf.PI * 2f) * walk * 1.4f;
            var rootPitch = currentAction == ArticulatedHumanoidAction.Attack ? -5f : currentAction == ArticulatedHumanoidAction.Place ? -3f : currentAction == ArticulatedHumanoidAction.Talk ? 2f : 0f;

            if (importedModelRoot != null)
            {
                importedModelRoot.localPosition = new Vector3(rootSide, importedModelRoot.localPosition.y, 0f);
                importedModelRoot.localRotation = Quaternion.Euler(0f, 0f, rootRoll);
            }

            if (importedModelVisual != null)
            {
                importedModelVisual.localRotation = GetImportedModelBaseRotation() * Quaternion.Euler(rootPitch, 0f, rootRoll * 0.35f);
            }

            if (importedSpineBone != null)
            {
                importedSpineBone.localRotation = Quaternion.Euler(spinePitch, 0f, 0f);
            }

            if (importedChestBone != null)
            {
                importedChestBone.localRotation = Quaternion.Euler(chestPitch, 0f, 0f);
            }

            if (importedHeadBone != null)
            {
                importedHeadBone.localRotation = Quaternion.Euler(idleHead, 0f, 0f);
            }

            if (importedRightUpperArmBone != null)
            {
                var attackLift = currentAction == ArticulatedHumanoidAction.Attack ? -42f : currentAction == ArticulatedHumanoidAction.Place ? -24f : currentAction == ArticulatedHumanoidAction.Talk ? -16f : 0f;
                importedRightUpperArmBone.localRotation = Quaternion.Euler(swing * armSwing + attackLift + idleArm, 0f, -4f);
            }

            if (importedLeftUpperArmBone != null)
            {
                importedLeftUpperArmBone.localRotation = Quaternion.Euler(-swing * armSwing - idleArm * 0.7f, 0f, 4f);
            }

            if (importedRightLowerArmBone != null)
            {
                var fore = currentAction == ArticulatedHumanoidAction.Attack ? -34f : currentAction == ArticulatedHumanoidAction.Place ? -24f : currentAction == ArticulatedHumanoidAction.Talk ? -16f : -8f * (1f - walk);
                importedRightLowerArmBone.localRotation = Quaternion.Euler(fore, 0f, 0f);
            }

            if (importedLeftLowerArmBone != null)
            {
                importedLeftLowerArmBone.localRotation = Quaternion.Euler(-8f * (1f - walk), 0f, 0f);
            }

            if (importedRightUpperLegBone != null)
            {
                importedRightUpperLegBone.localRotation = Quaternion.Euler(swing * legSwing, 0f, 0f);
            }

            if (importedLeftUpperLegBone != null)
            {
                importedLeftUpperLegBone.localRotation = Quaternion.Euler(-swing * legSwing, 0f, 0f);
            }

            if (importedRightLowerLegBone != null)
            {
                importedRightLowerLegBone.localRotation = Quaternion.Euler(Mathf.Max(0f, -swing) * kneeSwing, 0f, 0f);
            }

            if (importedLeftLowerLegBone != null)
            {
                importedLeftLowerLegBone.localRotation = Quaternion.Euler(Mathf.Max(0f, swing) * kneeSwing, 0f, 0f);
            }
        }

        private void UpdateImportedAnimationPlayback(float walk)
        {
            if (importedUsesRuntimeController && importedAnimator != null)
            {
                var controllerActionClip = SelectImportedActionClip();
                var targetClipName = controllerActionClip != null
                    ? controllerActionClip.name
                    : walk < 0.05f
                        ? (importedIdleClip ?? importedWalkClip)?.name
                        : (importedWalkClip ?? importedIdleClip)?.name;
                var targetState = SanitizeAnimatorStateName(targetClipName);

                if (!string.IsNullOrWhiteSpace(targetState) && importedCurrentStateName != targetState)
                {
                    importedAnimator.CrossFadeInFixedTime(targetState, 0.08f);
                    importedCurrentStateName = targetState;
                }

                return;
            }

            if (!importedGraphValid)
            {
                return;
            }

            var mixer = importedGraph.GetRootPlayable(0);
            if (!mixer.IsValid())
            {
                return;
            }

            var actionClip = SelectImportedActionClip();
            RefreshImportedActionClip(actionClip);
            if (actionClip != null)
            {
                importedWalkPlayable.SetSpeed(0d);
                if (importedIdlePlayable.IsValid())
                {
                    importedIdlePlayable.SetSpeed(0d);
                }
                if (importedActionPlayable.IsValid())
                {
                    importedActionPlayable.SetSpeed(1d);
                }
                mixer.SetInputWeight(0, 0f);
                mixer.SetInputWeight(1, 0f);
                mixer.SetInputWeight(2, 1f);
                return;
            }

            if (walk < 0.05f)
            {
                importedWalkPlayable.SetSpeed(0d);
                if (importedIdlePlayable.IsValid())
                {
                    importedIdlePlayable.SetSpeed(importedIdleClip != null ? 1d : 0d);
                }
                if (importedActionPlayable.IsValid())
                {
                    importedActionPlayable.SetSpeed(0d);
                }
                mixer.SetInputWeight(0, 0f);
                mixer.SetInputWeight(1, 1f);
                mixer.SetInputWeight(2, 0f);
                return;
            }

            importedWalkPlayable.SetSpeed(Mathf.Lerp(0.78f, 1.2f, walk));
            if (importedIdlePlayable.IsValid())
            {
                importedIdlePlayable.SetSpeed(0d);
            }
            if (importedActionPlayable.IsValid())
            {
                importedActionPlayable.SetSpeed(0d);
            }
            mixer.SetInputWeight(0, 1f);
            mixer.SetInputWeight(1, 0f);
            mixer.SetInputWeight(2, 0f);
        }

        private void RefreshImportedActionClip(AnimationClip actionClip)
        {
            if (!importedGraphValid || !importedActionPlayable.IsValid())
            {
                return;
            }

            var nextClip = actionClip ?? importedIdleClip ?? importedWalkClip;
            if (nextClip == null || nextClip == importedCurrentActionClip)
            {
                return;
            }

            var mixer = importedGraph.GetRootPlayable(0);
            if (!mixer.IsValid())
            {
                return;
            }

            importedGraph.Disconnect(mixer, 2);
            if (importedActionPlayable.IsValid())
            {
                importedActionPlayable.Destroy();
            }

            importedCurrentActionClip = nextClip;
            importedActionPlayable = AnimationClipPlayable.Create(importedGraph, importedCurrentActionClip);
            importedActionPlayable.SetApplyFootIK(false);
            importedActionPlayable.SetApplyPlayableIK(false);
            importedActionPlayable.SetSpeed(0d);
            importedGraph.Connect(importedActionPlayable, 0, mixer, 2);
            mixer.SetInputWeight(2, 0f);
        }

        private AnimationClip SelectImportedActionClip()
        {
            switch (currentAction)
            {
                case ArticulatedHumanoidAction.Talk:
                    return importedTalkClip;
                case ArticulatedHumanoidAction.Place:
                    return importedPlaceClip;
                case ArticulatedHumanoidAction.Attack:
                    return importedAttackClip ?? importedPlaceClip ?? importedTalkClip;
                case ArticulatedHumanoidAction.Hit:
                    return importedHitClip ?? importedAttackClip;
                case ArticulatedHumanoidAction.Inspect:
                    return importedPlaceClip ?? importedTalkClip;
                default:
                    return null;
            }
        }

        private static string SanitizeAnimatorStateName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                return string.Empty;
            }

            var sanitized = Regex.Replace(rawName, @"[^A-Za-z0-9_\- ]", "_");
            sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();
            return sanitized;
        }

        private static Transform CreateJoint(Transform parent, string name, Vector3 localPosition)
        {
            var joint = new GameObject(name).transform;
            joint.SetParent(parent, false);
            joint.localPosition = localPosition;
            return joint;
        }

        private static void CreatePiece(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            var piece = GameObject.CreatePrimitive(primitiveType);
            piece.name = name;
            piece.transform.SetParent(parent, false);
            piece.transform.localPosition = localPosition;
            piece.transform.localScale = localScale;

            var renderer = piece.GetComponent<Renderer>();
            if (renderer != null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }
                var material = new Material(shader);
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", color);
                }
                if (material.HasProperty("_Color"))
                {
                    material.color = color;
                }
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
        }
    }
}
