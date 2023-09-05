using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp5.Models
{
    public class RowModel
    {
        public RowModel()
        {
            Name = String.Empty;
            Qty = 0;
        }
        public RowModel(string name, int qty)
        {
            Name = name;
            Qty = qty;
        }
        public string Name { get; set; }

        public int Qty { get; set; }
    }
}
