using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Hearty_Bites.Models;
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Hearty_Bites.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        public async Task SendOrderContractEmailAsync(string toEmail, Order order, string customerName, string catererName, string menuText)
        {
            string appPassword = _configuration["Email:AppPassword"] ?? string.Empty;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("gulsentasci47@gmail.com", appPassword),
                EnableSsl = true,
                Timeout = 20000
            };

            byte[] invoicePdfBytes = GenerateInvoicePdf(order, customerName, catererName, menuText);
            byte[] contractPdfBytes = GenerateContractPdf(order, customerName, catererName, menuText);

            var mailMessage = new MailMessage
            {
                From = new MailAddress("gulsentasci47@gmail.com", "The Caterist"),
                Subject = $"🧾 Order Confirmed & Catering Agreement - Order #{order.Id}",
                IsBodyHtml = true,
                Body = $"<h3>Hello {customerName},</h3><p>Your order has been successfully received and has been sent to the <b>{catererName}</b> company.</p><p>Your distance sales agreement and invoice are attached as a PDF.</p><br><p>Have a nice day.<br>The Caterist Team</p>",
                BodyEncoding = Encoding.UTF8
            };

            mailMessage.To.Add(toEmail);

            var invoiceStream = new MemoryStream(invoicePdfBytes);
            var contractStream = new MemoryStream(contractPdfBytes);

            mailMessage.Attachments.Add(new Attachment(invoiceStream, $"Invoice_{order.Id}.pdf", "application/pdf"));
            mailMessage.Attachments.Add(new Attachment(contractStream, $"Agreement_{order.Id}.pdf", "application/pdf"));

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine("-> email giiti " + toEmail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-> email olmadı sebebi: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"-->gmail hatasimii: {ex.InnerException.Message}");
                }
            }
            finally
            {
                invoiceStream.Dispose();
                contractStream.Dispose();
                mailMessage.Dispose();
            }
        }

        private byte[] GenerateInvoicePdf(Order order, string customerName, string catererName, string menuText)
        {
            decimal grandTotal = order.TotalAmount;
            decimal vatRate = 0.10m;
            decimal subTotal = grandTotal / (1m + vatRate);
            decimal vatAmount = grandTotal - subTotal;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial").FontColor("#333333"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("THE CATERIST").FontSize(24).Bold().FontColor("#6b1d2f");
                            col.Item().Text("The Caterist Technologies Inc.").FontSize(9).Bold();
                            col.Item().Text("Universiteler Mah. Cyber Technopark No:42/A Cankaya/Ankara").FontSize(8).FontColor(Colors.Grey.Darken1);
                            col.Item().Text("Mersis No: 0123-4567-8901-0001 | Tax Office: Doganbey V.D.").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(180).Column(col =>
                        {
                            col.Item().Background("#6b1d2f").Padding(6).AlignCenter().Text("E-INVOICE").FontSize(11).Bold().FontColor(Colors.White);

                            col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).Row(r => {
                                r.RelativeItem().Text("Invoice No:").FontSize(8).Bold();
                                r.RelativeItem().AlignRight().Text($"INV-{order.Id}-{DateTime.Now.Year}").FontSize(8);
                            });
                            col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).Row(r => {
                                r.RelativeItem().Text("Issue Date:").FontSize(8).Bold();
                                r.RelativeItem().AlignRight().Text($"{order.OrderDate:dd.MM.yyyy}").FontSize(8);
                            });
                            col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).Row(r => {
                                r.RelativeItem().Text("Service Date:").FontSize(8).Bold();
                                r.RelativeItem().AlignRight().Text($"{order.EventDate:dd.MM.yyyy}").FontSize(8);
                            });
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("SERVICE PROVIDER (CATERER)").FontSize(9).Bold().FontColor("#6b1d2f");
                                c.Item().BorderBottom(1).BorderColor("#6b1d2f").PaddingBottom(2);
                                c.Item().PaddingTop(4).Text(catererName).Bold();
                                c.Item().Text("The Caterist Verified Independent Kitchen Member").FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                            });

                            row.ConstantItem(30);

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("SERVICE RECEIVER (CLIENT)").FontSize(9).Bold().FontColor("#6b1d2f");
                                c.Item().BorderBottom(1).BorderColor("#6b1d2f").PaddingBottom(2);
                                c.Item().PaddingTop(4).Text(customerName).Bold();
                                c.Item().Text($"Address: {order.DeliveryAddress}").FontSize(9);
                            });
                        });

                        col.Item().PaddingTop(1, Unit.Centimetre);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#f8f9fa").Padding(6).Text("Service / Menu Description").Bold().FontSize(9);
                                header.Cell().Background("#f8f9fa").Padding(6).AlignCenter().Text("Qty").Bold().FontSize(9);
                                header.Cell().Background("#f8f9fa").Padding(6).AlignRight().Text("Unit Price").Bold().FontSize(9);
                                header.Cell().Background("#f8f9fa").Padding(6).AlignCenter().Text("VAT").Bold().FontSize(9);
                                header.Cell().Background("#f8f9fa").Padding(6).AlignRight().Text("Amount").Bold().FontSize(9);
                            });

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                            {
                                c.Item().Text("Bulk Catering & Organization Service").Bold().FontSize(10);
                                c.Item().Text($"Package Content: {menuText}").FontSize(8).FontColor(Colors.Grey.Darken2);
                            });

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignCenter().Text("1").FontSize(10);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"{subTotal:N2} ₺").FontSize(10);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignCenter().Text("10%").FontSize(10);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).AlignRight().Text($"{subTotal:N2} ₺").FontSize(10);
                        });

                        col.Item().PaddingTop(0.5f, Unit.Centimetre);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem();

                            row.ConstantItem(200).Column(summary =>
                            {
                                summary.Item().PaddingVertical(2).Row(r => {
                                    r.RelativeItem().Text("Subtotal (Excl. VAT):").FontSize(9).FontColor(Colors.Grey.Darken2);
                                    r.RelativeItem().AlignRight().Text($"{subTotal:N2} ₺").FontSize(9);
                                });
                                summary.Item().PaddingVertical(2).Row(r => {
                                    r.RelativeItem().Text("Calculated VAT (10%):").FontSize(9).FontColor(Colors.Grey.Darken2);
                                    r.RelativeItem().AlignRight().Text($"{vatAmount:N2} ₺").FontSize(9);
                                });

                                summary.Item().Background("#f8f9fa").Padding(5).Row(r => {
                                    r.RelativeItem().Text("GRAND TOTAL:").Bold().FontSize(11).FontColor("#6b1d2f");
                                    r.RelativeItem().AlignRight().Text($"{grandTotal:N2} ₺").Bold().FontSize(11).FontColor("#6b1d2f");
                                });
                            });
                        });
                    });

                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(5).AlignCenter().Text("This document stands as an official e-invoice record.").FontSize(7).Italic().FontColor(Colors.Grey.Medium);
                        col.Item().AlignCenter().Text("The Caterist Core Infrastructure System Integrity Validation Verified.").FontSize(7).FontColor(Colors.Grey.Medium);
                    });
                });
            }).GeneratePdf();
        }

        private byte[] GenerateContractPdf(Order order, string customerName, string catererName, string menuText)
        {
            decimal grandTotal = order.TotalAmount;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial").FontColor("#333333"));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("CATERING SERVICES AGREEMENT").FontSize(14).Bold().FontColor("#6b1d2f");
                        col.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingVertical(0.5f, Unit.Centimetre).Column(col =>
                    {
                        col.Item().Text($"Contract Ref: CTR-{order.Id}-{DateTime.Now.Year}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().Text($"Date of Execution: {order.OrderDate:dd.MM.yyyy}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingBottom(0.5f, Unit.Centimetre);

                        col.Item().Text("1. PARTIES").Bold().FontColor("#6b1d2f");
                        col.Item().Text($"This Catering Services Agreement is entered into by and between the Client, {customerName}, residing at the delivery destination specified herein, and the assigned Caterer, {catererName}, operating as a verified service provider under The Caterist Marketplace Network.");
                        col.Item().PaddingBottom(0.4f, Unit.Centimetre);

                        col.Item().Text("2. SCOPE OF SERVICES & MENU COMPOSITION").Bold().FontColor("#6b1d2f");
                        col.Item().Text($"The Caterer agrees to prepare, package, and provision the exact menu configuration structure requested by the Client: {menuText}. The provisioning shall comply with standard food safety and hygiene protocols.");
                        col.Item().PaddingBottom(0.4f, Unit.Centimetre);

                        col.Item().Text("3. DELIVERY LOGISTICS & EVENT SCHEDULE").Bold().FontColor("#6b1d2f");
                        col.Item().Text($"The automated system records designate the target fulfillment date as {order.EventDate:dd.MM.yyyy}. Services shall be rendered and transported directly to the explicit location coordinated via the marketplace mapping interface: {order.DeliveryAddress}.");
                        col.Item().PaddingBottom(0.4f, Unit.Centimetre);

                        col.Item().Text("4. FINANCIAL TERMS & SECURE SETTLEMENT").Bold().FontColor("#6b1d2f");
                        col.Item().Text($"The grand total amount for the execution of this contract is established as {grandTotal:N2} ₺. This transactional volume has been collected and securely held in escrow within the marketplace pool account. Disbursement to the Caterer remains contingent upon successful logistical delivery and completion validation.");
                        col.Item().PaddingBottom(0.4f, Unit.Centimetre);

                        col.Item().Text("5. CANCELLATION, RETURNS & TERMINATION POLICY").Bold().FontColor("#6b1d2f");
                        col.Item().Text("Due to the perishable nature of bespoke culinary products, cancellations initiated less than 48 hours prior to the scheduled Service Date are subject to a forfeiture penalty. Material defects in catering execution must be logged instantly via the marketplace infrastructure support channels.");
                        col.Item().PaddingBottom(0.4f, Unit.Centimetre);

                        col.Item().Text("6. GOVERNING LAW & JURISDICTION").Bold().FontColor("#6b1d2f");
                        col.Item().Text("This digital agreement constitutes a legally binding transaction record. Any disputes, interpretations, or enforcement issues arising from the execution of this automated contract shall be exclusively governed by the substantive laws of the Republic of Turkiye, with Ankara West (Bati) Courts and Execution Directorates serving as the sole authorized jurisdictions.");
                        col.Item().PaddingBottom(0.8f, Unit.Centimetre);

                        col.Item().Text("Digitally Signed & Validated via System Cryptographic Handshake.").Italic().FontSize(8).FontColor(Colors.Grey.Medium);
                        col.Item().Text($"Timestamp: {DateTime.Now:dd.MM.yyyy HH:mm:ss} UTC+3").Italic().FontSize(8).FontColor(Colors.Grey.Medium);
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();
        }
    }
}