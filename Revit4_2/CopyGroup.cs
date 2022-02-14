using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            Reference refer = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Выберите группу");
            Element element = doc.GetElement(refer);

            var group = element as Group;

            XYZ point = uiDoc.Selection.PickPoint("Укажите точку вставки");

            using (var ts = new Transaction(doc, "Copying of group"))
            {
                ts.Start();

                doc.Create.PlaceGroup(point, group.GroupType);

                ts.Commit();
            }

            return Result.Succeeded;
        }
    }
}
