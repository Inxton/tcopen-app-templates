using x_template_xPlc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcoData.Models;

namespace x_template_xHmi.Wpf.Views.Data.BulkData
{
    public class BulkDataViewModel
    {
        public BulkTraversalModel<PlainProcessData, BulkDataItem> Data
        {
            get
            {
                return App.BulkModel;
            }           
        }
    }
}
