using ATS.TeklaCore.Services;
using BSL.StairDrawing.ViewModels;
using System.Reflection;
using System.Windows;
using Tekla.Structures.Dialog;
using Tekla.Structures.Drawing;

namespace BSL.StairDrawing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StairDrawingWindow : ApplicationWindowBase
    {
        public StairDrawingWindow()
        {
            InitializeComponent();
            this.ShowInTaskbar = true;
            this.DataContext = new StairDrawingViewModel();
            this.InitializeDataStorage(this.DataContext);
            LibraryContext.RegisterPluginAssembly(Assembly.GetExecutingAssembly());
            SetValue(Fusion.UI.Extensions.TopmostPinVisibilityProperty, Visibility.Visible);
            this.VersionControl.SetVersionLabel();
        }

        private void WpfOkCreateCancel_CreateClicked(object sender, EventArgs e)
        {
            var vm = this.DataContext as StairDrawingViewModel;
            var drawingService = new TeklaDrawingService();
            var activeDrawing = drawingService.GetActiveDrawing();
            
            if (activeDrawing is AssemblyDrawing assemblyDrawing)
            {
                try
                {
                    if (vm != null)
                    {
                        var builder = new StairDrawingBuilder(vm, assemblyDrawing);
                        builder.Build();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                return;
            }
            var selectedDrawings = drawingService.GetSelectedDrawings();
            foreach(var d in selectedDrawings)
            {
                if(d is AssemblyDrawing selectedDrawing)
                {
                    drawingService.SetActiveDrawing(selectedDrawing);
                    if (vm != null)
                    {
                        var builder = new StairDrawingBuilder(vm, selectedDrawing);
                        builder.Build();
                    }
                    drawingService.SaveDrawing();
                }
            }



        }

        private void WpfOkCreateCancel_OkClicked(object sender, EventArgs e)
        {
            this.Apply();
        }

        private void WpfOkCreateCancel_CancelClicked(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
