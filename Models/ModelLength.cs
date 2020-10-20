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
            List<double> listSizes = new List<double>();
            List<double> listLength = new List<double>();
            List<double> listArea = new List<double>();
            List<int> listCount = new List<int>(); 
            List<Element> listElements = new List<Element>();
            double sumLength = 0;
            const BuiltInParameter lengthBuiltInParameter = BuiltInParameter.CURVE_ELEM_LENGTH;
            const BuiltInParameter areaBuiltInParameter = BuiltInParameter.RBS_CURVE_SURFACE_AREA;

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
                        sumLength += element.get_Parameter(lengthBuiltInParameter).AsDouble();
                    }
                }

                List<Element> tempListSizes = listElements.GroupBy(x => x.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble()).Select(y => y.First()).ToList();
                tempListSizes = tempListSizes.OrderBy(x => x.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble()).ToList();
                List<Element> newList = listElements.OrderBy(x => x.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble()).ToList();

                foreach (Element element in tempListSizes)
                {
                    listSizes.Add(element.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble());
                }

                foreach (double size in listSizes)
                {
                    double length = 0;
                    double area = 0;
                    int count = 0;

                    foreach (Element element in newList)
                    {
                        if (element.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble() == size)
                        {
                            length += element.get_Parameter(lengthBuiltInParameter).AsDouble();
                            area += element.get_Parameter(areaBuiltInParameter).AsDouble();
                            count++;
                        }
                    }

                    length *= 304.8; // Units ft -> mm
                    area = area * 304.8 * 304.8 / 1000000;
                    listLength.Add(length);
                    listArea.Add(area);
                    listCount.Add(count);
                }

                Transfers.Diameter = listSizes;
                Transfers.Length = listLength;
                Transfers.Area = listArea;
                Transfers.Count = listCount;

                // Show window result
                WindowPipeInfo windowPipeInfo = new WindowPipeInfo();
                windowPipeInfo.ShowDialog();
            }
            else TaskDialog.Show("Error", "Please select pipes.");
        }
    }


    // Transfer data
    public static class Transfers
    {
        public static List<double> Diameter { get; set; }
        public static List<double> Length { get; set; }
        public static List<double> Area { get; set; }
        public static List<int> Count { get; set; }
    }


    public class PipeDetail 
    {
        // Fields
        public double Diameter { get; set; }
        public double Length { get; set; }
        public double Area { get; set; }
        public int Count { get; set; }
    }
}