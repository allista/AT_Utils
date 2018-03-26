//   TextureCache.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AT_Utils
{
    
    public static class TextureCache
    {
        static readonly string[] img_ext = { ".png", ".jpg", ".jpeg", ".PNG", ".JPG", ".JPEG" };
        static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        public static Texture2D GetTexture(string path, bool ksp_path=true)
        {
            Texture2D texture;
            if(!textures.TryGetValue(path, out texture))
            {
                texture = load_texture(ref path, ksp_path);
                textures[path] = texture;
            }
            return texture;
        }

        static Texture2D load_texture(ref string path, bool ksp_path)
        {
            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            if(ksp_path)
                path = Path.Combine(KSPUtil.ApplicationRootPath+"GameData/", path);
            path.Replace('/', Path.DirectorySeparatorChar);
            if(File.Exists(path))
            {
                texture.LoadImage(File.ReadAllBytes(path));
                return texture;
            }
            foreach(var ext in img_ext)
            {
                if(File.Exists(path+ext))
                {
                    path += ext;
                    texture.LoadImage(File.ReadAllBytes(path));
                    return texture;
                }
            }
            Utils.Log("Texture file not found: {}", path);
            return null;
        }
    }
}
