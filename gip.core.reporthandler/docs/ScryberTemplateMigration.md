# Scryber Template Syntax And Legacy FlowDoc Migration

This document explains how to author Scryber templates in iPlus and how to migrate old FlowDocument XAML templates.

## Scope

Applies to:

- gip.core.reporthandler
- gip.core.reporthandler.avui
- gip.core.reporthandlerwpf

## Rendering Pipeline Overview

The core project can render report templates through Scryber.

1. Template is detected as Scryber HTML/XHTML.
2. Report data is bound to document parameters.
3. Legacy helper forms are normalized where needed.
4. Barcode and resource-image metadata are resolved.
5. PDF is generated, or custom layout renderers can consume the same template data.

## Scryber Syntax In iPlus

### Data Binding

Common binding expressions:

- `{{model.Property}}`
- `{{values.KeyName}}`
- `{{reportData}}`
- `{{vb.Get('CurrentFacilityCharge/Material/MaterialNo')}}`

Example:

```html
<!doctype html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
  <meta charset="utf-8" />
  <style>
    @page { size: 420pt 595pt; margin: 20pt; }
    body { font-family: Arial, sans-serif; font-size: 12pt; }
    .label { font-size: 11pt; }
    .value { font-size: 14pt; font-weight: bold; }
  </style>
</head>
<body>
  <p class="label">Chargennummer:</p>
  <p class="value">{{vb.Get('CurrentFacilityCharge/FacilityLot/LotNo')}}</p>

  <p class="label">Material-Nr.:</p>
  <p class="value">{{vb.Get('CurrentFacilityCharge/Material/MaterialNo')}}</p>
</body>
</html>
```

### Barcode Metadata

Use metadata attributes on a host element (typically `span`).

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

Optional attributes:

- `data-barcode-width`
- `data-barcode-height`
- `data-vb-show-columns`
- `data-vb-show-columns-keys`

### Linux Print Media Override

For desktop printing via CUPS (`lp`), the AVUI and WPF print paths support explicit media override in the template:

- `data-cups-media="A4"`
- `data-print-media="Letter"`

Example:

```html
<body data-cups-media="A4">
  ...
</body>
```

If no explicit media attribute is provided, the print path tries to infer media from the first CSS `@page size` value (for example `A4`, `A5`, `Letter`, `Legal`).

### Resource Images

To resolve iPlus design resources directly inside template rendering:

```html
<img
  data-resource-key="ACProject(Root)\ACClass(ACRoot)\ACClass(Environment)\ACClassDesign(iPlusLogoPNG)"
  src="{{iPlusLogoPNG}}"
  width="40"
  height="40"
  alt="iPlus" />
```

`data-resource-key` is used to resolve a data URL for reliable rendering.

## Migration Guide: FlowDocument XAML To Scryber

### Conversion Rules

1. Convert `FlowDocument` root to XHTML root.
2. Move page sizing and padding into CSS `@page` and `body` styles.
3. Convert `Paragraph` blocks to `<p>` elements.
4. Convert `InlineDocumentValue VBContent="A\\B\\C"` to `{{vb.Get('A/B/C')}}`.
5. Convert `InlineBarcode` to metadata attributes on a regular HTML element.
6. Keep labels as static text and values as binding expressions.
7. Replace visual-only WPF properties with CSS equivalents.

### Element Mapping

| FlowDoc XAML | Scryber |
| --- | --- |
| `<FlowDocument PageWidth="500" PageHeight="700" PagePadding="20,20,20,20">` | `@page { size: 500pt 700pt; margin: 20pt; }` |
| `<Paragraph TextAlignment="Left">` | `<p style="text-align:left;">` |
| `<Run>Text</Run>` | Text node |
| `<vbr:InlineDocumentValue VBContent="CurrentFacilityCharge\\SplitNo"/>` | `{{vb.Get('CurrentFacilityCharge/SplitNo')}}` |
| `<vbr:InlineBarcode .../>` | `<span data-barcode-type="..." data-vb-content="..."></span>` |

### Before And After Example

Before (legacy FlowDoc):

```xml
<?xml version="1.0" encoding="utf-16"?>
<FlowDocument FontFamily="Calibri" ColumnWidth="365" PageWidth="500" PageHeight="700" PagePadding="20,20,20,20"
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:vbr="http://www.iplus-framework.com/report/xaml"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

  <Paragraph TextAlignment="Left" FontSize="12">
    <Run xml:lang="de-de">Chargennummer:</Run>
  </Paragraph>

  <Paragraph TextAlignment="Left">
    <vbr:InlineDocumentValue VBContent="CurrentFacilityCharge\FacilityLot\LotNo" FontWeight="Bold" FontSize="14" />
  </Paragraph>

  <Paragraph TextAlignment="Center">
    <vbr:InlineBarcode BarcodeType="QRCODE" QRPixelsPerModule="50"
      VBShowColumns="FacilityLot\LotNo,FacilityLot\ProductionDate"
      VBShowColumnsKeys="10,11"
      VBContent="CurrentFacilityCharge" />
  </Paragraph>
</FlowDocument>
```

After (Scryber template):

```html
<!doctype html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
  <meta charset="utf-8" />
  <style>
    @page { size: 500pt 700pt; margin: 20pt; }
    body { font-family: Calibri, Arial, sans-serif; font-size: 12pt; width: 365pt; margin: 0; }
    .label { text-align: left; font-size: 12pt; margin: 0 0 2pt 0; }
    .value { text-align: left; font-size: 14pt; font-weight: bold; margin: 0 0 6pt 0; }
    .center { text-align: center; }
  </style>
</head>
<body>
  <p class="label">Chargennummer:</p>
  <p class="value">{{vb.Get('CurrentFacilityCharge/FacilityLot/LotNo')}}</p>

  <p class="center">
    <span
      data-barcode-type="QRCODE"
      data-zpl-barcode-type="QRCODE"
      data-escpos-barcode-type="QRCODE"
      data-qr-pixels-per-module="50"
      data-vb-content="CurrentFacilityCharge"
      data-vb-show-columns="FacilityLot/LotNo,FacilityLot/ProductionDate"
      data-vb-show-columns-keys="10,11">
      {{vb.Get('CurrentFacilityCharge/FacilityChargeID')}}
    </span>
  </p>
</body>
</html>
```

## Practical Test Asset

A full converted label template exists in the repository:

- [../VBControlScripts/FlowDoc_Label_QR_Converted.scryber.html](../VBControlScripts/FlowDoc_Label_QR_Converted.scryber.html)

Use it as baseline for plugin testing (PDF, ZPL, and ESC/POS flows).

## Recommended Migration Workflow

1. Start from the old FlowDoc and list all `VBContent` fields.
2. Rebuild layout in XHTML plus CSS.
3. Replace each field with `{{vb.Get('...')}}`.
4. Replace each barcode control with metadata span syntax.
5. Validate in HTML preview.
6. Validate final output for all target print plugins.
