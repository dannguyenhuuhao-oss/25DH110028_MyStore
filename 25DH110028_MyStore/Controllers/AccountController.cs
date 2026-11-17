using _25DH110028_MyStore.Models;
using _25DH110028_MyStore.Models.ViewModel;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace _25DH110028_MyStore.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // ============ ĐĂNG KÝ ============
        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tên đăng nhập đã tồn tại chưa
                var existingUser = db.Users.SingleOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                    return View(model);
                }
                // Kiểm tra email đã tồn tại chưa
                if (db.Customers.Any(c => c.CustomerEmail == model.CustomerEmail))
                {
                    ModelState.AddModelError("CustomerEmail", "Email này đã được sử dụng!");
                    return View(model);
                }

                // 1 bản ghi trong bảng User
                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    UserRole = "C" // Mặc định khách hàng đăng ký là Customer ("C")
                };
                db.Users.Add(user);

                // 1 bản ghi trong bảng Customer
                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    CustomerPhone = model.CustomerPhone,
                    CustomerAddress = model.CustomerAddress,
                    Username = model.Username
                };
                db.Customers.Add(customer);

                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    foreach (var entry in ex.EntityValidationErrors)
                        foreach (var err in entry.ValidationErrors)
                            ModelState.AddModelError("", entry.Entry.Entity.GetType().Name + "." + err.PropertyName + ": " + err.ErrorMessage);

                    db.Entry(customer).State = System.Data.Entity.EntityState.Detached;
                    db.Entry(user).State = System.Data.Entity.EntityState.Detached;
                    return View(model);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    var inner = ex.InnerException;
                    while (inner != null && inner.InnerException != null) inner = inner.InnerException;
                    ModelState.AddModelError("", "Lỗi CSDL: " + (inner != null ? inner.Message : ex.Message));

                    db.Entry(customer).State = System.Data.Entity.EntityState.Detached;
                    db.Entry(user).State = System.Data.Entity.EntityState.Detached;
                    return View(model);
                }

                TempData["RegisterSuccess"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // ============ ĐĂNG NHẬP ============
        // GET: Account/Login
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User model)
        {
            var user = db.Users.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);
            if (user != null)
            {
                // 1. Lưu thông tin vào Session
                Session["Username"] = user.Username;
                Session["UserRole"] = user.UserRole;

                // 2. [QUAN TRỌNG] Tạo vé xác thực FormsAuthentication để hệ thống nhận diện User.Identity.Name
                FormsAuthentication.SetAuthCookie(user.Username, false);

                // 3. Phân quyền điều hướng
                string role = user.UserRole != null ? user.UserRole.Trim() : "";

                if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Equals("A", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Index", "Users", new { area = "Admin" });
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View(model);
        }

        // ============ ĐĂNG XUẤT ============
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Remove("Username");
            Session.Remove("UserRole"); // Giữ nguyên giỏ hàng trong Session (nếu có)
            return RedirectToAction("Index", "Home");
        }

        // ============ HÀM PHỤ: LẤY THÔNG TIN KHÁCH HÀNG ĐANG ĐĂNG NHẬP ============
        private ProfileVM LoadProfile()
        {
            var customer = db.Customers.FirstOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null) return null;

            return new ProfileVM
            {
                Username = customer.Username,
                CustomerName = customer.CustomerName,
                CustomerPhone = customer.CustomerPhone,
                CustomerEmail = customer.CustomerEmail,
                CustomerAddress = customer.CustomerAddress
            };
        }

        // ============ THÔNG TIN TÀI KHOẢN ============
        [Authorize]
        public ActionResult ProfileInfo()
        {
            var model = LoadProfile();
            if (model == null) return HttpNotFound();

            ViewBag.Saved = model;
            return View(model);
        }

        // ============ CẬP NHẬT THÔNG TIN ============
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(ProfileVM model)
        {
            model.Username = User.Identity.Name; // Không tin dữ liệu từ form gửi lên

            if (!ModelState.IsValid)
            {
                ViewBag.Saved = LoadProfile();
                ViewBag.OpenModal = "profile";
                return View("ProfileInfo", model);
            }

            var customer = db.Customers.FirstOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null) return HttpNotFound();

            customer.CustomerName = model.CustomerName;
            customer.CustomerPhone = model.CustomerPhone;
            customer.CustomerEmail = model.CustomerEmail;
            customer.CustomerAddress = model.CustomerAddress;
            db.SaveChanges();

            TempData["ProfileMessage"] = "Cập nhật thông tin thành công.";
            return RedirectToAction("ProfileInfo");
        }

        // ============ ĐỔI MẬT KHẨU ============
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
        {
            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return HttpNotFound();

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                TempData["PasswordError"] = "Mật khẩu mới phải có ít nhất 6 ký tự.";
            else if (newPassword.Length > 50)
                TempData["PasswordError"] = "Mật khẩu mới tối đa 50 ký tự.";
            else if ((user.Password ?? "").Trim() != oldPassword)
                TempData["PasswordError"] = "Mật khẩu hiện tại không đúng.";
            else if (newPassword != confirmNewPassword)
                TempData["PasswordError"] = "Mật khẩu mới và xác nhận không khớp.";
            else
            {
                user.Password = newPassword;
                db.SaveChanges();
                TempData["PasswordMessage"] = "Đổi mật khẩu thành công.";
                return RedirectToAction("ProfileInfo");
            }

            // Có lỗi: quay lại trang và tự mở cửa sổ đổi mật khẩu để hiện lỗi
            TempData["OpenModal"] = "password";
            return RedirectToAction("ProfileInfo");
        }

        public ActionResult Ping()
        {
            return Content("Account OK - user: " + User.Identity.Name);
        }

        // Cửa sổ thông tin khách hàng, nhúng vào layout
        [ChildActionOnly]
        public ActionResult CustomerInfoPV()
        {
            if (!User.Identity.IsAuthenticated) return new EmptyResult();

            var c = db.Customers.FirstOrDefault(x => x.Username == User.Identity.Name);
            if (c == null) return new EmptyResult();

            var model = new ProfileVM
            {
                Username = c.Username,
                CustomerName = c.CustomerName,
                CustomerPhone = c.CustomerPhone,
                CustomerEmail = c.CustomerEmail,
                CustomerAddress = c.CustomerAddress
            };
            return PartialView("_CustomerInfoPV", model);
        }
    }
}