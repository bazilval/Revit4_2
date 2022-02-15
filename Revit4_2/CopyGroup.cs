using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit4_2
{
    [Transaction(TransactionMode.Manual)]
    public class CopyGroup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;

                Reference refer = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, new GroupFilter(), "Выберите группу");
                Element element = doc.GetElement(refer);
                var group = element as Group;

                XYZ groupCenter = GetElementCenter(group);
                Room groupRoom = GetRoomByPoint(doc, groupCenter);
                XYZ roomCenter = GetElementCenter(groupRoom);
                XYZ offset = groupCenter - roomCenter;

                XYZ userPoint = uiDoc.Selection.PickPoint("Укажите точку вставки");
                Room newRoom = GetRoomByPoint(doc, userPoint);
                XYZ point;
                if (newRoom != null)
                {
                    point = offset + GetElementCenter(newRoom);
                }
                else
                {
                    point = userPoint;
                }

                using (var ts = new Transaction(doc, "Copying of group"))
                {
                    ts.Start();

                    doc.Create.PlaceGroup(point, group.GroupType);

                    ts.Commit();
                }

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;                
            }

            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;                
            }

        }

        public XYZ GetElementCenter(Element elem)
        {
            BoundingBoxXYZ boundingBox = elem.get_BoundingBox(null);
            return (boundingBox.Min + boundingBox.Max) / 2;
        }

        public Room GetRoomByPoint(Document doc, XYZ point)
        {
            var collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms);
            foreach (var elem in collector)
            {
                var room = elem as Room;
                if (room != null)
                {
                    if (room.IsPointInRoom(point))
                    {
                        return room;
                    }
                }
            }

            return null;
        }
    }

    public class GroupFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is Group;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
