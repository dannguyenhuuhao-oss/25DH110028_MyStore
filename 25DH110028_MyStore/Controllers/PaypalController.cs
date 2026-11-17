using _25DH110028_MyStore.Models;
using _25DH110028_MyStore.Models.ViewModel;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace _25DH110028_MyStore.Controllers
{
    public class PaypalController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private APIContext GetAPIContext()
        {
            var config = ConfigManager.Instance.GetProperties();
            var accessToken = new OAuthTokenCredential(config).GetAccessToken();
            return new APIContext(accessToken);
        }

        public ActionResult PaymentWithPaypal(CheckoutVM model)
        {
            APIContext apiContext = GetAPIContext();
            try
            {
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Paypal/PaymentWithPaypal?";
                    var guid = Convert.ToString(new Random().Next(100000));
                    var createdPayment = CreatePayment(apiContext, baseURI + "guid=" + guid, model);

                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = lnk.href;
                        }
                    }

                    Session.Add(guid, createdPayment.id);
                    Session["CheckoutModel"] = model; // Lưu thông tin đơn hàng để tạo sau khi thanh toán thành công
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }

                    // THANH TOÁN THÀNH CÔNG -> Lưu đơn hàng vào CSDL
                    var savedModel = Session["CheckoutModel"] as CheckoutVM;
                    if (savedModel != null)
                    {
                        var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
                        var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
                        var cart = Session["Cart"] as List<CartItem> ?? (Session["Cart"] as Cart)?.Items as List<CartItem>;

                        var order = new _25DH110028_MyStore.Models.Order
                        {
                            CustomerID = customer.CustomerID,
                            OrderDate = DateTime.Now,
                            TotalAmount = savedModel.TotalAmount,
                            PaymentStatus = "Thanh toán paypal",
                            PaymentMethod = "Paypal",
                            DeliveryMethod = savedModel.ShippingMethod,
                            ShippingAddress = savedModel.ShippingAddress,
                            OrderDetails = new List<_25DH110028_MyStore.Models.OrderDetail>()
                        };

                        if (cart != null)
                        {
                            foreach (var item in cart)
                            {
                                order.OrderDetails.Add(new OrderDetail
                                {
                                    ProductID = item.ProductID,
                                    Quantity = item.Quantity,
                                    UnitPrice = item.UnitPrice,
                                    TotalPrice = item.TotalPrice
                                });
                            }
                        }

                        db.Orders.Add(order);
                        db.SaveChanges();

                        Session["Cart"] = null; // Xóa giỏ hàng
                        return RedirectToAction("OrderSuccess", "Order", new { id = order.OrderID });
                    }
                }
            }
            catch (Exception)
            {
                return View("FailureView");
            }

            return RedirectToAction("Checkout", "Order");
        }

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            var payment = new Payment() { id = paymentId };
            return payment.Execute(apiContext, paymentExecution);
        }

        private Payment CreatePayment(APIContext apiContext, string redirectUrl, CheckoutVM model)
        {
            var itemList = new ItemList() { items = new List<Item>() };

            // Lưu ý: PayPal Sandbox bắt buộc dùng đơn vị USD (Không hỗ trợ VND). 
            // Ta quy đổi tạm giá tiền sang USD bằng cách chia tỷ giá (ví dụ 1 USD = 25,000 VND)
            decimal exchangeRate = 25000m;

            foreach (var item in model.CartItems)
            {
                decimal priceInUSD = Math.Round(item.UnitPrice / exchangeRate, 2);
                itemList.items.Add(new Item()
                {
                    name = item.ProductName,
                    currency = "USD",
                    price = priceInUSD.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                    quantity = item.Quantity.ToString(),
                    sku = item.ProductID.ToString()
                });
            }

            var payer = new Payer() { payment_method = "paypal" };

            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl
            };

            decimal totalUSD = Math.Round(model.TotalAmount / exchangeRate, 2);
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = totalUSD.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
            };

            var amount = new Amount()
            {
                currency = "USD",
                total = totalUSD.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                details = details
            };

            var transactionList = new List<Transaction>();
            transactionList.Add(new Transaction()
            {
                description = "Thanh toán đơn hàng sách từ The Kings Bookshop",
                invoice_number = Convert.ToString(new Random().Next(100000)),
                amount = amount,
                item_list = itemList
            });

            var payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            return payment.Create(apiContext);
        }
    }
}