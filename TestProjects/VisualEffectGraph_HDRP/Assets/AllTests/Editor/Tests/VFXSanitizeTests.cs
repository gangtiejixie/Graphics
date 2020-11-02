#if !UNITY_EDITOR_OSX || MAC_FORCE_TESTS
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine.VFX;
using UnityEditor.VFX;


using Object = UnityEngine.Object;
using System.IO;

namespace UnityEditor.VFX.Test
{
    [TestFixture]
    class VFXSanitizeTests
    {
        private static List<string> m_ToDeleteAsset = new List<string>();
        [OneTimeTearDown]
        public void CleanUp()
        {
            foreach (var asset in m_ToDeleteAsset)
                AssetDatabase.DeleteAsset(asset);
            m_ToDeleteAsset.Clear();
        }

        static readonly string tempBasePath = "Assets/TmpTests/";
        static readonly string tempFileFormat = tempBasePath + "vfx_{0}.vfx";

        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.CreateDirectory(tempBasePath);
        }

        [Test]
        public void Sanitize_Parameter_With_Sphere_Without_Angle()
        {
            string kSourceAsset = "Assets/AllTests/Editor/Tests/VFXSanitize_Test.vfx.rename_me";
            var rand = Guid.NewGuid().ToString();
            var name = string.Format(tempFileFormat, rand);
            File.Copy(kSourceAsset, name);
            m_ToDeleteAsset.Add(name);
            AssetDatabase.ImportAsset(name);
            var asset = AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(name);

            VisualEffectResource resource = asset.GetResource();
            var graph = resource.GetOrCreateGraph();

            Assert.AreEqual(3, graph.GetNbChildren());
            var parameter = graph.children.OfType<VFXParameter>().FirstOrDefault();
            var lerp = graph.children.OfType<Operator.Lerp>().FirstOrDefault();
            var inline = graph.children.OfType<VFXInlineOperator>().FirstOrDefault();
            Assert.IsNotNull(parameter);
            Assert.IsNotNull(lerp);
            Assert.IsNotNull(inline);

            Assert.IsTrue(parameter.exposed);

            Assert.IsTrue(inline.inputSlots[0].value is Sphere);
            Assert.IsTrue(parameter.outputSlots[0].value is Sphere);

            //0. Sphere/Center
            //1. Sphere/Angle
            //2. Sphere/Radius
            Assert.IsTrue(inline.inputSlots[0][0].HasLink());
            Assert.IsTrue(inline.inputSlots[0][2].HasLink());

            Assert.IsTrue(parameter.outputSlots[0][0].HasLink());
            Assert.IsTrue(parameter.outputSlots[0][2].HasLink());
        }
    }
}
#endif
