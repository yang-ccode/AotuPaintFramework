using System.Collections.Generic;

namespace AotuPaintFramework.Models
{
    /// <summary>
    /// Configuration model for saving and loading material painting settings.
    /// Contains category mappings, paint options, and picked faces.
    /// Note: PickedFaces contains Revit API objects which may not be serializable.
    /// Consider creating a separate DTO for serialization if persistence is needed.
    /// </summary>
    public class MappingConfiguration
    {
        /// <summary>
        /// Gets or sets the list of category to material mapping rules.
        /// </summary>
        public List<CategoryMapping> LastMapping { get; set; }
        
        /// <summary>
        /// Gets or sets the paint options configuration.
        /// </summary>
        public PaintOptionsConfig PaintOptions { get; set; }
        
        /// <summary>
        /// Gets or sets the list of manually picked faces.
        /// Note: Contains Revit API objects (Face, Plane) which are not serializable.
        /// </summary>
        public List<PickedFaceItem> PickedFaces { get; set; }

        public MappingConfiguration()
        {
            LastMapping = new List<CategoryMapping>();
            PaintOptions = new PaintOptionsConfig();
            PickedFaces = new List<PickedFaceItem>();
        }

        /// <summary>
        /// Configuration for paint options controlling which faces to paint.
        /// </summary>
        public class PaintOptionsConfig
        {
            /// <summary>
            /// Gets or sets whether to paint side faces of elements.
            /// </summary>
            public bool SideFace { get; set; }
            
            /// <summary>
            /// Gets or sets whether to paint bottom faces of elements.
            /// </summary>
            public bool BottomFace { get; set; }
            
            /// <summary>
            /// Gets or sets whether to paint interface/connection faces between elements.
            /// </summary>
            public bool Interfaces { get; set; }
        }
    }
}
