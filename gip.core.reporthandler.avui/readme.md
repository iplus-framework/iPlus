# gip.core.reporthandler.avui

Avalonia UI plugin for report editing, preview, and printing workflows.

This plugin sits on top of gip.core.reporthandler and shares the same Scryber template conventions.

## Scryber Template Syntax

Use the same binding expressions as in the core reporthandler engine:

- `{{model.PropertyName}}`
- `{{values.KeyName}}`
- `{{vb.Get('CurrentFacilityCharge/Material/MaterialNo')}}`

Example:

```html
<!doctype html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<meta charset="utf-8" />
</head>
<body>
	<p>Material: {{vb.Get('CurrentFacilityCharge/Material/MaterialNo')}}</p>
	<p>Split: {{vb.Get('CurrentFacilityCharge/SplitNo')}}</p>
</body>
</html>
```

## Barcode Syntax For Plugin Interop

Use barcode metadata attributes so downstream printer plugins can detect and render barcodes:

```html
<span
	data-barcode-type="QRCODE"
	data-zpl-barcode-type="QRCODE"
	data-escpos-barcode-type="QRCODE"
	data-vb-content="CurrentFacilityCharge/FacilityChargeID">
	{{vb.Get('CurrentFacilityCharge/FacilityChargeID')}}
</span>
```

## Legacy FlowDoc XAML To Scryber Conversion

Quick mapping:

| FlowDoc | Scryber |
| --- | --- |
| `<Paragraph><Run>Label</Run></Paragraph>` | `<p>Label</p>` |
| `<vbr:InlineDocumentValue VBContent="A\\B" />` | `{{vb.Get('A/B')}}` |
| `<vbr:InlineBarcode ... />` | `<span data-barcode-type="..." data-vb-content="..."></span>` |

Small before/after example:

```xml
<Paragraph TextAlignment="Left" FontSize="12">
	<Run>Material Nr:</Run>
</Paragraph>
<Paragraph TextAlignment="Left">
	<vbr:InlineDocumentValue VBContent="CurrentFacilityCharge\Material\MaterialNo" FontWeight="Bold" FontSize="12" />
</Paragraph>
```

```html
<p style="text-align:left; font-size:12pt;">Material Nr:</p>
<p style="text-align:left; font-size:12pt; font-weight:bold;">
	{{vb.Get('CurrentFacilityCharge/Material/MaterialNo')}}
</p>
```

## References

- Core plugin README: [../gip.core.reporthandler/readme.md](../gip.core.reporthandler/readme.md)
- Full conversion guide: [../gip.core.reporthandler/docs/ScryberTemplateMigration.md](../gip.core.reporthandler/docs/ScryberTemplateMigration.md)

