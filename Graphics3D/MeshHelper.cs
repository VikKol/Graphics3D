using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SharpDX;

namespace Graphics3D
{
    public class MeshHelper
    {
        //Source:https://blogs.msdn.microsoft.com/davrous/2013/06/17/tutorial-part-3-learning-how-to-write-a-3d-soft-engine-in-c-ts-or-js-loading-meshes-exported-from-blender/
        public static Mesh[] LoadFromJsonFile(string fileName)
        {
            var meshes = new List<Mesh>();
            var materials = new Dictionary<string, Material>();
            var data = File.ReadAllText(fileName);
            dynamic jsonObject = JsonConvert.DeserializeObject(data);

            for (var materialIndex = 0; materialIndex < jsonObject.materials.Count; materialIndex++)
            {
                var material = new Material();
                material.ID = jsonObject.materials[materialIndex].id.Value;
                material.Name = jsonObject.materials[materialIndex].name.Value;
                if (jsonObject.materials[materialIndex].diffuseTexture != null)
                    material.DiffuseTextureName = jsonObject.materials[materialIndex].diffuseTexture.name.Value;
                materials.Add(material.ID, material);
            }

            for (var meshIndex = 0; meshIndex < jsonObject.meshes.Count; meshIndex++)
            {
                var verticesArray = jsonObject.meshes[meshIndex].vertices;
                var facesArray = jsonObject.meshes[meshIndex].indices;
                var uvCount = jsonObject.meshes[meshIndex].uvCount.Value;

                var verticesStep = 1;

                // Depending of the number of texture's coordinates per vertex
                // we're jumping in the vertices array by 6, 8 & 10 windows frame
                switch ((int)uvCount)
                {
                    case 0:
                        verticesStep = 6;
                        break;
                    case 1:
                        verticesStep = 8;
                        break;
                    case 2:
                        verticesStep = 10;
                        break;
                }
                
                // the number of interesting vertices information for us
                var verticesCount = verticesArray.Count / verticesStep;
                
                // number of faces is the array divided by 3 (A, B, C)
                var facesCount = facesArray.Count / 3;
                var mesh = new Mesh(jsonObject.meshes[meshIndex].name.Value, verticesCount, facesCount);

                // Filling the Vertices array of mesh
                for (var index = 0; index < verticesCount; index++)
                {
                    var x = (float)verticesArray[index * verticesStep].Value;
                    var y = (float)verticesArray[index * verticesStep + 1].Value;
                    var z = (float)verticesArray[index * verticesStep + 2].Value;

                    // Loading the vertex normal
                    var nx = (float)verticesArray[index * verticesStep + 3].Value;
                    var ny = (float)verticesArray[index * verticesStep + 4].Value;
                    var nz = (float)verticesArray[index * verticesStep + 5].Value;
                    mesh.Vertices[index] = new Vertex {
                        Coordinates = new Vector3(x, y, z),
                        Normal = new Vector3(nx, ny, nz)
                    };

                    if (uvCount > 0)
                    {
                        // Loading the texture coordinates
                        float u = (float)verticesArray[index * verticesStep + 6].Value;
                        float v = (float)verticesArray[index * verticesStep + 7].Value;
                        mesh.Vertices[index].TextureCoordinates = new Vector2(u, v);
                    }
                }

                // Filling the Faces array
                for (var index = 0; index < facesCount; index++)
                {
                    var a = (int)facesArray[index * 3].Value;
                    var b = (int)facesArray[index * 3 + 1].Value;
                    var c = (int)facesArray[index * 3 + 2].Value;
                    mesh.Faces[index] = new Face { A = a, B = b, C = c };
                }

                // Getting the position set in Blender
                var position = jsonObject.meshes[meshIndex].position;
                mesh.Position = new Vector3((float)position[0].Value, (float)position[1].Value, (float)position[2].Value);

                if (uvCount > 0)
                {
                    // Loading the texture
                    var meshTextureID = jsonObject.meshes[meshIndex].materialId.Value;
                    var meshTextureName = materials[meshTextureID].DiffuseTextureName;
                    string path = fileName.Substring(0, fileName.LastIndexOf("/") + 1) + meshTextureName;
                    mesh.Texture = new Texture(path);
                }

                mesh.ComputeFacesNormals();

                meshes.Add(mesh);
            }

            return meshes.ToArray();
        }
    }
}
