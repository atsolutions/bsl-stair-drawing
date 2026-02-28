using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;

namespace BSL.StairDrawing.Models
{
    public class DrawingPartModel
    {
        public Part DrawingPart { get; set; }
        public TSM.Part ModelPart { get;set; }
        public DrawingPartModel(Part drawingPart)
        {
            this.DrawingPart = drawingPart;
            var services = new ATS.TeklaCore.Services.TeklaService();
            var model = services.GetModel();
            this.ModelPart = model.SelectModelObject(this.DrawingPart.ModelIdentifier) as TSM.Part;
        }

    }
}
