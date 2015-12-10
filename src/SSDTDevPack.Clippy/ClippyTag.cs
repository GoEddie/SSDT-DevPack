using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace SSDTDevPack.Clippy
{
    

    public class ClippyTag : IGlyphTag
    {
        private const double _glyphSize = 16.0;

        private readonly GlyphDefinition _definition;

        public TagSpan<ClippyTag> ParentTag;

        public ClippyTagger Tagger;

        public ClippyTag(GlyphDefinition definition)
        {
            _definition = definition;
            
        }

        public GlyphDefinition GetDefinition()
        {
            return _definition;
        }

        //~ClippyTag()
        //{
        //    Debug.WriteLine("Killed ClippyTag {0}", _definition.Line);
        //}

        Grid _grid;

        public UIElement GetEllipses()
        {
            var ellipse = new Ellipse();

            _grid = new Grid();
            _grid.ContextMenuOpening += ellipse_ContextMenuOpening;

            switch (_definition.Type)
            {
                case GlyphDefinitonType.Normal:

                    ellipse.Fill = Brushes.Coral;
                    ellipse.StrokeThickness = 1;
                    ellipse.Stroke = Brushes.DarkCyan;
                    ellipse.Height = _glyphSize;
                    ellipse.Width = _glyphSize;
                    _grid.Tag = this;
                    _grid.Children.Add(ellipse);
                    _grid.Children.Add(new TextBlock() { Text = _definition.Menu.Count(p => p.Type == MenuItemType.MenuItem).ToString(), Foreground = Brushes.WhiteSmoke, HorizontalAlignment = HorizontalAlignment.Center });
                    break;

                case GlyphDefinitonType.Error:

                    ellipse.Fill = Brushes.Red;
                    ellipse.StrokeThickness = 1;
                    ellipse.Stroke = Brushes.DarkCyan;
                    ellipse.Height = _glyphSize;
                    ellipse.Width = _glyphSize;
                    _grid.Children.Add(ellipse);
                    _grid.Children.Add(new TextBlock() { Text = "E", Foreground = Brushes.WhiteSmoke, HorizontalAlignment = HorizontalAlignment.Center });

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return _grid;
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public int X;
            public int Y;
        };

        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        private void ellipse_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (!(sender is Grid))
                return;

            var ellipse = sender as Grid;
            if (!(ellipse.Tag is ClippyTag))
                return;

            var tag = ellipse.Tag as ClippyTag;

            var definition = tag.GetDefinition();

            var window = new MainWindow(definition.Menu);
            window.WindowStyle = WindowStyle.None;

            var location = GetMousePosition();
            window.Left = location.X;
            window.Top = location.Y;

            window.Height = 150;
            window.Width = 300;
            window.ResizeMode = ResizeMode.NoResize;

            window.ShowInTaskbar = false;
            window.ShowActivated = true;

            window.Show();
        }

        public void ClearEllipses()
        {
            _grid.Children.Clear();
            _grid.ContextMenuOpening -= ellipse_ContextMenuOpening;
        }
    }
}