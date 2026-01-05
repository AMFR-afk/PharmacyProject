using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmacyProject.BridgeData;
using PharmacyProject.Helpers;     // Session Extensions
//using Project.Models;              // Prop
using PharmacyProject.Models;      // Medicine, CartItem, ErrorViewModel
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PharmacyProject.Controllers
{
    public class PharmacyController : Controller
    {
        public static ClientProp CurrentUser= null;
        private readonly ILogger<PharmacyController> _logger;
        private readonly Bridge _context;
        // =======================
        // Static Medicines List (Prop)
        // =======================
        public static List<Prop> Props = new List<Prop>();

        public PharmacyController(ILogger<PharmacyController> logger, Bridge context)
        {
            _logger = logger;
            _context = context;

            if (!_context.PropTable.Any())
            {
                var initialMedicines = new List<Prop>
        {
            // ملاحظة: شلت الـ Id عشان الداتابيز هي اللي تعطيه أرقام متسلسلة تلقائياً
            new Prop { Name = "Paracetamol 250 mg", Price = 4.00, Details = "Pain relief and fever reducer." },
            new Prop { Name = "Amoxicillin 250 mg", Price = 3.00, Details = "Antibiotic for bacterial infections." },
            new Prop { Name = "Vitamin C", Price = 2.50, Details = "Boosts immunity." },
            new Prop { Name = "Vitamin D", Price = 2.00, Details = "Supports bones and immunity." },
            new Prop { Name = "Loratadine 10 mg", Price = 2.00, Details = "Allergy relief." },
            new Prop { Name = "Omeprazole 20 mg", Price = 3.00, Details = "Reduces stomach acid." },
            new Prop { Name = "Ibuprofen 200 mg", Price = 1.50, Details = "Pain and inflammation relief." },
            new Prop { Name = "Amlodipine 10 mg", Price = 1.00, Details = "Treats hypertension." },
            new Prop { Name = "Glimepiride 4 mg", Price = 2.50, Details = "Lowers blood sugar." }
        };

                _context.PropTable.AddRange(initialMedicines); // إضافة الادوية للداتابيز
                _context.SaveChanges(); // احفظ في الداتابيز
            }
        }

        // =======================
        // Home / Medicines Talbe
        // =======================

        public IActionResult Table()
        {
            if (HttpContext.Session.GetString("UserName") == null) // تعديل لفحص السشن
            {
                return RedirectToAction("Login");
            }

            var data = _context.PropTable.ToList();
            return View(data);
        }

        public IActionResult Details(int id)
        {
            var item = _context.PropTable.FirstOrDefault(x => x.Id == id);
            return View(item);
        }


        // صفحة الأدمن
        public IActionResult AdminTable()
        {
            // إذا مش أدمن رجعه عالرئيسية
            if (HttpContext.Session.GetString("Role") != "ADMIN")
            {
                return RedirectToAction("Login");
            }

            var data = _context.PropTable.ToList(); 
            return View(data);
        }

        // =======================
        // Add Medicine
        // =======================
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Prop model) // [cite: 133]
        {
            if (ModelState.IsValid)
            {
                // إضافة الدواء للداتابيز
                await _context.PropTable.AddAsync(model); // [cite: 136]
                await _context.SaveChangesAsync();        // [cite: 137]

                return RedirectToAction("AdminTable");
            }
            return View(model);
        }

        // =======================
        // Edit Medicine
        // =======================

        [HttpGet]
        public async Task<IActionResult> Edit(int id) // [cite: 163]
        {
            // البحث عن الدواء بواسطة الـ ID
            var medicine = await _context.PropTable.FindAsync(id); // [cite: 157]

            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Prop model) // [cite: 163]
        {
            // 1. نجيب الدواء الأصلي من الداتابيز
            var existingMedicine = await _context.PropTable.FindAsync(model.Id); // [cite: 159]

            if (existingMedicine != null)
            {
                // 2. نعدل البيانات (Mapping) زي ما هو بالسلايد
                existingMedicine.Name = model.Name;
                existingMedicine.Price = model.Price;
                existingMedicine.Details = model.Details;

                // 3. نحفظ التغييرات
                await _context.SaveChangesAsync(); // [cite: 160]

                return RedirectToAction("AdminTable");
            }

            return View(model);
        }

        // =======================
        // Delete Medicine
        // =======================

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // فحص صلاحية
            if (HttpContext.Session.GetString("Role") != "ADMIN") return RedirectToAction("Login");

            var item = await _context.PropTable.FindAsync(id);
            if (item != null)
            {
                _context.PropTable.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("AdminTable");
        }

        // =======================
        // Cart
        // =======================
        public IActionResult Cart()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddToCart(int id, int quantity = 1) 
        {
            var med = _context.PropTable.FirstOrDefault(m => m.Id == id);// جلب الدواء من الداتابيز

            if (med == null)
                return RedirectToAction("Cart");

            var cart = HttpContext.Session.GetObject<List<CartItem>>("CART")
                                                    ?? new List<CartItem>();// جلب السلة من السشن أو إنشاء جديدة إذا مش موجودة

            var existing = cart.FirstOrDefault(x => x.MedicineId == med.Id);// فحص إذا الدواء موجود بالسلة
            if (existing == null)
            {
                cart.Add(new CartItem
                {
                    MedicineId = med.Id,
                    Name = med.Name,
                    Price = med.Price,
                    Quantity = quantity // استخدم الكمية اللي جاية من الفورم
                });// إضافة الدواء للسلة
            }
            else
            {
                existing.Quantity += quantity; // زيد الكمية الجديدة على القديمة
            }

            HttpContext.Session.SetObject("CART", cart);
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int medicineId)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("CART")
                       ?? new List<CartItem>();

            var item = cart.FirstOrDefault(x => x.MedicineId == medicineId);
            if (item != null)
                cart.Remove(item);

            HttpContext.Session.SetObject("CART", cart);
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("CART");
            return RedirectToAction("Cart");
        }

        // =======================
        // Checkout Logic
        // =======================
        public IActionResult Checkout()
        {
            // 1. فحص تسجيل الدخول: هل المستخدم مسجل دخول؟
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login"); // إذا مش مسجل، روح سجل دخول أول
            }

            // 2. يجيب السلة
            var cart = HttpContext.Session.GetObject<List<CartItem>>("CART");

            // فحص إذا السلة فاضية
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Table"); // ما في شي تشتريه، ارجع عالسوق
            }

            // تفريغ السلة بعد الشراء
            HttpContext.Session.Remove("CART");

            // توجيه لصفحة نجاح الطلب
            return View("OrderSuccess");
        }

        // =======================
        // Auth
        // =======================

        // =======================
        // Login
        // =======================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exist = await _context.UserData
                                          .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (exist == null) // إذا الإيميل مش موجود
                {
                    ModelState.AddModelError("", "الإيميل أو كلمة المرور غير صحيحة");
                    return View(model);
                }

                
                if (exist.Password != model.Password)//  اذا كلمة المرور غلط
                {
                    ModelState.AddModelError("", "الإيميل أو كلمة المرور غير صحيحة");
                    return View(model);
                }

                
                // حفظ السشن
                HttpContext.Session.SetString("UserName", exist.Name);
                HttpContext.Session.SetString("Role", exist.Role); // بنحفظ الرول كمان عشان نستخدمه بعدين

                // توزيع الصلاحيات
                if (exist.Role == "ADMIN")
                {
                    return RedirectToAction("AdminTable");
                }
                else // أي شي غير أدمن يعتبر يوزر
                {
                    return RedirectToAction("Table");
                }
            }

            return View(model);
        }

        // =======================
        // SignUp
        // =======================

        public async Task<IActionResult> SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(ClientProp newuser)
        {
            // فحص إذا الإيميل موجود في جدول UserData
            var existingUser = await _context.UserData
                                             .FirstOrDefaultAsync(u => u.Email == newuser.Email);

            if (existingUser != null)
            {
                // إذا الإيميل موجود، رجع خطأ للمستخدم
                ModelState.AddModelError("Email", "هذا الإيميل مستخدم مسبقاً، حاول تسجيل الدخول.");
                return View(newuser);
            }

            if (ModelState.IsValid) 
            {
                newuser.Role = "USER"; // تعيين الدور كـ "User" افتراضياً

                _context.UserData.Add(newuser);
                await _context.SaveChangesAsync(); 

                return RedirectToAction("Login");
            }

            // إذا في خطأ رجع نفس الصفحة 
            return View(newuser);
        }

        // =======================
        // Error / Privacy
        // =======================

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
