using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Assignment3
{
   public class CategoryService
    {
        private static readonly IList<Category> Categories = new List<Category>();

        static CategoryService()
        {
            Category firsCategory = new Category{Id = 1, Name = "Beverages"};
            Category secondCategory = new Category { Id = 2, Name = "Condiments" };
            Category thirdCategory = new Category { Id = 3, Name = "Confections" };


            Categories.Add(firsCategory);
            Categories.Add(secondCategory);
            Categories.Add(thirdCategory);
        }

        public IList<Category> GetCategories()
        {
            return Categories;
        }
        public Category GetCategoryByID(string id)
        {
            int idNumber = int.Parse(id);
            return Categories.FirstOrDefault(category => category.Id == idNumber);
        }

        public Category UpdateCategory(string id, Category category)
        {
            int idNumber = int.Parse(id);
            Category existingCategory = Categories.FirstOrDefault(c => c.Id == idNumber);
            if (existingCategory == null) return null;
            existingCategory.Id =category.Id;
            existingCategory.Name = category.Name;
            return existingCategory;
        }

        public Category DeleteCategory(string id)
        {
            Category category = GetCategoryByID(id);
            if (category == null) return null;
            bool removed = Categories.Remove(category);
            if (removed) return category;
            return null;
        }

        public static string ToJson(object data)
        {
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public static T FromJson<T>(string element)
        {
            return JsonSerializer.Deserialize<T>(element, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

    }
}
