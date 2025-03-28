using Common.DTOs.Responses;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Persistence.DBModels;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using Application.Interfaces.General;

namespace Infrastructure.Repositories
{
    public class FileManager : IFileManager
    {
        public FileManager()
        {
           // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<StatementResult> GenerateExcelStatement(Member member, List<Contribution> contributions, DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Contributions");

                    // Header
                    worksheet.Cells[1, 1].Value = "PENSION CONTRIBUTION STATEMENT";
                    worksheet.Cells[1, 1, 1, 4].Merge = true;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 14;
                    worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // Member info
                    worksheet.Cells[3, 1].Value = "Member:";
                    worksheet.Cells[3, 2].Value = $"{member.FirstName} {member.LastName}";
                    worksheet.Cells[4, 1].Value = "Member ID:";
                    worksheet.Cells[4, 2].Value = member.Id;
                    worksheet.Cells[5, 1].Value = "Period:";
                    worksheet.Cells[5, 2].Value = $"{startDate:dd-MMM-yyyy} to {endDate:dd-MMM-yyyy}";

                    // Summary
                    worksheet.Cells[7, 1].Value = "Summary";
                    worksheet.Cells[7, 1].Style.Font.Bold = true;
                    worksheet.Cells[8, 1].Value = "Total Contributions:";
                    worksheet.Cells[8, 2].Value = contributions.Sum(c => c.Amount);
                    worksheet.Cells[8, 2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[9, 1].Value = "Monthly Contributions:";
                    worksheet.Cells[9, 2].Value = contributions.Where(c => c.ContributionType == ContributionType.Monthly).Sum(c => c.Amount);
                    worksheet.Cells[9, 2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[10, 1].Value = "Voluntary Contributions:";
                    worksheet.Cells[10, 2].Value = contributions.Where(c => c.ContributionType == ContributionType.Voluntary).Sum(c => c.Amount);
                    worksheet.Cells[10, 2].Style.Numberformat.Format = "#,##0.00";

                    // Contributions table
                    worksheet.Cells[12, 1].Value = "Contributions";
                    worksheet.Cells[12, 1, 12, 4].Merge = true;
                    worksheet.Cells[12, 1].Style.Font.Bold = true;

                    // Table headers
                    worksheet.Cells[13, 1].Value = "Date";
                    worksheet.Cells[13, 2].Value = "Type";
                    worksheet.Cells[13, 3].Value = "Amount";
                    worksheet.Cells[13, 4].Value = "Account";
                    worksheet.Cells[13, 1, 13, 4].Style.Font.Bold = true;
                    worksheet.Cells[13, 1, 13, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[13, 1, 13, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                    // Table data
                    for (int i = 0; i < contributions.Count; i++)
                    {
                        var row = 14 + i;
                        worksheet.Cells[row, 1].Value = contributions[i].CreatedDate;
                        worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 2].Value = contributions[i].ContributionType.ToString();
                        worksheet.Cells[row, 3].Value = contributions[i].Amount;
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[row, 4].Value = contributions[i].PensionAccountNumber;
                    }

                    // Auto-fit columns
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Footer
                    worksheet.Cells[15 + contributions.Count, 4].Value = $"Generated on {DateTime.Now:dd-MMM-yyyy HH:mm}";
                    worksheet.Cells[15 + contributions.Count, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    return new StatementResult
                    {
                        Content = package.GetAsByteArray(),
                        ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        FileName = $"PensionStatement_{member.Id}_{DateTime.Now:yyyyMMdd}.xlsx"
                    };
                }
            });
        }

        public async Task<StatementResult> GeneratePdfStatement(Member member, List<Contribution> contributions, DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Create document  
                    var document = new Document(PageSize.A4, 50, 50, 25, 25);
                    var writer = PdfWriter.GetInstance(document, memoryStream);
                    document.Open();

                    // Add title  
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY);
                    var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                    var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                    var highlightFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.DARK_GRAY);

                    document.Add(new Paragraph("PENSION CONTRIBUTION STATEMENT", titleFont) { Alignment = Element.ALIGN_CENTER });
                    document.Add(new Paragraph("\n"));

                    // Member info  
                    document.Add(new Paragraph($"Member: {member.FirstName} {member.LastName}", headerFont));
                    document.Add(new Paragraph($"Member ID: {member.Id}", normalFont));
                    document.Add(new Paragraph($"Period: {startDate:dd-MMM-yyyy} to {endDate:dd-MMM-yyyy}", normalFont));
                    document.Add(new Paragraph("\n"));

                    // Summary table  
                    var summaryTable = new PdfPTable(2) { WidthPercentage = 50, HorizontalAlignment = Element.ALIGN_LEFT };
                    summaryTable.SetWidths(new[] { 2f, 3f });
                    summaryTable.SpacingBefore = 10f;
                    summaryTable.SpacingAfter = 10f;

                    summaryTable.AddCell(new PdfPCell(new Phrase("Total Contributions:", highlightFont)) { Border = Rectangle.NO_BORDER });
                    summaryTable.AddCell(new PdfPCell(new Phrase(contributions.Sum(c => c.Amount).ToString("C", CultureInfo.CurrentCulture), normalFont)) { Border = Rectangle.NO_BORDER });
                    summaryTable.AddCell(new PdfPCell(new Phrase("Monthly Contributions:", highlightFont)) { Border = Rectangle.NO_BORDER });
                    summaryTable.AddCell(new PdfPCell(new Phrase(contributions.Where(c => c.ContributionType == ContributionType.Monthly).Sum(c => c.Amount).ToString("C", CultureInfo.CurrentCulture), normalFont)) { Border = Rectangle.NO_BORDER });
                    summaryTable.AddCell(new PdfPCell(new Phrase("Voluntary Contributions:", highlightFont)) { Border = Rectangle.NO_BORDER });
                    summaryTable.AddCell(new PdfPCell(new Phrase(contributions.Where(c => c.ContributionType == ContributionType.Voluntary).Sum(c => c.Amount).ToString("C", CultureInfo.CurrentCulture), normalFont)) { Border = Rectangle.NO_BORDER });

                    document.Add(summaryTable);
                    document.Add(new Paragraph("\n"));

                    // Contributions table  
                    var table = new PdfPTable(4) { WidthPercentage = 100 };
                    table.SetWidths(new[] { 3f, 2f, 2f, 3f });

                    // Table headers  
                    table.AddCell(new PdfPCell(new Phrase("Date", headerFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                    table.AddCell(new PdfPCell(new Phrase("Type", headerFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                    table.AddCell(new PdfPCell(new Phrase("Amount", headerFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                    table.AddCell(new PdfPCell(new Phrase("Account", headerFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                    // Table rows  
                    foreach (var contribution in contributions)
                    {
                        table.AddCell(new PdfPCell(new Phrase(contribution.CreatedDate.ToString("dd-MMM-yyyy"), normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(contribution.ContributionType.ToString(), normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(contribution.Amount.ToString("C", CultureInfo.CurrentCulture), normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(contribution.PensionAccountNumber, normalFont)));
                    }

                    document.Add(table);
                    document.Add(new Paragraph("\n"));

                    // Footer  
                    document.Add(new Paragraph($"Generated on {DateTime.Now:dd-MMM-yyyy HH:mm}", normalFont) { Alignment = Element.ALIGN_RIGHT });

                    document.Close();

                    return new StatementResult
                    {
                        Content = memoryStream.ToArray(),
                        ContentType = "application/pdf",
                        FileName = $"PensionStatement_{member.Id}_{DateTime.Now:yyyyMMdd}.pdf"
                    };
                }
            });
        }
    }
}
