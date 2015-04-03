using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace SpellTextBox
{
    public class RedUnderlineAdorner : Adorner
    {
        public RedUnderlineAdorner(SpellTextBox textbox) : base(textbox)
        {
            textbox.TextChanged += delegate
            {
                SignalInvalidate();
            };

            textbox.SizeChanged += delegate
            {
                SignalInvalidate();
            };

            textbox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(
                delegate 
                {
                    SignalInvalidate();
                }));
        }

        SpellTextBox box;
        Pen pen = CreateErrorPen();

        void SignalInvalidate()
        {
            box = (SpellTextBox)this.AdornedElement;
            box.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)InvalidateVisual);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (box != null)
            {
                foreach (var word in box.Checker.MisspelledWords)
                {
                    Rect startRect = box.GetRectFromCharacterIndex(word.Index);
                    Rect endRect = box.GetRectFromCharacterIndex(word.Index + word.Length);

                    Rect rectangleBounds = new Rect();
                    rectangleBounds = box.TransformToVisual(VisualTreeHelper.GetParent(box) as Visual).TransformBounds(LayoutInformation.GetLayoutSlot(box));
                    if (rectangleBounds.Contains(startRect) && rectangleBounds.Contains(endRect))
                        drawingContext.DrawLine(pen, startRect.BottomLeft, endRect.BottomRight);
                }
            }
        }

        private static Pen CreateErrorPen()
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(0.0, 0.0), false, false);
                context.PolyLineTo(new[] {
                    new Point(0.75, 0.75),
                    new Point(1.5, 0.0),
                    new Point(2.25, 0.75),
                    new Point(3.0, 0.0)
                }, true, true);
            }

            var brushPattern = new GeometryDrawing
            {
                Pen = new Pen(Brushes.Red, 0.2),
                Geometry = geometry
            };

            var brush = new DrawingBrush(brushPattern)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0.0, 1.5, 9.0, 3.0),
                ViewportUnits = BrushMappingMode.Absolute
            };

            var pen = new Pen(brush, 3.0);
            pen.Freeze();

            return pen;
        }
    }
}
