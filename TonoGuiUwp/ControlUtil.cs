// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// UWP control utility
    /// </summary>
    public static class ControlUtil
    {
        /// <summary>
        /// reload page
        /// </summary>
        public static void Reload()
        {
            reloadProc(null);
        }

        private static bool reloadProc(object param)
        {
            var frame = Window.Current.Content as Frame;
            var type = frame.CurrentSourcePageType;
            if (frame.BackStack.Any())
            {
                type = frame.BackStack.Last().SourcePageType;
                param = frame.BackStack.Last().Parameter;
            }
            try
            {
                return frame.Navigate(type, param);
            }
            finally
            {
                frame.BackStack.Remove(frame.BackStack.Last());
            }
        }

        /// <summary>
        /// set title bar text
        /// </summary>
        /// <param name="text"></param>
        /// <remarks>
        /// (this text) - (application name)
        /// </remarks>
        public static void SetTitleText(string text)
        {
            var av = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            av.Title = text;
        }

        /// <summary>
        /// set full screen mode
        /// </summary>
        public static void SetFullScreenMode()
        {
            var av = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            av.TryEnterFullScreenMode();
        }

        /// <summary>
        /// check full screen mode or not
        /// </summary>
        /// <returns></returns>
        public static bool IsFullScreenMode()
        {
            var av = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            return av.IsFullScreenMode;
        }

        /// <summary>
        /// exit full screen mode to resizable window
        /// </summary>
        public static void ExitFullScreenMode()
        {
            var av = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            av.ExitFullScreenMode();
        }

        /// <summary>
        /// get emlement rectangle position
        /// </summary>
        /// <param name="tar"></param>
        /// <returns></returns>
        public static Windows.Foundation.Rect GetElementRect(FrameworkElement tar)
        {
            var r0 = new Windows.Foundation.Rect(0.0, 0.0, tar.ActualWidth, tar.ActualHeight);
            var ctlTransform = tar.TransformToVisual(null);
            return ctlTransform.TransformBounds(r0);
        }

        /// <summary>
        /// query child elements
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        public static IEnumerable<UIElement> DescendantsAndSelf(UIElement top)
        {
            UIElement dp = top;
            for (; ; )
            {
                if (dp is UIElement && dp is Frame == false && dp is Page == false)
                {
                    yield return dp;
                }
                if (dp is ContentControl content)
                {
                    dp = content.Content as UIElement;
                    if (dp == null)
                    {
                        yield break;
                    }
                    continue;
                }
                if (dp is UserControl page)
                {
                    dp = page.Content;
                    continue;
                }
                break;
            }

            if (dp is Panel panel)
            {
                foreach (var ue in panel.Children)
                {
                    foreach (var ret in DescendantsAndSelf(ue))
                    {
                        yield return ret;
                    }
                }
            }
        }



        /// <summary>
        /// file owner TGuiView
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static TGuiView FindView(FrameworkElement tar)
        {
            // HACK: support to find control in humburger menu
            var top = Window.Current.Content;
            var allControls = DescendantsAndSelf(top);
            var views = allControls.OfType<TGuiView>();

            return views.FirstOrDefault();
        }

        /// <summary>
        /// find control by name
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static FrameworkElement FindControl(FrameworkElement tar, string name)
        {
            var top = Window.Current.Content;
            var allControls = DescendantsAndSelf(top);
            var cos = allControls.OfType<FrameworkElement>();
            var tarview =
                from co in cos
                where co?.Name == name
                select co;
            var ret = tarview.FirstOrDefault();
            return ret;
        }


        /// <summary>
        /// find draw area controls for ZIndex concidering
        /// </summary>
        /// <param name="tar"></param>
        /// <returns></returns>
        public static IEnumerable<IDrawArea> FindDrawAreas(FrameworkElement tar)
        {
            var allControls = DescendantsAndSelf(tar);
            var views =
                from c in allControls.OfType<IDrawArea>()
                let c0 = c as DependencyObject
                where c0 != null
                orderby c0 is TGuiView ? int.MinValue : (int)c0.GetValue(Canvas.ZIndexProperty)
                select c;
            return views;
        }

        /// <summary>
        /// find a draw area control by name
        /// </summary>
        /// <param name="tar"></param>
        /// <returns></returns>
        public static IDrawArea FindDrawArea(FrameworkElement tar, string name)
        {
            var allControls = DescendantsAndSelf(tar);
            var views =
                from c in allControls.OfType<IDrawArea>()
                let c0 = c as FrameworkElement
                where c0 != null && c0.Name.Equals(name)
                select c;
            return views.FirstOrDefault();
        }

        /// <summary>
        /// check it is on design mode
        /// </summary>
        /// <returns></returns>
        public static bool IsDesignMode()
        {
            return Windows.ApplicationModel.DesignMode.DesignModeEnabled;
        }
    }
}
