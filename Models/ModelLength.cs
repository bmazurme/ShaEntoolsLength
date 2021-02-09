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
            ICollection<ElementId> selIds = uidoc.Selection.GetElementIds();
            // Create containers for sizes and lengths
            List<Element> lstEls = new List<Element>();
            List<double> lstSize = new List<double>();
            List<double> lstLeng = new List<double>();
            List<double> lstArea = new List<double>();
            List<int> lstCount = new List<int>();             
            const BuiltInParameter bipLeng = BuiltInParameter.CURVE_ELEM_LENGTH;
            const BuiltInParameter bipArea = BuiltInParameter.RBS_CURVE_SURFACE_AREA;
            const BuiltInParameter bipDiam = BuiltInParameter.RBS_PIPE_DIAMETER_PARAM;

            // Check count selected elements, if count == 0, than skip
            if (selIds.Count() > 0)
            {
                foreach (ElementId elmntId in selIds)
                {
                    // Get elements from elements Id
                    Element elmnt = doc.GetElement(elmntId);
                    Category cat = elmnt.Category;
                    BuiltInCategory bic = (BuiltInCategory)cat.Id.IntegerValue;
                    // Filters category
                    if (bic == BuiltInCategory.OST_PipeCurves) lstEls.Add(elmnt);
                }

                List<Element> tmpLstSiz = lstEls.GroupBy(x => x.get_Parameter(bipDiam).AsDouble()).Select(y => y.First()).ToList();
                              tmpLstSiz = tmpLstSiz.OrderBy(x => x.get_Parameter(bipDiam).AsDouble()).ToList();
                List<Element> newList = lstEls.OrderBy(x => x.get_Parameter(bipDiam).AsDouble()).ToList();

                foreach (Element element in tmpLstSiz)                
                    lstSize.Add(element.get_Parameter(bipDiam).AsDouble());                

                foreach (double size in lstSize)
                {
                    double leng = 0, area = 0;
                    int count = 0;

                    foreach (Element element in newList)
                    {
                        if (element.get_Parameter(bipDiam).AsDouble() == size)
                        {
                            leng += element.get_Parameter(bipLeng).AsDouble();
                            area += element.get_Parameter(bipArea).AsDouble();
                            count++;
                        }
                    }

                    leng *= 304.8; // Units ft -> mm
                    area = area * 304.8 * 304.8 / 1000000;
                    lstLeng.Add(leng);
                    lstArea.Add(area);
                    lstCount.Add(count);
                }

                Transfers.Diameter = lstSize;
                Transfers.Length = lstLeng;
                Transfers.Area = lstArea;
                Transfers.Count = lstCount;

                // Show window result
                WindowPipeInfo winInfo = new WindowPipeInfo();
                winInfo.ShowDialog();
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