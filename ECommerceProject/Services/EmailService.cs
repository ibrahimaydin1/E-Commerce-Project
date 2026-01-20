using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ECommerceProject.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendOrderConfirmationEmailAsync(string toEmail, string customerName, string orderNumber, decimal totalAmount)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = _configuration["EmailSettings:SmtpPort"];
                var smtpUser = _configuration["EmailSettings:SmtpUser"];
                var smtpPass = _configuration["EmailSettings:SmtpPass"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                using (var client = new System.Net.Mail.SmtpClient(smtpHost, int.Parse(smtpPort)))
                {
                    client.EnableSsl = true;
                    client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);

                    var fromAddress = new System.Net.Mail.MailAddress(fromEmail, fromName);
                    var toAddress = new System.Net.Mail.MailAddress(toEmail);
                    
                    var subject = "Siparişiniz Onaylandı - " + orderNumber;
                    var body = GenerateOrderConfirmationEmailBody(customerName, orderNumber, totalAmount);

                    var message = new System.Net.Mail.MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    await client.SendMailAsync(message);
                    return true;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("E-posta gönderilemedi: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> SendOrderStatusUpdateEmailAsync(string toEmail, string customerName, string orderNumber, string status)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = _configuration["EmailSettings:SmtpPort"];
                var smtpUser = _configuration["EmailSettings:SmtpUser"];
                var smtpPass = _configuration["EmailSettings:SmtpPass"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                using (var client = new System.Net.Mail.SmtpClient(smtpHost, int.Parse(smtpPort)))
                {
                    client.EnableSsl = true;
                    client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);

                    var fromAddress = new System.Net.Mail.MailAddress(fromEmail, fromName);
                    var toAddress = new System.Net.Mail.MailAddress(toEmail);
                    
                    var subject = "Sipariş Durumu Güncellendi - " + orderNumber;
                    var body = GenerateOrderStatusUpdateEmailBody(customerName, orderNumber, status);

                    var message = new System.Net.Mail.MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    await client.SendMailAsync(message);
                    return true;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Durum e-postası gönderilemedi: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string customerName)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = _configuration["EmailSettings:SmtpPort"];
                var smtpUser = _configuration["EmailSettings:SmtpUser"];
                var smtpPass = _configuration["EmailSettings:SmtpPass"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                using (var client = new System.Net.Mail.SmtpClient(smtpHost, int.Parse(smtpPort)))
                {
                    client.EnableSsl = true;
                    client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);

                    var fromAddress = new System.Net.Mail.MailAddress(fromEmail, fromName);
                    var toAddress = new System.Net.Mail.MailAddress(toEmail);
                    
                    var subject = "Hoş Geldiniz!";
                    var body = GenerateWelcomeEmailBody(customerName);

                    var message = new System.Net.Mail.MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    await client.SendMailAsync(message);
                    return true;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Hoş geldin e-postası gönderilemedi: " + ex.Message);
                return false;
            }
        }

        private string GenerateOrderConfirmationEmailBody(string customerName, string orderNumber, decimal totalAmount)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Sipariş Onayı</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Siparişiniz Onaylandı!</h1>
        </div>
        <div class='content'>
            <p>Merhaba <strong>{customerName}</strong>,</p>
            <p>Siparişiniz başarıyla alınmıştır. Sipariş detayları aşağıdadır:</p>
            
            <div style='background-color: white; padding: 15px; border-left: 4px solid #007bff; margin: 20px 0;'>
                <h3>Sipariş Bilgileri</h3>
                <p><strong>Sipariş No:</strong> {orderNumber}</p>
                <p><strong>Toplam Tutar:</strong> {totalAmount:F2} TL</p>
                <p><strong>Durum:</strong> Hazırlanıyor</p>
            </div>
            
            <p>Siparişiniz en kısa sürede kargoya verilecektir. Kargo takip numaranız size ayrıca bildirilecektir.</p>
            
            <p>Sipariş durumunuzu takip etmek için:</p>
            <a href='https://localhost:5214/Checkout/OrderHistory' class='button'>Siparişlerim</a>
            
            <p>Soru veya sorunlarınız için bize ulaşabilirsiniz.</p>
            
            <p>Teşekkür ederiz!</p>
            <p>E-Commerce Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 E-Commerce. Tüm hakları saklıdır.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateOrderStatusUpdateEmailBody(string customerName, string orderNumber, string status)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Sipariş Durum Güncellemesi</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .status {{ background-color: #28a745; color: white; padding: 10px; border-radius: 5px; text-align: center; margin: 20px 0; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Sipariş Durum Güncellemesi</h1>
        </div>
        <div class='content'>
            <p>Merhaba <strong>{customerName}</strong>,</p>
            <p>Siparişinizin durumu güncellenmiştir:</p>
            
            <div class='status'>
                <h3>{status}</h3>
            </div>
            
            <div style='background-color: white; padding: 15px; border-left: 4px solid #007bff; margin: 20px 0;'>
                <h3>Sipariş Bilgileri</h3>
                <p><strong>Sipariş No:</strong> {orderNumber}</p>
                <p><strong>Durum:</strong> {status}</p>
            </div>
            
            <p>Sipariş durumunuzu takip etmek için:</p>
            <a href='https://localhost:5214/Checkout/OrderHistory' class='button'>Siparişlerim</a>
            
            <p>Soru veya sorunlarınız için bize ulaşabilirsiniz.</p>
            
            <p>Teşekkür ederiz!</p>
            <p>E-Commerce Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 E-Commerce. Tüm hakları saklıdır.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateWelcomeEmailBody(string customerName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Hoş Geldiniz!</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Hoş Geldiniz!</h1>
        </div>
        <div class='content'>
            <p>Merhaba <strong>{customerName}</strong>,</p>
            <p>E-Commerce ailesine katıldığınız için çok mutluyuz!</p>
            
            <p>Size özel indirimler ve kampanyalardan haberdar olmak için bültenimize abone olmayı unutmayın.</p>
            
            <p>Alışverişe başlamak için:</p>
            <a href='https://localhost:5214/Product/Index' class='button'>Ürünleri Keşfet</a>
            
            <p>Soru veya sorunlarınız için bize ulaşabilirsiniz.</p>
            
            <p>İyi alışverişler dileriz!</p>
            <p>E-Commerce Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 E-Commerce. Tüm hakları saklıdır.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
