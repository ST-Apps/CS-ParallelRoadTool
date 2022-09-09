using ColossalFramework.UI;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public interface IUIFastListRow<O>
    {
        #region Methods to implement

        /// <summary>
        ///     Method invoked very often, make sure it is fast
        ///     Avoid doing any calculations, the data should be already processed any ready to display.
        /// </summary>
        /// <param name="data">What needs to be displayed</param>
        /// <param name="isRowOdd">Use this to display a different look for your odd rows</param>
        void Display(O data, int index);

        /// <summary>
        ///     Change the style of the selected item here
        /// </summary>
        /// <param name="index">Item index</param>
        void Select(bool isRowOdd);

        /// <summary>
        ///     Change the style of the item back from selected here
        /// </summary>
        /// <param name="isRowOdd">Use this to display a different look for your odd rows</param>
        void Deselect(bool isRowOdd);

        #endregion
    }

    /// <summary>
    ///     This component is specifically designed the handle the display of
    ///     very large amount of rows in a scrollable panel while minimizing
    ///     the impact on the performances.
    ///     This class will instantiate the rows for you based on the actual
    ///     height of the UIFastList and the rowHeight value provided.
    ///     How it works :
    ///     This class only instantiate as many rows as visible on screen (+1
    ///     extra to simulate in-between steps). Then the content of those is
    ///     updated according to what needs to be displayed by calling the
    ///     Display method declared in IUIFastListRow.
    ///     Provide the list of data with rowData. This data is send back to
    ///     your custom row when it needs to be displayed. For optimal
    ///     performances, make sure this data is already processed and ready
    ///     to display.
    /// </summary>
    public class UIFastList<O, I> : UIComponent
        where I : UIComponent, IUIFastListRow<O>
    {
        #region Events

        /// <summary>
        ///     Called when the currently selected row changed
        /// </summary>
        public event PropertyChangedEventHandler<int> EventSelectedIndexChanged;

        #endregion

        #region Private members

        private UIPanel _panel;
        private UIScrollbar _scrollbar;
        private FastList<I> _rows;
        private FastList<O> _rowsData;

        private string _backgroundSprite;
        private Color32 _color = new Color32(255, 255, 255, 255);
        private float _rowHeight = -1;
        private float _pos = -1;
        private float _stepSize;
        private bool _canSelect;
        private int _selectedDataId = -1;
        private int _selectedRowId = -1;
        private bool _lock;
        private bool _updateContent = true;
        private bool _autoHideScrollbar;
        private UIComponent _lastMouseEnter;

        #endregion

        #region Public accessors

        public bool autoHideScrollbar
        {
            get => _autoHideScrollbar;
            set
            {
                if (_autoHideScrollbar != value)
                {
                    _autoHideScrollbar = value;
                    UpdateScrollbar();
                }
            }
        }

        /// <summary>
        ///     Change the color of the background
        /// </summary>
        public Color32 backgroundColor
        {
            get => _color;
            set
            {
                _color = value;
                if (_panel != null)
                    _panel.color = value;
            }
        }

        /// <summary>
        ///     Change the sprite of the background
        /// </summary>
        public string backgroundSprite
        {
            get => _backgroundSprite;
            set
            {
                if (_backgroundSprite != value)
                {
                    _backgroundSprite = value;
                    if (_panel != null)
                        _panel.backgroundSprite = value;
                }
            }
        }

        /// <summary>
        ///     Can rows be selected by clicking on them
        ///     Default value is false
        ///     Rows can still be selected via selectedIndex
        /// </summary>
        public bool canSelect
        {
            get => _canSelect;
            set
            {
                if (_canSelect != value)
                {
                    _canSelect = value;

                    if (_rows == null) return;
                    for (var i = 0; i < _rows.m_size; i++)
                        if (_canSelect)
                            _rows[i].eventClick += OnRowClicked;
                        else
                            _rows[i].eventClick -= OnRowClicked;
                }
            }
        }

        /// <summary>
        ///     Change the position in the list
        ///     Display the data at the position in the top row.
        ///     This doesn't update the list if the position stay the same
        ///     Use DisplayAt for that
        /// </summary>
        public float listPosition
        {
            get => _pos;
            set
            {
                if (_rowHeight <= 0) return;
                if (_pos != value)
                {
                    var pos = Mathf.Max(Mathf.Min(value, _rowsData.m_size - height / _rowHeight), 0);
                    _updateContent = Mathf.FloorToInt(_pos) != Mathf.FloorToInt(pos);
                    DisplayAt(pos);
                }
            }
        }

        /// <summary>
        ///     This is the list of data that will be send to the IUIFastListRow.Display method
        ///     Changing this list will reset the display position to 0
        ///     You can also change rowsData._buffer and rowsData.m_size
        ///     and refresh the display with DisplayAt method
        /// </summary>
        public FastList<O> rowsData
        {
            get
            {
                if (_rowsData == null) _rowsData = new FastList<O>();
                return _rowsData;
            }
            set
            {
                if (_rowsData != value)
                {
                    _rowsData = value;
                    DisplayAt(0);
                }
            }
        }

        /// <summary>
        ///     This MUST be set, it is the height in pixels of each row
        /// </summary>
        public float rowHeight
        {
            get => _rowHeight;
            set
            {
                if (_rowHeight != value)
                {
                    _rowHeight = value;
                    CheckRows();
                }
            }
        }

        /// <summary>
        ///     Currently selected row
        ///     -1 if none selected
        /// </summary>
        public int selectedIndex
        {
            get => _selectedDataId;
            set
            {
                if (_rowsData == null || _rowsData.m_size == 0)
                {
                    _selectedDataId = -1;
                    return;
                }

                var oldId = _selectedDataId;
                if (oldId >= _rowsData.m_size) oldId = -1;
                _selectedDataId = Mathf.Min(Mathf.Max(-1, value), _rowsData.m_size - 1);

                var pos = Mathf.FloorToInt(_pos);
                var newRowId = Mathf.Max(-1, _selectedDataId - pos);
                if (newRowId >= _rows.m_size) newRowId = -1;

                if (newRowId >= 0 && newRowId == _selectedRowId && !_updateContent) return;

                if (_selectedRowId >= 0)
                {
                    _rows[_selectedRowId].Deselect(oldId % 2 == 1);
                    _selectedRowId = -1;
                }

                if (newRowId >= 0)
                {
                    _selectedRowId = newRowId;
                    _rows[_selectedRowId].Select(_selectedDataId % 2 == 1);
                }

                if (EventSelectedIndexChanged != null && _selectedDataId != oldId)
                    EventSelectedIndexChanged(this, _selectedDataId);
            }
        }

        public object selectedItem
        {
            get
            {
                if (_selectedDataId == -1) return null;
                return _rowsData.m_buffer[_selectedDataId];
            }
        }

        public bool selectOnMouseEnter { get; set; }

        /// <summary>
        ///     The number of pixels moved at each scroll step
        ///     When set to 0 or less, rowHeight is used instead.
        /// </summary>
        public float stepSize
        {
            get => _stepSize > 0 ? _stepSize : _rowHeight;
            set => _stepSize = value;
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Clear the list
        /// </summary>
        public void Clear()
        {
            _rowsData.Clear();

            for (var i = 0; i < _rows.m_size; i++) _rows[i].enabled = false;

            UpdateScrollbar();
        }

        /// <summary>
        ///     Display the data at the position in the top row.
        ///     This update the list even if the position remind the same
        /// </summary>
        /// <param name="pos">Index position in the list</param>
        public void DisplayAt(float pos)
        {
            if (_rowsData == null || _rowHeight <= 0) return;

            SetupControls();

            _pos = Mathf.Max(Mathf.Min(pos, _rowsData.m_size - height / _rowHeight), 0f);

            for (var i = 0; i < _rows.m_size; i++)
            {
                var dataPos = Mathf.FloorToInt(_pos + i);
                var offset = rowHeight * (_pos + i - dataPos);
                if (dataPos < _rowsData.m_size)
                {
                    if (_updateContent)
                        _rows[i].Display(_rowsData[dataPos], dataPos);

                    if (dataPos == _selectedDataId && _updateContent)
                    {
                        _selectedRowId = i;
                        _rows[_selectedRowId].Select(dataPos % 2 == 1);
                    }

                    _rows[i].enabled = true;
                }
                else
                {
                    _rows[i].enabled = false;
                }

                _rows[i].relativePosition = new Vector3(0, i * rowHeight - offset);
            }

            UpdateScrollbar();
            _updateContent = true;
        }

        /// <summary>
        ///     Refresh the display
        /// </summary>
        public void Refresh()
        {
            DisplayAt(_pos);
        }

        #endregion

        #region Overrides

        public override void Start()
        {
            base.Start();

            SetupControls();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (_panel == null) return;

            Destroy(_panel);
            Destroy(_scrollbar);

            if (_rows == null) return;

            for (var i = 0; i < _rows.m_size; i++) Destroy(_rows[i]);
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (_panel == null) return;

            _panel.size = size;

            _scrollbar.height = height;
            _scrollbar.trackObject.height = height;
            _scrollbar.AlignTo(this, UIAlignAnchor.TopRight);

            CheckRows();
        }

        protected override void OnMouseWheel(UIMouseEventParameter p)
        {
            base.OnMouseWheel(p);

            if (!p.used)
            {
                var prevPos = listPosition;
                if (_stepSize > 0 && _rowHeight > 0)
                    listPosition = _pos - p.wheelDelta * _stepSize / _rowHeight;
                else
                    listPosition = _pos - p.wheelDelta;

                if (prevPos != listPosition) p.Use();

                if (selectOnMouseEnter)
                    OnRowClicked(_lastMouseEnter, p);
            }
        }

        #endregion

        #region Private methods

        protected void OnRowClicked(UIComponent component, UIMouseEventParameter p)
        {
            if (selectOnMouseEnter) _lastMouseEnter = component;

            var max = Mathf.Min(_rowsData.m_size, _rows.m_size);
            for (var i = 0; i < max; i++)
                if (component == _rows[i])
                {
                    selectedIndex = i + Mathf.FloorToInt(_pos);
                    return;
                }
        }

        private void CheckRows()
        {
            if (_panel == null || _rowHeight <= 0) return;

            var nbRows = Mathf.CeilToInt(height / _rowHeight) + 1;

            if (_rows == null)
            {
                _rows = new FastList<I>();
                _rows.SetCapacity(nbRows);
            }

            if (_rows.m_size < nbRows)
            {
                // Adding missing rows
                for (var i = _rows.m_size; i < nbRows; i++)
                {
                    _rows.Add(_panel.AddUIComponent<I>());
                    if (_canSelect && !selectOnMouseEnter) _rows[i].eventClick += OnRowClicked;
                    else if (_canSelect) _rows[i].eventMouseEnter += OnRowClicked;
                }
            }
            else if (_rows.m_size > nbRows)
            {
                // Remove excess rows
                for (var i = nbRows; i < _rows.m_size; i++)
                    Destroy(_rows[i].gameObject);

                _rows.SetCapacity(nbRows);
            }

            UpdateScrollbar();
        }

        private void UpdateScrollbar()
        {
            if (_rowsData == null || _rowHeight <= 0) return;

            if (_autoHideScrollbar)
            {
                var isVisible = _rowsData.m_size * _rowHeight > height;
                var newPanelWidth = isVisible ? width - 10f : width;
                var newItemWidth = isVisible ? width - 20f : width;

                _panel.width = newPanelWidth;
                for (var i = 0; i < _rows.m_size; i++) _rows[i].width = newItemWidth;

                _scrollbar.isVisible = isVisible;
            }

            var H = _rowHeight * _rowsData.m_size;
            var scrollSize = height * height / (_rowHeight * _rowsData.m_size);
            var amount = stepSize * height / (_rowHeight * _rowsData.m_size);

            _scrollbar.scrollSize = Mathf.Max(10f, scrollSize);
            _scrollbar.minValue = 0f;
            _scrollbar.maxValue = height;
            _scrollbar.incrementAmount = Mathf.Max(1f, amount);

            UpdateScrollPosition();
        }

        private void UpdateScrollPosition()
        {
            if (_lock || _rowHeight <= 0) return;

            _lock = true;

            var pos = _pos * (height - _scrollbar.scrollSize) / (_rowsData.m_size - height / _rowHeight);
            if (pos != _scrollbar.value)
                _scrollbar.value = pos;

            _lock = false;
        }


        private void SetupControls()
        {
            if (_panel != null) return;

            // Panel 
            _panel = AddUIComponent<UIPanel>();
            _panel.width = width - 10f;
            _panel.height = height;
            _panel.backgroundSprite = _backgroundSprite;
            _panel.color = _color;
            _panel.clipChildren = true;
            _panel.relativePosition = Vector2.zero;

            // Scrollbar
            _scrollbar = AddUIComponent<UIScrollbar>();
            _scrollbar.width = 20f;
            _scrollbar.height = height;
            _scrollbar.orientation = UIOrientation.Vertical;
            _scrollbar.pivot = UIPivotPoint.BottomLeft;
            _scrollbar.AlignTo(this, UIAlignAnchor.TopRight);
            _scrollbar.minValue = 0;
            _scrollbar.value = 0;
            _scrollbar.incrementAmount = 50;

            var tracSprite = _scrollbar.AddUIComponent<UISlicedSprite>();
            tracSprite.relativePosition = Vector2.zero;
            tracSprite.autoSize = true;
            tracSprite.size = tracSprite.parent.size;
            tracSprite.fillDirection = UIFillDirection.Vertical;
            tracSprite.spriteName = "ScrollbarTrack";

            _scrollbar.trackObject = tracSprite;

            var thumbSprite = tracSprite.AddUIComponent<UISlicedSprite>();
            thumbSprite.relativePosition = Vector2.zero;
            thumbSprite.fillDirection = UIFillDirection.Vertical;
            thumbSprite.autoSize = true;
            thumbSprite.width = thumbSprite.parent.width - 8;
            thumbSprite.spriteName = "ScrollbarThumb";

            _scrollbar.thumbObject = thumbSprite;

            // Rows
            CheckRows();

            _scrollbar.eventValueChanged += (c, t) =>
                                            {
                                                if (_lock || _rowHeight <= 0) return;

                                                _lock = true;

                                                listPosition = _scrollbar.value * (_rowsData.m_size - height / _rowHeight) /
                                                               (height - _scrollbar.scrollSize - 1f);
                                                _lock = false;
                                            };
        }

        #endregion
    }
}
