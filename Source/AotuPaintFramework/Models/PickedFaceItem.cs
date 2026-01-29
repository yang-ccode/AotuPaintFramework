using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AotuPaintFramework.Models
{
    /// <summary>
    /// Model for picked faces in the interface list
    /// Note: Face and Plane properties are typed as object to avoid Revit API dependency at compile time
    /// At runtime, they should be cast to Autodesk.Revit.DB.Face and Autodesk.Revit.DB.Plane respectively
    /// </summary>
    public class PickedFaceItem : INotifyPropertyChanged
    {
        private bool _isChecked;
        private string? _faceName;
        private string? _categoryName;
        private int _elementId;
        private object? _face;
        private object? _plane;

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

        public string? FaceName
        {
            get => _faceName;
            set
            {
                if (_faceName != value)
                {
                    _faceName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? CategoryName
        {
            get => _categoryName;
            set
            {
                if (_categoryName != value)
                {
                    _categoryName = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ElementId
        {
            get => _elementId;
            set
            {
                if (_elementId != value)
                {
                    _elementId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The picked face (Autodesk.Revit.DB.Face at runtime)
        /// </summary>
        public object? Face
        {
            get => _face;
            set
            {
                if (_face != value)
                {
                    _face = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The plane of the face (Autodesk.Revit.DB.Plane at runtime)
        /// </summary>
        public object? Plane
        {
            get => _plane;
            set
            {
                if (_plane != value)
                {
                    _plane = value;
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
