using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Collections;

namespace SwipeableListDemo
{
    public partial class MainPage : PhoneApplicationPage
    {
        private bool scrolling = false;
        private bool deleteItem = false;
        private bool markAsComplete = false;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            for (int i = 0; i < 100; i++)
            {
                Items.Add(string.Format("Item {0}", i));
            }
        }

        void hgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "CompressionLeft")
            {
                deleteItem = true;
            }
            else if (e.NewState.Name == "CompressionRight")
            {
                markAsComplete = true;
            }
            else if (e.NewState.Name == "NoHorizontalCompression" && !scrolling)
            {
                deleteItem = false;
                markAsComplete = false;
            }
        }

        void group_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (scrolling && deleteItem)
            {
                var item = (e.Control as ScrollViewer).DataContext.ToString();
                Items.Remove(item);
                deleteItem = false;
            }
            else if (scrolling)
            {
                if (markAsComplete)
                {
                    var sv = e.Control as ScrollViewer;
                    var sp = sv.Content as StackPanel;
                    var centralPanel = sp.Children[1] as Grid;
                    if (centralPanel.Children.Count == 1)
                    {
                        var textBlock = centralPanel.Children[0] as TextBlock;
                        var line = new Line()
                        {
                            Stroke = new SolidColorBrush(Colors.White),
                            StrokeThickness = 2,
                            X1 = 0,
                            Y1 = textBlock.ActualHeight / 2,
                            X2 = textBlock.ActualWidth,
                            Y2 = textBlock.ActualHeight / 2
                        };

                        centralPanel.Children.Add(line);
                    }
                }
                var storyboard = new Storyboard();
                var animation = new DoubleAnimation()
                {
                    Duration = TimeSpan.FromMilliseconds(300),
                    From = (e.Control as ScrollViewer).HorizontalOffset,
                    To = 150,
                    EasingFunction = new CubicEase()
                    {
                        EasingMode = EasingMode.EaseOut
                    }
                };
                Storyboard.SetTarget(animation, e.Control.Tag as ScrollViewerOffsetMediator);
                Storyboard.SetTargetProperty(animation, new PropertyPath(ScrollViewerOffsetMediator.HorizontalOffsetProperty));

                storyboard.Children.Add(animation);
                storyboard.Begin();
                //(e.Control as ScrollViewer).ScrollToHorizontalOffset(150);
            }

            scrolling = e.NewState.Name == "Scrolling";
        }

        private ObservableCollection<string> _items = new ObservableCollection<string>();
        public ObservableCollection<string> Items
        {
            get { return _items; }
        }

        private UIElement FindElementRecursive(FrameworkElement parent, Type targetType)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            UIElement returnElement = null;
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    Object element = VisualTreeHelper.GetChild(parent, i);
                    if (element.GetType() == targetType)
                    {
                        return element as UIElement;
                    }
                    else
                    {
                        returnElement = FindElementRecursive(VisualTreeHelper.GetChild(parent, i) as FrameworkElement, targetType);
                    }
                }
            }
            return returnElement;
        }
        private IEnumerable<T> FindElements<T>(FrameworkElement parent) where T : class
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    Object element = VisualTreeHelper.GetChild(parent, i);
                    if (element.GetType() == typeof(T))
                    {
                        yield return element as T;
                    }
                    else
                    {
                        foreach (var el in FindElements<T>(VisualTreeHelper.GetChild(parent, i) as FrameworkElement))
                        {
                            yield return el;
                        }
                    }
                }
            }
        }

        private VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
                return null;

            IList groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (VisualStateGroup group in groups)
                if (group.Name == name)
                    return group;

            return null;
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var sv = sender as ScrollViewer;

            if (sv != null)
            {
                //sv.ScrollToHorizontalOffset(150);
                // Visual States are always on the first child of the control template 
                FrameworkElement element = VisualTreeHelper.GetChild(sv, 0) as FrameworkElement;
                if (element != null)
                {
                    VisualStateGroup group = FindVisualState(element, "ScrollStates");
                    if (group != null)
                    {
                        group.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(group_CurrentStateChanging);
                    }
                    VisualStateGroup hgroup = FindVisualState(element, "HorizontalCompression");
                    if (hgroup != null)
                    {
                        hgroup.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(hgroup_CurrentStateChanging);
                    }
                }
            }
        }
    }
}