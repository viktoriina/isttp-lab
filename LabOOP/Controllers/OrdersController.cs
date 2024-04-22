﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LabOOP.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ClosedXML.Excel;
using System.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace LabOOP.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly DBSHOPContext _context;
        private readonly UserManager<User> _userManager;
       // private readonly IUserService userService;

        public OrdersController(DBSHOPContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;

        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dBSHOPContext = await _context.Orders.Where(e=>e.UserId == userId).Include(o => o.Deliver).Include(o => o.ProductsOrders).ToListAsync();
           var dBProducts = await _context.ProductsOrders.ToListAsync();
                foreach (var item in dBSHOPContext)
                {             
                        if (item.DateOrder == null)
                        {
                           foreach(var orderproduct in item.ProductsOrders)
                             _context.Remove(orderproduct);
                           _context.Remove(item);
                           await _context.SaveChangesAsync();
                        }                  
                }
            var filteredOrders = await _context.Orders.Where(e => e.UserId == userId).Include(o => o.Deliver).ToListAsync();
            return View(filteredOrders);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmPurchase(int orderId)
        {
            var productCount = await _context.ProductsOrders.Where(elem => elem.OrderId == orderId).CountAsync();
            if (productCount > 0)
            {
                var order = await _context.Orders.FirstOrDefaultAsync(elem => elem.Id == orderId);
                if (order == null)
                    return NotFound();
                else
                {
                    order.DateOrder = DateTime.Now;
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }
            var order = await _context.Orders
                .Include(a => a.ProductsOrders)
                .Include(d => d.Deliver)
                .ThenInclude(b => b.Transport)
                .FirstOrDefaultAsync(m => m.Id == id);

            var productsInOrder = await _context.ProductsOrders.
                Where(elem => elem.OrderId == id).
                Include(o => o.Product).
                ThenInclude(b => b.Country).
                ToListAsync();

            ViewBag.orderId = id;
            if (order == null)
            {
                return NotFound();
            }
            var deliver = order.Deliver;
            ViewBag.Deliver = deliver;
            return View(productsInOrder);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["DeliverId"] = new SelectList(_context.Delivers.Include(o => o.Transport), "Id", "DisplayText");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClientId,DateOrder,DeliverId,,Address")] Order order)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                order.UserId = userId;
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction("AddProduct", "Products", new { id = order.Id });
            }
            ViewData["DeliverId"] = new SelectList(_context.Delivers.Include(o => o.Transport), "Id", "Name", order.DeliverId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["DeliverId"] = new SelectList(_context.Delivers.Include(o => o.Transport), "Id", "DisplayText", order.DeliverId);
            return View(order);
        }
        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,DateOrder,DeliverId,Address")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("AddProduct", "Products", new { id = id });
            }
            ViewData["DeliverId"] = new SelectList(_context.Delivers.Include(o => o.Transport), "Id", "DisplayText", order.DeliverId);
            return View(order);
        }
        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Deliver)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Orders == null)
            {
                return Problem("Entity set 'DBSHOPContext.Orders'  is null.");
            }
            var order = await _context.Orders
                .Include(o => o.ProductsOrders)
                .Include(o => o.Deliver).Include(o=>o.Feedbacks)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order != null)
            {
                foreach(var o in order.ProductsOrders)
                    _context.Remove(o);
                foreach (var o in order.Feedbacks)
                    _context.Remove(o);
                _context.Orders.Remove(order);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
          return _context.Orders.Any(e => e.Id == id);
        }

        public ActionResult Export()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var delivers = _context.Delivers.
                    Include(e => e.Transport).
                    Include(a => a.Orders.Where(e => e.UserId==userId))
                    .ThenInclude(b => b.Feedbacks)
                    .ToList();
                var transports = _context.Transports.Include(e => e.Delivers).ThenInclude(a => a.Orders.Where(e => e.UserId == userId)).ThenInclude(f => f.Feedbacks).ToList();
                foreach (var transport in transports)
                {
                    var worksheet = CreareWorkSheet(workbook, transport);
                    CreatePage(worksheet);
                    ValueInTable(worksheet, transport);
                }
                using (var stream = new MemoryStream())
                {
                   
                    workbook.SaveAs(stream);
                    stream.Flush();
                    if (stream.Length == 0 || stream == null)
                    {
                        return RedirectToAction("Index", "Orders");
                    }
                    return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"ПостачальникиЩоМаютьЗамовлення.xlsx"
                    };
                }
            }
        }
        private void ValueInTable(IXLWorksheet? worksheet, Transport transport)
        {
            if (worksheet == null)
                return;
            int rowMumber = 1;
            foreach (var item in transport.Delivers)
            {
                foreach (var order in item.Orders)
                {
                        rowMumber++;
                        worksheet.Cell(rowMumber, 1).Value = item.PhoneNumber;
                        worksheet.Cell(rowMumber, 1).Style.NumberFormat.Format = "0";
                        worksheet.Cell(rowMumber, 2).Value = item.Name;
                        worksheet.Cell(rowMumber, 3).Value = item.Surname;
                        worksheet.Cell(rowMumber, 4).Value = order.DateOrder?.ToString("M/d/yyyy h:mm:ss tt");
                        worksheet.Cell(rowMumber, 5).Value = order.Address;
                }
            }
        }
        private IXLWorksheet? CreareWorkSheet(XLWorkbook workbook, Transport transport)
        {
            int orderCount = 0;
            foreach (var item in transport.Delivers) 
            { 
               foreach(var order in item.Orders)
                {
                    orderCount++;
                }
            }
            if (orderCount == 0)
                return null;
            else
            {
                var worksheet = workbook.Worksheets.Add(transport.Name);
                return worksheet;
            }
        }
        private void CreatePage(IXLWorksheet? worksheet)
        {
            if (worksheet == null)
                return;
            worksheet.Cell("A1").Value = "Номер телефону";
            worksheet.Cell("B1").Value = "Ім`я";
            worksheet.Cell("C1").Value = "Фамілія";
            worksheet.Cell("D1").Value = "Час замовлення";
            worksheet.Cell("E1").Value = "Куди має прийти";
            worksheet.Row(1).Style.Font.Bold = true;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile fileExcel)
        {
            string result;
            if (ModelState.IsValid)
            {
                if (fileExcel != null)
                {
                    using (var stream = new FileStream(fileExcel.FileName, FileMode.Create))
                    {
                        await fileExcel.CopyToAsync(stream);
                        using (XLWorkbook workBook = new XLWorkbook(stream, XLEventTracking.Disabled))
                        {
                            foreach (IXLWorksheet worksheet in workBook.Worksheets)
                            {
                                var transport = FindTransportOrCreate(worksheet);
                                result = WorkWithRow(worksheet, transport);
                                if (result != "Success")
                                { 
                                    return RedirectToAction(nameof(BadResult), "Orders", new { result = result });
                                }
                            }
                        }
                    }
                }
              //  await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> BadResult(string result)
        {
            ViewBag.Success = result;
            return View();
        }
        private Transport FindTransportOrCreate(IXLWorksheet worksheet)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var c = (from item in _context.Transports.Include(e => e.Delivers).ThenInclude(a => a.Orders).ToList()
                     where item.Name.Contains(worksheet.Name) select item).ToList();
            if (c.Count>0)
                return c[0];
            else
            {
                Transport transport = new Transport { Name = worksheet.Name };
                _context.Transports.Add(transport);
                _context.SaveChanges();
                return transport;
            }
        }
        private string WorkWithRow(IXLWorksheet worksheet, Transport transport)
        {
            Deliver? deliver;
            Order? order;
            int rowNumber = 2;
            foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
            {
                deliver = RowDelivery(row, transport);
                if (deliver == null)
                    return $"Check the delivery value on the {transport.Name} page in the {rowNumber} line, the first and last name must not be empty and the number must start with 380 and after that any 9 digits and be unique";
                order = RowOrder(row, deliver);
                if (order == null)
                    return $"Check the value of the order on the page {transport.Name} in the line {rowNumber}, the delivery address must not be empty and the date must match the format M/d/yyyy h:mm:ss tt";
                rowNumber++;
            }
            return "Success";
        }
        private Deliver? RowDelivery(IXLRow row, Transport transport)
        {
            Deliver deliver = new Deliver();
            deliver.PhoneNumber = row.Cell(1)?.Value?.ToString() ?? "N/I";
            deliver.Name = row.Cell(2)?.Value?.ToString() ?? "N/I";
            deliver.Surname = row.Cell(3)?.Value?.ToString() ?? "N/I";
            if (!NameSurnamePhoneIsValid(deliver.PhoneNumber, deliver.Name, deliver.Surname))
                return null;
            if (DeliverExist(deliver) == false)
            {
                deliver.Transport = transport;
                _context.Add(deliver);
                _context.SaveChanges();
            }
            else
            {
                if (DeliverThisTransport(deliver, transport) == false)
                    return null;
                else
                {
                    deliver = _context.Delivers.Include(e => e.Transport).FirstOrDefault(e => e.Transport.Name == transport.Name && e.PhoneNumber == deliver.PhoneNumber && e.Name == deliver.Name && e.Surname == deliver.Surname);
                }
                   
            }
            return deliver;
        }

        private Order? RowOrder(IXLRow row, Deliver deliver)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            Order order = new Order();
            DateTime dateTime;
            if (DateTime.TryParseExact(row?.Cell(4)?.Value?.ToString(), "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime) && (order.Address != String.Empty))
                order.DateOrder = dateTime;
            else
                return null;
            order.Address = row?.Cell(5)?.Value?.ToString() ?? "N/I";
           if(IfOrderExist(order, deliver) == false)
            {
                order.UserId = userId;
                order.Deliver = deliver;
                _context.Add(order);
                _context.SaveChanges();
            }
            return order;
        }
        private bool IfOrderExist(Order order, Deliver deliver) 
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (order.Address == null || order.Address == String.Empty)
                return false;
            foreach (var item in deliver.Orders.Where(e=>e.UserId==userId))
            {
                string dateString = order.DateOrder?.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture) ?? "N/a";
                string dataString2 = item.DateOrder?.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture) ?? "N/a";
                if (dateString.CompareTo(dataString2) == 0 && item.Address == order.Address)
                    return true;
            }
            return false;
        }
        private bool DeliverExist(Deliver deliver)
        {
            return _context.Delivers.Any(a => a.PhoneNumber== deliver.PhoneNumber); 
        }
        private bool DeliverThisTransport(Deliver deliver, Transport transport) 
        { 
           if (_context.Delivers.Include(e => e.Transport).FirstOrDefault(e => e.Transport.Name == transport.Name && e.PhoneNumber == deliver.PhoneNumber && e.Name == deliver.Name && e.Surname == deliver.Surname) != null) 
                return true;
           else 
                return false;

        }
        private bool NameSurnamePhoneIsValid(string phoneNumber, string name, string surname)
        {
            return Regex.IsMatch(phoneNumber, @"^380\d{9}$") && (name != String.Empty) && (surname != String.Empty);

        }
    }
}
