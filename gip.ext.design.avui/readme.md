## gip.ext.design.avui
This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.
https://github.com/icsharpcode/SharpDevelop


event MouseButtonEventHandler MouseDown > event EventHandler<PointerEventArgs>? PointerEntered
event MouseButtonEventHandler MouseUp > event EventHandler<PointerEventArgs>? PointerExited
GeneralTransform > Matrix
ContentControl
ContentElement > 
VisualTreeHelper.GetParent > visual.GetVisualParent();
Visual.IsDescendantOf() > 
RenderSize > Bounds
GetTemplateChild > e.NameScope.Find<T>
pt.GetBindingExpression(PointThumb.RelativeToPointProperty); > BindingOperations.GetBindingExpressionBase(pt, PointThumb.RelativeToPointProperty)?.UpdateTarget();

https://docs.avaloniaui.net/docs/guides/data-binding/binding-from-code
adornedControl.GetObservable(Canvas.LeftProperty).Subscribe(_ => BindAndPlaceHandle());

https://docs.avaloniaui.net/docs/guides/custom-controls/how-to-create-attached-properties
PointProperty.Changed.AddClassHandler<PointThumb>((x, e) => OnPointChanged(x, e));

OnMouseLeave -> OnPointerExited

ArrowAngleProperty.DefaultMetadata.DefaultValue => ArrowAngleProperty.GetMetadata(typeof(ArrowLineBase))