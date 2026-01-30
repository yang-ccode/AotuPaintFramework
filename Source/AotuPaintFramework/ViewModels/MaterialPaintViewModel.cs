using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using AotuPaintFramework.Models;
using AotuPaintFramework.Utils;

namespace AotuPaintFramework.ViewModels
{
    /// <summary>
    /// ViewModel for the Material Paint window.
    /// Handles element selection, category mapping, face picking, and paint operations.
    /// </summary>
    public class MaterialPaintViewModel : INotifyPropertyChanged
    {
        private readonly UIDocument _uiDocument;
        private Window? _window;
        private List<Element> _selectedElements;
        private bool _isSideFaceChecked;
        private bool _isBottomFaceChecked;
        private bool _isInterfacesChecked;
        private bool _isInterfacesSectionVisible;

        public MaterialPaintViewModel(UIDocument uiDocument)
        {
            // Validate parameter before entering try-catch to avoid double-logging
            if (uiDocument == null)
            {
                Logger.Error(new ArgumentNullException(nameof(uiDocument)), "UIDocument is null in MaterialPaintViewModel constructor");
                throw new ArgumentNullException(nameof(uiDocument));
            }

            try
            {
                _uiDocument = uiDocument;
                _selectedElements = new List<Element>();

                CategoryMappings = new ObservableCollection<CategoryMapping>();
                PickedFaces = new ObservableCollection<PickedFaceItem>();
                AvailableParameters = new ObservableCollection<string>();
                AvailableMaterials = new ObservableCollection<string>();

                Logger.Info("Initializing MaterialPaintViewModel");
                InitializeCommands();
                LoadAvailableParametersAndMaterials();
                Logger.Info("MaterialPaintViewModel initialized successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in MaterialPaintViewModel constructor");
                throw;
            }
        }

        #region Properties

        public UIDocument UIDocument => _uiDocument;

        public ObservableCollection<CategoryMapping> CategoryMappings { get; }

        public ObservableCollection<PickedFaceItem> PickedFaces { get; }

        public ObservableCollection<string> AvailableParameters { get; }

        public ObservableCollection<string> AvailableMaterials { get; }

        public List<Element> SelectedElements
        {
            get => _selectedElements;
            private set
            {
                _selectedElements = value;
                OnPropertyChanged();
            }
        }

        public bool IsSideFaceChecked
        {
            get => _isSideFaceChecked;
            set
            {
                if (_isSideFaceChecked != value)
                {
                    _isSideFaceChecked = value;
                    OnPropertyChanged();

                    if (value)
                    {
                        IsInterfacesChecked = false;
                    }
                }
            }
        }

        public bool IsBottomFaceChecked
        {
            get => _isBottomFaceChecked;
            set
            {
                if (_isBottomFaceChecked != value)
                {
                    _isBottomFaceChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsInterfacesChecked
        {
            get => _isInterfacesChecked;
            set
            {
                if (_isInterfacesChecked != value)
                {
                    _isInterfacesChecked = value;
                    OnPropertyChanged();

                    if (value)
                    {
                        IsSideFaceChecked = false;
                    }

                    IsInterfacesSectionVisible = value;
                }
            }
        }

        public bool IsInterfacesSectionVisible
        {
            get => _isInterfacesSectionVisible;
            private set
            {
                if (_isInterfacesSectionVisible != value)
                {
                    _isInterfacesSectionVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SelectedFacesCount => PickedFaces.Count(f => f.IsChecked);

        #endregion

        #region Commands

        public RelayCommand SelectElementCommand { get; private set; } = null!;
        public RelayCommand CheckAllCommand { get; private set; } = null!;
        public RelayCommand UncheckAllCommand { get; private set; } = null!;
        public RelayCommand PickFacesCommand { get; private set; } = null!;
        public RelayCommand ClearAllFacesCommand { get; private set; } = null!;
        public RelayCommand<CategoryMapping> ParameterChangedCommand { get; private set; } = null!;
        public RelayCommand<PickedFaceItem> FaceClickedCommand { get; private set; } = null!;
        public RelayCommand PaintCommand { get; private set; } = null!;
        public RelayCommand RemovePaintCommand { get; private set; } = null!;
        public RelayCommand CloseCommand { get; private set; } = null!;

        private void InitializeCommands()
        {
            SelectElementCommand = new RelayCommand(ExecuteSelectElement);
            CheckAllCommand = new RelayCommand(ExecuteCheckAll);
            UncheckAllCommand = new RelayCommand(ExecuteUncheckAll);
            PickFacesCommand = new RelayCommand(ExecutePickFaces);
            ClearAllFacesCommand = new RelayCommand(ExecuteClearAllFaces);
            ParameterChangedCommand = new RelayCommand<CategoryMapping>(ExecuteParameterChanged);
            FaceClickedCommand = new RelayCommand<PickedFaceItem>(ExecuteFaceClicked);
            PaintCommand = new RelayCommand(ExecutePaint);
            RemovePaintCommand = new RelayCommand(ExecuteRemovePaint);
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        #endregion

        #region Command Implementations

        private void ExecuteSelectElement()
        {
            try
            {
                Logger.Info("ExecuteSelectElement started");
                
                var selection = _uiDocument.Selection;
                var references = selection.PickObjects(ObjectType.Element, "Select elements to paint");

                var doc = _uiDocument.Document;
                var elements = references.Select(r => doc.GetElement(r)).Where(e => e != null).ToList();

                if (elements.Count == 0)
                {
                    Logger.Info("No elements selected");
                    return;
                }

                SelectedElements = elements;
                Logger.Info($"Selected {elements.Count} elements");

                // Group elements by category
                var categoryGroups = elements.GroupBy(e => e.Category?.Name ?? "Uncategorized");

                CategoryMappings.Clear();
                foreach (var group in categoryGroups)
                {
                    CategoryMappings.Add(new CategoryMapping
                    {
                        IsChecked = true,
                        Category = group.Key
                    });
                }

                // Load saved configuration
                LoadSavedConfiguration();
                
                Logger.Info("ExecuteSelectElement completed successfully");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                Logger.Info("Element selection cancelled by user");
                // User cancelled selection
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in ExecuteSelectElement");
                MessageBox.Show($"Error selecting elements: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCheckAll()
        {
            foreach (var mapping in CategoryMappings)
            {
                mapping.IsChecked = true;
            }
        }

        private void ExecuteUncheckAll()
        {
            foreach (var mapping in CategoryMappings)
            {
                mapping.IsChecked = false;
            }
        }

        private void ExecutePickFaces()
        {
            try
            {
                Logger.Info("ExecutePickFaces started");
                
                MessageBox.Show("Pick faces for interfaces. Press ESC or Finish when done.", 
                    "Pick Faces", MessageBoxButton.OK, MessageBoxImage.Information);

                if (_window != null)
                {
                    _window.Hide();
                }

                var selection = _uiDocument.Selection;
                var doc = _uiDocument.Document;
                
                if (doc == null)
                {
                    Logger.Warning("Document is null in ExecutePickFaces");
                    return;
                }

                int faceCount = 0;
                while (true)
                {
                    try
                    {
                        var reference = selection.PickObject(ObjectType.Face, "Pick a face (ESC to finish)");
                        var element = doc.GetElement(reference.ElementId);
                        
                        if (element == null)
                        {
                            Logger.Warning("Element is null for picked face");
                            continue;
                        }
                        
                        var face = element.GetGeometryObjectFromReference(reference) as Face;

                        if (face != null && face is PlanarFace planarFace)
                        {
                            var plane = Plane.CreateByNormalAndOrigin(planarFace.FaceNormal, planarFace.Origin);
                            
                            var pickedItem = new PickedFaceItem
                            {
                                IsChecked = true,
                                FaceName = $"Face {PickedFaces.Count + 1}",
                                CategoryName = element.Category?.Name ?? "Uncategorized",
                                ElementId = (int)element.Id.Value,
                                Face = face,
                                Plane = plane
                            };

                            PickedFaces.Add(pickedItem);
                            faceCount++;
                            OnPropertyChanged(nameof(SelectedFacesCount));
                        }
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        Logger.Info($"Face picking cancelled by user. Picked {faceCount} faces");
                        break;
                    }
                }

                if (_window != null)
                {
                    _window.Show();
                }
                
                Logger.Info($"ExecutePickFaces completed. Total faces picked: {faceCount}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in ExecutePickFaces");
                
                if (_window != null)
                {
                    _window.Show();
                }
                MessageBox.Show($"Error picking faces: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteClearAllFaces()
        {
            PickedFaces.Clear();
            OnPropertyChanged(nameof(SelectedFacesCount));
        }

        private void ExecuteParameterChanged(CategoryMapping? mapping)
        {
            if (mapping == null || string.IsNullOrEmpty(mapping.Parameter))
                return;

            // Get unique parameter values from selected elements
            var doc = _uiDocument.Document;
            var parameterValues = new HashSet<string>();

            foreach (var element in SelectedElements)
            {
                if (element.Category?.Name != mapping.Category)
                    continue;

                var parameter = element.LookupParameter(mapping.Parameter);
                if (parameter != null && parameter.HasValue)
                {
                    var value = parameter.AsValueString() ?? parameter.AsString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        parameterValues.Add(value);
                    }
                }
            }

            // Update available values (this would typically update a dropdown in the UI)
            // For now, we'll just clear the parameter value to let user select a new one
            if (parameterValues.Count > 0)
            {
                mapping.ParameterValue = null;
            }
        }

        private void ExecuteFaceClicked(PickedFaceItem? faceItem)
        {
            if (faceItem == null)
                return;

            try
            {
                var doc = _uiDocument.Document;
                var elementId = new ElementId((long)faceItem.ElementId);
                var element = doc.GetElement(elementId);

                if (element != null)
                {
                    var selection = _uiDocument.Selection;
                    selection.SetElementIds(new List<ElementId> { elementId });
                    _uiDocument.ShowElements(elementId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error highlighting element: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecutePaint()
        {
            try
            {
                Logger.Info("ExecutePaint started");
                
                if (!ValidatePaintOperation())
                {
                    Logger.Info("Paint operation validation failed");
                    return;
                }

                var result = MessageBox.Show(
                    "Do you want to join adjacent elements with the same material?",
                    "Join Confirmation",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    Logger.Info("Paint operation cancelled by user");
                    return;
                }

                bool joinElements = result == MessageBoxResult.Yes;
                Logger.Info($"Paint operation confirmed. Join elements: {joinElements}");

                var doc = _uiDocument.Document;
                if (doc == null)
                {
                    Logger.Warning("Document is null in ExecutePaint");
                    return;
                }

                using (var transaction = new Transaction(doc, "Paint Materials"))
                {
                    transaction.Start();

                    foreach (var mapping in CategoryMappings.Where(m => m.IsChecked))
                    {
                        if (string.IsNullOrEmpty(mapping.Material))
                            continue;

                        var materialId = GetMaterialId(mapping.Material);
                        if (materialId == null || materialId == ElementId.InvalidElementId)
                        {
                            Logger.Warning($"Material '{mapping.Material}' not found for category '{mapping.Category}'");
                            continue;
                        }

                        var elementsToProcess = GetElementsForMapping(mapping);
                        Logger.Info($"Processing {elementsToProcess.Count} elements for category '{mapping.Category}' with material '{mapping.Material}'");

                        foreach (var element in elementsToProcess)
                        {
                            if (element == null)
                                continue;

                            if (IsSideFaceChecked)
                            {
                                PaintService.PaintSideFaces(doc, element, materialId);
                            }

                            if (IsBottomFaceChecked)
                            {
                                PaintService.PaintBottomFaces(doc, element, materialId);
                            }

                            if (IsInterfacesChecked)
                            {
                                var checkedFaces = PickedFaces.Where(f => f != null && f.IsChecked).ToList();
                                if (checkedFaces.Count > 0)
                                {
                                    PaintService.PaintInterfaces(doc, element, checkedFaces, materialId);
                                }
                            }
                        }
                    }

                    transaction.Commit();
                    Logger.Info("Paint transaction committed successfully");
                }

                SaveConfiguration();
                Logger.Info("ExecutePaint completed successfully");
                MessageBox.Show("Paint operation completed successfully.", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in ExecutePaint");
                MessageBox.Show($"Error during paint operation: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteRemovePaint()
        {
            try
            {
                Logger.Info("ExecuteRemovePaint started");
                
                if (!ValidatePaintOperation())
                {
                    Logger.Info("Remove paint operation validation failed");
                    return;
                }

                var result = MessageBox.Show(
                    "Are you sure you want to remove paint from selected elements?",
                    "Remove Paint Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    Logger.Info("Remove paint operation cancelled by user");
                    return;
                }

                var doc = _uiDocument.Document;
                if (doc == null)
                {
                    Logger.Warning("Document is null in ExecuteRemovePaint");
                    return;
                }

                using (var transaction = new Transaction(doc, "Remove Paint"))
                {
                    transaction.Start();

                    foreach (var mapping in CategoryMappings.Where(m => m.IsChecked))
                    {
                        var elementsToProcess = GetElementsForMapping(mapping);
                        Logger.Info($"Removing paint from {elementsToProcess.Count} elements in category '{mapping.Category}'");

                        foreach (var element in elementsToProcess)
                        {
                            if (element == null)
                                continue;

                            if (IsSideFaceChecked)
                            {
                                PaintService.RemovePaintSideFaces(doc, element);
                            }

                            if (IsBottomFaceChecked)
                            {
                                PaintService.RemovePaintBottomFaces(doc, element);
                            }

                            if (IsInterfacesChecked)
                            {
                                var checkedFaces = PickedFaces.Where(f => f != null && f.IsChecked).ToList();
                                if (checkedFaces.Count > 0)
                                {
                                    PaintService.RemovePaintInterfaces(doc, element, checkedFaces);
                                }
                            }
                        }
                    }

                    transaction.Commit();
                    Logger.Info("Remove paint transaction committed successfully");
                }

                Logger.Info("ExecuteRemovePaint completed successfully");
                MessageBox.Show("Remove paint operation completed successfully.", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in ExecuteRemovePaint");
                MessageBox.Show($"Error during remove paint operation: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteClose()
        {
            SaveConfiguration();
            _window?.Close();
        }

        #endregion

        #region Helper Methods

        public void SetWindow(Window window)
        {
            _window = window;
        }

        private void LoadAvailableParametersAndMaterials()
        {
            var doc = _uiDocument.Document;

            // Load common parameters
            var commonParameters = new[]
            {
                "Mark", "Comments", "Type Name", "Family", "Level",
                "Phase Created", "Phase Demolished", "Workset"
            };

            foreach (var param in commonParameters)
            {
                AvailableParameters.Add(param);
            }

            // Load materials
            var materials = new FilteredElementCollector(doc)
                .OfClass(typeof(Material))
                .Cast<Material>()
                .OrderBy(m => m.Name)
                .Select(m => m.Name);

            foreach (var material in materials)
            {
                AvailableMaterials.Add(material);
            }
        }

        private void LoadSavedConfiguration()
        {
            try
            {
                var config = ConfigManager.LoadConfiguration();

                // Restore paint options
                IsSideFaceChecked = config.PaintOptions.SideFace;
                IsBottomFaceChecked = config.PaintOptions.BottomFace;
                IsInterfacesChecked = config.PaintOptions.Interfaces;

                // Match saved mappings with current categories
                foreach (var mapping in CategoryMappings)
                {
                    var saved = config.LastMapping.FirstOrDefault(m => m.Category == mapping.Category);
                    if (saved != null)
                    {
                        mapping.Parameter = saved.Parameter;
                        mapping.ParameterValue = saved.ParameterValue;
                        mapping.Material = saved.Material;
                    }
                }
            }
            catch (Exception ex)
            {
                // Configuration load failed, continue with defaults
                System.Diagnostics.Debug.WriteLine($"Failed to load configuration: {ex.Message}");
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                var config = new MappingConfiguration
                {
                    LastMapping = CategoryMappings.ToList(),
                    PaintOptions = new MappingConfiguration.PaintOptionsConfig
                    {
                        SideFace = IsSideFaceChecked,
                        BottomFace = IsBottomFaceChecked,
                        Interfaces = IsInterfacesChecked
                    }
                };

                ConfigManager.SaveConfiguration(config);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save configuration: {ex.Message}");
            }
        }

        private bool ValidatePaintOperation()
        {
            // Check if paint options conflict
            if (IsSideFaceChecked && IsInterfacesChecked)
            {
                MessageBox.Show("Cannot select both Side Face and Interfaces options.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Check if at least one option is selected
            if (!IsSideFaceChecked && !IsBottomFaceChecked && !IsInterfacesChecked)
            {
                MessageBox.Show("Please select at least one paint option (Side Face, Bottom Face, or Interfaces).",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Check if interfaces is selected but no faces picked
            if (IsInterfacesChecked && !PickedFaces.Any(f => f.IsChecked))
            {
                MessageBox.Show("Please pick at least one face for interface painting.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Check if at least one category is selected
            if (!CategoryMappings.Any(m => m.IsChecked))
            {
                MessageBox.Show("Please select at least one category mapping.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Check if selected categories have materials assigned
            var missingMaterials = CategoryMappings.Where(m => m.IsChecked && string.IsNullOrEmpty(m.Material)).ToList();
            if (missingMaterials.Any())
            {
                var categories = string.Join(", ", missingMaterials.Select(m => m.Category));
                MessageBox.Show($"Please assign materials to the following categories: {categories}",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private ElementId? GetMaterialId(string materialName)
        {
            var doc = _uiDocument.Document;
            var material = new FilteredElementCollector(doc)
                .OfClass(typeof(Material))
                .Cast<Material>()
                .FirstOrDefault(m => m.Name == materialName);

            return material?.Id;
        }

        private List<Element> GetElementsForMapping(CategoryMapping mapping)
        {
            var elements = SelectedElements.Where(e => e.Category?.Name == mapping.Category).ToList();

            // Filter by parameter if specified
            if (!string.IsNullOrEmpty(mapping.Parameter) && !string.IsNullOrEmpty(mapping.ParameterValue))
            {
                elements = elements.Where(e =>
                {
                    var parameter = e.LookupParameter(mapping.Parameter);
                    if (parameter != null && parameter.HasValue)
                    {
                        var value = parameter.AsValueString() ?? parameter.AsString();
                        return value == mapping.ParameterValue;
                    }
                    return false;
                }).ToList();
            }

            return elements;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
