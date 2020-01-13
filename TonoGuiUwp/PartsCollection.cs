// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI;
using static Tono.Gui.Uwp.CastUtil;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// parts collection manager
    /// </summary>
    /// <remarks>
    /// also, customize how to draw (speed kaizen...)
    /// </remarks>
    public class PartsCollection
    {
        private readonly Dictionary<NamedId/*LayerNo*/, Dictionary<string/*name of IDrawArea*/, List<IPartsDraw>>> _dat = new Dictionary<NamedId, Dictionary<string, List<IPartsDraw>>>();
        private readonly Dictionary<IDrawArea, string/*name of IDrawArea*/> _danames = new Dictionary<IDrawArea, string>();

        private static int _nonamecounter = 0;

        /// <summary>
        /// gui sheared assets
        /// </summary>
        public GuiAssets Assets { get; internal set; }
        public Queue<(IDrawArea pane, IPartsDraw parts, NamedId layer)> Dels { get => Dels1; set => Dels1 = value; }
        public Queue<(IDrawArea pane, IPartsDraw parts, NamedId layer)> Dels1 { get => _dels; set => _dels = value; }

        /// <summary>
        /// make layer key
        /// </summary>
        /// <param name="da">TGuiViewか、TPaneのインスタンス</param>
        /// <returns></returns>
        private string GetName(IDrawArea da)
        {
            if (da == null)
            {
                return $"<all>";
            }
            if (_danames.TryGetValue(da, out var name) == false)
            {
                if (da.Name != null)
                {
                    name = da.Name;
                }
                else
                {
                    name = $"noname{++_nonamecounter}";
                }
                _danames[da] = name;
            }
            return name;
        }

        private Queue<(IDrawArea pane, IPartsDraw parts, NamedId layer)> _dels = new Queue<(IDrawArea pane, IPartsDraw parts, NamedId layer)>();

        /// <summary>
        /// remove parts request
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="pt"></param>
        /// <param name="layer">0=lower</param>
        public void Remove(IDrawArea pane, IPartsDraw pt, NamedId layer)
        {
            Dels.Enqueue((pane, pt, layer));
        }

        /// <summary>
        /// add parts
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="parts"></param>
        /// <param name="layer">0=lower</param>
        public void Add(IDrawArea pane, IPartsDraw parts, NamedId layer)
        {
            lock (_dat)
            {
                if (_dat.TryGetValue(layer, out var ppts) == false)
                {
                    _dat[layer] = ppts = new Dictionary<string, List<IPartsDraw>>();
                }
                var name = GetName(pane);
                if (ppts.TryGetValue(name, out var lpts) == false)
                {
                    ppts[name] = lpts = new List<IPartsDraw>();
                }
                parts.Assets = Assets;
                lpts.Add(parts);
            }
        }

        /// <summary>
        /// add parts set
        /// </summary>
        /// <param name="partsset"></param>
        /// <param name="layer">0=lower</param>
        public void Add(IEnumerable<IPartsDraw> partsset, NamedId layer)
        {
            Add(null, partsset, layer);
        }


        /// <summary>
        /// add parts set
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="pts"></param>
        /// <param name="layer">0=lower</param>
        public void Add(IDrawArea pane, IEnumerable<IPartsDraw> pts, NamedId layer)
        {
            lock (_dat)
            {
                if (_dat.TryGetValue(layer, out var ppts) == false)
                {
                    _dat[layer] = ppts = new Dictionary<string, List<IPartsDraw>>();
                }
                var name = GetName(pane);
                if (ppts.TryGetValue(name, out var lpts) == false)
                {
                    ppts[name] = lpts = new List<IPartsDraw>();
                }
                lpts.AddRange(pts);
            }
        }

        /// <summary>
        /// get parts set
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="pane"></param>
        /// <param name="layer"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEnumerable<ISelectableParts> GetParts(ScreenPos pos, IDrawArea pane, NamedId layer, Func<ISelectableParts, bool> filter = null)
        {
            var ret =
                from pt in GetParts(layer)
                let sp = pt as ISelectableParts
                where sp != null
                where filter == null || filter?.Invoke(sp) == true
                let score = sp.SelectingScore(pane, pos)
                where score < 1 && score >= 0
                orderby score
                select sp;
            return ret;
        }

        /// <summary>
        /// get parts set
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="filter">parts filter</param>
        /// <returns></returns>
        public IEnumerable<IPartsDraw> GetParts(NamedId layer, Func<IPartsDraw, bool> filter = null)
        {
            return GetParts(new[] { layer }, filter);
        }

        /// <summary>
        /// get parts set
        /// </summary>
        /// <param name="layers"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEnumerable<IPartsDraw> GetParts(IEnumerable<NamedId> layers, Func<IPartsDraw, bool> filter = null)
        {
            var tarParts =
                from layer in layers
                from kv in _dat
                where kv.Key.Equals(layer)
                from kv2 in kv.Value
                from pt in kv2.Value
                where filter == null || filter?.Invoke(pt) == true
                select pt;
            return tarParts;
        }

        /// <summary>
        /// get parts set
        /// </summary>
        /// <typeparam name="T">specified Parts class based PartsBase</typeparam>
        /// <param name="layer"></param>
        /// <param name="filter">parts filter</param>
        /// <returns></returns>
        public IEnumerable<T> GetParts<T>(NamedId layer, Func<T, bool> filter = null)
        {
            return GetParts<T>(new[] { layer }, filter);
        }

        /// <summary>
        /// get parts set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="layers"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEnumerable<T> GetParts<T>(IEnumerable<NamedId> layers, Func<T, bool> filter = null)
        {
            var tarParts =
                from layer in layers
                from kv in _dat
                where kv.Key.Equals(layer)
                from kv2 in kv.Value
                from pt in kv2.Value
                where pt is T
                where filter == null || filter?.Invoke((T)pt) == true
                select (T)pt;
            return tarParts;
        }


        private readonly Dictionary<NamedId/*layer*/, string/*doubleBufferName*/> _doubleBufferLayerGroups = new Dictionary<NamedId, string>();
        private readonly Dictionary<string/*double buffer name*/, DrawPropertyDoubleBuffer> _doubleBuffers = new Dictionary<string, DrawPropertyDoubleBuffer>();

        /// <summary>
        /// clear double buffer for request to reconstruct it.
        /// </summary>
        /// <param name="bufferName"></param>
        public void ClearDoubleBuffer(string bufferName, bool isSizeChanged)
        {
            lock (_doubleBuffers)
            {
                if (_doubleBuffers.TryGetValue(bufferName, out var dbuf))
                {
                    dbuf.ClearBuffer(isSizeChanged);
                }
            }
        }

        /// <summary>
        /// register double buffer layers
        /// </summary>
        /// <param name="name">double buffer name</param>
        /// <param name="layers">layer collectino</param>
        public void RegisterDoubleBufferName(string bufferName, params NamedId[] layers)
        {
            lock (_doubleBufferLayerGroups)
            {
                foreach (var layerno in layers)
                {
                    _doubleBufferLayerGroups[layerno] = bufferName;
                }
            }
            lock (_doubleBuffers)
            {
                if (_doubleBuffers.TryGetValue(bufferName, out var ddp))
                {
                    var lno = MathUtil.Max<int>(ddp.Layer.Id.Value, layers.Select(a => a.Id.Value).Max());
                    ddp.Layer = NamedId.From($"(auto.doublebufferlayer.{lno})", lno);
                    ddp.ClearBuffer(false);
                }
                else
                {
                    var lno = layers.Select(a => a.Id.Value).Max();
                    _doubleBuffers[bufferName] = ddp = new DrawPropertyDoubleBuffer
                    {
                        Layer = NamedId.From($"(auto.doublebufferlayer.{lno})", lno),
                        Status = DrawPropertyDoubleBuffer.Statuses.Empty,
                    };
                }
            }
        }

        /// <summary>
        /// draw parts set
        /// </summary>
        /// <param name="tarPane">target pane</param>
        /// <param name="sender">target canvas</param>
        /// <param name="cds">canvas drawing session</param>
        public void ProvideDraw(IDrawArea tarPane, CanvasControl sender, CanvasDrawingSession cds)
        {
            if (tarPane.Rect.IsEmptyNegative())
            {
                return;
            }

            // do remove request here.
            lock (_dat)
            {
                while (Dels.Count() > 0)
                {
                    var (pane, parts, layer) = Dels.Dequeue();
                    _dat[layer][GetName(pane)].Remove(parts);
                }
            }
            Dels.Clear();

            // draw
            var tarPaneName = GetName(tarPane);
            var layerpartsset =
                from kv in _dat
                let layer = kv.Key
                orderby layer.Id.Value ascending
                from kv2 in kv.Value
                let paneName = kv2.Key
                where paneName == tarPaneName || paneName == "<all>"
                select (layer, kv2.Value);

            foreach (var (layer, partsset) in layerpartsset)
            {
                DrawProperty dp = null;
                DrawPropertyDoubleBuffer ddp = null;

                var sw = Stopwatch.StartNew();
                var drewCount = 0;

                if (_doubleBufferLayerGroups.TryGetValue(layer, out var buffername))
                {
                    dp = ddp = _doubleBuffers[buffername];
                    if (ddp.Status == DrawPropertyDoubleBuffer.Statuses.Empty)
                    {
                        if (ddp.RenderTarget == null)
                        {
                            var device = CanvasDevice.GetSharedDevice();
                            ddp.RenderTarget = new CanvasRenderTarget(device, tarPane.Rect.Width, tarPane.Rect.Height, sender.Dpi);
                        }
                        else
                        {
                            ddp.Graphics.Clear(Colors.Transparent);
                        }
                        ddp.Pane = tarPane;
                        ddp.Canvas = sender;
                        ddp.PaneRect = tarPane.Rect;
                        ddp.Status = DrawPropertyDoubleBuffer.Statuses.Creating;
                    }
                }
                else
                {
                    dp = new DrawProperty // draw property (for speed up)
                    {
                        Pane = tarPane,
                        Canvas = sender,
                        Graphics = cds,
                        PaneRect = tarPane.Rect,
                        Layer = layer,
                    };
                }

                if (ddp == null || ddp.Status == DrawPropertyDoubleBuffer.Statuses.Creating)
                {
                    foreach (var pt in from pt in partsset orderby pt.ZIndex select pt)
                    {
                        try
                        {
                            pt.Draw(dp);
                            drewCount++;
                        }
#pragma warning disable CS0168, CS0219
                        catch (Exception ex)
                        {
                            var a = 1;
                        }
#pragma warning restore CS0168, CS0219
                    }
                }
                if (ddp?.Layer.Equals(layer) ?? false)
                {
                    ddp.Status = DrawPropertyDoubleBuffer.Statuses.Created;
                    ddp.ClearDrawingSession();
                    cds.DrawImage(ddp.RenderTarget, _(ddp.PaneRect));
                }

                sw.Stop();
                if (sw.ElapsedMilliseconds > 300)
                {
                    LOG.WriteLine(LLV.DEV, $"HEAVY DRAW {sw.Elapsed.TotalMilliseconds:0}[ms] : Layer:{layer} : # of parts = {drewCount}");
                }
            }
        }
    }
}
