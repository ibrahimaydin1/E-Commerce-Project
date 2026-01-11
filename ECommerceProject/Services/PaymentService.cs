using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ECommerceProject.Services
{
    public class PaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public PaymentService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                // Iyzico API ayarları
                var apiKey = _configuration["Iyzico:ApiKey"];
                var secretKey = _configuration["Iyzico:SecretKey"];
                var baseUrl = _configuration["Iyzico:BaseUrl"] ?? "https://api.iyzico.com";

                // Basit ödeme isteği oluştur
                var paymentData = new
                {
                    locale = "tr",
                    conversationId = request.OrderNumber,
                    price = request.Amount.ToString("F2").Replace(",", "."),
                    paidPrice = request.Amount.ToString("F2").Replace(",", "."),
                    currency = "TRY",
                    installment = 1,
                    basketId = request.OrderNumber,
                    paymentChannel = "web",
                    paymentGroup = "product",
                    paymentCard = new
                    {
                        cardHolderName = request.CardHolderName,
                        cardNumber = request.CardNumber,
                        expireMonth = request.ExpireMonth,
                        expireYear = request.ExpireYear,
                        cvc = request.Cvc,
                        registerCard = 0
                    },
                    buyer = new
                    {
                        id = request.UserId,
                        name = request.FirstName,
                        surname = request.LastName,
                        email = request.Email,
                        phone = request.Phone,
                        identityNumber = request.IdentityNumber,
                        address = request.Address,
                        ip = request.IpAddress,
                        city = request.City,
                        country = request.Country,
                        zipCode = request.ZipCode
                    },
                    billingAddress = new
                    {
                        address = request.Address,
                        zipCode = request.ZipCode,
                        contactName = $"{request.FirstName} {request.LastName}",
                        city = request.City,
                        country = request.Country
                    },
                    basketItems = request.BasketItems
                };

                var json = JsonSerializer.Serialize(paymentData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Authorization header oluştur
                var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{secretKey}"));
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authString);

                var response = await _httpClient.PostAsync($"{baseUrl}/v2/iyzipay/payment/iyzipay/auth/ecom", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var paymentResult = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    
                    return new PaymentResponse
                    {
                        Success = true,
                        PaymentId = paymentResult?["paymentId"]?.ToString(),
                        Status = "success",
                        Message = "Ödeme başarıyla tamamlandı"
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new PaymentResponse
                    {
                        Success = false,
                        Status = "failed",
                        Message = $"Ödeme hatası: {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new PaymentResponse
                {
                    Success = false,
                    Status = "error",
                    Message = $"Ödeme işleminde hata: {ex.Message}"
                };
            }
        }

        // Test ödeme metodu (gerçek API olmadan)
        public PaymentResponse ProcessTestPayment(PaymentRequest request)
        {
            try
            {
                // Basit test kartı kontrolü
                if (request.CardNumber == "4242424242424242")
                {
                    return new PaymentResponse
                    {
                        Success = true,
                        PaymentId = "TEST_" + DateTime.Now.Ticks,
                        Status = "success",
                        Message = "Test ödemesi başarıyla tamamlandı"
                    };
                }
                else
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        Status = "failed",
                        Message = "Geçersiz kart bilgileri"
                    };
                }
            }
            catch (Exception ex)
            {
                return new PaymentResponse
                {
                    Success = false,
                    Status = "error",
                    Message = $"Ödeme hatası: {ex.Message}"
                };
            }
        }
    }

    public class PaymentRequest
    {
        public string? OrderNumber { get; set; }
        public decimal Amount { get; set; }
        public string? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? IdentityNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? ZipCode { get; set; }
        public string? IpAddress { get; set; }
        public string? CardHolderName { get; set; }
        public string? CardNumber { get; set; }
        public string? ExpireMonth { get; set; }
        public string? ExpireYear { get; set; }
        public string? Cvc { get; set; }
        public List<object>? BasketItems { get; set; }
    }

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string? PaymentId { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
    }
}
