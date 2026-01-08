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
using MimeKit;
using MailKit.Net.Smtp;

namespace PharmacyProject.Controllers
{
    public class PharmacyController : Controller
    {
        public static ClientProp CurrentUser = null;
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
            new Prop { Name = "Glimepiride 4 mg", Price = 2.50, Details = "Lowers blood sugar." },
            new Prop { Name = "Vitamin B", Price = 4.50, Details = "Supports energy production and nervous system health" },
            new Prop { Name = "Iron Supplement", Price = 5.25, Details = "Used to prevent and treat iron deficiency anemia" },
            new Prop { Name = "Calcium", Price = 4.00, Details = "Essential for strong bones and teeth" },
            new Prop { Name = "Omega 3", Price = 7.50, Details = "Supports heart and brain health" },
            new Prop { Name = "Zinc", Price = 3.50, Details = "Strengthens immune system and supports healing" },
            new Prop { Name = "Magnesium", Price = 4.75, Details = "Helps with muscle function and relaxation" },
            new Prop { Name = "Pain Relief Tablets", Price = 2.50, Details = "Used to relieve mild to moderate pain" },
            new Prop { Name = "Cough Syrup", Price = 3.25, Details = "Relieves cough and soothes throat irritation" },
            new Prop { Name = "Antibiotic Capsules", Price = 6.80, Details = "Used to treat bacterial infections" },
        };

                _context.PropTable.AddRange(initialMedicines); // إضافة الادوية للداتابيز
                _context.SaveChanges(); // احفظ في الداتابيز
            }
        }

        // =======================
        // Home / Medicines Talbe
        // =======================


        [HttpGet]
        public IActionResult Table(string searchString)
        {

            if (HttpContext.Session.GetString("UserName") == null) //  فحص تسجيل الدخول
            {
                return RedirectToAction("Login");
            }


            var medicines = from m in _context.PropTable
                            select m;//  جلب كل الأدوية من الداتابيز



            if (!string.IsNullOrEmpty(searchString))//  إذا المستخدم كتب شي في البحث
            {
                // ابحث في الاسم أو التفاصيل
                medicines = medicines.Where(s => s.Name.Contains(searchString) || s.Details.Contains(searchString));
            }

            // 4. حفظ كلمة البحث عشان ترجع تنعرض في المربع (User Friendly)
            ViewData["CurrentFilter"] = searchString;

            // 5. تنفيذ الاستعلام وإرسال النتائج
            return View(medicines.ToList());
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
        public async Task<IActionResult> Add(Prop model)
        {
            if (ModelState.IsValid)
            {
                // إضافة الدواء للداتابيز
                await _context.PropTable.AddAsync(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("AdminTable");
            }
            return View(model);
        }

        // =======================
        // Edit Medicine
        // =======================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // البحث عن الدواء بواسطة الـ ID
            var medicine = await _context.PropTable.FindAsync(id);

            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Prop model)
        {
            // 1. نجيب الدواء الأصلي من الداتابيز
            var existingMedicine = await _context.PropTable.FindAsync(model.Id);

            if (existingMedicine != null)
            {
                // 2. نعدل البيانات (Mapping) 
                existingMedicine.Name = model.Name;
                existingMedicine.Price = model.Price;
                existingMedicine.Details = model.Details;

                // 3. نحفظ التغييرات
                await _context.SaveChangesAsync();

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
        [HttpPost]
        public IActionResult RemoveFromCart(int medicineId)
        {
            // 1. جلب السلة من السشن
            var cart = HttpContext.Session.GetObject<List<CartItem>>("CART");

            if (cart != null)
            {
                // 2. البحث عن العنصر وحذفه
                var item = cart.FirstOrDefault(i => i.MedicineId == medicineId);
                if (item != null)
                {
                    cart.Remove(item);

                    // 3. تحديث السشن بالسلة الجديدة
                    HttpContext.Session.SetObject("CART", cart);
                }
            }

            // 4. الرجوع لصفحة السلة
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
            // بنجيب الإيميل المخزن في السشن (تأكد إنك خزنته في Login)
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userName = HttpContext.Session.GetString("UserName");

            if (userName == null || userEmail == null)
            {
                return RedirectToAction("Login"); // إذا مش مسجل، روح سجل دخول أول
            }

            // 2. جلب السلة
            var cart = HttpContext.Session.GetObject<List<CartItem>>("CART");

            // فحص إذا السلة فاضية
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Table"); // ما في شي تشتريه، ارجع عالسوق
            }

            // ==========================================
            // 🔥 هون المكان الصحيح لإرسال الإيميل 🔥
            // ==========================================
            SendOrderEmail("novateam2026@gmail.com", userEmail, userName, cart);

            // ==========================================

            // 3. تفريغ السلة بعد الشراء (عشان ما يشتري نفس الغراض مرتين)
            HttpContext.Session.Remove("CART");

            // 4. توجيه لصفحة نجاح الطلب
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

                if (exist == null)
                {
                    ModelState.AddModelError("", "الإيميل أو كلمة المرور غير صحيحة");
                    return View(model);
                }

                if (exist.Password != model.Password)
                {
                    ModelState.AddModelError("", "الإيميل أو كلمة المرور غير صحيحة");
                    return View(model);
                }

                // ===============================================
                // هون المكان الصحيح! (لأن الدخول نجح)
                // ===============================================

                // حفظنا الاسم عشان الترحيب
                HttpContext.Session.SetString("UserName", exist.Name);

                // حفظنا الرول عشان الصلاحيات
                HttpContext.Session.SetString("Role", exist.Role);

                // 🔥 حفظنا الإيميل عشان نستخدمه في إيميل الطلب (Checkout) 🔥
                HttpContext.Session.SetString("UserEmail", exist.Email);

                // ===============================================

                if (exist.Role == "ADMIN")
                {
                    return RedirectToAction("AdminTable");
                }
                else
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

        // =======================
        // Email Helper Helper
        // =======================
        private void SendOrderEmail(string adminEmail, string userEmail, string userName, List<CartItem> cart)
        {
            try
            {
                var message = new MimeMessage();

                // 1. إعدادات المرسل والمستقبل
                // المرسل: إيميل الصيدلية الجديد
                message.From.Add(new MailboxAddress("Nova Pharmacy Team", "novateam2026@gmail.com"));

                // المستقبل: إيميل الأدمن (هو نفسه نوفا تيم حسب طلبك)
                message.To.Add(new MailboxAddress("Admin", "novateam2026@gmail.com"));

                message.Subject = $"New Order Alert: {userName}";

                // 2. بناء محتوى الإيميل (HTML Table)
                var bodyBuilder = new BodyBuilder();

                // بداية الجدول وتنسيقه
                string emailBody = $@"
            <div style='font-family: Arial, sans-serif; color: #333;'>
                <h2 style='color: #0d6efd;'>New Order Received!</h2>
                <p><strong>Customer Name:</strong> {userName}</p>
                <p><strong>Customer Email:</strong> {userEmail}</p>
                <hr>
                <table style='width: 100%; border-collapse: collapse; border: 1px solid #ddd;'>
                    <tr style='background-color: #f8f9fa;'>
                        <th style='padding: 10px; border: 1px solid #ddd; text-align: left;'>Medicine</th>
                        <th style='padding: 10px; border: 1px solid #ddd; text-align: center;'>Qty</th>
                        <th style='padding: 10px; border: 1px solid #ddd; text-align: right;'>Price</th>
                        <th style='padding: 10px; border: 1px solid #ddd; text-align: right;'>Total</th>
                    </tr>";

                // حلقة تكرار لإضافة الأدوية سطر سطر
                double grandTotal = 0;
                foreach (var item in cart)
                {
                    emailBody += $@"
                    <tr>
                        <td style='padding: 8px; border: 1px solid #ddd;'>{item.Name}</td>
                        <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                        <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>{item.Price:F2} JOD</td>
                        <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>{item.LineTotal:F2} JOD</td>
                    </tr>";
                    grandTotal += item.LineTotal;
                }

                // إغلاق الجدول وإضافة المجموع الكلي
                emailBody += $@"
                    <tr style='background-color: #e9ecef; font-weight: bold;'>
                        <td colspan='3' style='padding: 10px; border: 1px solid #ddd; text-align: right;'>Grand Total</td>
                        <td style='padding: 10px; border: 1px solid #ddd; text-align: right; color: #198754;'>{grandTotal:F2} JOD</td>
                    </tr>
                </table>
                <br>
                <p style='font-size: 12px; color: #777;'>Sent automatically from Nova Pharmacy System.</p>
            </div>";

                bodyBuilder.HtmlBody = emailBody;
                message.Body = bodyBuilder.ToMessageBody();

                // 3. الاتصال والإرسال
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    // الاتصال (بما إنه 465 زبط معك خليه زي ما هو)
                    client.Connect("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);

                    // الدخول (تأكد من وضع App Password الخاص بـ novateam2026)
                    client.Authenticate("novateam2026@gmail.com", "zaxw xscx ityn pshi");

                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }

        // =======================
        // Logout
        // =======================
        public IActionResult Logout()
        {
            // حذف كل بيانات السشن (اسم المستخدم، السلة، الرول)
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
//"novateam2026@gmail.com", "zaxw xscx ityn pshi"