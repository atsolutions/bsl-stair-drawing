using ATS.TeklaCore.Extensions;
using ATS.TeklaCore.Geometry;
using ATS.TeklaCore.Models;
using BSL.StairDrawing.Enums;
using BSL.StairDrawing.ViewModels;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace BSL.StairDrawing.Models
{
    public class StairModel
    {
        private Assembly assembly;
        private StairDrawingViewModel viewModel;
        
        public List<Part> Stringers { get; set; }
        public StringerTypeEnum StringerType { get; set; }
        public double StairAngle { get; set; }

        public StairModel(Assembly assembly, StairDrawingViewModel viewModel)
        {
            this.assembly = assembly;
            this.viewModel = viewModel;
            LoadData();
        }

        private void LoadData()
        {
            Stringers = assembly.GetPartsByName(new NameList(viewModel.StringerNames).Names);
            if(Stringers.Count == 2)
            {
                StringerType = StringerTypeEnum.SINGLEPLATE;
            }
            else
            {
                StringerType = StringerTypeEnum.SINGLEPLATE;
            }
            LoadAngle();
        }

        private void LoadAngle()
        {
            if (StringerType == StringerTypeEnum.SINGLEPLATE)
            {
                var mainPart = this.assembly.GetMainPart() as ContourPlate;
                if (mainPart == null)
                    throw new Exception("Main part is not a contour plate.");

                var contour = mainPart.Contour;

                var point1 = contour.ContourPoints[0] as Point;
                var point2 = contour.ContourPoints[1] as Point;
                var point3 = contour.ContourPoints[2] as Point;

                var horizontalVector = new Vector(point2 - point1);
                var angledVector = new Vector(point3 - point2);


                var angle = horizontalVector.GetAngleBetween(angledVector);
                this.StairAngle = AngleCalculator.ToDegrees(angle);


            }
        }
    }
}
