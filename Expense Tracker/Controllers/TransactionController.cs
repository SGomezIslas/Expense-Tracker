﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Models;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout;

namespace Expense_Tracker.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Transaction
        //public async Task<IActionResult> Index()
        //{
        //    var applicationDbContext = _context.Transactions.Include(t => t.Category);
        //    return View(await applicationDbContext.ToListAsync());
        //}

        public async Task<IActionResult> Index(string? categoria, DateTime? date)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Title", "Title");

            var applicationDbContext = _context.Transactions.Include(t => t.Category);
            if (categoria != null)
            {
                var applicationDbContextCategoria = _context.Transactions.Include(t => t.Category).Where(t => t.Category.Title == categoria);
                return View(await applicationDbContextCategoria.ToListAsync());
            }
            if (date != null)
            {
                var applicationDbContextDate = _context.Transactions.Include(t => t.Category).Where(t => t.Date == date);
                return View(await applicationDbContextDate.ToListAsync());
            }
            else
            {
                return View(await applicationDbContext.ToListAsync());
            }
        }

        public IActionResult AddOrEdit(int id = 0)
        {
            PopulateCategories();
            if (id == 0)
                return View(new Transaction());
            else
                return View(_context.Transactions.Find(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("TransactionId,CategoryId,Amount,Note,Date")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                if (transaction.TransactionId == 0)
                    _context.Add(transaction);
                else
                    _context.Update(transaction);

                transaction.Date = DateTime.Now;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateCategories();
            return View(transaction);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Transactions == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Transactions'  is null.");
            }
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [NonAction]
        public void PopulateCategories()
        {
            var CategoryCollection = _context.Categories.ToList();
            Category DefaultCategory = new Category() { CategoryId = 0, Title = "Choose a Category" };
            CategoryCollection.Insert(0, DefaultCategory);
            ViewBag.Categories = CategoryCollection;
        }
        public ActionResult GenerarPdf()
        {
            var applicationDbContext = _context.Transactions.Include(t => t.Category).ToList();

            string rutaTempPdf = Path.GetTempFileName() + ".pdf";

            using (PdfDocument pdfDocument = new PdfDocument(new PdfWriter(rutaTempPdf)))
            {
                using (Document document = new Document(pdfDocument))
                {
                    document.Add(new Paragraph("Transacciones"));

                    Table table = new Table(3);
                    table.SetWidth(UnitValue.CreatePercentValue(100));

                    table.AddHeaderCell("Categoria");
                    table.AddHeaderCell("Monto");
                    table.AddHeaderCell("Fecha");

                    foreach (Models.Transaction trans in applicationDbContext)
                    {
                        table.AddCell(trans.Category.Title);
                        table.AddCell(trans.Amount.ToString());
                        table.AddCell(trans.Date.ToString());

                    }

                    document.Add(table);
                }
            }

            // Leer el archivo PDF como un arreglo de bytes
            byte[] fileBytes = System.IO.File.ReadAllBytes(rutaTempPdf);

            // Eliminar el archivo temporal
            System.IO.File.Delete(rutaTempPdf);

            // Descargar el archivo PDF
            return new FileStreamResult(new MemoryStream(fileBytes), "application/pdf")
            {
                FileDownloadName = "Estado.pdf"
            };
        }
        public ActionResult PdfIngresos()
        {
            var applicationDbContext = _context.Transactions.Include(t => t.Category)
                .Where(t => t.Category.Type == "Income").ToList();

            string rutaTempPdf = Path.GetTempFileName() + ".pdf";

            using (PdfDocument pdfDocument = new PdfDocument(new PdfWriter(rutaTempPdf)))
            {
                using (Document document = new Document(pdfDocument))
                {
                    document.Add(new Paragraph("Resumen de Ingresos"));

                    Table table = new Table(3);
                    table.SetWidth(UnitValue.CreatePercentValue(100));

                    table.AddHeaderCell("Categoria");
                    table.AddHeaderCell("Monto");
                    table.AddHeaderCell("Fecha");

                    foreach (Models.Transaction trans in applicationDbContext)
                    {
                        table.AddCell(trans.Category.Title);
                        table.AddCell(trans.Amount.ToString());
                        table.AddCell(trans.Date.ToString());
                        

                    }

                    document.Add(table);
                }
            }

            // Leer el archivo PDF como un arreglo de bytes
            byte[] fileBytes = System.IO.File.ReadAllBytes(rutaTempPdf);

            // Eliminar el archivo temporal
            System.IO.File.Delete(rutaTempPdf);

            // Descargar el archivo PDF
            return new FileStreamResult(new MemoryStream(fileBytes), "application/pdf")
            {
                FileDownloadName = "IngresosTransactions.pdf"
            };
        }
        public ActionResult PdfEgresos()
        {
            var applicationDbContext = _context.Transactions.Include(t => t.Category)
                .Where(t => t.Category.Type == "Expense").ToList();

            string rutaTempPdf = Path.GetTempFileName() + ".pdf";

            using (PdfDocument pdfDocument = new PdfDocument(new PdfWriter(rutaTempPdf)))
            {
                using (Document document = new Document(pdfDocument))
                {
                    document.Add(new Paragraph("Resumen de Egresos"));

                    Table table = new Table(3);
                    table.SetWidth(UnitValue.CreatePercentValue(100));

                    table.AddHeaderCell("Categoia");
                    table.AddHeaderCell("monto");
                    table.AddHeaderCell("fecha");

                    foreach (Models.Transaction trans in applicationDbContext)
                    {
                        table.AddCell(trans.Category.Title);
                        table.AddCell(trans.Amount.ToString());
                        table.AddCell(trans.Date.ToString());
                        

                    }

                    document.Add(table);
                }
            }

            // Leer el archivo PDF como un arreglo de bytes
            byte[] fileBytes = System.IO.File.ReadAllBytes(rutaTempPdf);

            // Eliminar el archivo temporal
            System.IO.File.Delete(rutaTempPdf);

            // Descargar el archivo PDF
            return new FileStreamResult(new MemoryStream(fileBytes), "application/pdf")
            {
                FileDownloadName = "EgresosTransactions.pdf"
            };
        }

    }
}