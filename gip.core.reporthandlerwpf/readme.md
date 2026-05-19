# gip.core.reporthandlerwpf

WPF report plugin with legacy FlowDocument tooling and print rendering integration.

This plugin is the main compatibility bridge for older FlowDoc designs, while newer designs should be authored as Scryber templates.

## Scryber Template Syntax

Supported binding styles used by the core rendering engine:

- `{{model.PropertyName}}`
- `{{values.KeyName}}`
- `{{vb.Get('Path/To/Value')}}`

Example:

```html
<!doctype html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<meta charset="utf-8" />
</head>
<body>
	<p>Chargennummer:</p>
	<p style="font-weight:bold;">{{vb.Get('CurrentFacilityCharge/FacilityLot/LotNo')}}</p>
</body>
</html>
```

## Barcode Metadata Example

```html
<p style="text-align:center;">
	<span
		data-barcode-type="QRCODE"
		data-zpl-barcode-type="QRCODE"
		data-escpos-barcode-type="QRCODE"
		data-qr-pixels-per-module="20"
		data-vb-content="CurrentFacilityCharge/FacilityChargeID">
		{{vb.Get('CurrentFacilityCharge/FacilityChargeID')}}
	</span>
</p>
```

## Legacy FlowDoc XAML To Scryber Conversion

Migration mapping:

| Legacy Element | Scryber Equivalent |
| --- | --- |
| `<FlowDocument>` | XHTML template with CSS `@page` |
| `<Paragraph>` | `<p>` or table row block |
| `<Run>` | inline text |
| `<vbr:InlineDocumentValue VBContent="..." />` | `{{vb.Get('...')}}` |
| `<vbr:InlineBarcode ... />` | metadata span (`data-barcode-type`, `data-vb-content`, optional size attrs) |

Before (legacy):

```xml
<Paragraph TextAlignment="Left" FontSize="12">
	<Run xml:lang="de-de">Split:</Run>
</Paragraph>
<Paragraph TextAlignment="Left">
	<vbr:InlineDocumentValue VBContent="CurrentFacilityCharge\SplitNo" FontWeight="Bold" FontSize="14" />
</Paragraph>
```

After (Scryber):

```html
<p style="text-align:left; font-size:12pt;">Split:</p>
<p style="text-align:left; font-size:14pt; font-weight:bold;">
	{{vb.Get('CurrentFacilityCharge/SplitNo')}}
</p>
```

## References

- Core plugin README: [../gip.core.reporthandler/readme.md](../gip.core.reporthandler/readme.md)
- Full conversion guide: [../gip.core.reporthandler/docs/ScryberTemplateMigration.md](../gip.core.reporthandler/docs/ScryberTemplateMigration.md)

