using System;
using System.Collections.Generic;
using System.Text;

namespace PlayWrightAutomation.Helpers
{
    internal static class Helpers
    {
        internal static string ExtractPdfText(string pdfPath)
        {
            var sb = new StringBuilder();
            using var pdf = UglyToad.PdfPig.PdfDocument.Open(pdfPath);
            foreach (var p in pdf.GetPages())
            {
                sb.AppendLine(p.Text);
            }
            return sb.ToString();
        }
    }
}
