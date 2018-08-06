using System.Reflection;
using ColossalFramework.UI;
using UnityEngine;

namespace ParallelRoadTool.Utils
{
    internal static class ResourceLoader
    {
        public static UITextureAtlas CreateTextureAtlas(string atlasName, string[] spriteNames, string assemblyPath)
        {
            var maxSize = 1024;
            var texture2D = new Texture2D(maxSize, maxSize, TextureFormat.ARGB32, false);
            var textures = new Texture2D[spriteNames.Length];
            var regions = new Rect[spriteNames.Length];

            for (var i = 0; i < spriteNames.Length; i++)
                textures[i] = LoadTextureFromAssembly(assemblyPath + "." + spriteNames[i] + ".png");

            regions = texture2D.PackTextures(textures, 2, maxSize);

            var textureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            var material = Object.Instantiate(UIView.GetAView().defaultAtlas.material);
            material.mainTexture = texture2D;
            textureAtlas.material = material;
            textureAtlas.name = atlasName;

            for (var i = 0; i < spriteNames.Length; i++)
            {
                var item = new UITextureAtlas.SpriteInfo
                {
                    name = spriteNames[i],
                    texture = textures[i],
                    region = regions[i]
                };

                textureAtlas.AddSprite(item);
            }

            return textureAtlas;
        }

        public static void AddTexturesInAtlas(UITextureAtlas atlas, Texture2D[] newTextures, bool locked = false)
        {
            var textures = new Texture2D[atlas.count + newTextures.Length];

            for (var i = 0; i < atlas.count; i++)
            {
                var texture2D = atlas.sprites[i].texture;

                if (locked)
                {
                    // Locked textures workaround
                    var renderTexture = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 0);
                    Graphics.Blit(texture2D, renderTexture);

                    var active = RenderTexture.active;
                    texture2D = new Texture2D(renderTexture.width, renderTexture.height);
                    RenderTexture.active = renderTexture;
                    texture2D.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
                    texture2D.Apply();
                    RenderTexture.active = active;

                    RenderTexture.ReleaseTemporary(renderTexture);
                }

                textures[i] = texture2D;
                textures[i].name = atlas.sprites[i].name;
            }

            for (var i = 0; i < newTextures.Length; i++)
                textures[atlas.count + i] = newTextures[i];

            var regions = atlas.texture.PackTextures(textures, atlas.padding, 4096, false);

            atlas.sprites.Clear();

            for (var i = 0; i < textures.Length; i++)
            {
                var spriteInfo = atlas[textures[i].name];
                atlas.sprites.Add(new UITextureAtlas.SpriteInfo
                {
                    texture = textures[i],
                    name = textures[i].name,
                    border = spriteInfo != null ? spriteInfo.border : new RectOffset(),
                    region = regions[i]
                });
            }

            atlas.RebuildIndexes();
        }

        public static UITextureAtlas GetAtlas(string name)
        {
            var atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            for (var i = 0; i < atlases.Length; i++)
                if (atlases[i].name == name)
                    return atlases[i];

            return UIView.GetAView().defaultAtlas;
        }

        private static Texture2D LoadTextureFromAssembly(string path)
        {
            var manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);

            var array = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(array, 0, array.Length);

            var texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            texture2D.LoadImage(array);

            return texture2D;
        }
    }
}