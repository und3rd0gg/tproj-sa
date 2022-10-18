using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Global

namespace FlatKit
{
#if FLAT_KIT_DEV
[CreateAssetMenu(fileName = "Readme", menuName = "FlatKit/Internal/Readme", order = 0)]
#endif // FLAT_KIT_DEV

    [ExecuteAlways]
    public class FlatKitReadme : ScriptableObject
    {
        private const string UrpPackageID = "com.unity.render-pipelines.universal";

        private static readonly GUID StylizedShaderGuid =
            new GUID("bee44b4a58655ee4cbff107302a3e131");

        [NonSerialized] public readonly string FlatKitVersion = "2.9.8";

        [NonSerialized] public bool FlatKitInstalled;

        [NonSerialized] [CanBeNull] public string PackageManagerError;

        [NonSerialized] public string UnityVersion = Application.unityVersion;

        [NonSerialized] public bool? UrpInstalled;

        [NonSerialized] public string UrpVersionInstalled = "N/A";

        public void Refresh()
        {
            UrpInstalled = false;
            FlatKitInstalled = false;
            PackageManagerError = null;

            var packages = GetPackageList();
            foreach (var p in packages)
                if (p.name == UrpPackageID)
                {
                    UrpInstalled = true;
                    UrpVersionInstalled = p.version;
                }

            var path = AssetDatabase.GUIDToAssetPath(StylizedShaderGuid.ToString());
            var flatKitSourceAsset = AssetDatabase.LoadAssetAtPath<Shader>(path);
            FlatKitInstalled = flatKitSourceAsset != null;

            UnityVersion = Application.unityVersion;
        }

        private PackageCollection GetPackageList()
        {
            var listRequest = Client.List(true);

            while (listRequest.Status == StatusCode.InProgress) continue;

            if (listRequest.Status == StatusCode.Failure)
            {
                PackageManagerError = listRequest.Error.message;
                Debug.LogWarning("[Flat Kit] Failed to get packages from Package Manager.");
                return null;
            }

            return listRequest.Result;
        }
    }
}