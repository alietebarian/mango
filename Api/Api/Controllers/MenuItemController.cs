using Api.Data;
using Api.Migrations;
using Api.Models;
using Api.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MenuItemController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MenuItemController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMenuItems()
    {
        var menuItems = await _context.MenuItems.ToListAsync();

        return Ok(menuItems);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMenuItem(int id)
    {
        var menuItem = await _context.MenuItems.FirstOrDefaultAsync(x => x.Id == id);

        if (menuItem == null)
            return NotFound("there is no item with this id");

        return Ok(menuItem);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMenuItem([FromForm] CreateMenuItemDto model)
    {
        // مسیر فولدر برای ذخیره عکس‌ها
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

        // اگر فولدر وجود نداره، بساز
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string uniqueFileName = null;

        if (model.File != null && model.File.Length > 0)
        {
            // ایجاد نام یکتا برای فایل
            uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);

            // مسیر کامل فایل
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // ذخیره فایل روی سرور
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(fileStream);
            }
        }

        // ایجاد منو آیتم
        MenuItem menuItemToCreate = new()
        {
            Name = model.Name,
            Price = model.Price,
            Category = model.Category,
            SpecialTag = model.SpecialTag,
            Description = model.Description,
            Image = uniqueFileName != null ? "images/" + uniqueFileName : null
        };

        // ذخیره در دیتابیس
        _context.MenuItems.Add(menuItemToCreate);
        await _context.SaveChangesAsync();

        return Ok(menuItemToCreate);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMenuItem(int id, [FromForm] UpdateMenuItemDto model)
    {
        var menuItem = await _context.MenuItems.FindAsync(id);

        if (menuItem == null)
            return NotFound("There is no item with this id");

        // مسیر فولدر تصاویر
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // بررسی اینکه آیا فایل جدید ارسال شده
        if (model.File != null && model.File.Length > 0)
        {
            // ایجاد نام یکتا برای فایل
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // ذخیره فایل جدید روی سرور
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(fileStream);
            }

            // حذف فایل قبلی در صورت وجود (اختیاری)
            if (!string.IsNullOrEmpty(menuItem.Image))
            {
                string oldFilePath = Path.Combine(uploadsFolder, Path.GetFileName(menuItem.Image));
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);
            }

            // بروزرسانی مسیر تصویر
            menuItem.Image = "images/" + uniqueFileName;
        }

        // بروزرسانی سایر فیلدها
        menuItem.Name = model.Name;
        menuItem.Price = model.Price;
        menuItem.Category = model.Category;
        menuItem.SpecialTag = model.SpecialTag;
        menuItem.Description = model.Description;

        // ذخیره تغییرات
        await _context.SaveChangesAsync();

        return Ok(menuItem);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMenuItem(int id)
    {
        var menuItem = await _context.MenuItems.FindAsync(id);

        if (menuItem == null)
            return NotFound("There is no item with this id");

        _context.Remove(menuItem);
        await _context.SaveChangesAsync();

        return Ok("delete successFully");
    }
}
