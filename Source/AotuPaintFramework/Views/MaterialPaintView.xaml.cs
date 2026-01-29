using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.UI;
using AotuPaintFramework.Models;
using AotuPaintFramework.ViewModels;

namespace AotuPaintFramework.Views
{
    /// <summary>
    /// Interaction logic for MaterialPaintView.xaml
    /// </summary>
    public partial class MaterialPaintView : Window
    {
        private readonly MaterialPaintViewModel _viewModel;

        public MaterialPaintView(UIDocument uiDocument)
        {
            InitializeComponent();

            _viewModel = new MaterialPaintViewModel(uiDocument);
            DataContext = _viewModel;

            _viewModel.SetWindow(this);
        }

        private void OnParameterChanged(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBoxItem item && item.DataContext is CategoryMapping mapping)
            {
                _viewModel.ParameterChangedCommand?.Execute(mapping);
            }
        }

        private void OnFaceItemClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item && item.DataContext is PickedFaceItem faceItem)
            {
                _viewModel.FaceClickedCommand?.Execute(faceItem);
            }
        }
    }
}
