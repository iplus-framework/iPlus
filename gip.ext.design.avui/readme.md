## gip.ext.design.avui
This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.
https://github.com/icsharpcode/SharpDevelop

https://github.com/Kira-NT/HotAvalonia

https://github.com/wieslawsoltes/Xaml.Behaviors


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

Example nested Style: NullableComboBox


TODO: Incompatibilites with Avalonia 11.0.0
MultiDataTrigger doesn't exist and therfore Condition-Collections are unsupported. Instead build OutlineViews for DataTriggerBehavior from the Xaml Behaviors extensions. See ConditionEditor.cs and MultiDataTriggerOutlineNode.
DataTrigerr -> DataTriggerBehavior
PropertyTrigger, Multitrigger -> StyledElementBehavior 
EventTrigger -> EventTriggerBehavior

This is old WPF code for editing TriggersCollection in a designer. Even though Avalonia UI doesn't have triggers, please convert these files to Avaloni UI code. In a later step, I'll adapt it by using the BehaviorCollection element from the Xaml Behaviors extension.

ItemsControl vs. SelectingItemsControl:
SelectedValuePath -> SelectedValueBinding (Binding Path=...))
DisplayMemberPath -> ItemTemplate = {DataTemplate TextBlock Text={Binding ...}}
SelectedValue -> SelectedValue
ItemsSource -> ItemsSource
SelectedItem -> SelectedItem
xxx -> SelectedItems
