// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Aurora.Shared.Controls
{
    public sealed class LoadingDot : Control
    {
        private Ellipse el0;
        private Storyboard loading;

        public LoadingDot()
        {
            this.DefaultStyleKey = typeof(LoadingDot);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            el0 = GetTemplateChild("Ellipse0") as Ellipse;
            el1 = GetTemplateChild("Ellipse1") as Ellipse;
            el2 = GetTemplateChild("Ellipse2") as Ellipse;
            trans0 = GetTemplateChild("Trans0") as CompositeTransform;
            trans1 = GetTemplateChild("Trans1") as CompositeTransform;
            trans2 = GetTemplateChild("Trans2") as CompositeTransform;
            loading = GetTemplateChild("Loading") as Storyboard;
            finish = GetTemplateChild("Finish") as Storyboard;
            start = GetTemplateChild("Start") as Storyboard;
            root = GetTemplateChild("Root") as StackPanel;
            rootHeightOut = GetTemplateChild("RootHeightOut") as DiscreteDoubleKeyFrame;
            root.Loaded += El0_Loaded;
        }

        private void El0_Loaded(object sender, RoutedEventArgs e)
        {
            loading.Completed += Loading_Completed;
            finish.Completed += Finish_Completed;
            start.Completed += Start_Completed;
            var m = el0.Height / 3;
            var t = new Thickness(m, 0, m, 0);
            el0.Margin = t;
            el1.Margin = t;
            el2.Margin = t;
            rootHeightOut.Value = el0.Height + 64d;
            if (IsActive)
            {
                start.Begin();
            }
        }

        private void Start_Completed(object sender, object e)
        {
            if (IsActive)
            {
                loading.RepeatBehavior = RepeatBehavior.Forever;
                loading.Begin();
            }
            else
            {
                loading.RepeatBehavior = new RepeatBehavior(0);
                finish.Begin();
            }
        }

        private void Finish_Completed(object sender, object e)
        {
            trans0.ScaleX = 0;
            trans1.ScaleX = 0;
            trans2.ScaleX = 0;
            trans2.ScaleY = 0;
            trans0.ScaleY = 0;
            trans1.ScaleY = 0;
            DotFinish?.Invoke(this, new EventArgs());
        }

        private void Loading_Completed(object sender, object e)
        {
            finish.Begin();
        }

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(LoadingDot), new PropertyMetadata(false, OnIsActiveChanged));
        private Storyboard finish;

        private static async void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dot = d as LoadingDot;
            if (dot.loading == null)
                return;
            if ((bool)e.NewValue)
            {
                if ((bool)e.OldValue)
                {
                    return;
                }
                else
                {
                    dot.start.Begin();
                }
            }
            else
            {
                if ((bool)e.OldValue)
                {
                    if (dot.loading.RepeatBehavior == RepeatBehavior.Forever)
                    {
                        var l = dot.loading.GetCurrentTime();
                        await Task.Delay(l.Seconds < 2.88 ? (TimeSpan.FromSeconds(2.88) - l) : TimeSpan.FromMilliseconds(0));
                        dot.loading.RepeatBehavior = new RepeatBehavior(0);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        public double DotHeight
        {
            get { return (double)GetValue(DotHeightProperty); }
            set { SetValue(DotHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DotHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DotHeightProperty =
            DependencyProperty.Register("DotHeight", typeof(double), typeof(LoadingDot), new PropertyMetadata(32d));

        public double DotWidth
        {
            get { return (double)GetValue(DotWidthProperty); }
            set { SetValue(DotWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DotHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DotWidthProperty =
            DependencyProperty.Register("DotWidth", typeof(double), typeof(LoadingDot), new PropertyMetadata(32d));
        private StackPanel root;
        private DiscreteDoubleKeyFrame rootHeightOut;
        private Ellipse el1;
        private Ellipse el2;
        private Storyboard start;
        private CompositeTransform trans0;
        private CompositeTransform trans1;
        private CompositeTransform trans2;

        public event EventHandler DotFinish;
    }
}

