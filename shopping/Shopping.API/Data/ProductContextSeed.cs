using MongoDB.Driver;
using Shopping.API.Models;

namespace Shopping.API.Data
{
    public static class ProductContextSeed
    {
        public static void SeedData(IMongoCollection<Product> productCollection)
        {
            bool existProduct = productCollection.Find(p => true).Any();
            
            if (!existProduct)
            {
                productCollection.InsertManyAsync(GetPreconfiguredProducts());
            }
        }

        private static IEnumerable<Product> GetPreconfiguredProducts()
        {
            return new List<Product>()
            {
                new Product()
                {
                    Name = "IPhone X",
                    Description = "最新的 iPhone，擁有強大的處理器和高品質相機",
                    ImageFile = "",
                    Price = 950.00M,
                    Category = "Smart Phone"
                },
                new Product()
                {
                    Name = "Samsung 10",
                    Description = "韓國製造的優質手機，AMOLED 螢幕",
                    ImageFile = "",
                    Price = 840.00M,
                    Category = "Smart Phone"
                },
                new Product()
                {
                    Name = "Huawei Plus",
                    Description = "中國製造的創新手機，徠卡鏡頭",
                    ImageFile = "",
                    Price = 650.00M,
                    Category = "White Appliances"
                },
                new Product()
                {
                    Name = "Xiaomi Mi 9",
                    Description = "小米旗艦機，性價比之王",
                    ImageFile = "",
                    Price = 470.00M,
                    Category = "White Appliances"
                },
                new Product()
                {
                    Name = "HTC U11+ Plus",
                    Description = "台灣之光，優質手機品牌",
                    ImageFile = "",
                    Price = 380.00M,
                    Category = "Smart Phone"
                },
                new Product()
                {
                    Name = "LG G7 ThinQ",
                    Description = "LG 旗艦機種，AI 相機功能",
                    ImageFile = "",
                    Price = 240.00M,
                    Category = "Home Kitchen"
                },
                new Product()
                {
                    Name = "Sony Xperia XZ3",
                    Description = "日本製造，OLED 螢幕",
                    ImageFile = "",
                    Price = 720.00M,
                    Category = "Smart Phone"
                },
                new Product()
                {
                    Name = "Google Pixel 3",
                    Description = "Google 原生系統，最佳相機體驗",
                    ImageFile = "",
                    Price = 799.00M,
                    Category = "Smart Phone"
                }
            };
        }
    }
}