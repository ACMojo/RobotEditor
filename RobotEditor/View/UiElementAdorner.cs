using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace RobotEditor.View
{
    /// <summary>
    ///     An adorner that can display one and only one UIElement.
    ///     That element can be a panel, which contains multiple other elements.
    ///     The element is added to the adorner's visual and logical trees, enabling it to
    ///     particpate in dependency property value inheritance, amongst other things.
    /// </summary>
    public class UiElementAdorner<TElement> : Adorner where TElement : UIElement
    {
        #region Fields

        private TElement _child;
        private double _offsetLeft;
        private double _offsetTop;

        #endregion

        #region Instance

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="adornedElement">The element to which the adorner will be bound.</param>
        public UiElementAdorner(UIElement adornedElement)
            : base(adornedElement) { }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets/sets the child element hosted in the adorner.
        /// </summary>
        public TElement Child
        {
            get { return _child; }
            set
            {
                if (_child != null)
                {
                    RemoveLogicalChild(_child);
                    RemoveVisualChild(_child);
                }

                _child = value;

                if (_child != null)
                {
                    AddLogicalChild(_child);
                    AddVisualChild(_child);
                }
            }
        }

        /// <summary>
        ///     Gets/sets the horizontal offset of the adorner.
        /// </summary>
        public double OffsetLeft
        {
            get { return _offsetLeft; }
            set
            {
                _offsetLeft = value;
                UpdateLocation();
            }
        }

        /// <summary>
        ///     Gets/sets the vertical offset of the adorner.
        /// </summary>
        public double OffsetTop
        {
            get { return _offsetTop; }
            set
            {
                _offsetTop = value;
                UpdateLocation();
            }
        }

        /// <summary>
        ///     Override.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                var list = new ArrayList();
                if (_child != null)
                    list.Add(_child);
                return list.GetEnumerator();
            }
        }

        /// <summary>
        ///     Override.
        /// </summary>
        protected override int VisualChildrenCount => _child == null ? 0 : 1;

        #endregion

        #region Public methods

        /// <summary>
        ///     Override.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var result = new GeneralTransformGroup();
            var desiredTransform = base.GetDesiredTransform(transform);
            Debug.Assert(desiredTransform != null);
            result.Children.Add(desiredTransform);
            result.Children.Add(new TranslateTransform(_offsetLeft, _offsetTop));
            return result;
        }

        /// <summary>
        ///     Updates the location of the adorner in one atomic operation.
        /// </summary>
        public void SetOffsets(double left, double top)
        {
            _offsetLeft = left;
            _offsetTop = top;
            UpdateLocation();
        }

        #endregion

        #region Protected methods

        /// <summary>
        ///     Override.
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size constraint)
        {
            if (_child == null)
                return base.MeasureOverride(constraint);

            _child.Measure(constraint);
            return _child.DesiredSize;
        }

        /// <summary>
        ///     Override.
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_child == null)
                return base.ArrangeOverride(finalSize);

            _child.Arrange(new Rect(finalSize));
            return finalSize;
        }

        /// <summary>
        ///     Override.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override Visual GetVisualChild(int index)
        {
            return _child;
        }

        #endregion

        #region Private methods

        private void UpdateLocation()
        {
            var adornerLayer = Parent as AdornerLayer;
            adornerLayer?.Update(AdornedElement);
        }

        #endregion
    }
}