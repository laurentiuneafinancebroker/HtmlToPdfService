using EvoPdf;
using HtmlToPdfService.ViewModel;
using System;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.Http;
using System.Web.WebPages;

namespace HtmlToPdfService.Controllers
{
    public class ValuesController : ApiController
    {
        // POST api/values
        public void GeneratePdf([FromBody] PdfUrl urls)
        {
            HtmlToPdfConverter htmlToPdfConverter = new HtmlToPdfConverter();
            htmlToPdfConverter.LicenseKey = "4W9+bn19bn5ue2B+bn1/YH98YHd3d3c=";
            htmlToPdfConverter.PrepareRenderPdfPageEvent += new PrepareRenderPdfPageDelegate(htmlToPdfConverter_PrepareRenderPdfPageEvent);

            htmlToPdfConverter.TableOfContentsOptions.AutoTocItemsEnabled = true;
            htmlToPdfConverter.TableOfContentsOptions.Title = "Table of Contents";

            string backgroundColor = "background-color:white;";
            string level1TextStyle = "color:black; font-family:'Times New Roman'; font-size:20px; font-weight:bold; font-style:normal;";
            htmlToPdfConverter.TableOfContentsOptions.SetItemStyle(1, level1TextStyle);

            for (int i = 1; i <= 6; i++)
            {
                htmlToPdfConverter.TableOfContentsOptions.SetItemStyle(i, backgroundColor);
                htmlToPdfConverter.TableOfContentsOptions.SetPageNumberStyle(i, backgroundColor);
            }

            htmlToPdfConverter.PdfDocumentOptions.PageBreakBeforeHtmlElementsSelectors = new string[] { ".break-before" };
            htmlToPdfConverter.PdfDocumentOptions.PageBreakAfterHtmlElementsSelectors = new string[] { ".break-after" };
            htmlToPdfConverter.PdfDocumentOptions.TopMargin = 30;

            if (!urls.HeaderUrl.IsEmpty())
            {
                htmlToPdfConverter.PdfDocumentOptions.ShowHeader = true;
                DrawHeader(htmlToPdfConverter, urls);
            }

            if (!urls.FooterUrl.IsEmpty())
            {
                htmlToPdfConverter.PdfDocumentOptions.ShowFooter = true;
                DrawFooter(htmlToPdfConverter, urls);
            }

            byte[] outPdfBuffer = htmlToPdfConverter.ConvertUrl(urls.ContentUrl);
            HttpContext.Current.Response.AddHeader("Content-Type", "application/pdf");
            HttpContext.Current.Response.AddHeader("Content-Disposition", string.Format("{0}; filename=Getting_Started.pdf; size={1}", "attachment", outPdfBuffer.Length.ToString()));
            HttpContext.Current.Response.BinaryWrite(outPdfBuffer);
            HttpContext.Current.Response.End();
        }

        private void DrawHeader(HtmlToPdfConverter htmlToPdfConverter, PdfUrl urls)
        {
            string headerHtmlUrl = urls.HeaderUrl;
            htmlToPdfConverter.PdfHeaderOptions.HeaderBackColor = Color.White;
            HtmlToPdfElement headerHtml = new HtmlToPdfElement(headerHtmlUrl);
            headerHtml.NavigationCompletedEvent += new NavigationCompletedDelegate(headerHtml_NavigationCompletedEvent);
            htmlToPdfConverter.PdfHeaderOptions.AddElement(headerHtml);
            headerHtml.NavigationCompletedEvent += new NavigationCompletedDelegate(headerHtml_NavigationCompletedEvent);

            void headerHtml_NavigationCompletedEvent(NavigationCompletedParams eventParams)
            {
                float headerHtmlWidth = eventParams.HtmlContentWidthPt;
                float headerHtmlHeight = eventParams.HtmlContentHeightPt;
                float headerWidth = htmlToPdfConverter.PdfDocumentOptions.PdfPageSize.Width - htmlToPdfConverter.PdfDocumentOptions.LeftMargin -
                            htmlToPdfConverter.PdfDocumentOptions.RightMargin;

                float resizeFactor = 1;
                if (headerHtmlWidth > headerWidth)
                    resizeFactor = headerWidth / headerHtmlWidth;

                float headerHeight = headerHtmlHeight * resizeFactor;

                if (!(headerHeight < htmlToPdfConverter.PdfDocumentOptions.PdfPageSize.Height - htmlToPdfConverter.PdfDocumentOptions.TopMargin -
                            htmlToPdfConverter.PdfDocumentOptions.BottomMargin))
                {
                    throw new Exception("The header height cannot be bigger than PDF page height");
                }
                htmlToPdfConverter.PdfDocumentOptions.DocumentObject.Header.Height = headerHeight;
            }
        }

        private void DrawFooter(HtmlToPdfConverter htmlToPdfConverter, PdfUrl urls)
        {
            string footerHtmlString = File.ReadAllText(urls.FooterUrl);
            string footerHtmlUrl = urls.FooterUrl;

            HtmlToPdfVariableElement footerHtmlWithPageNumbers = new HtmlToPdfVariableElement(footerHtmlString, footerHtmlUrl);
            htmlToPdfConverter.PdfFooterOptions.AddElement(footerHtmlWithPageNumbers);
        }

        private void htmlToPdfConverter_PrepareRenderPdfPageEvent(PrepareRenderPdfPageParams eventParams)
        {
            eventParams.Page.ShowHeader = true;
            eventParams.Page.ShowFooter = true;
        }
    }
}
