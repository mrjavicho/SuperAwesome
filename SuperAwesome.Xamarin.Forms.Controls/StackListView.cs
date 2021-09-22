using System;
using System.Collections;
using System.Windows.Input;
using Xamarin.Forms;

namespace SuperAwesome.Xamarin.Forms.Controls
{
    public class StackListView : StackLayout
    {
        public static readonly BindableProperty ItemSelectedCommandProperty =
            BindableProperty.Create(nameof(ItemSelectedCommand), typeof(ICommand), typeof(StackListView),
                default(ICommand), BindingMode.OneWay);

        public ICommand ItemSelectedCommand
        {
            get { return (ICommand)this.GetValue(ItemSelectedCommandProperty); }
            set { this.SetValue(ItemSelectedCommandProperty, value); }
        }

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(StackListView), null,
                BindingMode.TwoWay);

        public object SelectedItem
        {
            get { return (object)this.GetValue(SelectedItemProperty); }
            set { this.SetValue(SelectedItemProperty, value); }
        }

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty
            .Create(nameof(ItemsSource), typeof(IList), typeof(StackListView), default(IList), BindingMode.OneWay,
                coerceValue: HandleItemsSourceCoerceValue);

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        static object HandleItemsSourceCoerceValue(BindableObject bindable, object value)
        {
            var instance = (StackListView)bindable;
            var items = (IList)value;
            instance?.CreateItemsFromSource(instance.ItemTemplate, items);
            return value;
        }

        public DataTemplate ItemTemplate { get; set; }
        
        public event EventHandler<object> ItemSelected;
        
        public StackListView()
        {
            
        }
        

        void CreateItemsFromSource(DataTemplate itemTemplate, IList itemsSource)
        {
            if (itemTemplate == null || itemsSource == null)
                return;
            this.Children?.Clear();
            double itemsHeightRequestSum = 0;
            double itemsWidthRequestSum = 0;

            foreach (var newItem in itemsSource)
            {
                DataTemplate itemDataTemplate;
                if (itemTemplate is DataTemplateSelector templateSelector)
                {
                    itemDataTemplate = templateSelector.SelectTemplate(newItem, this);
                }
                else
                {
                    itemDataTemplate = itemTemplate;
                }

                var creation = itemDataTemplate.CreateContent();
                View view;
                if (creation is ViewCell viewCell)
                {
                    viewCell.BindingContext = newItem;
                    view = viewCell.View;
                }
                else
                {
                    view = (View)creation;
                }

                var tapGesture = new TapGestureRecognizer();
                tapGesture.NumberOfTapsRequired = 1;
                tapGesture.Tapped += (sender, e) =>
                {
                    SelectedItem = newItem;
                    ItemSelectedCommand?.Execute(newItem);
                    ItemSelected?.Invoke(this, newItem);
                };
                view.GestureRecognizers.Add(tapGesture);

                if (view is BindableObject bindableObject)
                {
                    bindableObject.BindingContext = newItem;
                }
                itemsHeightRequestSum += view.Height;
                itemsWidthRequestSum += view.Width;
                var measure = view.Measure(view.Width, view.Height);
                this.Children.Add(view);
            }

            if (Orientation == StackOrientation.Horizontal)
            {
                if (itemsWidthRequestSum > 0)
                    this.WidthRequest = itemsWidthRequestSum + (Spacing * itemsSource.Count);
            }
            else
            {
                if (itemsHeightRequestSum > 0)
                    this.HeightRequest = itemsHeightRequestSum + (Spacing * itemsSource.Count);
            }
        }
    }
}