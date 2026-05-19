# gip.core.reporthandler

Core reporting and printing plugin for iPlus.

This project contains the shared rendering pipeline for:

- Scryber HTML/XHTML templates to PDF.
- Legacy FlowDocument support and compatibility helpers.
- Barcode and resource-image preprocessing for template-driven output.

## Scryber Template Syntax

The core engine resolves templates through Scryber and binds report data with these conventions:

- `{{model.PropertyName}}` for model-first access.
- `{{values.KeyName}}` for named report values.
- `{{reportData}}` for full report context.
- `{{vb.Get('CurrentFacilityCharge/Material/MaterialNo')}}` for legacy VBContent-style paths.

Minimal template example:

```html
<!doctype html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<meta charset="utf-8" />
	<style>
		@page { size: 420pt 595pt; margin: 20pt; }
		body { font-family: Arial, sans-serif; font-size: 11pt; }
	</style>
</head>
<body>
	<h1>{{model.Title}}</h1>
	<p>Lot: {{vb.Get('CurrentFacilityCharge/FacilityLot/LotNo')}}</p>
</body>
</html>
```

## Barcode And Image Metadata

For printer plugins and PDF output, templates can carry metadata attributes:

- `data-barcode-type` or printer-specific variants (`data-zpl-barcode-type`, `data-escpos-barcode-type`).
- `data-vb-content` to resolve barcode payload from report data.
- `data-barcode-width`, `data-barcode-height`, `data-qr-pixels-per-module` for sizing.
- `data-resource-key` on `<img>` for design-resource image resolution.
- `data-cups-media` (or `data-print-media`) to override Linux CUPS paper size (for example `A4`, `Letter`).

Barcode element example:

```html
<span
	data-barcode-type="QRCODE"
	data-zpl-barcode-type="QRCODE"
	data-escpos-barcode-type="QRCODE"
	data-qr-pixels-per-module="20"
	data-vb-content="CurrentFacilityCharge/FacilityChargeID">
	{{vb.Get('CurrentFacilityCharge/FacilityChargeID')}}
</span>
```

## Legacy FlowDoc XAML To Scryber

Use this mapping when converting old templates.

| Legacy FlowDoc XAML | Scryber Template |
| --- | --- |
| `<FlowDocument ...>` | `<!doctype html><html ...>` with `@page` CSS |
| `<Paragraph><Run>Text</Run></Paragraph>` | `<p>Text</p>` |
| `<vbr:InlineDocumentValue VBContent="Path" />` | `{{vb.Get('Path/with/slashes')}}` |
| `<vbr:InlineBarcode ... />` | `<span data-barcode-type="..." data-vb-content="..."></span>` |

Before (legacy FlowDoc):

```xml
<Paragraph TextAlignment="Left" FontSize="12">
	<Run xml:lang="de-de">Chargennummer:</Run>
</Paragraph>
<Paragraph TextAlignment="Left">
	<vbr:InlineDocumentValue VBContent="CurrentFacilityCharge\FacilityLot\LotNo" FontWeight="Bold" FontSize="14" />
</Paragraph>
<Paragraph TextAlignment="Center">
	<vbr:InlineBarcode BarcodeType="QRCODE" VBContent="CurrentFacilityCharge" VBShowColumns="FacilityLot\LotNo" VBShowColumnsKeys="10" />
</Paragraph>
```

After (Scryber):

```html
<p style="font-size:12pt; text-align:left;">Chargennummer:</p>
<p style="font-size:14pt; font-weight:bold; text-align:left;">
	{{vb.Get('CurrentFacilityCharge/FacilityLot/LotNo')}}
</p>
<p style="text-align:center;">
	<span
		data-barcode-type="QRCODE"
		data-zpl-barcode-type="QRCODE"
		data-escpos-barcode-type="QRCODE"
		data-vb-content="CurrentFacilityCharge"
		data-vb-show-columns="FacilityLot/LotNo"
		data-vb-show-columns-keys="10">
		{{vb.Get('CurrentFacilityCharge/FacilityChargeID')}}
	</span>
</p>
```

## In-Repo Examples

- Converted test template: [VBControlScripts/FlowDoc_Label_QR_Converted.scryber.html](VBControlScripts/FlowDoc_Label_QR_Converted.scryber.html)
- Full migration guide: [docs/ScryberTemplateMigration.md](docs/ScryberTemplateMigration.md)
