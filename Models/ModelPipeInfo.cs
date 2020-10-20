using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Entools.Views;

namespace Entools.Model
{
    class PipeInfo
    {
        // Get information about selected pipes
        public void Main(ExternalCommandData revit)
        {
            UIApplication uiapp = revit.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            // Get Ids of selected elements
            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();

            // Create containers for sizes and lengths
            List<Double> listSizes = new List<double>();
            List<Double> listLength = new List<double>();
            List<Element> listElements = new List<Element>();

            double sumLength = 0;
            const BuiltInParameter lengthBuiltInParameter = BuiltInParameter.CURVE_ELEM_LENGTH;

            // Check count selected elements, if count == 0, than skip
            if (selectedIds.Count() > 0)
            {
                foreach (ElementId elementId in selectedIds)
                {
                    // Get elements from elements Id
                    Element element = doc.GetElement(elementId);
                    Category category = element.Category;
                    BuiltInCategory enumCategory = (BuiltInCategory)category.Id.IntegerValue;

                    // Filters category
                    if (enumCategory.ToString() == "OST_PipeCurves")
                    {
                        listElements.Add(element);
                        sumLength = sumLength + element.get_Parameter(lengthBuiltInParameter).AsDouble();
                    }
                }

                List<Element> tempListSizes = listElements.GroupBy(x => x.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM)
                                                                               .AsDouble()).Select(y => y.First()).ToList();
                tempListSizes = tempListSizes.OrderBy(x => x.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble()).ToList();
                List<Element> newList = listElements.OrderBy(x => x.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM)
                                                                                                     .AsDouble()).ToList();

                foreach (Element element in tempListSizes)
                {
                    listSizes.Add(element.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble());
                }

                foreach (double size in listSizes)
                {
                    double length = 0;

                    foreach (Element element in newList)
                    {
                        if (element.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble() == size)
                            length = length + element.get_Parameter(lengthBuiltInParameter).AsDouble();
                    }

                    // Units ft -> mm
                    length = length * 304.8;
                    listLength.Add(length);
                }

                Transfer.Diameter = listSizes;
                Transfer.Length = listLength;

                // Show window result
                WindowPipeInfo windowPipeInfo = new WindowPipeInfo();//listSizes, listLength);
                windowPipeInfo.ShowDialog();
            }
            else
            {
                // 
                TaskDialog.Show("Error", "Трубопроводы не выбраны.");
            }
        }
    }


    // Transfer data
    public static class Transfer
    {
        public static List<double> Diameter { get; set; }
        public static List<double> Length { get; set; }
    }


    public class PipeDetail 
    {
        // Fields
        public double Diameter { get; set; }
        public double Length { get; set; }
    }
}