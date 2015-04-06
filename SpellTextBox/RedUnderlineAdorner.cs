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

        DependencyObject GetTopLevelControl(DependencyObject control)
        {
            DependencyObject tmp = control;
            DependencyObject parent = null;
            while ((tmp = VisualTreeHelper.GetParent(tmp)) != null)
            {
                parent = tmp;
            }
            return parent;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (box != null && box.IsSpellCheckEnabled)
            {
                foreach (var word in box.Checker.MisspelledWords)
                {
                    Rect rectangleBounds = new Rect();
                    rectangleBounds = box.TransformToVisual(GetTopLevelControl(box) as Visual).TransformBounds(LayoutInformation.GetLayoutSlot(box));

                    Rect startRect = box.GetRectFromCharacterIndex(word.Index);
                    Rect endRect = box.GetRectFromCharacterIndex(word.Index + word.Length);

                    Rect startRectM = box.GetRectFromCharacterIndex(word.Index);
                    Rect endRectM = box.GetRectFromCharacterIndex(word.Index + word.Length);

                    startRectM.X += rectangleBounds.X;
                    startRectM.Y += rectangleBounds.Y;
                    endRectM.X += rectangleBounds.X;
                    endRectM.Y += rectangleBounds.Y;

                    if (rectangleBounds.Contains(startRectM) && rectangleBounds.Contains(endRectM))
                        drawingContext.DrawLine(pen, startRect.BottomLeft, endRect.BottomRight);
                }
            }
        }

        private static Pen CreateErrorPen()
        {
            double size = 4.0;

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(0.0, 0.0), false, false);
                context.PolyLineTo(new[] {
                    new Point(size * 0.25, size * 0.25),
                    new Point(size * 0.5, 0.0),
                    new Point(size * 0.75, size * 0.25),
                    new Point(size, 0.0)
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
                Viewport = new Rect(0.0, size * 0.33, size * 3.0, size),
                ViewportUnits = BrushMappingMode.Absolute
            };

            var pen = new Pen(brush, size);
            pen.Freeze();

            return pen;
        }
    }
}
