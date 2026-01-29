using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AotuPaintFramework.Models
{
    public class CategoryMapping : INotifyPropertyChanged
    {
        private bool _isChecked;
        private string? _category;
        private string? _parameter;
        private string? _parameterValue;
        private string? _material;

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
