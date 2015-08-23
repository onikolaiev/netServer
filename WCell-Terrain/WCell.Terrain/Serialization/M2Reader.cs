using System;
using System.Collections.Generic;
using System.IO;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.MPQTool;
using WCell.Terrain.MPQ;
using WCell.Terrain.MPQ.ADTs;
using WCell.Terrain.MPQ.M2s;
using WCell.Terrain.MPQ.WMOs;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain.Serialization
{
    public class M2Reader
    {
        public static M2Model ReadM2(MpqLibrarian librarian, string filePath)
        {
            if (!librarian.FileExists(filePath))
            {
                var altFilePath = Path.ChangeExtension(filePath, ".m2");
                if (!librarian.FileExists(altFilePath))
                {
                    throw new Exception("File does not exist: " + filePath);
                }

                filePath = altFilePath;
            }

            var model = new M2Model();

            using (var stream = librarian.OpenFile(filePath))
            using (var br = new BinaryReader(stream))
            {
                ReadHeader(br, model);
                ReadGlobalSequences(br, model);
                ReadAnimations(br, model);
                ReadAnimationLookup(br, model);
                ReadBones(br, model);
                ReadKeyBoneLookup(br, model);
                ReadVertices(br, model);
                ReadColors(br, model);
                ReadTextures(br, model);
                ReadTransparency(br, model);
                ReadUVAnimation(br, model);
                ReadTexReplace(br, model);
                ReadRenderFlags(br, model);
                ReadBoneLookupTable(br, model);
                ReadTexLookup(br, model);
                ReadTexUnits(br, model);
                ReadTransLookup(br, model);
                ReadUVAnimLookup(br, model);
                ReadBoundingTriangles(br, model);
                ReadBoundingVertices(br, model);
                ReadBoundingNormals(br, model);
                ReadAttachments(br, model);
                ReadAttachLookups(br, model);
                ReadEvents(br, model);
                ReadLights(br, model);
                ReadCameras(br, model);
                ReadCameraLookup(br, model);
                ReadRibbonEmitters(br, model);
                ReadParticleEmitters(br, model);

                if (model.Header.HasUnknownFinalPart)
                {
                    ReadOptionalSection(br, model);
                }
            }

            return model;
        }

        static void ReadHeader(BinaryReader br, M2Model model)
        {
            var header = model.Header = new ModelHeader();

            header.Magic = br.ReadUInt32();
            header.Version = br.ReadUInt32();
            header.NameLength = br.ReadInt32();
            header.NameOffset = br.ReadInt32();
            header.GlobalModelFlags = (GlobalModelFlags) br.ReadUInt32();

            br.ReadOffsetLocation(ref header.GlobalSequences);
            br.ReadOffsetLocation(ref header.Animations);
            br.ReadOffsetLocation(ref header.AnimationLookup);
            br.ReadOffsetLocation(ref header.Bones);
            br.ReadOffsetLocation(ref header.KeyBoneLookup);
            br.ReadOffsetLocation(ref header.Vertices);
            header.ViewCount = br.ReadUInt32();
            br.ReadOffsetLocation(ref header.Colors);
            br.ReadOffsetLocation(ref header.Textures);
            br.ReadOffsetLocation(ref header.Transparency);
            br.ReadOffsetLocation(ref header.UVAnimation);
            br.ReadOffsetLocation(ref header.TexReplace);
            br.ReadOffsetLocation(ref header.RenderFlags);
            br.ReadOffsetLocation(ref header.BoneLookupTable);
            br.ReadOffsetLocation(ref header.TexLookup);
            br.ReadOffsetLocation(ref header.TexUnits);
            br.ReadOffsetLocation(ref header.TransLookup);
            br.ReadOffsetLocation(ref header.UVAnimLookup);

            header.VertexBox = br.ReadBoundingBox();
            header.VertexRadius = br.ReadSingle();
            header.BoundingBox = br.ReadBoundingBox();
            header.BoundingRadius = br.ReadSingle();

            br.ReadOffsetLocation(ref header.BoundingTriangles);
            br.ReadOffsetLocation(ref header.BoundingVertices);
            br.ReadOffsetLocation(ref header.BoundingNormals);
            br.ReadOffsetLocation(ref header.Attachments);
            br.ReadOffsetLocation(ref header.AttachLookup);
            br.ReadOffsetLocation(ref header.Events);
            br.ReadOffsetLocation(ref header.Lights);
            br.ReadOffsetLocation(ref header.Cameras);
            br.ReadOffsetLocation(ref header.CameraLookup);
            br.ReadOffsetLocation(ref header.RibbonEmitters);
            br.ReadOffsetLocation(ref header.ParticleEmitters);

            if (header.HasUnknownFinalPart)
            {
                br.ReadOffsetLocation(ref header.OptionalUnk);
            }


            br.BaseStream.Position = model.Header.NameOffset;
            //model.Name = Encoding.UTF8.GetString(br.ReadBytes(model.Header.NameLength));
            model.Name = br.ReadCString();
        }
        static void ReadGlobalSequences(BinaryReader br, M2Model model)
        {
            var gsInfo = model.Header.GlobalSequences;
            model.GlobalSequenceTimestamps = new uint[gsInfo.Count];

            br.BaseStream.Position = gsInfo.Offset;

            for (int i = 0; i < model.GlobalSequenceTimestamps.Length; i++)
            {
                model.GlobalSequenceTimestamps[i] = br.ReadUInt32();
            }
        }
        static void ReadAnimations(BinaryReader br, M2Model model)
        {
        }
        static void ReadAnimationLookup(BinaryReader br, M2Model model)
        {
        }
        static void ReadBones(BinaryReader br, M2Model model)
        {
        }
        static void ReadKeyBoneLookup(BinaryReader br, M2Model model)
        {
        }
        static void ReadVertices(BinaryReader br, M2Model model)
        {
            var vertInfo = model.Header.Vertices;

            model.Vertices = new ModelVertices[vertInfo.Count];

            br.BaseStream.Position = vertInfo.Offset;
            for (int i = 0; i < vertInfo.Count; i++)
            {
                var mv = new ModelVertices
                             {
                                 Position = br.ReadVector3(),
                                 BoneWeight = br.ReadBytes(4),
                                 BoneIndices = br.ReadBytes(4),
                                 Normal = br.ReadVector3(),
                                 TextureCoordinates = br.ReadVector2(),
                                 Float_1 = br.ReadSingle(),
                                 Float_2 = br.ReadSingle()
                             };

                model.Vertices[i] = mv;
            }
        }
        static void ReadColors(BinaryReader br, M2Model model)
        {
        }
        static void ReadTextures(BinaryReader br, M2Model model)
        {
        }
        static void ReadTransparency(BinaryReader br, M2Model model)
        {
        }
        static void ReadUVAnimation(BinaryReader br, M2Model model)
        {
        }
        static void ReadTexReplace(BinaryReader br, M2Model model)
        {
        }
        static void ReadRenderFlags(BinaryReader br, M2Model model)
        {
        }
        static void ReadBoneLookupTable(BinaryReader br, M2Model model)
        {
        }
        static void ReadTexLookup(BinaryReader br, M2Model model)
        {
        }
        static void ReadTexUnits(BinaryReader br, M2Model model)
        {
        }
        static void ReadTransLookup(BinaryReader br, M2Model model)
        {
        }
        static void ReadUVAnimLookup(BinaryReader br, M2Model model)
        {
        }
        static void ReadBoundingTriangles(BinaryReader br, M2Model model)
        {
            var btInfo = model.Header.BoundingTriangles;
            model.BoundingTriangles = new Index3[btInfo.Count / 3];

            br.BaseStream.Position = btInfo.Offset;

            for (var i = 0; i < model.BoundingTriangles.Length; i++)
            {
                model.BoundingTriangles[i] = new Index3
                {
                    Index2 = br.ReadInt16(),
                    Index1 = br.ReadInt16(),
                    Index0 = br.ReadInt16()
                };

            }
        }
        static void ReadBoundingVertices(BinaryReader br, M2Model model)
        {
            var bvInfo = model.Header.BoundingVertices;

            model.BoundingVertices = new Vector3[bvInfo.Count];
            br.BaseStream.Position = bvInfo.Offset;

            for (var i = 0; i < bvInfo.Count; i++)
            {
                model.BoundingVertices[i] = br.ReadVector3();
            }
        }
        static void ReadBoundingNormals(BinaryReader br, M2Model model)
        {
            var bnInfo = model.Header.BoundingVertices;

            model.BoundingNormals = new Vector3[bnInfo.Count];
            br.BaseStream.Position = bnInfo.Offset;

            for (var i = 0; i < bnInfo.Count; i++)
            {
                model.BoundingNormals[i] = br.ReadVector3();
            }
        }
        static void ReadAttachments(BinaryReader br, M2Model model)
        {
        }
        static void ReadAttachLookups(BinaryReader br, M2Model model)
        {
        }
        static void ReadEvents(BinaryReader br, M2Model model)
        {
        }
        static void ReadLights(BinaryReader br, M2Model model)
        {
        }
        static void ReadCameras(BinaryReader br, M2Model model)
        {
        }
        static void ReadCameraLookup(BinaryReader br, M2Model model)
        {
        }
        static void ReadRibbonEmitters(BinaryReader br, M2Model model)
        {
        }
        static void ReadParticleEmitters(BinaryReader br, M2Model model)
        {
        }
        static void ReadOptionalSection(BinaryReader br, M2Model model)
        {
        }

		public static M2 ReadM2(MpqLibrarian librarian, MapDoodadDefinition doodadDefinition)
		{
			var filePath = doodadDefinition.FilePath;
			var ext = Path.GetExtension(filePath);

			if (ext.Equals(".mdx") ||
				ext.Equals(".mdl"))
			{
				filePath = Path.ChangeExtension(filePath, ".m2");
			}

			var model = M2Reader.ReadM2(librarian, doodadDefinition.FilePath);

			var tempIndices = new List<int>();
			foreach (var tri in model.BoundingTriangles)
			{
				tempIndices.Add(tri.Index2);
				tempIndices.Add(tri.Index1);
				tempIndices.Add(tri.Index0);
			}

			var currentM2 = TransformM2(model, tempIndices, doodadDefinition);
			var tempVertices = currentM2.Vertices;
			var bounds = new BoundingBox(tempVertices.ToArray());
			currentM2.Bounds = bounds;

			return currentM2;
		}

		static M2 TransformM2(M2Model model, IEnumerable<int> indicies, MapDoodadDefinition mddf)
		{
			var currentM2 = new M2();
			currentM2.Vertices.Clear();
			currentM2.Indices.Clear();

			var posX = (mddf.Position.X - TerrainConstants.CenterPoint) * -1;
			var posY = (mddf.Position.Y - TerrainConstants.CenterPoint) * -1;
			var origin = new Vector3(posX, posY, mddf.Position.Z);

			// Create the scale matrix used in the following loop.
			Matrix scaleMatrix;
			Matrix.CreateScale(mddf.Scale, out scaleMatrix);

			// Creation the rotations
			var rotateZ = Matrix.CreateRotationZ(MathHelper.ToRadians(mddf.OrientationB + 180));
			var rotateY = Matrix.CreateRotationY(MathHelper.ToRadians(mddf.OrientationA));
			var rotateX = Matrix.CreateRotationX(MathHelper.ToRadians(mddf.OrientationC));

			var worldMatrix = Matrix.Multiply(scaleMatrix, rotateZ);
			worldMatrix = Matrix.Multiply(worldMatrix, rotateX);
			worldMatrix = Matrix.Multiply(worldMatrix, rotateY);

			for (var i = 0; i < model.BoundingVertices.Length; i++)
			{
				var position = model.BoundingVertices[i];
				var normal = model.BoundingNormals[i];

				// Scale and Rotate
				Vector3 rotatedPosition;
				Vector3.Transform(ref position, ref worldMatrix, out rotatedPosition);

				Vector3 rotatedNormal;
				Vector3.Transform(ref normal, ref worldMatrix, out rotatedNormal);
				rotatedNormal.Normalize();

				// Translate
				Vector3 finalVector;
				Vector3.Add(ref rotatedPosition, ref origin, out finalVector);

				currentM2.Vertices.Add(finalVector);
			}

			currentM2.Indices.AddRange(indicies);
			return currentM2;
		}
    }
}
