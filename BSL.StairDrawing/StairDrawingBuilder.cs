using ATS.TeklaCore.Extensions;
using ATS.TeklaCore.Models;
using ATS.TeklaCore.Services;
using BSL.StairDrawing.Models;
using BSL.StairDrawing.ViewModels;
using System.Data;
using System.Diagnostics;
using System.Runtime.Remoting.Lifetime;
using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Solid;
using static Tekla.Structures.Drawing.StraightDimensionSet;

namespace BSL.StairDrawing
{
    public class StairDrawingBuilder
    {
        private StairDrawingViewModel Data;
        private AssemblyDrawing Drawing;
        private Assembly DrawingAssembly;
        private StairModel StairModel;

        private Tekla.Structures.Drawing.View TopView;
        private Tekla.Structures.Drawing.View FrontView;
        private Tekla.Structures.Drawing.View DimensionalView;
        private Tekla.Structures.Drawing.View DetailView;

        public DimensionBase TopLengthDimension;
        public List<DimensionBase> TopWidthDimensions;
        public DimensionSetBase TopBoltDimensions;
        public DimensionSetBase TopAngleBoltDimensions;

        public List<DimensionSetBase> FrontStepDimensions;
        public List<DimensionSetBase> FrontSringerDimensions;
        public List<DimensionSetBase> FrontBoltDimensions;

        public StairDrawingBuilder(StairDrawingViewModel viewModel, AssemblyDrawing drawing)
        {
            this.Data = viewModel;
            this.Drawing = drawing;
            this.DrawingAssembly = new TeklaService().GetModel().SelectModelObject(this.Drawing.AssemblyIdentifier) as Assembly;
            this.StairModel = new StairModel(this.DrawingAssembly, viewModel);
            SetViews();
        }

        public bool Build()
        {
            bool result = true;
            var teklaService = new TeklaService();
            var plane = teklaService.GetCurrentPlane();
            try
            {
                CreateFrontDimensions();
                CreateTopDimensions();
                if (Data.CreateDetailView)
                    CreateDetailView();
                OrderViews();
                this.Drawing.PlaceViews();
            }
            catch (Exception ex)
            {
                teklaService.SetWorkPlane(plane);
                throw (ex);
            }
                   

            return result;
        }

        private List<ContourPlate> GetStringers(List<Tekla.Structures.Model.ContourPlate> stringers, string position)
        {
            double tolerance = 0.001;
            List<ContourPlate> resultPlates = new List<ContourPlate>();
            var maxY = stringers.Max(str => str.Contour.ContourPoints.Cast<Point>().Max(p => p.Y));

            var stringersWithMaxY = stringers
            .Select(str => new
            {
                Stringer = str,
                MaxY = str.Contour.ContourPoints.Cast<Point>().Max(p => p.Y)
            })
            .ToList();
            var globalMaxY = stringersWithMaxY.Max(x => x.MaxY);

            if (position.Equals("TOP", StringComparison.OrdinalIgnoreCase))
            {
                return stringersWithMaxY
                    .Where(x => Math.Abs(x.MaxY - globalMaxY) < tolerance)
                    .Select(x => x.Stringer)
                    .ToList();
            }
            else
            {
                return stringersWithMaxY
                    .Where(x => Math.Abs(x.MaxY - globalMaxY) >= tolerance)
                    .Select(x => x.Stringer)
                    .ToList();
            }

        }


        public bool CreateTopDimensions()
        {
            TopView.ClearDimensions();
            bool result = true;
            var topViewCs = TopView.ViewCoordinateSystem;
            using (new WorkPlaneScope(topViewCs))
            {
                var stringers = this.DrawingAssembly.GetPartsByName(Data.StringerNamesList.Names).Cast<ContourPlate>().ToList();
                var steps = this.DrawingAssembly.GetPartsByName(Data.TreadNamesList.Names);
                var angles = this.DrawingAssembly.GetPartsByName(Data.AngleNamesList.Names);
                steps.ForEach(s=>s.Select());
                angles.ForEach(a=>a.Select());
                stringers.ForEach(s=>s.Select());

                StraightDimension highLengthDimension;
                StraightDimension lowLengthDimension;

                List<ContourPlate> highPlates = GetStringers(stringers, "TOP");
                List<ContourPlate> lowPlates = GetStringers(stringers, "BOT");

                if (highPlates.Count() ==0|| lowPlates.Count() ==0)
                    throw new Exception("Could not find stringers in Top View");

                
                    //Add Length Dimension
                    highLengthDimension = InsertLengthDimension(highPlates, true);
                    lowLengthDimension = InsertLengthDimension(lowPlates, false);

                //Add Width Dimensions
                if (Data.CreateTopWidthDimensions)
                {
                    var dimLeft = AddDimension(TopView, lowLengthDimension.StartPoint, highLengthDimension.StartPoint, Vectors.UnitX * -1);
                    var dimRight = AddDimension(TopView, lowLengthDimension.EndPoint, highLengthDimension.EndPoint, Vectors.UnitX);
                    this.TopWidthDimensions = new List<DimensionBase>() { dimLeft, dimRight };
                }
                
                //Add Bolt Dimensions
                if (Data.CreateTopBoltDimensions)
                {
                    var boltPositions = GetTreadBolts(steps).ToList();
                    if (boltPositions.Count > 0)
                    {
                        boltPositions.Insert(0, lowLengthDimension.StartPoint);
                        boltPositions.Add(highLengthDimension.StartPoint);

                        var pList = new PointList();
                        foreach (var bp in boltPositions)
                        {
                            pList.Add(bp);
                        }

                        this.TopBoltDimensions = AddDimensionSet(TopView, pList, Vectors.UnitX * -1);
                    }
                }

                if (Data.CreateTopAngleBoltDimensions)
                {
                    var boltPositions = GetTreadBolts(angles).ToList();
                    if (boltPositions.Count > 0)
                    {
                        boltPositions.Insert(0, lowLengthDimension.StartPoint);
                        boltPositions.Add(highLengthDimension.StartPoint);

                        var pList = new PointList();
                        foreach (var bp in boltPositions)
                        {
                            pList.Add(bp);
                        }

                        this.TopAngleBoltDimensions = AddDimensionSet(TopView, pList, Vectors.UnitX * -1);
                    }
                }

                lowLengthDimension.Delete();
                if(!Data.CreateTLengthDim)
                {
                    highLengthDimension.Delete();
                }
                else
                {
                    this.TopLengthDimension = highLengthDimension;
                }
                    return TopView.Modify();
            }

        }
             

        public bool CreateFrontDimensions()
        {
            bool result = true;
            this.FrontView.ClearDimensions();


            //Step dimensions
            if (Data.CreateFrontStepDimensions)
            {
                using (new WorkPlaneScope(FrontView.ViewCoordinateSystem))
                {

                    var segment = GetMainSegments().OrderByDescending(s => s.Length()).First();
                    var segmentDirection = segment.GetDirectionVector();
                    var axisY = segmentDirection.GetPerpendicular();
                    var newCS = new CoordinateSystem(new Point(), segmentDirection, axisY);

                    List<Point> topPoints = new List<Point>();
                    List<Point> botPoints = new List<Point>();
                    using (new WorkPlaneScope(newCS))
                    {

                        var stringers = this.DrawingAssembly.GetPartsByName(Data.StringerNamesList.Names);
                        var steps = this.DrawingAssembly.GetPartsByName(Data.TreadNamesList.Names);

                        stringers.ForEach(s => s.Select());
                        steps.ForEach(s => s.Select());

                        var orderedSteps = steps.OrderBy(s => s.GetSolid().MinimumPoint.X).ToList();
                        var stepPoints = new List<Point>();

                        int i = 0;
                        foreach (var step in steps)
                        {
                            List<Point> points = new List<Point>();
                            if(step is PolyBeam polyStep)
                            {
                                points  = polyStep.Contour.ContourPoints.Cast<Point>().ToList();
                                if (i == 0)
                                {
                                    points.RemoveAt(1);
                                }
                                else
                                {
                                    points.RemoveAt(0);
                                }
                            }
                            if (step is Beam beamStep)
                            {
                                points = new List<Point>() { beamStep.StartPoint, beamStep.EndPoint };
                            }
                            i++;
                                stepPoints.AddRange(points);
                        }

                        stepPoints.ForEach(sp => sp.Z = 0);

                        var averageY = stepPoints.Average(p => p.Y);
                        topPoints = stepPoints.Where(p => p.Y > averageY).ToList();
                        botPoints = stepPoints.Where(p => p.Y < averageY).ToList();

                    }
                    var matrix = MatrixFactory.FromCoordinateSystem(newCS);

                    var topSet = AddDimensionSet(FrontView, matrix.Transform(topPoints).ToList().ToPointList(), axisY);
                    var botSet = AddDimensionSet(FrontView, matrix.Transform(botPoints).ToList().ToPointList(), axisY * -1);

                    FrontStepDimensions = new List<DimensionSetBase>() { topSet, botSet };
                    result &= FrontView.Modify();

                }
            }
            if (Data.CreateFrontStringerDimensions)
            {
                this.FrontSringerDimensions = new List<DimensionSetBase>();
                using (new WorkPlaneScope(FrontView.ViewCoordinateSystem))
                {
                    var allSegments = GetMainSegments();
                    var maxZ = allSegments.Max(seg => seg.Point1.Z);

                    var segments = allSegments.Where(seg=>seg.Point1.Z == maxZ).OrderByDescending(s => s.Length()).Skip(2);
                    foreach (var segment in segments)
                    {

                        FrontSringerDimensions.Add(AddDimensionSet(FrontView, segment.GetPoints().ToPointList(), segment.GetDirectionVector().GetPerpendicular()*-1));
                    }

                }
                result &= FrontView.Modify();
            }


            if (Data.CreateFrontBoltDimensions)
            {
                this.FrontBoltDimensions = new List<DimensionSetBase>();
                using (new WorkPlaneScope(FrontView.ViewCoordinateSystem))
                {
                    var mainPart = this.DrawingAssembly.GetMainPart() as ContourPlate;
                    var bolts = mainPart.GetBolts().ToList<BoltGroup>();
                    var segments = GetMainSegments().ToList();
                    foreach(var bolt in bolts)
                    {
                        var boltVector = new Vector(bolt.SecondPosition - bolt.FirstPosition).GetNormal(); ;
                       
                        var parallelSegments = segments.Where(s => Tekla.Structures.Geometry3d.Parallel.VectorToVector(s.GetDirectionVector(), boltVector));
                        var closestSegment = parallelSegments.OrderBy(p => Distance.PointToLine(bolt.FirstPosition, p.ToLine())).First();

                        var positions = bolt.BoltPositions.Cast<Point>().ToList();
                        foreach (var position in positions)
                        {
                            position.Z = 0;
                        }

                        var crossingPoints = GetCrossingSegmentPoints(segments, new Tekla.Structures.Geometry3d.Line(bolt.FirstPosition, bolt.SecondPosition));

                        positions.AddRange(crossingPoints);


                        this.FrontBoltDimensions.Add(AddDimensionSet(FrontView, positions.ToPointList(), Vectors.UnitX));

                        var boltPosition1 = positions.First();
                        var projection1 = Projection.PointToLine(boltPosition1, closestSegment.ToLine());
                        var sideDimensionList = new PointList();
                        sideDimensionList.Add(boltPosition1);
                        sideDimensionList.Add(projection1);

                        this.FrontBoltDimensions.Add(AddDimensionSet(FrontView, sideDimensionList, Vectors.UnitY));

                    }

                }
                        result &= FrontView.Modify();
            }

            return result;
        }

        private void DrawLine(Tekla.Structures.Drawing.View view, LineSegment segment)
        {
            var line = new Tekla.Structures.Drawing.Line(view, segment.StartPoint, segment.EndPoint);
            line.Insert();
        }

        private List<Point> GetCrossingSegmentPoints(List<LineSegment> segments, Tekla.Structures.Geometry3d.Line line)
        {
            var result = new List<Point>();
            foreach (var item in segments)
            {
                item.StartPoint.Z = 0;
                item.EndPoint.Z = 0;
                line.Origin.Z = 0;
                line.Direction.Z = 0;
                var intersection = Intersection.LineToLine(item.ToLine(), line);
                if (intersection != null && intersection.Length() < 0.001)
                {
                    if (item.IsPointOnLineSegment(intersection.Point1))
                    {
                        result.Add(intersection.Point1);
                    }
                }
            }
            return result;
        }



      

        private StraightDimension AddDimension(Tekla.Structures.Drawing.View view, Point p1, Point p2, Vector direction) {

            var lengthDimension = new StraightDimension(view, p1,p2, direction, 100);
            lengthDimension.Insert();
            return lengthDimension;
        }

        private List<Point> GetTreadBolts(List<Tekla.Structures.Model.Part> treads)
        {
            var result = new List<Point>();
            foreach (var tread in treads)
            {
                var bolts= tread.GetBolts().ToList<BoltGroup>();
                foreach(var bolt in bolts)
                {
                    result.AddRange(bolt.BoltPositions.Cast<Point>());
                }


               }
            return result;
        }



        private StraightDimension InsertLengthDimension(List<ContourPlate> plates, bool maxY)
        {
            plates.ForEach(p => p.Select());
            List<LineSegment> segments = new List<LineSegment>();
            plates.ForEach(p=>segments.AddRange(GetAllEdges(p)));
            var platePoints = segments.SelectMany(pp => pp.GetPoints());

            Point minPoint = null;
            Point maxPoint = null;

            if (maxY)
            {
                //The higher plate
                minPoint = platePoints.OrderBy(p => p.X).ThenByDescending(p => p.Y).First();
                maxPoint = platePoints.OrderBy(p => p.X).ThenBy(p => p.Y).Last();

            }
            else
            {
                //The lower plate
                minPoint = platePoints.OrderBy(p => p.X).ThenBy(p => p.Y).First();
                maxPoint = platePoints.OrderBy(p => p.X).ThenByDescending(p => p.Y).Last();
            }
                var lengthDimension = new StraightDimension(TopView, minPoint, maxPoint, Vectors.UnitY, 100);
            lengthDimension.Insert();
            return lengthDimension;
        }

        private void SetViews()
        {
            var views = this.Drawing.GetSheet().GetAllViews().ToList<Tekla.Structures.Drawing.View>();
            TopView = views.Where(x => x.ViewType == Tekla.Structures.Drawing.View.ViewTypes.TopView).FirstOrDefault();
            FrontView = views.Where(x => x.ViewType == Tekla.Structures.Drawing.View.ViewTypes.FrontView).FirstOrDefault();
            DimensionalView = views.Where(x => x.ViewType == Tekla.Structures.Drawing.View.ViewTypes._3DView).FirstOrDefault();

            if (TopView == null)
                throw new Exception("No top view found in the drawing.");

            if (FrontView == null)
                throw new Exception("No front view found in the drawing.");

            if (DimensionalView == null)
                throw new Exception("No 3D view found in the drawing.");
        }

        public void OrderViews()
        {

            AlignFrontView();
            //TODO: Do we need to rotate detail view as well?
            //TODO: Figure out 3D view alignment
            Align3DView();

        }

        public void Align3DView()
        {

            DimensionalView.DisplayCoordinateSystem = Helpers.RotateCoordinateSystem(DimensionalView.ViewCoordinateSystem, 20, Vectors.UnitX);
            DimensionalView.DisplayCoordinateSystem = Helpers.RotateCoordinateSystem(DimensionalView.DisplayCoordinateSystem, 140, Vectors.UnitZ);
            DimensionalView.Modify();
        }


        public void CreateDetailView()
        {
            var frontViewCs = FrontView.DisplayCoordinateSystem;
            var drawingParts = this.GetModelParts(FrontView.GetAllObjects().ToList<Tekla.Structures.Drawing.Part>());
            var treads = drawingParts
                .Where(dp=>Data.TreadNamesList.Names.Contains(dp.ModelPart.Name))
                .Where(dp=>dp.ModelPart is PolyBeam)
                .Select(dp=>dp.ModelPart)
                .Cast<PolyBeam>()
                .ToList();

            var matrix = MatrixFactory.ToCoordinateSystem(frontViewCs);
            var orderedTreads = treads.OrderBy(t => matrix.Transform((t.Contour.ContourPoints[1] as Point)).X);

            var count = orderedTreads.Count();
            int midId = count / 2;
            var previousTread = orderedTreads.ElementAt(midId - 1);
            var midTread = orderedTreads.ElementAt(midId);
            var nextTread = orderedTreads.ElementAt(midId + 1);

            var centerPoint = matrix.Transform((midTread.Contour.ContourPoints[1] as Point));
            var firstPoint = matrix.Transform((previousTread.Contour.ContourPoints[1] as Point));
            var lastPoint = matrix.Transform((nextTread.Contour.ContourPoints[0] as Point));
            var point0 = matrix.Transform((midTread.Contour.ContourPoints[0] as Point));
            var point2 = matrix.Transform((midTread.Contour.ContourPoints[2] as Point));

            var distance = Math.Max(Distance.PointToPoint(centerPoint, firstPoint), Distance.PointToPoint(centerPoint, lastPoint))  ;


            var points = new List<Point> { firstPoint , lastPoint, centerPoint, point0, point2};
            var averageY = points.Average(p => p.Y);
            var averageX = points.Average(p => p.X);
            foreach (var p in points)
            {
                if (p.Y > averageY)
                {
                    p.Y = p.Y + 50;
                }
                else
                {
                    p.Y = p.Y - 50;
                }

                if (p.X > averageX)
                {
                    p.X = p.X + 50;
                }
                else
                {
                    p.X = p.X - 50;
                }
            }


            var aabb = new AABB(points);
             

            var center = aabb.GetCenterPoint();


            DetailMark.DetailMarkAttributes detAttr = new DetailMark.DetailMarkAttributes(Data.DetailMarkAttributes);
            detAttr.BoundaryShape = DetailMark.DetailMarkAttributes.DetailBoundaryShape.Circular;
            detAttr.BoundingLine.Color = DrawingColors.Red;
            detAttr.MarkName = Data.DetailName;
            Tekla.Structures.Drawing.View.ViewAttributes viewAttr = new Tekla.Structures.Drawing.View.ViewAttributes(Data.DetailViewAttributes);
            Tekla.Structures.Drawing.DetailMark detMark;
            Tekla.Structures.Drawing.View detView;


            var Origin = FrontView.Origin.Move(Vectors.UnitX, FrontView.Width/2).Move(Vectors.UnitY *-1 , FrontView.Height/2);
            

            Tekla.Structures.Drawing.View.CreateDetailView(FrontView, center, aabb.MaxPoint, aabb.MaxPoint.Move(Vectors.UnitY, 100).Move(Vectors.UnitX, 100), 
                Origin, viewAttr, detAttr, out detView, out detMark);

            detView.Origin = detView.Origin.Move(Vectors.UnitY * -1, detView.Height / 2);

            var service = new TeklaService();
            var plane = service.GetCurrentPlane();
            this.DetailView = detView;
            if(Data.CreateDetailDimensions)
                AddDetailDimensions(detView, midTread, previousTread, nextTread);
        }
        
        private void AddDetailDimensions(Tekla.Structures.Drawing.View detView, PolyBeam midTread, PolyBeam previousTread, PolyBeam nextTread)
        {

            using (new WorkPlaneScope(detView.ViewCoordinateSystem))
            {
                var drawingParts = this.GetModelParts(detView.GetModelObjects().ToList<Tekla.Structures.Drawing.Part>());
                var steps = drawingParts.Where(dp => Data.TreadNamesList.Names.Contains(dp.ModelPart.Name));
                var stringers = drawingParts.Where(dp => Data.StringerNamesList.Names.Contains(dp.ModelPart.Name));
                var matrix = MatrixFactory.ToCoordinateSystem(detView.ViewCoordinateSystem);
                midTread.Select();
                previousTread.Select();
                nextTread.Select();
                var p1 = (midTread.Contour.ContourPoints[0] as Point);
                var p2 = (midTread.Contour.ContourPoints[1] as Point);
                var p3 = (midTread.Contour.ContourPoints[2] as Point);
                var p4 = (previousTread.Contour.ContourPoints[1] as Point);
                var p5 = (nextTread.Contour.ContourPoints[0] as Point);

                //AddDimension(detView, p1, p2, Vectors.UnitY);
                //AddDimension(detView, p2, p3, Vectors.UnitX);

                //TODO: GetDirectionVector
                var v1 = new Vector(p4 - p2).GetNormal();
                var v2 = new Vector(p5 - p1).GetNormal();
                var segments = GetMainSegments();
                var mainSegment1 = segments.OrderByDescending(s => s.Length()).First();
                var mainSegment2 = segments.OrderByDescending(s => s.Length()).ElementAt(1);

                var projectionP2 = GetProjection(mainSegment1, mainSegment2, p2, out Tekla.Structures.Geometry3d.Line lineP2);
                var projectionP1 = GetProjection(mainSegment1, mainSegment2, p1, out Tekla.Structures.Geometry3d.Line lineP1);

                AddDimensionSet(detView, new List<Point>() { p2, p4 }.ToPointList(), new Vector(p2 - projectionP2) *-1);
                AddDimensionSet(detView, new List<Point>() { p1, p5 }.ToPointList(), new Vector(p1 - projectionP1) *-1);
                AddDimensionSet(detView, new List<Point>() { p2, p4 }.ToPointList(), Vectors.UnitX);



                var projection1 = GetProjection(mainSegment1, mainSegment2, p3, out Tekla.Structures.Geometry3d.Line line1);
                var projection2 = GetProjection(mainSegment1, mainSegment2, p4, out Tekla.Structures.Geometry3d.Line line2);


               
                AddDimensionSet(detView, new List<Point>() { p3, projection1 }.ToPointList(), line1.Direction *-1);
                AddDimensionSet(detView, new List<Point>() { p4, projection2 }.ToPointList(), line2.Direction *-1);

                detView.Modify();
            }




        }

        private Point GetProjection(LineSegment segment1, LineSegment segment2, Point point, out Tekla.Structures.Geometry3d.Line line)
        {
            var line1 = new Tekla.Structures.Geometry3d.Line(segment2.StartPoint, segment2.EndPoint);
            var line2 = new Tekla.Structures.Geometry3d.Line(segment1.StartPoint, segment1.EndPoint);

            var projection1 = Projection.PointToLine(point, line1);
            var projection2 = Projection.PointToLine(point, line2);

            var dist1 = Distance.PointToPoint(point, projection1);
            var dist2 = Distance.PointToPoint(point, projection2);

            if(dist1< dist2)
            {
                line = line1;
                return projection1;
            }
            else
            {
                line = line2;
                return projection2;

            }

        }

        private StraightDimensionSet AddDimensionSet(Tekla.Structures.Drawing.View view, PointList points, Vector direction)
        {
            var attributes = new StraightDimensionSetAttributes("standard");

            var handler = new StraightDimensionSetHandler();
            var set = handler.CreateDimensionSet(view, points, direction, 1, attributes);
            return set;

        }


        List<LineSegment> GetcontourSegments(ContourPlate plate = null)
        {
            ContourPlate mainPart;
            mainPart = plate;
            if (plate == null)
            {
                mainPart = this.DrawingAssembly.GetMainPart() as ContourPlate;
            }
            mainPart.Select();
            var points = mainPart.Contour.ContourPoints.Cast<Point>().ToList();
            var segments = new List<LineSegment>();
            for (int i = 0; i < points.Count; i++)
            {
                var p1 = points[i];
                var p2 = points[(i + 1) % points.Count];
                segments.Add(new LineSegment(p1, p2));
            }

            return segments;
        }


        private List<LineSegment> GetFaceSegments(Face face)
        {
            var loops = face.GetLoopEnumerator();
            loops.MoveNext();
            var loop = loops.Current as Loop;
            var points = loop.GetVertexEnumerator().ToList();

            var segments = new List<LineSegment>();
            for (int i = 0; i < points.Count; i++)
            {
                var p1 = points[i];
                var p2 = points[(i + 1) % points.Count];
                segments.Add(new LineSegment(p1, p2));
            }

            return CombineCollinearSegments(segments);
        }


        List<LineSegment> GetAllEdges(ContourPlate part)
        {
            var edges = new List<LineSegment>();
            part.Select();
            var solid = part.GetSolid();
            var edgeSegments = solid.GetEdgeEnumerator().ToList();
            return edgeSegments;

        }


        List<LineSegment> GetMainSegments(ContourPlate part = null)
        {

            var mainPart = this.DrawingAssembly.GetMainPart() as ContourPlate;
            if (part != null)
            {
                mainPart = part;
            }
            mainPart.Select();

            var stringers = this.DrawingAssembly.GetPartsByName(Data.StringerNamesList.Names);
            var result = new List<LineSegment>();
            if(stringers.Count > 2)
            {
                //In this case there is no single plate on the side but multiple ones. We need to get the proper line segments.
                foreach(var stringer in stringers)
                {
                    stringer.Select();
                    
                    result.AddRange(GetFaceSegments(GetOuterFace(stringer)));
                }
                var maxZ = result.Max(ls => ls.Point1.Z);
                var returnList = result.Where(ls => ls.Point1.Z == maxZ).ToList();
                var equalSegments = new List<LineSegment>();
                foreach (var line in returnList)
                { 
                    if(returnList.Any(l=>l.StartPoint == line.EndPoint && l.EndPoint == line.StartPoint))
                    {
                        equalSegments.Add(line);
                    }
                }
                return returnList.Except(equalSegments).ToList();
            }

           
            result.AddRange(GetFaceSegments(GetOuterFace(mainPart)));
            return result;
           
        }

        private Face GetOuterFace(Tekla.Structures.Model.Part part)
        {
            var solid = part.GetSolid(Solid.SolidCreationTypeEnum.HIGH_ACCURACY);
            var faces = solid.GetFaceEnumerator().ToList().OrderByDescending(f => f.GetArea());
            Face mainFace = null;
            var largeFaces = faces.Take(2);
            var outerFace = largeFaces.OrderByDescending(f => f.GetFaceCenterPoint().Z).First();
            return outerFace;
        }


        private void AlignFrontView()
        {
            //we get the mainpart from model. Convert thw points that should be horizontal to drawing coordinates.
            //If the line is parallel to X axis then it is ok. we do not rotate.
            var frontViewCs = FrontView.DisplayCoordinateSystem;

            var plate = this.DrawingAssembly.GetMainPart() as ContourPlate;
            var point1 = plate.Contour.ContourPoints[0] as Point;
            var point2 = plate.Contour.ContourPoints[1] as Point;
            var point3 = plate.Contour.ContourPoints[2] as Point;


            var matrix = MatrixFactory.ToCoordinateSystem(frontViewCs);

            var p1d = matrix.Transform(point1);
            var p2d = matrix.Transform(point2);
            var p3d = matrix.Transform(point3);

            var mainVector = new Vector(p3d - p2d).GetNormal();
            if (!Tekla.Structures.Geometry3d.Parallel.VectorToVector(mainVector, Vectors.UnitX))
            {
                FrontView.RotateViewOnDrawingPlane(this.StairModel.StairAngle * -1);
            }
            if(DetailView!=null)
                DetailView.RotateViewOnDrawingPlane(this.StairModel.StairAngle * -1);


        }

        List<LineSegment> CombineCollinearSegments(List<LineSegment> segments)
        {
            var result = new List<LineSegment>();

            foreach (var seg in segments)
            {
                bool merged = false;

                for (int i = 0; i < result.Count; i++)
                {
                    if (AreCollinear(result[i], seg))
                    {
                        result[i] = MergeSegments(result[i], seg);
                        merged = true;
                        break;
                    }
                }

                if (!merged)
                    result.Add(seg);
            }

            return result;
        }

        bool AreCollinear(LineSegment s1, LineSegment s2)
        {
            Vector v1 = new Vector(s1.EndPoint - s1.StartPoint);
            Vector v2 = new Vector(s2.EndPoint- s2.StartPoint);

            if (!Tekla.Structures.Geometry3d.Parallel.VectorToVector(v1, v2))
                return false;

            return IsPointOnLine(s2.Point1, s1.Point1, v1);
        }


        LineSegment MergeSegments(LineSegment s1, LineSegment s2)
        {
            Vector dir = new Vector(
                s1.Point2.X - s1.Point1.X,
                s1.Point2.Y - s1.Point1.Y,
                s1.Point2.Z - s1.Point1.Z);

            dir.Normalize();

            // Project points onto line
            double Project(Point p)
            {
                return (p.X - s1.Point1.X) * dir.X +
                       (p.Y - s1.Point1.Y) * dir.Y +
                       (p.Z - s1.Point1.Z) * dir.Z;
            }

            var points = new List<Point>
    {
        s1.Point1, s1.Point2,
        s2.Point1, s2.Point2
    };

            var ordered = points
                .OrderBy(p => Project(p))
                .ToList();

            return new LineSegment(ordered.First(), ordered.Last());
        }


        bool IsPointOnLine(Point p, Point lineStart, Vector lineDir, double tolerance = 1e-6)
        {
            Vector v = new Vector(p.X - lineStart.X,
                                  p.Y - lineStart.Y,
                                  p.Z - lineStart.Z);

            var cross = Vector.Cross(v, lineDir);
            return cross.GetLength() < tolerance;
        }

        bool AreParallel(Vector v1, Vector v2, double tolerance = 1e-6)
        {
            var cross = Vector.Cross(v1, v2);
            return cross.GetLength() < tolerance;
        }




        private List<DrawingPartModel> GetModelParts(List<Tekla.Structures.Drawing.Part> drawingParts)
        {
            var result = new List<DrawingPartModel>();
            foreach (var dp in drawingParts)
            {
                result.Add(new DrawingPartModel(dp));
            }
            return result;
        }









    }
}
