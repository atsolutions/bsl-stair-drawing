using ATS.TeklaCore.Models;
using ATS.TeklaCore.Services;
using BSL.StairDrawing;
using BSL.StairDrawing.Models;
using BSL.StairDrawing.ViewModels;
using FluentAssertions.Execution;
using RenderData;
using System.Diagnostics;
using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace StairDrawingTests
{
    public class DrawingTests
    {
        [Fact]
        public void TestViewOrdering()
        {
            var vm = new StairDrawingViewModel();
            vm.StringerNames = "STRINGER";
            vm.TreadNames = "STEP";
            var service = new TeklaDrawingService();
            var drawing = service.GetActiveDrawing() as AssemblyDrawing;
            if (drawing == null)
                Xunit.Assert.Fail("Active drawing is not an AssemblyDrawing.");

            var builder = new StairDrawingBuilder(vm, drawing);
            builder.OrderViews();
        }
        [Fact]
        public void TestStairModelLoading()
        {
            var vm = new StairDrawingViewModel();
            vm.StringerNames = "STRINGER";
            var service = new TeklaDrawingService();
            var drawing = service.GetActiveDrawing() as AssemblyDrawing;
            var model = new TeklaService().GetModel();
            var assembly = model.SelectModelObject(drawing.AssemblyIdentifier) as Assembly;
            if (assembly == null)
                Xunit.Assert.Fail("Assembly not found");

            var stairModel = new StairModel(assembly, vm);

        }

        [Fact]
        public void TestDetailViews()
        {
            var vm = new StairDrawingViewModel();
            vm.StringerNames = "STRINGER";
            vm.TreadNames = "STEP";
            var service = new TeklaDrawingService();
            var drawing = service.GetActiveDrawing() as AssemblyDrawing;
            var model = new TeklaService().GetModel();
            var assembly = model.SelectModelObject(drawing.AssemblyIdentifier) as Assembly;
            if (assembly == null)
                Xunit.Assert.Fail("Assembly not found");

            var stairModel = new StairModel(assembly, vm);
            var builder = new StairDrawingBuilder(vm, drawing);
            builder.CreateDetailView();

        }

        [Fact]
        public void TestTopViewDimensions()
        {
            var vm = new StairDrawingViewModel();
            vm.StringerNames = "STRINGER";
            vm.TreadNames = "STEP";
            vm.AngleNames = "ANGLE";
            vm.CreateTLengthDim = true;
            vm.CreateTopAngleBoltDimensions = true;
            vm.CreateTopBoltDimensions = true;
            vm.CreateTopWidthDimensions = true;
            var service = new TeklaDrawingService();
            var drawing = service.GetActiveDrawing() as AssemblyDrawing;
            var model = new TeklaService().GetModel();
            var assembly = model.SelectModelObject(drawing.AssemblyIdentifier) as Assembly;
            if (assembly == null)
                Xunit.Assert.Fail("Assembly not found");

            var stairModel = new StairModel(assembly, vm);
            var builder = new StairDrawingBuilder(vm, drawing);
            builder.CreateTopDimensions();

            var result = builder.CreateTopDimensions();

            Xunit.Assert.True(result, "Failed to create top view length dimension.");
            Xunit.Assert.Equal(builder.TopWidthDimensions.Count, 2);
            Xunit.Assert.NotNull(builder.TopAngleBoltDimensions);
            Xunit.Assert.NotNull(builder.TopLengthDimension);



        }


        [Fact]
        public void TestFrontViewDimensions()
        {
            var vm = new StairDrawingViewModel();
            vm.StringerNames = "STRINGER";
            vm.TreadNames = "STEP";
           


            var service = new TeklaDrawingService();
            var drawing = service.GetActiveDrawing() as AssemblyDrawing;
            var model = new TeklaService().GetModel();
            var assembly = model.SelectModelObject(drawing.AssemblyIdentifier) as Assembly;
            if (assembly == null)
                Xunit.Assert.Fail("Assembly not found");

            var stairModel = new StairModel(assembly, vm);
            var builder = new StairDrawingBuilder(vm, drawing);
            builder.CreateFrontDimensions();

        }


        [Fact]
        public void TestTopViewLengthDimension()
        {
            var vm = new StairDrawingViewModel();
            vm.StringerNames = "STRINGER";
            vm.TreadNames = "STEP";
            vm.AngleNames = "ANGLE";
            vm.CreateTLengthDim = true;
            vm.CreateTopBoltDimensions = false;
            vm.CreateTopWidthDimensions = false;

            var service = new TeklaDrawingService();
            var drawing = service.GetActiveDrawing() as AssemblyDrawing;
            var model = new TeklaService().GetModel();
            var assembly = model.SelectModelObject(drawing.AssemblyIdentifier) as Assembly;
            if (assembly == null)
                Xunit.Assert.Fail("Assembly not found");

            var stairModel = new StairModel(assembly, vm);
            var builder = new StairDrawingBuilder(vm, drawing);
            var result = builder.CreateTopDimensions();

            Xunit.Assert.True(result, "Failed to create top view length dimension.");
            Xunit.Assert.NotNull(builder.TopLengthDimension);


        }

        [Fact]
        public void TestTopViewWidthDimension()
        {
            var vm = new StairDrawingViewModel();
            vm.StringerNames = "STRINGER";
            vm.TreadNames = "STEP";
            vm.AngleNames = "ANGLE";
            vm.CreateTLengthDim = false;
            vm.CreateTopBoltDimensions = false;
            vm.CreateTopWidthDimensions = true;

            var service = new TeklaDrawingService();
            var drawing = service.GetActiveDrawing() as AssemblyDrawing;
            var model = new TeklaService().GetModel();
            var assembly = model.SelectModelObject(drawing.AssemblyIdentifier) as Assembly;
            if (assembly == null)
                Xunit.Assert.Fail("Assembly not found");

            var stairModel = new StairModel(assembly, vm);
            var builder = new StairDrawingBuilder(vm, drawing);
            var result = builder.CreateTopDimensions();

            Xunit.Assert.True(result, "Failed to create top view length dimension.");
            Xunit.Assert.Equal(builder.TopWidthDimensions.Count, 2);

        }


        [Fact]
        public void TestTopViewBoltDimension()
        {
            var vm = new StairDrawingViewModel();
            vm.StringerNames = "STRINGER";
            vm.TreadNames = "STEP ANGLE";
            vm.CreateTLengthDim = false;
            vm.CreateTopBoltDimensions = true;
            vm.CreateTopWidthDimensions = false;

            var service = new TeklaDrawingService();
            var drawing = service.GetActiveDrawing() as AssemblyDrawing;
            var model = new TeklaService().GetModel();
            var assembly = model.SelectModelObject(drawing.AssemblyIdentifier) as Assembly;
            if (assembly == null)
                Xunit.Assert.Fail("Assembly not found");

            var stairModel = new StairModel(assembly, vm);
            var builder = new StairDrawingBuilder(vm, drawing);
            var result = builder.CreateTopDimensions();

            Xunit.Assert.True(result, "Failed to create top view length dimension.");
            Xunit.Assert.NotNull(builder.TopBoltDimensions);

        }


        [Fact]
        public void TestAllTopViewDimensions()
        {
            var vm = new StairDrawingViewModel();
            vm.StringerNames = "STRINGER";
            vm.TreadNames = "STEP ANGLE";
            vm.CreateTLengthDim = true;
            vm.CreateTopBoltDimensions = true;
            vm.CreateTopWidthDimensions = true;

            var service = new TeklaDrawingService();
            var drawing = service.GetActiveDrawing() as AssemblyDrawing;
            var model = new TeklaService().GetModel();
            var assembly = model.SelectModelObject(drawing.AssemblyIdentifier) as Assembly;
            if (assembly == null)
                Xunit.Assert.Fail("Assembly not found");

            var stairModel = new StairModel(assembly, vm);
            var builder = new StairDrawingBuilder(vm, drawing);
            var result = builder.CreateTopDimensions();

            using (new AssertionScope()) {
                Xunit.Assert.True(result, "Failed to create top view dimension.");
                Xunit.Assert.NotNull(builder.TopBoltDimensions);
                Xunit.Assert.Equal(builder.TopWidthDimensions.Count, 2);
                Xunit.Assert.NotNull(builder.TopLengthDimension);
            }

        }

        [Fact]
        public void TestFrontStepViewDimensions()
        {
            var vm = new StairDrawingViewModel();
            vm.StringerNames = "STRINGER";
            vm.TreadNames = "STEP";
            vm.CreateFrontStepDimensions = true;
            vm.CreateFrontStringerDimensions = false;
            vm.CreateFrontBoltDimensions = false;

            var service = new TeklaDrawingService();
            var drawing = service.GetActiveDrawing() as AssemblyDrawing;
            var model = new TeklaService().GetModel();
            var assembly = model.SelectModelObject(drawing.AssemblyIdentifier) as Assembly;
            if (assembly == null)
                Xunit.Assert.Fail("Assembly not found");

            var stairModel = new StairModel(assembly, vm);
            var builder = new StairDrawingBuilder(vm, drawing);
            var result = builder.CreateFrontDimensions();

            using (new AssertionScope()) { 
                Xunit.Assert.True(result, "Failed to create front view step dimensions.");
                Xunit.Assert.Equal(builder.FrontStepDimensions.Count, 2);
            }

        }

        [Fact]
        public void TestFrontStringerViewDimensions()
        {
            var vm = new StairDrawingViewModel();
            vm.StringerNames = "STRINGER";
            vm.TreadNames = "STEP";
            vm.CreateFrontStepDimensions = false;
            vm.CreateFrontStringerDimensions = true;
            vm.CreateFrontBoltDimensions = false;

            var service = new TeklaDrawingService();
            var drawing = service.GetActiveDrawing() as AssemblyDrawing;
            var model = new TeklaService().GetModel();
            var assembly = model.SelectModelObject(drawing.AssemblyIdentifier) as Assembly;
            if (assembly == null)
                Xunit.Assert.Fail("Assembly not found");

            var stairModel = new StairModel(assembly, vm);
            var builder = new StairDrawingBuilder(vm, drawing);
            var result = builder.CreateFrontDimensions();

            using (new AssertionScope())
            {
                Xunit.Assert.True(result, "Failed to create front view step dimensions.");
                Xunit.Assert.Equal(6,builder.FrontSringerDimensions.Count);
            }

        }

        [Fact]
        public void TestFrontViewBoltDimensions()
        {
            var vm = new StairDrawingViewModel();
            vm.StringerNames = "STRINGER";
            vm.TreadNames = "STEP";
            vm.CreateFrontStepDimensions = false;
            vm.CreateFrontStringerDimensions = false;
            vm.CreateFrontBoltDimensions = true;

            var service = new TeklaDrawingService();
            var drawing = service.GetActiveDrawing() as AssemblyDrawing;
            var model = new TeklaService().GetModel();
            var assembly = model.SelectModelObject(drawing.AssemblyIdentifier) as Assembly;
            if (assembly == null)
                Xunit.Assert.Fail("Assembly not found");

            var stairModel = new StairModel(assembly, vm);
            var builder = new StairDrawingBuilder(vm, drawing);
            var result = builder.CreateFrontDimensions();

            using (new AssertionScope())
            {
                Xunit.Assert.True(result, "Failed to create front view step dimensions.");
                Xunit.Assert.Equal(2, builder.FrontBoltDimensions.Count);
            }

        }

        [Fact]
        public void TestDrawSegments()
        {
            var vm = new StairDrawingViewModel();
            vm.StringerNames = "STRINGER";
            vm.TreadNames = "STEP";
            vm.CreateFrontStepDimensions = false;
            vm.CreateFrontStringerDimensions = false;
            vm.CreateFrontBoltDimensions = true;

            var service = new TeklaDrawingService();
            var drawing = service.GetActiveDrawing() as AssemblyDrawing;
            var model = new TeklaService().GetModel();
            var assembly = model.SelectModelObject(drawing.AssemblyIdentifier) as Assembly;
            if (assembly == null)
                Xunit.Assert.Fail("Assembly not found");

            var stairModel = new StairModel(assembly, vm);
            var builder = new StairDrawingBuilder(vm, drawing);
            //builder.DrawSegments();


        }

        [Fact]
        public void TestNormalDimensions()
        {
            var handler = new DrawingHandler();
            var drawing = handler.GetActiveDrawing();
            var views = drawing.GetSheet().GetAllViews();
            foreach (var view in views)
            {
               var v= view as View;
                var vt = v.ViewType;
                if (vt == View.ViewTypes._3DView)
                {
                    using (new WorkPlaneScope(v.DisplayCoordinateSystem))
                    {
                        Tekla.Structures.Model.UI.ViewHandler.RedrawWorkplane();
                        //for (int i = 0; i < 10; i++)
                        //{

                        //v.Modify();
                        //}
                    }
                    //Tekla.Structures.Model.UI.ViewHandler.RedrawWorkplane();

                    v.Modify();

                    //v.RotateViewOnAxisX(5);
                }
            }



        }

      




    }
}