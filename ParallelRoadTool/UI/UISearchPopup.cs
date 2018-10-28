using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Base;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UISearchPopup : UIBasePopup
    {

        #region Properties

        private UITextField DropdownFilterField { get; set; }

        #endregion

        #region Base
        protected override string WindowTitle()
        {
            return "Search";
        }

        protected override string WindowName()
        {
            return "Search";
        }

        protected override void InitComponent()
        {
            DropdownFilterField = UIUtil.CreateTextField(this);
            DropdownFilterField.size = new Vector2(size.x - 16, 40);
            DropdownFilterField.relativePosition = Vector2.zero + new Vector2(8,0);
            // DropdownFilterField.eventTextChanged += DropdownFilterField_eventTextChanged;
        }
        #endregion
    }
}
