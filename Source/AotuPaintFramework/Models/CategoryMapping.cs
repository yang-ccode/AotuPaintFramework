using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AotuPaintFramework.Models
{
    /// <summary>
    /// Model representing a category to material mapping rule for automatic material painting.
    /// Used to define which materials should be applied to elements based on their category and parameter values.
    /// </summary>
    public class CategoryMapping : INotifyPropertyChanged
    {
        private bool _isChecked;
        private string? _category;
        private string? _parameter;
        private string? _parameterValue;
        private string? _material;

        public CategoryMapping()
        {
            AvailableParameterValues = new ObservableCollection<string>();
        }

        /// <summary>
        /// Gets or sets whether this mapping rule is enabled.
        /// </summary>
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Revit element category to filter by.
        /// </summary>
        public string? Category
        {
            get => _category;
            set
            {
                if (_category != value)
                {
                    _category = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the parameter name to check for filtering elements.
        /// </summary>
        public string? Parameter
        {
            get => _parameter;
            set
            {
                if (_parameter != value)
                {
                    _parameter = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the parameter value to match against.
        /// </summary>
        public string? ParameterValue
        {
            get => _parameterValue;
            set
            {
                if (_parameterValue != value)
                {
                    _parameterValue = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the material name to apply when the category and parameter conditions are met.
        /// </summary>
        public string? Material
        {
            get => _material;
            set
            {
                if (_material != value)
                {
                    _material = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the collection of available parameter values for the selected parameter.
        /// Populated dynamically when a parameter is selected.
        /// </summary>
        public ObservableCollection<string> AvailableParameterValues { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
