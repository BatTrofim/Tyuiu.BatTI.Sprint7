﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tyuiu.BatTI.Sprint7.Project.V10.Lib
{
    public class ItemModel
    {
        public string Code { get; set; }          // Артикул
        public string Name { get; set; }          // Название
        public string Category { get; set; }      // НОВОЕ: Категория товара
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Supplier { get; set; }
        public string DeliveryDate { get; set; }
    }

    public class DataService
    {
        public List<ItemModel> LoadData(string path)
        {
            var list = new List<ItemModel>();
            if (!File.Exists(path)) return list;

            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            foreach (var line in lines)
            {
                var parts = line.Split(';');
                if (parts.Length >= 8) // Стало больше полей
                {
                    int.TryParse(parts[3], out int qty);
                    decimal.TryParse(parts[4].Replace('.', ','), out decimal price);

                    list.Add(new ItemModel
                    {
                        Code = parts[0],
                        Name = parts[1],
                        Category = parts[2], // Читаем категорию
                        Quantity = qty,
                        Price = price,
                        Description = parts[5],
                        Supplier = parts[6],
                        DeliveryDate = parts[7]
                    });
                }
            }
            return list;
        }

        public void SaveData(string path, List<ItemModel> data)
        {
            var sb = new StringBuilder();
            foreach (var item in data)
            {
                sb.AppendLine($"{item.Code};{item.Name};{item.Category};{item.Quantity};{item.Price};{item.Description};{item.Supplier};{item.DeliveryDate}");
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        // --- ЛОГИКА ---

        // Добавление товара
        public void AddItem(List<ItemModel> data, ItemModel newItem)
        {
            data.Add(newItem);
        }

        // Удаление товара по индексу
        public void RemoveItem(List<ItemModel> data, int index)
        {
            if (index >= 0 && index < data.Count) data.RemoveAt(index);
        }

        // Статистика
        public int GetTotalQuantity(List<ItemModel> data) => data.Sum(x => x.Quantity);
        public decimal GetTotalStockValue(List<ItemModel> data) => data.Sum(x => x.Price * x.Quantity);
        public decimal GetAveragePrice(List<ItemModel> data) => data.Count > 0 ? data.Average(x => x.Price) : 0;
        public decimal GetMaxPrice(List<ItemModel> data) => data.Count > 0 ? data.Max(x => x.Price) : 0;
        public decimal GetMinPrice(List<ItemModel> data) => data.Count > 0 ? data.Min(x => x.Price) : 0;

        // Поиск
        public List<ItemModel> Search(List<ItemModel> data, string query)
        {
            query = query.ToLower();
            return data.Where(x => x.Name.ToLower().Contains(query) || x.Code.Contains(query)).ToList();
        }

        // Фильтр по категории
        public List<ItemModel> FilterByCategory(List<ItemModel> data, string category)
        {
            if (category == "Все") return data;
            return data.Where(x => x.Category == category).ToList();
        }
    }
}