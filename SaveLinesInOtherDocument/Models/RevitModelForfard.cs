using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Collections.ObjectModel;
using SaveLinesInOtherDocument.Models;
using Microsoft.Win32;

namespace SaveLinesInOtherDocument
{
    public class RevitModelForfard
    {
        private UIApplication Uiapp { get; set; } = null;
        private Application App { get; set; } = null;
        private UIDocument Uidoc { get; set; } = null;
        private Document Doc { get; set; } = null;

        public RevitModelForfard(UIApplication uiapp)
        {
            Uiapp = uiapp;
            App = uiapp.Application;
            Uidoc = uiapp.ActiveUIDocument;
            Doc = uiapp.ActiveUIDocument.Document;
        }

        public List<DirectShape> Lines { get; set; }

        private string _linesElemIds;
        public string LinesElemIds
        {
            get => _linesElemIds;
            set => _linesElemIds = value;
        }

        public void GetLinesBySelection()
        {
            Lines = RevitGeometryUtils.GetLinseBySelection(Uiapp, out _linesElemIds);
        }

        #region Проверка на то существуют линии оси и линии на поверхности в модели
        public bool IsLinesExistInModel(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);

            return RevitGeometryUtils.IsElemsExistInModel(Doc, elemIds, typeof(DirectShape));
        }
        #endregion

        #region Получение линий Settings
        public void GetLinesBySettings(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);
            Lines = RevitGeometryUtils.GetDirectShapeLinesById(Doc, elemIds);
        }
        #endregion

        #region Сохранение линий в файл
        public void SaveLines()
        {
            string docPath = GetDocumentPath();
            if (string.IsNullOrEmpty(docPath))
            {
                TaskDialog.Show("Ошибка", "Не выбран файл");
                return;
            }

            var docForSaving = App.OpenDocumentFile(docPath);
            ElementId categoryId = new ElementId(BuiltInCategory.OST_Lines);

            using (Transaction trans = new Transaction(docForSaving, "Create Lines"))
            {
                trans.Start();
                foreach(var line in Lines)
                {
                    var lineList = new List<GeometryObject>();
                    var curves = GetCurvesByDirectShape(line);
                    lineList.AddRange(curves);
                    DirectShape directShape = DirectShape.CreateElement(docForSaving, categoryId);
                    if (directShape.IsValidShape(lineList))
                    {
                        directShape.SetShape(lineList);
                    }
                }
                trans.Commit();

                var uiDocument = new UIDocument(docForSaving);
                if (uiDocument.GetOpenUIViews().Count == 0)
                {
                    docForSaving.Close();
                }
            }
        }
        #endregion

        private string GetDocumentPath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Revit files (*.rfa;*.rvt)|*.rfa;*.rvt";

            if (openFileDialog.ShowDialog() == true)
            {
                string familyPath = openFileDialog.FileName;
                return familyPath;
            }

            return string.Empty;
        }

        // Получение линий на основе элементов DirectShape
        private List<GeometryObject> GetCurvesByDirectShape(DirectShape directShape)
        {
            var curves = new List<GeometryObject>();

            Options options = new Options();
            var geometries = directShape.get_Geometry(options);

            foreach (var geom in geometries)
            {
                curves.Add(geom);
            }

            return curves;
        }

        // Метод получения списка линий на основе полилинии
        private IEnumerable<Curve> GetCurvesByPolyline(PolyLine polyLine)
        {
            var curves = new List<Curve>();

            for (int i = 0; i < polyLine.NumberOfCoordinates - 1; i++)
            {
                var line = Line.CreateBound(polyLine.GetCoordinate(i), polyLine.GetCoordinate(i + 1));
                curves.Add(line);
            }

            return curves;
        }
    }
}
