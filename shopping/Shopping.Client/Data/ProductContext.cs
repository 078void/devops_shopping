using Shopping.Client.Models;

namespace Shopping.Client.Data
{
    public static class ProductContext
    {
        public static readonly List<Product> Products = new List<Product>
        {
            new Product
            {
                Name = "IPhone X",
                Description = "最新的 iPhone，擁有強大的處理器",
                ImageFile = "",
                Price = 950.00M,
                Category = "Smart Phone"
            },
            new Product
            {
                Name = "Samsung 10",
                Description = "韓國製造的優質手機",
                ImageFile = "",
                Price = 840.00M,
                Category = "Smart Phone"
            },
            new Product
            {
                Name = "Huawei Plus",
                Description = "中國製造的創新手機",
                ImageFile = "",
                Price = 650.00M,
                Category = "White Appliances"
            },
            new Product
            {
                Name = "Xiaomi Mi 9",
                Description = "小米旗艦機，性價比之王",
                ImageFile = "",
                Price = 470.00M,
                Category = "White Appliances"
            },
            new Product
            {
                Name = "HTC U11+ Plus",
                Description = "台灣之光，優質手機",
                ImageFile = "",
                Price = 380.00M,
                Category = "Smart Phone"
            },
            new Product
            {
                Name = "LG G7 ThinQ",
                Description = "LG 旗艦機種",
                ImageFile = "",
                Price = 240.00M,
                Category = "Home Kitchen"
            }
        };
    }
}