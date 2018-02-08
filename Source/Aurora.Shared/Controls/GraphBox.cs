// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Shared.Extensions;
using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Aurora.Shared.Controls
{
    public sealed class GraphBox : Control
    {
        private const double GRAPH_ACTUALHEIGHT = 512 * 8 / 10;
        private const double GRAPH_ACTUALSTART = 512 * 9 / 10;

        public GraphBox()
        {
            this.DefaultStyleKey = typeof(GraphBox);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PathFigure0 = GetTemplateChild("PathFigure0") as PathFigure;
            this.SubPath = GetTemplateChild("SubPath") as Path;
            title = GetTemplateChild("Title") as TextBlock;
            Y0 = GetTemplateChild("Y0") as TextBlock;
            Y1 = GetTemplateChild("Y1") as TextBlock;
            Y2 = GetTemplateChild("Y2") as TextBlock;
            Y3 = GetTemplateChild("Y3") as TextBlock;
            Y4 = GetTemplateChild("Y4") as TextBlock;
            X0 = GetTemplateChild("X0") as TextBlock;
            X1 = GetTemplateChild("X1") as TextBlock;
            X2 = GetTemplateChild("X2") as TextBlock;
            X3 = GetTemplateChild("X3") as TextBlock;
            X4 = GetTemplateChild("X4") as TextBlock;

            if (Values0 == null || Values0.Count < 1)
            {
                return;
            }
            double bottom;
            double top;
            double labelStep;
            if (Values1 == null || Values1.Count < 1)
            {
                SubPath.Visibility = Visibility.Collapsed;
                var max = Values0.Max();
                var min = Values0.Min();
                if (max == min)
                {
                    min -= min * 0.1;
                    max += max * 0.1;
                }

                bottom = min - (max - min) * 0.1;
                top = max + (max - min) * 0.1;
                bottom = bottom < Minimum ? Minimum : bottom;
                top = top > Maximum ? Maximum : top;
                var center = (max + min) / 2;
                labelStep = (top - bottom) / 4;
            }
            else
            {
                SubPath.Visibility = Visibility.Visible;
                var max = Math.Max(Values0.Max(), Values1.Max());
                var min = Math.Min(Values0.Min(), Values1.Min());
                if (max == min)
                {
                    min -= min * 0.1;
                    max += max * 0.1;
                }
                bottom = min - (max - min) * 0.1;
                top = max + (max - min) * 0.1;
                bottom = bottom < Minimum ? Minimum : bottom;
                top = top > Maximum ? Maximum : top;
                var center = (max + min) / 2;
                labelStep = (top - bottom) / 4;
            }
            var pathFigure1 = ((((SubPath.Data as GeometryGroup).Children[0] as PathGeometry).Figures as PathFigureCollection)[0] as PathFigure);

            PathFigure0.Segments.Clear();
            pathFigure1.Segments.Clear();

            title.Text = Title;
            Y0.Text = bottom.ToString("0.0") + FormatDecoration;
            Y1.Text = (bottom + labelStep).ToString("0.0") + FormatDecoration;
            Y2.Text = (bottom + 2 * labelStep).ToString("0.0") + FormatDecoration;
            Y3.Text = (bottom + 3 * labelStep).ToString("0.0") + FormatDecoration;
            Y4.Text = (bottom + 4 * labelStep).ToString("0.0") + FormatDecoration;

            if (!XText.IsNullorEmpty())
            {
                var xTexts = XText.Split(',');
                X0.Text = xTexts[0];
                X1.Text = xTexts[1];
                X2.Text = xTexts[2];
                X3.Text = xTexts[3];
                X4.Text = xTexts[4];
            }

            var actualMin = bottom;
            var actualMax = bottom + 4 * labelStep;

            var length = actualMax - actualMin;
            var step = 512 / (Values0.Count + 1);
            var actaulStart = GRAPH_ACTUALSTART - ((Values0[0] - actualMin) / length) * GRAPH_ACTUALHEIGHT;
            PathFigure0.StartPoint = new Windows.Foundation.Point(step, actaulStart);
            for (int i = 0; i < Values0.Count; i++)
            {
                var actaulY = GRAPH_ACTUALSTART - ((Values0[i] - actualMin) / length) * GRAPH_ACTUALHEIGHT;
                PathFigure0.Segments.Add(new LineSegment
                {
                    Point = new Windows.Foundation.Point(step * (i + 1), actaulY)
                });
            }
            if (Values1 != null && Values1.Count > 0)
            {
                var step1 = 512 / (Values1.Count + 1);
                var actaulStart1 = GRAPH_ACTUALSTART - ((Values1[0] - actualMin) / length) * GRAPH_ACTUALHEIGHT;
                pathFigure1.StartPoint = new Windows.Foundation.Point(step1, actaulStart1);
                for (int i = 0; i < Values1.Count; i++)
                {
                    var actaulY = GRAPH_ACTUALSTART - ((Values1[i] - actualMin) / length) * GRAPH_ACTUALHEIGHT;
                    pathFigure1.Segments.Add(new LineSegment
                    {
                        Point = new Windows.Foundation.Point(step1 * (i + 1), actaulY)
                    });
                }
            }
        }

        public DoubleCollection Values0
        {
            get { return (DoubleCollection)GetValue(Values0Property); }
            set { SetValue(Values0Property, value); }
        }
        // Using a DependencyProperty as the backing store for Values.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Values0Property =
            DependencyProperty.Register("Values0", typeof(DoubleCollection), typeof(GraphBox), new PropertyMetadata(new DoubleCollection(), OnValuesChanged));

        public DoubleCollection Values1
        {
            get { return (DoubleCollection)GetValue(Values1Property); }
            set { SetValue(Values1Property, value); }
        }
        // Using a DependencyProperty as the backing store for Values.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Values1Property =
            DependencyProperty.Register("Values1", typeof(DoubleCollection), typeof(GraphBox), new PropertyMetadata(new DoubleCollection(), OnValuesChanged));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(GraphBox), new PropertyMetadata(string.Empty));

        public Brush Stroke0
        {
            get { return (Brush)GetValue(Stroke0Property); }
            set { SetValue(Stroke0Property, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Stroke0Property =
            DependencyProperty.Register("Stroke0", typeof(Brush), typeof(GraphBox), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0))));

        public Brush Stroke1
        {
            get { return (Brush)GetValue(Stroke1Property); }
            set { SetValue(Stroke1Property, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Stroke1Property =
            DependencyProperty.Register("Stroke1", typeof(Brush), typeof(GraphBox), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0))));



        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set
            {
                if (value > Maximum)
                {
                    throw new ArgumentException("Minimum is higher than Maximum.");
                }
                SetValue(MinimumProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(GraphBox), new PropertyMetadata(0d));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set
            {
                if (value < Minimum)
                {
                    throw new ArgumentException("Maximum is lower than Minimum.");
                }
                SetValue(MaximumProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(GraphBox), new PropertyMetadata(100d));

        public Brush SeparatorFill
        {
            get { return (Brush)GetValue(SeparatorFillProperty); }
            set { SetValue(SeparatorFillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SeparatorFillProperty =
            DependencyProperty.Register("SeparatorFill", typeof(Brush), typeof(GraphBox), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 0, 0, 0))));

        public string FormatDecoration
        {
            get { return (string)GetValue(FormatDecorationProperty); }
            set { SetValue(FormatDecorationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FormatDecorationProperty =
            DependencyProperty.Register("FormateDecoration", typeof(string), typeof(GraphBox), new PropertyMetadata(string.Empty, OnDecorationChanged));



        public string XText
        {
            get { return (string)GetValue(XTextProperty); }
            set { SetValue(XTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for XText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XTextProperty =
            DependencyProperty.Register("XText", typeof(string), typeof(GraphBox), new PropertyMetadata("", OnValuesChanged));



        private static void OnDecorationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as GraphBox;
            if (box.Y0 == null)
            {
                return;
            }
            box.Y0.Text = (box.Y0.Text.Trim((e.OldValue as string).ToCharArray())) + (e.NewValue as string);
            box.Y1.Text = (box.Y1.Text.Trim((e.OldValue as string).ToCharArray())) + (e.NewValue as string);
            box.Y2.Text = (box.Y2.Text.Trim((e.OldValue as string).ToCharArray())) + (e.NewValue as string);
            box.Y3.Text = (box.Y3.Text.Trim((e.OldValue as string).ToCharArray())) + (e.NewValue as string);
            box.Y4.Text = (box.Y4.Text.Trim((e.OldValue as string).ToCharArray())) + (e.NewValue as string);
        }

        private PathFigure PathFigure0;
        private TextBlock title;
        private TextBlock Y0;
        private TextBlock Y1;
        private TextBlock Y2;
        private TextBlock Y3;
        private TextBlock Y4;
        private TextBlock X0;
        private TextBlock X1;
        private TextBlock X2;
        private TextBlock X3;
        private TextBlock X4;
        private Path SubPath;

        private static void OnValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as GraphBox;
            if (box.PathFigure0 == null)
            {
                return;
            }

            if (box.Values0 == null || box.Values0.Count < 1)
            {
                return;
            }
            double bottom;
            double top;
            double labelStep;
            if (box.Values1 == null || box.Values1.Count < 1)
            {
                box.SubPath.Visibility = Visibility.Collapsed;
                var max = box.Values0.Max();
                var min = box.Values0.Min();
                if (max == min)
                {
                    min -= min * 0.1;
                    max += max * 0.1;
                }

                bottom = min - (max - min) * 0.1;
                top = max + (max - min) * 0.1;
                bottom = bottom < box.Minimum ? box.Minimum : bottom;
                top = top > box.Maximum ? box.Maximum : top;
                var center = (max + min) / 2;
                labelStep = (top - bottom) / 4;
            }
            else
            {
                box.SubPath.Visibility = Visibility.Visible;
                var max = Math.Max(box.Values0.Max(), box.Values1.Max());
                var min = Math.Min(box.Values0.Min(), box.Values1.Min());
                if (max == min)
                {
                    min -= min * 0.1;
                    max += max * 0.1;
                }
                bottom = min - (max - min) * 0.1;
                top = max + (max - min) * 0.1;
                bottom = bottom < box.Minimum ? box.Minimum : bottom;
                top = top > box.Maximum ? box.Maximum : top;
                var center = (max + min) / 2;
                labelStep = (top - bottom) / 4;
            }
            var pathFigure1 = ((((box.SubPath.Data as GeometryGroup).Children[0] as PathGeometry).Figures as PathFigureCollection)[0] as PathFigure);

            box.PathFigure0.Segments.Clear();
            pathFigure1.Segments.Clear();

            box.title.Text = box.Title;
            box.Y0.Text = bottom.ToString("0.0") + box.FormatDecoration;
            box.Y1.Text = (bottom + labelStep).ToString("0.0") + box.FormatDecoration;
            box.Y2.Text = (bottom + 2 * labelStep).ToString("0.0") + box.FormatDecoration;
            box.Y3.Text = (bottom + 3 * labelStep).ToString("0.0") + box.FormatDecoration;
            box.Y4.Text = (bottom + 4 * labelStep).ToString("0.0") + box.FormatDecoration;

            if (!box.XText.IsNullorEmpty())
            {
                var xTexts = box.XText.Split(',');
                box.X0.Text = xTexts[0];
                box.X1.Text = xTexts[1];
                box.X2.Text = xTexts[2];
                box.X3.Text = xTexts[3];
                box.X4.Text = xTexts[4];
            }

            var actualMin = bottom;
            var actualMax = bottom + 4 * labelStep;

            var length = actualMax - actualMin;
            var step = 512 / (box.Values0.Count + 1);
            var actaulStart = GRAPH_ACTUALSTART - ((box.Values0[0] - actualMin) / length) * GRAPH_ACTUALHEIGHT;
            box.PathFigure0.StartPoint = new Windows.Foundation.Point(step, actaulStart);

            for (int i = 0; i < box.Values0.Count; i++)
            {
                var actaulY = GRAPH_ACTUALSTART - ((box.Values0[i] - actualMin) / length) * GRAPH_ACTUALHEIGHT;
                box.PathFigure0.Segments.Add(new LineSegment
                {
                    Point = new Windows.Foundation.Point(step * (i + 1), actaulY)
                });
            }
            if (box.Values1 != null && box.Values1.Count > 0)
            {
                var step1 = 512 / (box.Values1.Count + 1);
                var actaulStart1 = GRAPH_ACTUALSTART - ((box.Values1[0] - actualMin) / length) * GRAPH_ACTUALHEIGHT;
                pathFigure1.StartPoint = new Windows.Foundation.Point(step1, actaulStart1);
                for (int i = 0; i < box.Values1.Count; i++)
                {
                    var actaulY = GRAPH_ACTUALSTART - ((box.Values1[i] - actualMin) / length) * GRAPH_ACTUALHEIGHT;
                    pathFigure1.Segments.Add(new LineSegment
                    {
                        Point = new Windows.Foundation.Point(step1 * (i + 1), actaulY)
                    });
                }
            }
        }
    }
}
