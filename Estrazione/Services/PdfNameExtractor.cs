

using Microsoft.AspNetCore.Mvc.RazorPages;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Estrazione.Services;

public class PdfNameExtractor
{
    public List<string> ExtractNames(Stream pdfStream)
    {
        List<string> result = new();

        using PdfDocument document = PdfDocument.Open(pdfStream);

        foreach (UglyToad.PdfPig.Content.Page page in document.GetPages())
        {
            List<Word> words = page.GetWords().ToList();

            Word? nomeHeader = words.FirstOrDefault(w =>
                string.Equals(w.Text, "Nome", StringComparison.OrdinalIgnoreCase));

            Word? cognomeHeader = words.FirstOrDefault(w =>
                string.Equals(w.Text, "Cognome", StringComparison.OrdinalIgnoreCase));

            if (nomeHeader == null || cognomeHeader == null)
            {
                continue;
            }

            double nomeX = nomeHeader.BoundingBox.Left;
            double cognomeX = cognomeHeader.BoundingBox.Left;

            double middleX = (nomeX + cognomeX) / 2;
            double rightLimit = cognomeX + 250;

            List<Word> dataWords = words
                .Where(w => w.BoundingBox.Bottom < nomeHeader.BoundingBox.Bottom)
                .OrderByDescending(w => w.BoundingBox.Bottom)
                .ThenBy(w => w.BoundingBox.Left)
                .ToList();

            List<IGrouping<int, Word>> rows = dataWords
                .GroupBy(w => (int)Math.Round(w.BoundingBox.Bottom / 5))
                .OrderByDescending(g => g.Key)
                .ToList();
            foreach (IGrouping<int, Word> row in rows)
            {
                string[] values = row
                    .OrderBy(w => w.BoundingBox.Left)
                    .Select(w => w.Text)
                    .ToArray();

                if (values.Length >= 2 && values[0] != "Order" && values[0] != "--")
                {
                    result.Add($"{values[0]} {values[1]}");
                }
            }
        }


        result = result
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        return result;
    }
}