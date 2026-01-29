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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
