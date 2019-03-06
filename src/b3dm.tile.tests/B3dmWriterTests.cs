﻿using Gltf.Core;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using Wkx;

namespace B3dm.Tile.Tests
{
    public class B3dmWriterTests
    {
        [Test]
        public void WriteB3dmTest()
        {
            const string testfile = "B3dm.Tile.Tests.testfixtures.building.wkb";
            const string headerfile = "testfixtures/expected_building_header.json";
            var header = File.ReadAllText(headerfile);
            var buildingWkb = Assembly.GetExecutingAssembly().GetManifestResourceStream(testfile);
            var gltfFile = GetGltfFile(buildingWkb);
            Assert.IsTrue(gltfFile != null);
            var actualHeader = gltfFile.Header;
            var actualHeaderString = JsonConvert.SerializeObject(actualHeader);
            // File.WriteAllText("actual_building_header.json", actualHeaderString);
            var expectedHeader = JsonConvert.DeserializeObject<Header>(header);
            Assert.IsTrue(actualHeader.asset.version.Equals(expectedHeader.asset.version));
            Assert.IsTrue(!actualHeader.asset.generator.Equals(expectedHeader.asset.generator));

        }

        private GltfFile GetGltfFile(Stream buildingWkb)
        {
            var g = Geometry.Deserialize<WkbSerializer>(buildingWkb);
            var polyhedralsurface = ((PolyhedralSurface)g);
            var bb = polyhedralsurface.GetBoundingBox3D();

            var triangles = Triangulator.Triangulate(polyhedralsurface);
            Assert.IsTrue(triangles.Count == 22);

            var bytesPositions = triangles.PositionsToBinary();
            Assert.IsTrue(bytesPositions.Length == 792);

            var bytesNormals = triangles.NormalsToBinary();
            Assert.IsTrue(bytesNormals.Length == 792);

            // what's this for matrix??? 
            // translation : 1842015.125, 5177109.25, 247.87364196777344
            // Later comment: World coordinates transformation flattend matrix
            var m = new Matrix4x4(1, 0, 0, 1842015.125f,
                0, 1, 0, 5177109.25f,
                0, 0, 1, 247.87364196777344f,
                0, 0, 0, 1
                );
            var transform = m.Flatten();

            var gltfArray = new Body
            {
                Positions = bytesPositions,
                Normals = bytesNormals,
                BBox = bb
            };

            var n = (int)Math.Round((double)gltfArray.Positions.Length / 12, 0);
            Assert.IsTrue(n == 66);
            var binIds = BinaryConvertor.ToBinary(new float[n]);
            Assert.IsTrue(binIds[0] == 0);
            Assert.IsTrue(binIds.Length == 264);

            var header = GetHeader(gltfArray, transform, n);
            // todo: convert to Json and compare with expected header
            var gltfBodyLength = gltfArray.Positions.Length + gltfArray.Normals.Length + binIds.Length;
            Assert.IsTrue(gltfBodyLength == 1848);

            var gltfFile = new GltfFile() { Header = header, Body = gltfArray };
            return gltfFile;
        }

        public Header GetHeader(Body  gltfArray, float[] transform, int n)
        {
            Assert.IsTrue(gltfArray.Positions.Length == 792);
            Assert.IsTrue(gltfArray.Positions[0] == 184);
            var batchLength = 1;
            var byteLength = gltfArray.Positions.Length * 2;
            Assert.IsTrue(byteLength == 1584);
            byteLength += gltfArray.Positions.Length / 3;
            Assert.IsTrue(byteLength == 1848);
            var buffers = new List<Gltf.Core.Buffer>();
            var buffer = new Gltf.Core.Buffer() { byteLength = byteLength };
            buffers.Add(buffer);

            var bufferViews = new List<Bufferview>();
            // q: whats 34962? ## vertices
            bufferViews.Add(new Bufferview() { buffer = 0, byteLength = gltfArray.Positions.Length, byteOffset = 0, target = 34962 });
            bufferViews.Add(new Bufferview() { buffer = 0, byteLength = gltfArray.Positions.Length, byteOffset = gltfArray.Positions.Length, target = 34962 });
            bufferViews.Add(new Bufferview() { buffer = 0, byteLength = gltfArray.Positions.Length/3, byteOffset = 2*gltfArray.Positions.Length, target = 34962 });

            var accessors = new List<Accessor>();
            var bb = gltfArray.BBox;
            // q: max and min are reversed in next py code?
            // # vertices
            accessors.Add(new Accessor()
            {
                bufferView = 0,
                byteOffset = 0,
                componentType = 5126,
                count = n,
                max = new double[3] { bb.YMin, bb.ZMin, bb.XMin },
                min = new double[3] { bb.YMax, bb.ZMax, bb.XMax },
                type = "VEC3"
            });

            // # normals
            accessors.Add(new Accessor()
            {
                bufferView = 1,
                byteOffset = 0,
                componentType = 5126,
                count = n,
                max = new double[3] { 1,1,1 },
                min = new double[3] { -1,-1,-1 },
                type = "VEC3"
            });

            // # batched
            accessors.Add(new Accessor()
            {
                bufferView = 2,
                byteOffset = 0,
                componentType = 5126,
                count = n,
                max = new double[1] { batchLength},
                min = new double[1] { 0 },
                type = "SCALAR"
            });

            // # meshes
            var mesh = new Mesh() {};
            var primitives = new List<Primitive>();
            var attributes = new Attributes() { POSITION = 2, NORMAL = 1, _BATCHID = 2 };
            var primitive = new Primitive() { attributes=attributes, material = 0, mode = 4 };
            primitives.Add(primitive);
            mesh.primitives = primitives.ToArray();
            var meshes = new List<Mesh>();
            meshes.Add(mesh);

            // # nodes
            var nodes = new List<Node>();
            var node = new Node() { matrix = transform, mesh = 0 };
            nodes.Add(node);

            // # materials
            var materials = new List<Material>();
            var material = new Material() { name = "Material", pbrMetallicRoughness = new Pbrmetallicroughness() { metallicFactor = 0 } };
            materials.Add(material);

            // # GltfHeader
            var gltfHeader = new Header();
            gltfHeader.asset = new Asset() { generator = "Glt.Core", version = "2.0" };
            gltfHeader.scene = 0;
            var gltfScenes = new List<Scene>();
            gltfScenes.Add(new Scene() { nodes = new int[nodes.Count] });
            gltfHeader.scenes = gltfScenes.ToArray();
            gltfHeader.nodes = nodes.ToArray();
            gltfHeader.meshes = meshes.ToArray();
            gltfHeader.materials = materials.ToArray();
            gltfHeader.accessors = accessors.ToArray();
            gltfHeader.bufferViews = bufferViews.ToArray();
            gltfHeader.buffers = buffers.ToArray();
            return gltfHeader;
        }
    }
}