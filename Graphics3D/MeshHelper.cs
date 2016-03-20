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
            var data = File.ReadAllText(fileName);
            dynamic jsonObject = JsonConvert.DeserializeObject(data);

            for (var meshIndex = 0; meshIndex < jsonObject.meshes.Count; meshIndex++)
            {
                var verticesArray = jsonObject.meshes[meshIndex].vertices;
                var facesArray = jsonObject.meshes[meshIndex].indices;

                var verticesStep = 1;
                var uvCount = jsonObject.meshes[meshIndex].uvCount.Value;

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
                    mesh.Vertices[index] = new Vector3(x, y, z);
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
                meshes.Add(mesh);
            }

            return meshes.ToArray();
        }
    }
}
