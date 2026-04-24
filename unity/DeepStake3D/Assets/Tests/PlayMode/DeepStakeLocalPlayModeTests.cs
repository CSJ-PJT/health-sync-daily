using System.Collections;
using System.IO;
using DeepStake.Boot;
using DeepStake.CameraRig;
using DeepStake.Core;
using DeepStake.Interaction;
using DeepStake.Quests;
using DeepStake.Save;
using DeepStake.Settlement;
using DeepStake.UI;
using DeepStake.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace DeepStake.Tests.PlayMode
{
    public sealed class DeepStakeLocalPlayModeTests
    {
        private const string BootScene = "Boot";
        private const string MainMenuScene = "MainMenu";
        private const string WorldScene = "WorldPrototype3D";

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            DeepStakeDevLaunchOptions.ClearEditorOverrides();
            yield return LoadScene(BootScene);
            yield return CleanupPersistentState();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            DeepStakeDevLaunchOptions.ClearEditorOverrides();
            yield return CleanupPersistentState();
        }

        [UnityTest]
        public IEnumerator BootScene_CanLoad()
        {
            yield return LoadScene(BootScene);

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(BootScene));
            Assert.That(Object.FindFirstObjectByType<DeepStakeBootstrap>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator MainMenuScene_CanLoad()
        {
            yield return LoadScene(MainMenuScene);

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(MainMenuScene));
            Assert.That(Object.FindFirstObjectByType<MainMenuController>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<Canvas>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator WorldPrototype3D_CanLoad_WithCoreObjects()
        {
            DeepStakeDevLaunchOptions.SetEditorOverrides(false, true, "playmode-world-load");
            yield return LoadScene(WorldScene);
            yield return WaitFrames(3);

            var world = Object.FindFirstObjectByType<WorldPrototype3DController>();
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(WorldScene));
            Assert.That(world, Is.Not.Null);
            Assert.That(world.PlayerTransform, Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<QuarterViewCameraRig>(), Is.Not.Null);
            Assert.That(Camera.main, Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<GuidanceOverlayView>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<MobileControlsOverlay>(), Is.Not.Null);
            Assert.That(world.PrimaryInteractable, Is.Not.Null);
            Assert.That(world.TertiaryInteractable, Is.Not.Null);
            Assert.That(world.QuestNpc, Is.Not.Null);
            Assert.That(world.PrimaryPlacement, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator LocalDevAutoEntry_ReachesPlayableField()
        {
            DeepStakeDevLaunchOptions.SetEditorOverrides(true, true, "playmode-autorun");
            yield return LoadScene(BootScene);
            yield return WaitForScene(WorldScene, 120);

            var world = Object.FindFirstObjectByType<WorldPrototype3DController>();
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(WorldScene));
            Assert.That(world, Is.Not.Null);
            Assert.That(world.PlayerTransform, Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<DeepStakeGameState>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator LocalSaveService_CanCreateSaveAndReload()
        {
            var savePath = LocalSaveService.GetSavePath();
            var backupPath = savePath + ".playmode-backup";
            var hadExistingSave = File.Exists(savePath);

            if (hadExistingSave)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(backupPath) ?? Path.GetDirectoryName(savePath) ?? ".");
                File.Copy(savePath, backupPath, true);
            }

            try
            {
                var created = LocalSaveService.CreateDefault();
                created.LastStatus = "PlayMode test save";
                LocalSaveService.Save(created);

                var loaded = LocalSaveService.LoadOrCreate();
                Assert.That(File.Exists(savePath), Is.True);
                Assert.That(loaded, Is.Not.Null);
                Assert.That(loaded.CurrentZoneId, Is.EqualTo("recovery-field"));
                Assert.That(loaded.Player.MapId, Is.EqualTo("recovery-field"));
                Assert.That(loaded.LastStatus, Is.EqualTo("PlayMode test save"));
            }
            finally
            {
                if (hadExistingSave)
                {
                    File.Copy(backupPath, savePath, true);
                    File.Delete(backupPath);
                }
                else if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
            }

            yield return null;
        }

        private static IEnumerator LoadScene(string sceneName)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (operation != null && !operation.isDone)
            {
                yield return null;
            }

            yield return null;
        }

        private static IEnumerator WaitForScene(string expectedSceneName, int maxFrames)
        {
            for (var frame = 0; frame < maxFrames; frame++)
            {
                if (SceneManager.GetActiveScene().name == expectedSceneName)
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail("Timed out waiting for scene " + expectedSceneName + ". Active scene=" + SceneManager.GetActiveScene().name);
        }

        private static IEnumerator WaitFrames(int frameCount)
        {
            for (var index = 0; index < frameCount; index++)
            {
                yield return null;
            }
        }

        private static IEnumerator CleanupPersistentState()
        {
            var gameStates = Object.FindObjectsByType<DeepStakeGameState>(FindObjectsSortMode.None);
            for (var index = 0; index < gameStates.Length; index++)
            {
                Object.Destroy(gameStates[index].gameObject);
            }

            yield return null;
        }
    }
}
