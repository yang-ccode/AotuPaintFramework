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
                if (doc == null || element == null || materialId == null || materialId == ElementId.InvalidElementId)
                {
                    Logger.Warning($"PaintSideFaces called with invalid parameters: doc={doc != null}, element={element != null}, materialId={materialId != null}");
                    return;
                }

                Logger.Info($"Painting side faces for element {element.Id.Value}");

                var solid = GeometryHelper.GetElementSolid(element);
                if (solid == null)
                {
                    Logger.Warning($"No solid geometry found for element {element.Id.Value}");
                    return;
                }

                int paintedCount = 0;
                foreach (Face face in solid.Faces)
                {
                    if (GeometryHelper.IsSideFace(face) && !GeometryHelper.IsIntersectingFace(face, element, doc))
                    {
                        try
                        {
                            doc.Paint(element.Id, face, materialId);
                            paintedCount++;
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"Failed to paint side face on element {element.Id.Value}: {ex.Message}");
                        }
                    }
                }
                
                Logger.Info($"Painted {paintedCount} side faces on element {element.Id.Value}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error painting side faces for element {element?.Id.Value}");
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
                if (doc == null || element == null || materialId == null || materialId == ElementId.InvalidElementId)
                {
                    Logger.Warning($"PaintBottomFaces called with invalid parameters: doc={doc != null}, element={element != null}, materialId={materialId != null}");
                    return;
                }

                Logger.Info($"Painting bottom faces for element {element.Id.Value}");

                var solid = GeometryHelper.GetElementSolid(element);
                if (solid == null)
                {
                    Logger.Warning($"No solid geometry found for element {element.Id.Value}");
                    return;
                }

                int paintedCount = 0;
                foreach (Face face in solid.Faces)
                {
                    if (GeometryHelper.IsBottomFace(face))
                    {
                        try
                        {
                            doc.Paint(element.Id, face, materialId);
                            paintedCount++;
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"Failed to paint bottom face on element {element.Id.Value}: {ex.Message}");
                        }
                    }
                }
                
                Logger.Info($"Painted {paintedCount} bottom faces on element {element.Id.Value}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error painting bottom faces for element {element?.Id.Value}");
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
                if (doc == null || element == null || pickedFaces == null || pickedFaces.Count == 0 
                    || materialId == null || materialId == ElementId.InvalidElementId)
                {
                    Logger.Warning($"PaintInterfaces called with invalid parameters: doc={doc != null}, element={element != null}, pickedFaces={pickedFaces?.Count ?? 0}, materialId={materialId != null}");
                    return;
                }

                Logger.Info($"Painting interface faces for element {element.Id.Value} with {pickedFaces.Count} picked faces");

                int paintedCount = 0;
                foreach (var pickedItem in pickedFaces)
                {
                    if (pickedItem?.Plane == null)
                    {
                        Logger.Warning("Skipping null picked item or plane");
                        continue;
                    }

                    // Cast object to Plane (as per PickedFaceItem documentation)
                    if (!(pickedItem.Plane is Plane plane))
                    {
                        Logger.Warning("Failed to cast Plane object");
                        continue;
                    }

                    var facesOnPlane = GeometryHelper.GetFacesOnPlane(element, plane);
                    foreach (var face in facesOnPlane)
                    {
                        try
                        {
                            doc.Paint(element.Id, face, materialId);
                            paintedCount++;
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"Failed to paint interface face on element {element.Id.Value}: {ex.Message}");
                        }
                    }
                }
                
                Logger.Info($"Painted {paintedCount} interface faces on element {element.Id.Value}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error painting interface faces for element {element?.Id.Value}");
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
                if (doc == null || element == null)
                {
                    Logger.Warning($"RemovePaintSideFaces called with invalid parameters: doc={doc != null}, element={element != null}");
                    return;
                }

                Logger.Info($"Removing paint from side faces for element {element.Id.Value}");

                var solid = GeometryHelper.GetElementSolid(element);
                if (solid == null)
                {
                    Logger.Warning($"No solid geometry found for element {element.Id.Value}");
                    return;
                }

                int removedCount = 0;
                foreach (Face face in solid.Faces)
                {
                    if (GeometryHelper.IsSideFace(face))
                    {
                        try
                        {
                            doc.RemovePaint(element.Id, face);
                            removedCount++;
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"Failed to remove paint from side face on element {element.Id.Value}: {ex.Message}");
                        }
                    }
                }
                
                Logger.Info($"Removed paint from {removedCount} side faces on element {element.Id.Value}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error removing paint from side faces for element {element?.Id.Value}");
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
                if (doc == null || element == null)
                {
                    Logger.Warning($"RemovePaintBottomFaces called with invalid parameters: doc={doc != null}, element={element != null}");
                    return;
                }

                Logger.Info($"Removing paint from bottom faces for element {element.Id.Value}");

                var solid = GeometryHelper.GetElementSolid(element);
                if (solid == null)
                {
                    Logger.Warning($"No solid geometry found for element {element.Id.Value}");
                    return;
                }

                int removedCount = 0;
                foreach (Face face in solid.Faces)
                {
                    if (GeometryHelper.IsBottomFace(face))
                    {
                        try
                        {
                            doc.RemovePaint(element.Id, face);
                            removedCount++;
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"Failed to remove paint from bottom face on element {element.Id.Value}: {ex.Message}");
                        }
                    }
                }
                
                Logger.Info($"Removed paint from {removedCount} bottom faces on element {element.Id.Value}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error removing paint from bottom faces for element {element?.Id.Value}");
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
                if (doc == null || element == null || pickedFaces == null || pickedFaces.Count == 0)
                {
                    Logger.Warning($"RemovePaintInterfaces called with invalid parameters: doc={doc != null}, element={element != null}, pickedFaces={pickedFaces?.Count ?? 0}");
                    return;
                }

                Logger.Info($"Removing paint from interface faces for element {element.Id.Value} with {pickedFaces.Count} picked faces");

                int removedCount = 0;
                foreach (var pickedItem in pickedFaces)
                {
                    if (pickedItem?.Plane == null)
                    {
                        Logger.Warning("Skipping null picked item or plane");
                        continue;
                    }

                    // Cast object to Plane (as per PickedFaceItem documentation)
                    if (!(pickedItem.Plane is Plane plane))
                    {
                        Logger.Warning("Failed to cast Plane object");
                        continue;
                    }

                    var facesOnPlane = GeometryHelper.GetFacesOnPlane(element, plane);
                    foreach (var face in facesOnPlane)
                    {
                        try
                        {
                            doc.RemovePaint(element.Id, face);
                            removedCount++;
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"Failed to remove paint from interface face on element {element.Id.Value}: {ex.Message}");
                        }
                    }
                }
                
                Logger.Info($"Removed paint from {removedCount} interface faces on element {element.Id.Value}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error removing paint from interface faces for element {element?.Id.Value}");
            }
        }
    }
}
