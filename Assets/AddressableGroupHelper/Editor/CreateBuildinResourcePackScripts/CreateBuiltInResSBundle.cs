using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

public class CreateBuiltInResSBundle : IBuildTask
    {
        private static readonly GUID k_BuiltInGuid = new GUID("0000000000000000f000000000000000");

        [InjectContext(ContextUsage.In, false)]
        private IDependencyData m_DependencyData;

        [InjectContext(ContextUsage.InOut, true)]
        private IBundleExplictObjectLayout m_Layout;

        public int Version => 1;
        public string BundleName { get; set; }

        /// <summary>
        /// Stores the name for the built-in shaders bundle.
        /// </summary>
        public string ShaderBundleName { get; set; }

        /// <summary>
        /// Create the built-in shaders bundle.
        /// </summary>
        /// <param name="shaderBundleName">The name of the shader bundle</param>
        /// <param name="bundleName">The name of the other builtin resources bundle.</param>
        public CreateBuiltInResSBundle(string shaderBundleName, string bundleName)
        {
            ShaderBundleName = shaderBundleName;
            BundleName = bundleName + "test";
        }

        /// <inheritdoc />
        public ReturnCode Run()
        {
            HashSet<ObjectIdentifier> hashSet = new HashSet<ObjectIdentifier>();
            foreach (AssetLoadInfo value in m_DependencyData.AssetInfo.Values)
            {
                hashSet.UnionWith(value.referencedObjects.Where((ObjectIdentifier x) => x.guid == k_BuiltInGuid));
            }

            foreach (SceneDependencyInfo value2 in m_DependencyData.SceneInfo.Values)
            {
                hashSet.UnionWith(value2.referencedObjects.Where((ObjectIdentifier x) => x.guid == k_BuiltInGuid));
            }

            ObjectIdentifier[] array = hashSet.ToArray();
            Type[] mainTypeForObjects = BuildCacheUtility.GetMainTypeForObjects(array);
            if (m_Layout == null)
            {
                m_Layout = new BundleExplictObjectLayout();
            }

            Type typeFromHandle = typeof(Shader);
            for (int i = 0; i < mainTypeForObjects.Length; i++)
            {
                if (!(mainTypeForObjects[i] != typeFromHandle))
                {
                    m_Layout.ExplicitObjectLocation.Add(array[i], ShaderBundleName);
                    
                }
                else
                {
                    m_Layout.ExplicitObjectLocation.Add(array[i], BundleName);
            }
            }

            if (m_Layout.ExplicitObjectLocation.Count == 0)
            {
                m_Layout = null;
            }

            return ReturnCode.Success;
        }
    }