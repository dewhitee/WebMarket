﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebMarket.Data;

namespace WebMarket.Models
{
    [Serializable]
    public class Product : IComparable<Product>
    {
        public int ID { get; set; }

        [Required]
        [StringLength(32)]
        public string Name { get; set; }

        [Required, StringLength(32)]
        public string Type { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [NotMapped]
        public int CostIntegral { get => (int)Math.Truncate(Price); }

        [NotMapped]
        public decimal CostFractional { get => Price - CostIntegral; }
        public float Discount { get; set; }

        [StringLength(512)]
        public string Description { get; set; }

        [StringLength(128)]
        public string Link { get; set; }

        [DisplayName("Only registered user can comment")]
        public bool OnlyRegisteredCanComment { get; set; }

        [DisplayName("Only one comment per user")]
        public bool OnlyOneCommentPerUser { get; set; }

        [DisplayName("File Name")]
        public string FileName { get; set; }

        [Obsolete]
        public List<UserComment> Comments = new List<UserComment>();

        [Display(Name = "Added Date")]
        [DataType(DataType.Date)]
        public DateTime AddedDate { get; set; }
        public string OwnerID { get; set; }

        public decimal FinalPrice { get => Price - (Price * (decimal)Discount * 0.01M); }
        public string FinalPriceString { get => FinalPrice > 0 ? FinalPrice.ToString("0.##") + "€" : "free"; }
        public string PriceString { get => Price > 0 ? Price.ToString() + "€" : "free"; }
        public string DiscountString { get => Discount > 0 ? Discount.ToString() + "%" : "no"; }
        public string DiscountSupString { get => Discount > 0 ? Discount.ToString() + "%" : ""; }
        public string LinkTableString { get => string.IsNullOrWhiteSpace(Link) ? "no link" : "yes"; }

        // Property for disabling buying/selling actions on product page
        [NotMapped]
        public bool NonTradableMode { get; set; }

        public string IsBoughtString(IMainRepository repo, AppUser user)
        {
            return IsBought(repo, user) ? "Bought" : "+";
        }

        public bool IsBought(IMainRepository repository, AppUser byUser)
        {
            var boughtProductIds = repository.GetBoughtProductsByUserId(byUser?.Id);
            if (boughtProductIds.Any())
            {
                return (from bp in boughtProductIds where bp.ProductRefId == ID select bp.ProductRefId).Contains(ID);
            }
            return false;
        }

        public bool HasValidID()
        {
            return ID < 0;
        }

        public bool ContainsTags(List<string> findTags, IMainRepository repository, bool fullyMatching)
        {
            var repoTags = repository.GetTagNamesByProductId(ID);
            if (fullyMatching)
            {
                foreach (var tag in findTags)
                {
                    if (!repoTags.Contains(tag))
                        return false;
                }
                return true;
            }
            else
            {
                foreach (var tag in findTags)
                {
                    if (repoTags.Contains(tag))
                        return true;
                }
                return false;
            }
        }

        public float GetRateAvg(IEnumerable<UserComment> userComments)
        {
            return GetRateSum(userComments) / userComments.Count();
        }

        public float GetRate(IEnumerable<UserComment> userComments)
        {
            float sum = GetRateSum(userComments);
            float max = userComments.Count() * 5f;
            return sum / max;
        }

        public float GetRateSum(IEnumerable<UserComment> userComments)
        {
            float sum = 0;
            foreach (var i in userComments)
            {
                sum += i.Rate;
            }
            return sum;
        }
        //public int GetStarsCount(int stars, IMainRepository repository)
        //{
        //    if (stars > 5) return -1;
        //    int count = 0;
        //    foreach (var i in repository.GetUserCommentsByProdID(ID))
        //    {
        //        if (Math.Truncate((decimal)i.Rate) == stars)
        //            count++;
        //    }
        //    return count;
        //}
        public int GetStarsCount(int stars, IEnumerable<UserComment> userComments)
        {
            if (stars > 5) return -1;
            int count = 0;
            foreach (var i in userComments)
            {
                if (Math.Truncate((decimal)i.Rate) == stars)
                    count++;
            }
            return count;
        }
        //public uint GetTotalCountOfNotNulledComments(IMainRepository repository)
        //{
        //    uint count = 0;
        //    foreach (var i in repository.GetUserCommentsByProdID(ID))
        //    {
        //        if (i.Rate != 0f)
        //            count++;
        //    }
        //    return count;
        //}
        public uint GetTotalCountOfNotNulledComments(IEnumerable<UserComment> userComments)
        {
            uint count = 0;
            foreach (var i in userComments)
            {
                if (i.Rate != 0f)
                    count++;
            }
            return count;
        }
        //public float GetStarsPercent(int stars, IMainRepository repository)
        //{
        //    Console.WriteLine($"Getting stars percent for {stars} stars...");
        //    if (stars > 5) return 0f;
        //    float starsCount = GetStarsCount(stars, repository.GetUserCommentsByProdID(ID));
        //    float totalComments = GetTotalCountOfNotNulledComments(repository);
        //    Console.WriteLine($"Final stars count is {starsCount}. This is {(starsCount <= 0 ? starsCount : starsCount / totalComments)} percent from the total amount of stars.");
        //    return starsCount <= 0 ? 0f : starsCount / totalComments;
        //}

        public float GetStarsPercent(int stars, IEnumerable<UserComment> userComments)
        {
            Console.WriteLine($"Getting stars percent for {stars} stars...");
            if (stars > 5) return 0f;
            float starsCount = GetStarsCount(stars, userComments);
            float totalComments = GetTotalCountOfNotNulledComments(userComments);
            Console.WriteLine($"Final stars count is {starsCount}. This is {(starsCount <= 0 ? starsCount : starsCount / totalComments)} percent from the total amount of stars.");
            return starsCount <= 0 ? 0f : starsCount / totalComments;
        }
 
        public uint GetTotalStarsCount(IEnumerable<UserComment> userComments)
        {
            uint count = 0;
            foreach (var i in userComments)
            {
                count += i.Stars;
            }
            return count;
        }

        public static int MakeNewID(IEnumerable<Product> allProducts)
        {
            int newID = allProducts.Count() + 1;
            while (allProducts.Any(p => p.ID == newID))
            {
                newID++;
            }
            return newID;
        }

        public long GetFileSize(IHostingEnvironment hostingEnvironment)
        {
            if (FileName.Length > 0)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "file");
                string filePath = Path.Combine(uploadsFolder, FileName);
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            return 0;
        }
        public string FormatFileSize(IHostingEnvironment hostingEnvironment)
        {
            var fileSize = GetFileSize(hostingEnvironment);
            if (fileSize > 1000000000) return new string((fileSize / 1000000000f).ToString() + " Gb");
            if (fileSize > 1000000) return new string((fileSize / 1000000f).ToString() + " Mb");
            if (fileSize > 1000) return new string((fileSize / 1000f).ToString() + " Kb");
            return new string(fileSize.ToString() + " bytes");
        }

        public string GetAddToCartButtonString(IMainRepository repo, AppUser user)
        {
            if (OwnerID == user?.Id)
                return "Yours";
            else if (IsBought(repo, user))
                return "Bought";
            else if (CatalogViewModel.ChoosenProductID == this.ID)
                return "Selected";
            else return "+";
        }

        public string GetImageSrc(IMainRepository repository, int? index)
        {
            Image image;
            if (index == null)
                image = repository.GetImagesByProductID(ID).FirstOrDefault();
            else
                image = repository.GetImageByOrderIndex(ID, index.Value);

            if (image != null)
            {
                return image.Link.Length > 0 ? image.Link : "https://abovethelaw.com/uploads/2019/09/GettyImages-508514140-300x200.jpg";
            }
            else return "https://abovethelaw.com/uploads/2019/09/GettyImages-508514140-300x200.jpg";
        }

        public static string CheckTypeString(string type)
        {
            if (type == "Choose type")
                return "No type specified";
            else return type;
        }

        //public string GetPriceTableClassString(IMainRepository repo, AppUser user)
        //{
        //    if (CatalogViewModel.ChoosenProductID == this.ID || OwnerID == user?.Id)
        //        return "bg-dark text-white";
        //    if (IsBought(repo, user))
        //        return "bg-primary text-dark";
        //    if (Price == 0 || FinalPrice == 0)
        //        return "bg-success text-dark";
        //    if (Price < 10 || FinalPrice < 10 || Discount > 50)
        //        return "bg-success text-dark";
        //    if (Price > 250 || FinalPrice > 250)
        //        return "bg-danger text-dark";
        //    return "";
        //}

        public string GetProductTableLinkClassString(IMainRepository repo, AppUser user)
        {
            if (IsBought(repo, user) || CatalogViewModel.ChoosenProductID == this.ID || OwnerID == user?.Id)
                return "text-white";
            else
                return "text-dark";
        }

        public string GetAddToCartButtonClassString(IMainRepository repo, AppUser user)
        {
            if (CatalogViewModel.ChoosenProductID == this.ID || OwnerID == user?.Id)
            {
                return "btn btn-outline-light";
            }
            else if (user?.Money < FinalPrice)
                return "btn btn-outline-danger";
            else if (!IsBought(repo, user))
                return "btn btn-outline-success";
            else return "btn btn-primary";
        }

        public string GetTableHeaderClassString(IMainRepository repo, AppUser user)
        {
            if (CatalogViewModel.ChoosenProductID == this.ID || OwnerID == user?.Id)
            {
                //if ()
                return "bg-dark text-white";
            }
            else if (IsBought(repo, user))
            {
                return "bg-primary text-white";
            }
            else return "";
        }

        public static int CompareByName(Product x, Product y)
        {
            return x.Name.CompareTo(y.Name);
        }
        public static int CompareByType(Product x, Product y)
        {
            return x.Type.CompareTo(y.Type);
        }
        public static int CompareByPrice(Product x, Product y)
        {
            return x.Price.CompareTo(y.Price);
        }
        public static int CompareByDiscount(Product x, Product y)
        {
            return y.Discount.CompareTo(x.Discount);
        }
        public static int CompareByFinalPrice(Product x, Product y)
        {
            return x.FinalPrice.CompareTo(y.FinalPrice);
        }

        public int CompareTo([AllowNull] Product other)
        {
            if (this.ID < other.ID)
                return 1;
            else if (this.ID > other.ID)
                return -1;
            else
                return 0;
        }
    }
}
