using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace gip.core.layoutengine.avui.Internals;

internal static class PdfDocumentReflection
{
    private static readonly Type PdfDocumentType = typeof(PDFtoImage.Conversion).Assembly.GetType("PDFtoImage.Internals.PdfDocument")!;

    private static readonly MethodInfo LoadMethod = PdfDocumentType.GetMethod("Load", BindingFlags.Public | BindingFlags.Static)!;

    private static readonly PropertyInfo PageSizesProperty = PdfDocumentType.GetProperty("PageSizes", BindingFlags.Public | BindingFlags.Instance)!;

    public static object Load(Stream stream, string password, bool disposeStream) => LoadMethod.Invoke(null, [stream, password, disposeStream])!;

    public static IList<SizeF> PageSizes(object pdfDocument) => (IList<SizeF>)PageSizesProperty.GetValue(pdfDocument)!;
}