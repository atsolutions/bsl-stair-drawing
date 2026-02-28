using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATS.TeklaCore.Models;
using ATS.TeklaCore.ViewModels;
using Tekla.Structures.Dialog;
using TD = Tekla.Structures.Datatype;

namespace BSL.StairDrawing.ViewModels
{
    public class StairDrawingViewModel : BaseViewModel
    {
        public StairDrawingViewModel()
        {
            this.Authorization = new AuthorizationViewModel("BSLTools");
            this.SaveSettings = true;
            if (!this.Authorization.IsAuthorized)
            {
                this.SelectedTab = 1;
            }
        }

        private AuthorizationViewModel authorization;

        public AuthorizationViewModel Authorization
        {
            get { return authorization; }
            set { authorization = value; }
        }

        private string stringerNames;
        [StructuresDialog(nameof(StringerNames))]
        public string StringerNames
        {
            get { return stringerNames; }
            set
            {
                stringerNames = value;
                OnPropertyChanged(nameof(StringerNames));
            }
        }

        private string treadNames;
        [StructuresDialog(nameof(TreadNames))]
        public string TreadNames
        {
            get { return treadNames; }
            set
            {
                treadNames = value;
                OnPropertyChanged(nameof(TreadNames));
            }
        }

        private string angleNames;
        [StructuresDialog(nameof (AngleNames))]
        public string AngleNames
        {
            get { return angleNames; }
            set
            {
                angleNames = value;
                OnPropertyChanged(nameof(AngleNames));
            }
        }

        public NameList StringerNamesList { get { return new NameList(stringerNames); } }
        public NameList TreadNamesList { get { return new NameList(treadNames); } }
        public NameList AngleNamesList { get { return new NameList(angleNames); } }

        private bool saveSettings;

        [StructuresDialogFilter(nameof(CreateTLengthDim))]
        public bool SaveSettings
        {
            get { return saveSettings; }
            set
            {
                saveSettings = value; OnPropertyChanged(nameof(SaveSettings));
            }
        }

        private bool createTopLengthDimension;
        private bool createTopWidthDimensions;
        private bool createTopBoltDimensions;
        private bool createTopAngleBoltDimensions;

        private bool createFrontBoltDimensions;
        private bool createFrontStepDimensions;
        private bool createFrontStringerDimensions;

        private bool createDetailView;
        private bool createDetailViewDimensions;
        [StructuresDialog(nameof(CreateTLengthDim), "bool")]
        public bool CreateTLengthDim
        {
            get { return createTopLengthDimension; }
            set
            {
                createTopLengthDimension = value;
                OnPropertyChanged(nameof(CreateTLengthDim));
            }
        }
        [StructuresDialog(nameof(CreateTopWidthDimensions), "bool")]
        public bool CreateTopWidthDimensions
        {
            get { return createTopWidthDimensions; }
            set
            {
                createTopWidthDimensions = value;
                OnPropertyChanged(nameof(CreateTopWidthDimensions));
            }
        }
        [StructuresDialog(nameof(CreateTopBoltDimensions), "bool")]
        public bool CreateTopBoltDimensions
        {
            get { return createTopBoltDimensions; }
            set
            {
                createTopBoltDimensions = value;
                OnPropertyChanged(nameof(CreateTopBoltDimensions));
            }

        }
        [StructuresDialog(nameof(CreateTopAngleBoltDimensions), "bool")]
        public bool CreateTopAngleBoltDimensions
        {
            get { return createTopAngleBoltDimensions; }
            set
            {
                createTopAngleBoltDimensions = value;
                OnPropertyChanged(nameof(CreateTopAngleBoltDimensions));
            }

        }
        [StructuresDialog (nameof(CreateFrontBoltDimensions), "bool")]
        public bool CreateFrontBoltDimensions
        {
            get { return createFrontBoltDimensions; }
            set
            {
                createFrontBoltDimensions = value;
                OnPropertyChanged(nameof(CreateFrontBoltDimensions));
                
            }
        }
        [StructuresDialog(nameof(CreateFrontStepDimensions), "bool")]
        public bool CreateFrontStepDimensions
        {
            get { return createFrontStepDimensions; }
            set
            {
                createFrontStepDimensions = value;
                OnPropertyChanged(nameof(CreateFrontStepDimensions));
            }
        }
        [StructuresDialog(nameof(CreateFrontStringerDimensions), "bool")]
        public bool CreateFrontStringerDimensions
        {
            get { return createFrontStringerDimensions; }
            set
            {
                createFrontStringerDimensions = value;
                OnPropertyChanged(nameof(CreateFrontStringerDimensions));
            }

        }


        [StructuresDialog(nameof(CreateDetailDimensions), "bool")]
        public bool CreateDetailDimensions
        {
            get { return createDetailViewDimensions; }
            set
            {
                createDetailViewDimensions = value;
                OnPropertyChanged(nameof(CreateDetailDimensions));
            }

        }


        [StructuresDialog(nameof(CreateDetailView), "bool")]
        public bool CreateDetailView
        {
            get { return createDetailView; }
            set
            {
                createDetailView = value;
                OnPropertyChanged(nameof(CreateDetailView));
                
            }

        }


        private int selectedTab;
        public int SelectedTab
        {
            get { return selectedTab; }
            set
            {
                selectedTab = value;
                OnPropertyChanged(nameof(SelectedTab));
            }
        }

        private string detailName;
        private string detailMarkAttributes;
        private string detailViewAttributes;

        [StructuresDialog(nameof(DetailName))]
        public string DetailName
        {
            get { return detailName; }
            set
            {
                detailName = value;
                OnPropertyChanged(nameof(DetailName));
            }
        }


        [StructuresDialog(nameof(DetailMarkAttributes))]
        public string DetailMarkAttributes
        {
            get { return detailMarkAttributes; }
            set
            {
                detailMarkAttributes = value;
                OnPropertyChanged(nameof(DetailMarkAttributes));
            }
        }


        [StructuresDialog(nameof(DetailViewAttributes))]
        public string DetailViewAttributes
        {
            get { return detailViewAttributes; }
            set
            {
                detailViewAttributes = value;
                OnPropertyChanged(nameof(DetailViewAttributes));
            }
        }




    }
}
