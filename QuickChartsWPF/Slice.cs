﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AmCharts.Windows.QuickCharts
{
    public class Slice : Control, ILegendItem
    {
        public Slice()
        {
            this.DefaultStyleKey = typeof(Slice);
        }

        /// <summary>
        /// Identifies <see cref="Title"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(Slice),
            new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets the title of the slice.
        /// This is a dependency property.
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Identifies <see cref="Brush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            "Brush", typeof(Brush), typeof(Slice),
            new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets brush for the slice.
        /// This is a dependency property.
        /// </summary>
        public Brush Brush
        {
            get { return (Brush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }

        private Path _sliceVisual;

        public override void OnApplyTemplate()
        {
            _sliceVisual = (Path)TreeHelper.TemplateFindName("PART_SliceVisual", this);

            RenderSlice();
        }

        private double _radius = 0;
        private double _percentage = 0;

        public void SetDimensions(double radius, double percentage)
        {
            _radius = radius;
            _percentage = percentage;

            RenderSlice();
        }

        public void RenderSlice()
        {
            if (_sliceVisual != null)
            {
                PathGeometry geometry = new PathGeometry();
                PathFigure figure = new PathFigure();
                geometry.Figures.Add(figure);
                _sliceVisual.Data = geometry;

                _sliceVisual.Fill = Brush;

                double endAngleRad = _percentage * 360 * Math.PI / 180;
                Point endPoint = new Point(_radius * Math.Cos(endAngleRad), _radius * Math.Sin(endAngleRad));

                figure.Segments.Add(new LineSegment() { Point = new Point(_radius, 0) });
                figure.Segments.Add(new ArcSegment()
                {
                    Size = new Size(_radius, _radius),
                    Point = endPoint,
                    SweepDirection = SweepDirection.Clockwise,
                    IsLargeArc = _percentage > 0.5
                });
                figure.Segments.Add(new LineSegment() { Point = new Point(0, 0) });
            }
        }

    }
}