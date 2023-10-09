using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using SaveLinesInOtherDocument.Models.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveLinesInOtherDocument.Models
{
    public class RevitGeometryUtils
    {
        // Метод для получения directshape линий
        public static List<DirectShape> GetLinseBySelection(UIApplication uiapp, out string elementIds)
        {
            Selection sel = uiapp.ActiveUIDocument.Selection;
            var pickedLines = sel.PickElementsByRectangle(new DirectShapeClassFilter(), "Выберете линии").OfType<DirectShape>().ToList();
            elementIds = ElementIdToString(pickedLines);

            return pickedLines;
        }

        // Метод получения строки с ElementId
        private static string ElementIdToString(IEnumerable<Element> elements)
        {
            var stringArr = elements.Select(e => "Id" + e.Id.IntegerValue.ToString()).ToArray();
            string resultString = string.Join(", ", stringArr);

            return resultString;
        }
    }
}
