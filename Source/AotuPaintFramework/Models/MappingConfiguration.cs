using System.Collections.Generic;

namespace AotuPaintFramework.Models
{
    public class MappingConfiguration
    {
        public List<CategoryMapping>? LastMapping { get; set; }
        public PaintOptionsConfig? PaintOptions { get; set; }
        public List<PickedFaceItem>? PickedFaces { get; set; }

        public MappingConfiguration()
        {
            LastMapping = new List<CategoryMapping>();
            PaintOptions = new PaintOptionsConfig();
            PickedFaces = new List<PickedFaceItem>();
        }

        public class PaintOptionsConfig
        {
            public bool SideFace { get; set; }
            public bool BottomFace { get; set; }
            public bool Interfaces { get; set; }
        }
    }
}
