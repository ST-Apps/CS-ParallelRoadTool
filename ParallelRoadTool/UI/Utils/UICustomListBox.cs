//using ColossalFramework.UI;
//using ParallelRoadTool.UI.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace ParallelRoadTool.UI.Utils
//{
//    internal class UICustomListBox<TUIListableItem> : UIListBox where TUIListableItem : IUIListabeItem
//    {
//        protected new TUIListableItem[] m_Items = new TUIListableItem[0];

//        public new TUIListableItem[] items
//        {
//            get
//            {
//                if (this.m_Items == null)
//                    this.m_Items = new TUIListableItem[0];
//                return this.m_Items;
//            }
//            set
//            {
//                if (value == this.m_Items)
//                    return;
//                this.selectedIndex = -1;
////                 this.m_ScrollPosition = 0.0f;
//                if (value == null)
//                    value = new TUIListableItem[0];
//                this.m_Items = value;
//                this.Invalidate();
//            }
//        }

//        public override void Start()
//        {
//            base.Start();

//            // TODO: get from render items
//            m_ItemHeight = 64;
//        }

//        private void RenderItems()
//        {

//        }
//    }
//}
