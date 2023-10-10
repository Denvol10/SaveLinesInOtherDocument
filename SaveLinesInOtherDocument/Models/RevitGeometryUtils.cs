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

        // Получение id элементов на основе списка в виде строки
        public static List<int> GetIdsByString(string elems)
        {
            if (string.IsNullOrEmpty(elems))
            {
                return null;
            }

            var elemIds = elems.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => int.Parse(s.Remove(0, 2)))
                         .ToList();

            return elemIds;
        }

        // Проверка на то существуют ли элементы с данным Id в модели
        public static bool IsElemsExistInModel(Document doc, IEnumerable<int> elems, Type type)
        {
            if (elems is null)
            {
                return false;
            }

            foreach (var elem in elems)
            {
                ElementId id = new ElementId(elem);
                Element curElem = doc.GetElement(id);
                if (curElem is null || !(curElem.GetType() == type))
                {
                    return false;
                }
            }

            return true;
        }

        // Получение линий по их id
        public static List<DirectShape> GetDirectShapeLinesById(Document doc, IEnumerable<int> ids)
        {
            var directShapeLines = new List<DirectShape>();
            foreach (var id in ids)
            {
                ElementId elemId = new ElementId(id);
                DirectShape line = doc.GetElement(elemId) as DirectShape;
                directShapeLines.Add(line);
            }

            return directShapeLines;
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
