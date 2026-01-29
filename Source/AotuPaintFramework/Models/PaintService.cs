using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace AotuPaintFramework.Models
{
    /// <summary>
    /// Static service class for paint operations on Revit elements.
    /// All methods assume they are called within an active transaction.
    /// </summary>
    public static class PaintService
    {
        /// <summary>
        /// Paints all side faces of an element, excluding intersecting faces.
        /// </summary>
        /// <param name="doc">The Revit document.</param>
        /// <param name="element">The element to paint.</param>
        /// <param name="materialId">The material ID to apply.</param>
        public static void PaintSideFaces(Document doc, Element element, ElementId materialId)
        {
            if (doc == null || element == null || materialId == null || materialId == ElementId.InvalidElementId)
                return;

            var solid = GeometryHelper.GetElementSolid(element);
            if (solid == null)
                return;

            foreach (Face face in solid.Faces)
            {
                if (GeometryHelper.IsSideFace(face) && !GeometryHelper.IsIntersectingFace(face, element, doc))
                {
                    try
                    {
                        doc.Paint(element.Id, face, materialId);
                    }
                    catch
                    {
                        // Silently ignore painting errors (e.g., already painted, invalid face)
                    }
                }
            }
        }

        /// <summary>
        /// Paints all bottom faces of an element.
        /// </summary>
        /// <param name="doc">The Revit document.</param>
        /// <param name="element">The element to paint.</param>
        /// <param name="materialId">The material ID to apply.</param>
        public static void PaintBottomFaces(Document doc, Element element, ElementId materialId)
        {
            if (doc == null || element == null || materialId == null || materialId == ElementId.InvalidElementId)
                return;

            var solid = GeometryHelper.GetElementSolid(element);
            if (solid == null)
                return;

            foreach (Face face in solid.Faces)
            {
                if (GeometryHelper.IsBottomFace(face))
                {
                    try
                    {
                        doc.Paint(element.Id, face, materialId);
                    }
                    catch
                    {
                        // Silently ignore painting errors
                    }
                }
            }
        }

        /// <summary>
        /// Paints faces on the planes specified by picked face items.
        /// </summary>
        /// <param name="doc">The Revit document.</param>
        /// <param name="element">The element to paint.</param>
        /// <param name="pickedFaces">List of picked face items containing plane information.</param>
        /// <param name="materialId">The material ID to apply.</param>
        public static void PaintInterfaces(Document doc, Element element, List<PickedFaceItem> pickedFaces, ElementId materialId)
        {
            if (doc == null || element == null || pickedFaces == null || pickedFaces.Count == 0 
                || materialId == null || materialId == ElementId.InvalidElementId)
                return;

            foreach (var pickedItem in pickedFaces)
            {
                if (pickedItem?.Plane == null)
                    continue;

                // Cast object to Plane (as per PickedFaceItem documentation)
                if (!(pickedItem.Plane is Plane plane))
                    continue;

                var facesOnPlane = GeometryHelper.GetFacesOnPlane(element, plane);
                foreach (var face in facesOnPlane)
                {
                    try
                    {
                        doc.Paint(element.Id, face, materialId);
                    }
                    catch
                    {
                        // Silently ignore painting errors
                    }
                }
            }
        }

        /// <summary>
        /// Removes paint from all side faces of an element.
        /// </summary>
        /// <param name="doc">The Revit document.</param>
        /// <param name="element">The element to remove paint from.</param>
        public static void RemovePaintSideFaces(Document doc, Element element)
        {
            if (doc == null || element == null)
                return;

            var solid = GeometryHelper.GetElementSolid(element);
            if (solid == null)
                return;

            foreach (Face face in solid.Faces)
            {
                if (GeometryHelper.IsSideFace(face))
                {
                    try
                    {
                        doc.RemovePaint(element.Id, face);
                    }
                    catch
                    {
                        // Silently ignore removal errors (e.g., face not painted)
                    }
                }
            }
        }

        /// <summary>
        /// Removes paint from all bottom faces of an element.
        /// </summary>
        /// <param name="doc">The Revit document.</param>
        /// <param name="element">The element to remove paint from.</param>
        public static void RemovePaintBottomFaces(Document doc, Element element)
        {
            if (doc == null || element == null)
                return;

            var solid = GeometryHelper.GetElementSolid(element);
            if (solid == null)
                return;

            foreach (Face face in solid.Faces)
            {
                if (GeometryHelper.IsBottomFace(face))
                {
                    try
                    {
                        doc.RemovePaint(element.Id, face);
                    }
                    catch
                    {
                        // Silently ignore removal errors
                    }
                }
            }
        }

        /// <summary>
        /// Removes paint from faces on the planes specified by picked face items.
        /// </summary>
        /// <param name="doc">The Revit document.</param>
        /// <param name="element">The element to remove paint from.</param>
        /// <param name="pickedFaces">List of picked face items containing plane information.</param>
        public static void RemovePaintInterfaces(Document doc, Element element, List<PickedFaceItem> pickedFaces)
        {
            if (doc == null || element == null || pickedFaces == null || pickedFaces.Count == 0)
                return;

            foreach (var pickedItem in pickedFaces)
            {
                if (pickedItem?.Plane == null)
                    continue;

                // Cast object to Plane (as per PickedFaceItem documentation)
                if (!(pickedItem.Plane is Plane plane))
                    continue;

                var facesOnPlane = GeometryHelper.GetFacesOnPlane(element, plane);
                foreach (var face in facesOnPlane)
                {
                    try
                    {
                        doc.RemovePaint(element.Id, face);
                    }
                    catch
                    {
                        // Silently ignore removal errors
                    }
                }
            }
        }
    }
}
