// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// manage icon assets utility
    /// </summary>
    public class GuiAssets
    {
        private static readonly Dictionary<string, CanvasBitmap> bmps = new Dictionary<string, CanvasBitmap>();

        public GuiAssets(CanvasControl canvas, params string[] addFolders)
        {
            Load(canvas, addFolders);
        }

        /// <summary>
        /// lazy image loading (png, jpg, gif)
        /// </summary>
        /// <param name="canvas">canvas object reference</param>
        /// <param name="addFolders">addtional folder</param>
        /// <remarks>
        /// set path string to addFolders as below instruction
        /// "./Assets"                              Assets folder of main project (set default)
        /// "./Assets/＜sub folder＞"                Assets sub folder of main project
        /// "./＜sub projct＞/Assets"                 Assets folder of sub project
        /// "./＜sub project＞/Assets/＜sub folder＞"   Assets sub folder of sub project
        /// </remarks>
        private async void Load(CanvasControl canvas, params string[] addFolders)
        {
            var folders = new List<string>
            {
                "./Assets",
            };
            folders.AddRange(addFolders);
            foreach (var folder in folders)
            {
                var fo = folder.StartsWith("./") ? folder.Substring(1) : folder;
                foreach (var pattern in new[] { "*.png", "*.jpg", "*.gif" })
                {
                    var keys = Directory.GetFiles(folder, pattern, SearchOption.AllDirectories);
                    foreach (var key0 in keys)
                    {
                        var key = key0;
                        if (key.StartsWith("./"))
                        {
                            key = key.Substring(1);
                        }
                        var path = $"ms-appx://{key}";
                        var bmp = await CanvasBitmap.LoadAsync(canvas, new Uri(path));
                        bmps[Path.GetFileNameWithoutExtension(key)] = bmp;
                    }
                }
            }
        }

        /// <summary>
        /// get image object from assets
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public CanvasBitmap Image(string key)
        {
            if (bmps.TryGetValue(key, out var ret))
            {
                return ret;
            }
            else
            {
                return null;
            }
        }
    }
}
