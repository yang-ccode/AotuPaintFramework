using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using AotuPaintFramework.Utils;

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
            try
            {
                if (doc == null)
                {
                    Logger.Warning("Document is null in PaintSideFaces");
                    return;
                }
                
                if (element == null)
                {
                    Logger.Warning("Element is null in PaintSideFaces");
                    return;
                }
                
                if (materialId == null || materialId == ElementId.InvalidElementId)
                {
                    Logger.Warning($"Invalid material ID in PaintSideFaces for element {element.Id}");
                    return;
                }

                var solid = GeometryHelper.GetElementSolid(element);
                if (solid == null)
                {
                    Logger.Warning($"No solid found for element {element.Id} in PaintSideFaces");
                    return;
                }

                int paintedCount = 0;
                foreach (Face face in solid.Faces)
                {
                    if (face == null)
                        continue;

                    if (GeometryHelper.IsSideFace(face) && !GeometryHelper.IsIntersectingFace(face, element, doc))
                    {
                        try
                        {
                            doc.Paint(element.Id, face, materialId);
                            paintedCount++;
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"Failed to paint side face on element {element.Id}: {ex.Message}");
                        }
                    }
                }
                
                Logger.Info($"Painted {paintedCount} side faces on element {element.Id}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error in PaintSideFaces for element {element?.Id}");
                throw;
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
            try
            {
                if (doc == null)
                {
                    Logger.Warning("Document is null in PaintBottomFaces");
                    return;
                }
                
                if (element == null)
                {
                    Logger.Warning("Element is null in PaintBottomFaces");
                    return;
                }
                
                if (materialId == null || materialId == ElementId.InvalidElementId)
                {
                    Logger.Warning($"Invalid material ID in PaintBottomFaces for element {element.Id}");
                    return;
                }

                var solid = GeometryHelper.GetElementSolid(element);
                if (solid == null)
                {
                    Logger.Warning($"No solid found for element {element.Id} in PaintBottomFaces");
                    return;
                }

                int paintedCount = 0;
                foreach (Face face in solid.Faces)
                {
                    if (face == null)
                        continue;

                    if (GeometryHelper.IsBottomFace(face))
                    {
                        try
                        {
                            doc.Paint(element.Id, face, materialId);
                            paintedCount++;
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"Failed to paint bottom face on element {element.Id}: {ex.Message}");
                        }
                    }
                }
                
                Logger.Info($"Painted {paintedCount} bottom faces on element {element.Id}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error in PaintBottomFaces for element {element?.Id}");
                throw;
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
            try
            {
                if (doc == null)
                {
                    Logger.Warning("Document is null in PaintInterfaces");
                    return;
                }
                
                if (element == null)
                {
                    Logger.Warning("Element is null in PaintInterfaces");
                    return;
                }
                
                if (pickedFaces == null || pickedFaces.Count == 0)
                {
                    Logger.Warning($"No picked faces provided in PaintInterfaces for element {element.Id}");
                    return;
                }
                
                if (materialId == null || materialId == ElementId.InvalidElementId)
                {
                    Logger.Warning($"Invalid material ID in PaintInterfaces for element {element.Id}");
                    return;
                }

                int paintedCount = 0;
                foreach (var pickedItem in pickedFaces)
                {
                    if (pickedItem?.Plane == null)
                    {
                        Logger.Warning("Picked item or Plane is null in PaintInterfaces");
                        continue;
                    }

                    // Cast object to Plane (as per PickedFaceItem documentation)
                    if (!(pickedItem.Plane is Plane plane))
                    {
                        Logger.Warning("Failed to cast Plane in PaintInterfaces");
                        continue;
                    }

                    var facesOnPlane = GeometryHelper.GetFacesOnPlane(element, plane);
                    foreach (var face in facesOnPlane)
                    {
                        if (face == null)
                            continue;

                        try
                        {
                            doc.Paint(element.Id, face, materialId);
                            paintedCount++;
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"Failed to paint interface face on element {element.Id}: {ex.Message}");
                        }
                    }
                }
                
                Logger.Info($"Painted {paintedCount} interface faces on element {element.Id}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error in PaintInterfaces for element {element?.Id}");
                throw;
            }
        }

        /// <summary>
        /// Removes paint from all side faces of an element.
        /// </summary>
        /// <param name="doc">The Revit document.</param>
        /// <param name="element">The element to remove paint from.</param>
        public static void RemovePaintSideFaces(Document doc, Element element)
        {
            try
            {
                if (doc == null)
                {
                    Logger.Warning("Document is null in RemovePaintSideFaces");
                    return;
                }
                
                if (element == null)
                {
                    Logger.Warning("Element is null in RemovePaintSideFaces");
                    return;
                }

                var solid = GeometryHelper.GetElementSolid(element);
                if (solid == null)
                {
                    Logger.Warning($"No solid found for element {element.Id} in RemovePaintSideFaces");
                    return;
                }

                int removedCount = 0;
                foreach (Face face in solid.Faces)
                {
                    if (face == null)
                        continue;

                    if (GeometryHelper.IsSideFace(face))
                    {
                        try
                        {
                            doc.RemovePaint(element.Id, face);
                            removedCount++;
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"Failed to remove paint from side face on element {element.Id}: {ex.Message}");
                        }
                    }
                }
                
                Logger.Info($"Removed paint from {removedCount} side faces on element {element.Id}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error in RemovePaintSideFaces for element {element?.Id}");
                throw;
            }
        }

        /// <summary>
        /// Removes paint from all bottom faces of an element.
        /// </summary>
        /// <param name="doc">The Revit document.</param>
        /// <param name="element">The element to remove paint from.</param>
        public static void RemovePaintBottomFaces(Document doc, Element element)
        {
            try
            {
                if (doc == null)
                {
                    Logger.Warning("Document is null in RemovePaintBottomFaces");
                    return;
                }
                
                if (element == null)
                {
                    Logger.Warning("Element is null in RemovePaintBottomFaces");
                    return;
                }

                var solid = GeometryHelper.GetElementSolid(element);
                if (solid == null)
                {
                    Logger.Warning($"No solid found for element {element.Id} in RemovePaintBottomFaces");
                    return;
                }

                int removedCount = 0;
                foreach (Face face in solid.Faces)
                {
                    if (face == null)
                        continue;

                    if (GeometryHelper.IsBottomFace(face))
                    {
                        try
                        {
                            doc.RemovePaint(element.Id, face);
                            removedCount++;
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"Failed to remove paint from bottom face on element {element.Id}: {ex.Message}");
                        }
                    }
                }
                
                Logger.Info($"Removed paint from {removedCount} bottom faces on element {element.Id}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error in RemovePaintBottomFaces for element {element?.Id}");
                throw;
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
            try
            {
                if (doc == null)
                {
                    Logger.Warning("Document is null in RemovePaintInterfaces");
                    return;
                }
                
                if (element == null)
                {
                    Logger.Warning("Element is null in RemovePaintInterfaces");
                    return;
                }
                
                if (pickedFaces == null || pickedFaces.Count == 0)
                {
                    Logger.Warning($"No picked faces provided in RemovePaintInterfaces for element {element.Id}");
                    return;
                }

                int removedCount = 0;
                foreach (var pickedItem in pickedFaces)
                {
                    if (pickedItem?.Plane == null)
                    {
                        Logger.Warning("Picked item or Plane is null in RemovePaintInterfaces");
                        continue;
                    }

                    // Cast object to Plane (as per PickedFaceItem documentation)
                    if (!(pickedItem.Plane is Plane plane))
                    {
                        Logger.Warning("Failed to cast Plane in RemovePaintInterfaces");
                        continue;
                    }

                    var facesOnPlane = GeometryHelper.GetFacesOnPlane(element, plane);
                    foreach (var face in facesOnPlane)
                    {
                        if (face == null)
                            continue;

                        try
                        {
                            doc.RemovePaint(element.Id, face);
                            removedCount++;
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"Failed to remove paint from interface face on element {element.Id}: {ex.Message}");
                        }
                    }
                }
                
                Logger.Info($"Removed paint from {removedCount} interface faces on element {element.Id}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error in RemovePaintInterfaces for element {element?.Id}");
                throw;
            }
        }
    }
}
