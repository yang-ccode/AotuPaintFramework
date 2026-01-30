using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using AotuPaintFramework.Utils;

namespace AotuPaintFramework.Models
{
    /// <summary>
    /// Static helper class for geometry operations on Revit elements.
    /// </summary>
    public static class GeometryHelper
    {
        private const double DefaultTolerance = 0.001;
        private const double VerticalAngleTolerance = 0.087; // ~5 degrees in radians

        /// <summary>
        /// Extracts solid geometry from an element.
        /// </summary>
        /// <param name="element">The element to extract geometry from.</param>
        /// <returns>The first solid found, or null if no solid exists.</returns>
        public static Solid? GetElementSolid(Element element)
        {
            try
            {
                if (element == null)
                {
                    Logger.Warning("Element is null in GetElementSolid");
                    return null;
                }

                var options = new Options
                {
                    ComputeReferences = true,
                    DetailLevel = ViewDetailLevel.Fine,
                    IncludeNonVisibleObjects = false
                };

                var geometryElement = element.get_Geometry(options);
                if (geometryElement == null)
                {
                    Logger.Warning($"No geometry found for element {element.Id} in GetElementSolid");
                    return null;
                }

                foreach (var geometryObject in geometryElement)
                {
                    if (geometryObject == null)
                        continue;

                    if (geometryObject is Solid solid && solid.Volume > DefaultTolerance)
                        return solid;

                    if (geometryObject is GeometryInstance instance)
                    {
                        var instanceGeometry = instance.GetInstanceGeometry();
                        if (instanceGeometry != null)
                        {
                            foreach (var instObj in instanceGeometry)
                            {
                                if (instObj == null)
                                    continue;

                                if (instObj is Solid instSolid && instSolid.Volume > DefaultTolerance)
                                    return instSolid;
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error in GetElementSolid for element {element?.Id}");
                return null;
            }
        }

        /// <summary>
        /// Checks if a face is a vertical side face using its normal vector.
        /// </summary>
        /// <param name="face">The face to check.</param>
        /// <returns>True if the face is vertical (side face), false otherwise.</returns>
        public static bool IsSideFace(Face face)
        {
            try
            {
                if (face == null)
                {
                    Logger.Warning("Face is null in IsSideFace");
                    return false;
                }

                var bbox = face.GetBoundingBox();
                if (bbox == null)
                {
                    Logger.Warning("Bounding box is null in IsSideFace");
                    return false;
                }

                var center = (bbox.Min + bbox.Max) / 2.0;
                var normal = face.ComputeNormal(new UV(0.5, 0.5));

                // Check if normal is horizontal (perpendicular to Z-axis)
                var zComponent = System.Math.Abs(normal.Z);
                return zComponent < System.Math.Sin(VerticalAngleTolerance);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in IsSideFace");
                return false;
            }
        }

        /// <summary>
        /// Checks if a face is a horizontal face facing downward (bottom face).
        /// </summary>
        /// <param name="face">The face to check.</param>
        /// <returns>True if the face is horizontal and facing down, false otherwise.</returns>
        public static bool IsBottomFace(Face face)
        {
            try
            {
                if (face == null)
                {
                    Logger.Warning("Face is null in IsBottomFace");
                    return false;
                }

                var normal = face.ComputeNormal(new UV(0.5, 0.5));

                // Check if normal points downward (negative Z)
                var isHorizontal = System.Math.Abs(System.Math.Abs(normal.Z) - 1.0) < DefaultTolerance;
                var pointsDown = normal.Z < -DefaultTolerance;

                return isHorizontal && pointsDown;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in IsBottomFace");
                return false;
            }
        }

        /// <summary>
        /// Checks if a face intersects with other elements in the document.
        /// </summary>
        /// <param name="face">The face to check.</param>
        /// <param name="element">The element containing the face.</param>
        /// <param name="doc">The Revit document.</param>
        /// <returns>True if the face intersects with other elements, false otherwise.</returns>
        public static bool IsIntersectingFace(Face face, Element element, Document doc)
        {
            try
            {
                if (face == null)
                {
                    Logger.Warning("Face is null in IsIntersectingFace");
                    return false;
                }
                
                if (element == null)
                {
                    Logger.Warning("Element is null in IsIntersectingFace");
                    return false;
                }
                
                if (doc == null)
                {
                    Logger.Warning("Document is null in IsIntersectingFace");
                    return false;
                }

                // Get mesh triangulation to find bounds in XYZ coordinates
                var mesh = face.Triangulate();
                if (mesh == null || mesh.NumTriangles == 0)
                {
                    Logger.Warning($"No mesh found for face on element {element.Id} in IsIntersectingFace");
                    return false;
                }

                // Calculate bounding box from mesh vertices
                XYZ? min = null;
                XYZ? max = null;

                for (int i = 0; i < mesh.NumTriangles; i++)
                {
                    var triangle = mesh.get_Triangle(i);
                    if (triangle == null)
                        continue;

                    for (int j = 0; j < 3; j++)
                    {
                        var vertex = triangle.get_Vertex(j);
                        if (vertex == null)
                            continue;

                        if (min == null)
                        {
                            min = vertex;
                            max = vertex;
                        }
                        else
                        {
                            min = new XYZ(
                                System.Math.Min(min.X, vertex.X),
                                System.Math.Min(min.Y, vertex.Y),
                                System.Math.Min(min.Z, vertex.Z));
                            
                            if (max != null)
                            {
                                max = new XYZ(
                                    System.Math.Max(max.X, vertex.X),
                                    System.Math.Max(max.Y, vertex.Y),
                                    System.Math.Max(max.Z, vertex.Z));
                            }
                        }
                    }
                }

                if (min == null || max == null)
                {
                    Logger.Warning($"Failed to calculate bounding box for face on element {element.Id} in IsIntersectingFace");
                    return false;
                }

                // Expand bounding box slightly for intersection detection
                var minExpanded = min - new XYZ(DefaultTolerance, DefaultTolerance, DefaultTolerance);
                var maxExpanded = max + new XYZ(DefaultTolerance, DefaultTolerance, DefaultTolerance);
                var outline = new Outline(minExpanded, maxExpanded);

                var bboxFilter = new BoundingBoxIntersectsFilter(outline);
                var collector = new FilteredElementCollector(doc)
                    .WherePasses(bboxFilter)
                    .WhereElementIsNotElementType();

                foreach (var otherElement in collector)
                {
                    if (otherElement == null)
                        continue;

                    // Skip the same element
                    if (otherElement.Id.Value == element.Id.Value)
                        continue;

                    // Check if other element has solid geometry
                    var otherSolid = GetElementSolid(otherElement);
                    if (otherSolid != null && otherSolid.Volume > DefaultTolerance)
                        return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error in IsIntersectingFace for element {element?.Id}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a face lies on a specific plane within tolerance.
        /// </summary>
        /// <param name="face">The face to check.</param>
        /// <param name="plane">The plane to compare against.</param>
        /// <param name="tolerance">Distance tolerance for plane comparison.</param>
        /// <returns>True if the face lies on the plane, false otherwise.</returns>
        public static bool IsFaceOnPlane(Face face, Plane plane, double tolerance = DefaultTolerance)
        {
            try
            {
                if (face == null)
                {
                    Logger.Warning("Face is null in IsFaceOnPlane");
                    return false;
                }
                
                if (plane == null)
                {
                    Logger.Warning("Plane is null in IsFaceOnPlane");
                    return false;
                }

                // Check if face is planar
                if (!(face is PlanarFace planarFace))
                    return false;

                var faceNormal = planarFace.FaceNormal;
                var planeNormal = plane.Normal;

                // Check if normals are parallel (same direction or opposite)
                var dotProduct = faceNormal.DotProduct(planeNormal);
                if (System.Math.Abs(System.Math.Abs(dotProduct) - 1.0) > tolerance)
                    return false;

                // Check if face origin is on the plane
                var faceOrigin = planarFace.Origin;
                var distance = System.Math.Abs(plane.Normal.DotProduct(faceOrigin - plane.Origin));

                return distance < tolerance;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in IsFaceOnPlane");
                return false;
            }
        }

        /// <summary>
        /// Gets all faces of an element that lie on a specific plane.
        /// </summary>
        /// <param name="element">The element to extract faces from.</param>
        /// <param name="plane">The plane to filter faces by.</param>
        /// <returns>List of faces on the specified plane.</returns>
        public static List<Face> GetFacesOnPlane(Element element, Plane plane)
        {
            var result = new List<Face>();

            try
            {
                if (element == null)
                {
                    Logger.Warning("Element is null in GetFacesOnPlane");
                    return result;
                }
                
                if (plane == null)
                {
                    Logger.Warning("Plane is null in GetFacesOnPlane");
                    return result;
                }

                var solid = GetElementSolid(element);
                if (solid == null)
                {
                    Logger.Warning($"No solid found for element {element.Id} in GetFacesOnPlane");
                    return result;
                }

                foreach (Face face in solid.Faces)
                {
                    if (face == null)
                        continue;

                    if (IsFaceOnPlane(face, plane))
                        result.Add(face);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error in GetFacesOnPlane for element {element?.Id}");
                return result;
            }
        }
    }
}
